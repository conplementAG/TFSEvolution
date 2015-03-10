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
using System.Collections.ObjectModel;
using TFSDataAccessPortable;

namespace TFSExpert.ViewModels
{
    public class Team
    {
        #region members

        // the underlying DataModel
        private TeamTFSData _objModel;

        private ObservableCollection<TeamMember> _cobjTeamMembers;

        #endregion

        #region properties

        public string Name
        {
            get { return _objModel.TeamName; }
        }

        public bool IsDefaultTeam
        {
            get { return _objModel.IsDefaultTeam; }
        }

        public string CurrentIterationPath
        {
            get { return _objModel.CurrentIterationPath; }
        }

        public string[] ActiveIterations
        {
            get { return _objModel.ActiveIterations; }
        }

        public List<TeamTFSData.AreaData> Areas
        {
            get { return _objModel.Areas; }
        }

        public ObservableCollection<TeamMember> TeamMembers
        {
            get { return _cobjTeamMembers; }
        }

        #endregion

        public Team(TeamTFSData model, IEnumerable<PersonData> teamMembers = null)
        {
            _objModel = model;

            if (teamMembers != null)
            {
                _cobjTeamMembers = new ObservableCollection<TeamMember>();
                TeamMember notAssigned = new TeamMember("not assigned");
                _cobjTeamMembers.Insert(0, notAssigned);

                foreach (var member in teamMembers)
                {
                    if (member.DisplayName != "TFS Expert")
                        _cobjTeamMembers.Add(new TeamMember(member));
                }
            }
        }
    }
}