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
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TFSExpert.Common;
using TFSExpert.Navigation;
using TFSExpert.ViewModels;
using TFSExpert.Views;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace TFSExpert
{
    /// <summary>
    ///     A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        ///     NavigationHelper is used on each page to aid in navigation and
        ///     process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return navigationHelper; }
        }

        /// <summary>
        ///     This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return defaultViewModel; }
        }

        public DashboardPage()
        {
            InitializeComponent();

            navigationHelper = new NavigationHelper(this);
            navigationHelper.LoadState += navigationHelper_LoadState;
        }

        /// <summary>
        ///     Populates the page with content passed during navigation.  Any saved state is also
        ///     provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        ///     The source of the event; typically <see cref="NavigationHelper" />
        /// </param>
        /// <param name="e">
        ///     Event data that provides both the navigation parameter passed to
        ///     <see cref="Frame.Navigate(Type, Object)" /> when this page was initially requested and
        ///     a dictionary of state preserved by this page during an earlier
        ///     session.  The state will be null the first time a page is visited.
        /// </param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            ((App) Application.Current).DataSource.SprintDataLoaded += DataSource_SprintDataLoaded;
            ((App) Application.Current).DataSource.SprintDataLoading += DataSource_SprintDataLoading;

            ((App) Application.Current).DataSource.LoadDashboardDataAsync();
            DefaultViewModel["LatestBuildEvents"] = ((App) Application.Current).DataSource.LatestBuildEvents;
            DefaultViewModel["LatestCheckinEvents"] = ((App) Application.Current).DataSource.LatestCheckinEvents;
            DefaultViewModel["CurrentTeamProject"] = Navigator.CurrentTeamProject;
            DefaultViewModel["Sprint"] = ((App) Application.Current).DataSource.Sprint;
        }

        private void DataSource_SprintDataLoading()
        {
            VisualStateManager.GoToState(SprintStatusCtrl, SprintStatusInfoControl.States.Loading.ToString(), false);
            VisualStateManager.GoToState(BurnDownCtrl, BurndownControl.States.Loading.ToString(), false);
        }

        private void DataSource_SprintDataLoaded(bool success, Sprint sprintData)
        {
            if (success)
            {
                if (sprintData != null && sprintData.BurndownData != null)
                {
                    sprintData.RefreshBurndownData();
                    VisualStateManager.GoToState(SprintStatusCtrl, SprintStatusInfoControl.States.Loaded.ToString(),
                        false);

                    // if we have no data with remaining work --> no work
                    VisualStateManager.GoToState(BurnDownCtrl,
                        sprintData.BurndownData.FirstOrDefault(dp => dp.RemainingWork.HasValue) == null
                            ? BurndownControl.States.NoData.ToString()
                            : BurndownControl.States.Loaded.ToString(), false);
                }
                else
                {
                    VisualStateManager.GoToState(SprintStatusCtrl, SprintStatusInfoControl.States.NoData.ToString(),
                        false);
                    VisualStateManager.GoToState(BurnDownCtrl, BurndownControl.States.NoSprint.ToString(), false);
                }
            }
            else
            {
                VisualStateManager.GoToState(SprintStatusCtrl, SprintStatusInfoControl.States.Error.ToString(), false);
                VisualStateManager.GoToState(BurnDownCtrl, BurndownControl.States.Error.ToString(), false);
            }
        }

        private void OnSwitchTeamClicked(object sender, RoutedEventArgs e)
        {
            FrameworkElement context = sender as FrameworkElement;

            if (context != null && context.DataContext != null)
            {
                Team selectedTeam = (Team) context.DataContext;
                Navigator.CurrentTeamProject.SwitchTeam(selectedTeam);
                ((App) Application.Current).DataSource.LoadDashboardDataAsync();

                TopAppBar.IsOpen = false;
                BottomAppBar.IsOpen = false;
            }
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="GridCS.Common.NavigationHelper.LoadState" />
        /// and
        /// <see cref="GridCS.Common.NavigationHelper.SaveState" />
        /// .
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void OnReloadClicked(object sender, RoutedEventArgs e)
        {
            ((App) Application.Current).DataSource.LoadDashboardDataAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (Taskboard));
        }
    }
}