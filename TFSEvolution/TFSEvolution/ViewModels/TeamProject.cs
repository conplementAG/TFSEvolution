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
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using TFSDataAccessPortable;
using TFSExpert.Common;

namespace TFSExpert.ViewModels
{
    public class TeamProject : BindableBase
    {
        // the underlying DataModel
        private ProjectData _objDataModel;

        #region properties

        public Guid Id
        {
            get { return _objDataModel.ID; }
        }

        public string ProjectName
        {
            get { return _objDataModel.Name; }
        }

        public string TFSUrl
        {
            get { return _objDataModel.Account.ProjectAddress.ToString(); }
        }

        public VersionControl VersionControl
        {
            get { return _objDataModel.VersionControl; }
        }

        public ProcessTemplate ProcessTemplate
        {
            get { return _objDataModel.ProcessTemplate; }
        }

        public ObservableCollection<Team> Teams { get; private set; }

        public Team CurrentTeam { get; set; }

        public string BackgroundImage { get; set; }

        public bool IsBackgroundDark { get; set; }

        public LinkCommand ProjectSelectedCommand { get; private set; }

        #endregion

        public TeamProject(ProjectData model)
        {
            _objDataModel = model;
            Teams = new ObservableCollection<Team>();
            ProjectSelectedCommand = new LinkCommand(OnProjectClicked);
        }

        internal void SwitchTeam(Team newTeam)
        {
            if (Teams.Contains(newTeam))
            {
                CurrentTeam = newTeam;
                OnPropertyChanged("CurrentTeam");
            }
        }

        public bool IsProjectSupportedByApp
        {
            get
            {
                switch (ProcessTemplate)
                {
                    case ProcessTemplate.Scrum:
                        return true;
                    default:
                        return false;
                }
            }
        }


        /// <summary>
        ///     Invoked when an project is clicked.
        /// </summary>
        private void OnProjectClicked(object param)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            ((App) Application.Current).Navigator.Navigate(typeof (ShowcaseSelectionPage), this);
        }
    }
}