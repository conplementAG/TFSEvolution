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

using System.Collections.Generic;
using System.Web.Http;

namespace TFSWebService
{
    public class SprintsController : ApiController
    {
        /// <summary>
        ///     Gets all sprints defined for the given project.
        ///     API Call:
        ///     GET: api/sprints?tfsuri=
        ///     <tfsserveraddress>&user=<username>&pwd=<password>&projectCollection=<projectcollection>&project=<projectname>
        /// </summary>
        /// <param name="tfsuri">Url of the TFS.</param>
        /// <param name="user">Username for accessing the TFS.</param>
        /// <param name="pwd">Password for the given user.</param>
        /// <param name="projectCollection">The project collection to project is part of.</param>
        /// <param name="project">Project name.</param>
        /// <returns>A list of Sprint objects.</returns>
        public IEnumerable<Sprint> GetSprintsByProject(string tfsuri, string user, string pwd, string projectCollection,
            string project)
        {
            return TFSClient.GetSprintsByProject(tfsuri, user, pwd, projectCollection, project);
        }
    }
}