#region License and Terms

// /***************************************************************************
// Copyright (c) 2015 Conplement AG
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  
// ***************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.TeamFoundation.Server;

namespace TFSWebService
{
    public static class Extensions
    {
        /// <summary>
        ///     Get Iterations for a team project from TFS
        /// </summary>
        /// <param name="css">CommonStructureService of the TFS</param>
        /// <param name="projectUri">Uri of the project</param>
        /// <returns>A list of iterations.</returns>
        public static IEnumerable<Iteration> GetIterations(this ICommonStructureService4 css, string projectUri)
        {
            NodeInfo[] structures = css.ListStructures(projectUri);
            NodeInfo iterations = structures.FirstOrDefault(n => n.StructureType.Equals("ProjectLifecycle"));
            List<Iteration> schedule = null;

            if (iterations != null)
            {
                string projectName = css.GetProject(projectUri).Name;

                XmlElement iterationsTree = css.GetNodesXml(new[] {iterations.Uri}, true);
                GetIterationData(iterationsTree.ChildNodes[0], projectName, ref schedule);
            }

            return schedule;
        }

        /// <summary>
        ///     Get Iteration information (start, end, name) from an XML Node an add it to the given iterations.
        /// </summary>
        /// <param name="node">XML Node with the iteration data.</param>
        /// <param name="projectName">Name of the team project</param>
        /// <param name="schedule">The list of iterations, where the data should be added.</param>
        private static void GetIterationData(XmlNode node, string projectName, ref List<Iteration> schedule)
        {
            if (schedule == null)
                schedule = new List<Iteration>();

            if (node != null)
            {
                string iterationPath = node.Attributes["Path"].Value;
                if (!string.IsNullOrEmpty(iterationPath))
                {
                    // Attempt to read the start and end dates if they exist.
                    string strStartDate = (node.Attributes["StartDate"] != null)
                        ? node.Attributes["StartDate"].Value
                        : null;
                    string strEndDate = (node.Attributes["FinishDate"] != null)
                        ? node.Attributes["FinishDate"].Value
                        : null;
                    string strName = (node.Attributes["Name"]).Value;

                    bool bIsStartDateDefined = false, bIsEndDateDefined = false;
                    DateTime startDate, endDate;

                    bool success = DateTime.TryParse(strStartDate, out startDate);
                    if (success)
                    {
                        bIsStartDateDefined = true;
                    }

                    success = DateTime.TryParse(strEndDate, out endDate);
                    if (success)
                    {
                        bIsEndDateDefined = true;
                    }

                    schedule.Add(new Iteration
                    {
                        Name = strName,
                        Path = iterationPath.Replace(string.Concat("\\", projectName, "\\Iteration"), projectName),
                        IsStartDateDefined = bIsStartDateDefined,
                        StartDate = startDate,
                        IsEndDateDefined = bIsEndDateDefined,
                        EndDate = endDate
                    });
                }

                // Visit any child nodes (sub-iterations).
                if (node.FirstChild != null)
                {
                    // The first child node is the <Children> tag, which we'll skip.
                    for (int nChild = 0; nChild < node.ChildNodes[0].ChildNodes.Count; nChild++)
                        GetIterationData(node.ChildNodes[0].ChildNodes[nChild], projectName, ref schedule);
                }
            }
        }
    }
}