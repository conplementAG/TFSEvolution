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
using Newtonsoft.Json.Linq;

namespace TFSDataAccessPortable
{
    /// <summary>
    ///     Data class that corresponds to the TeamInfo class in our TFS Web Service
    /// </summary>
    public class TeamTFSData
    {
        public class AreaData
        {
            public string AreaName { get; private set; }
            public bool IncludeChildren { get; private set; }

            public AreaData(dynamic areaData)
            {
                AreaName = areaData.Name;
                IncludeChildren = areaData.IncludeChildren;
            }
        }

        public string TeamName { get; private set; }

        public Guid TeamID { get; private set; }

        public bool IsDefaultTeam { get; private set; }

        public string CurrentIterationPath { get; private set; }

        public string[] ActiveIterations { get; private set; }

        public List<AreaData> Areas { get; private set; }

        internal TeamTFSData(dynamic teamData)
        {
            TeamName = teamData.TeamName;
            TeamID = teamData.TeamId;
            IsDefaultTeam = teamData.IsDefaultTeam;
            CurrentIterationPath = teamData.CurrentIterationInfo;
            if (teamData.ActiveIterations is JArray)
            {
                ActiveIterations = teamData.ActiveIterations.ToObject<string[]>();
            }

            Areas = new List<AreaData>();
            if (teamData.Areas is JArray)
            {
                foreach (var item in teamData.Areas)
                {
                    AreaData area = new AreaData(item);
                    Areas.Add(area);
                }
            }
        }
    }
}