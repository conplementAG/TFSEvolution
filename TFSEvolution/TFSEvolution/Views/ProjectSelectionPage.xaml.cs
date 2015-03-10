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
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TFSExpert.Auth;
using TFSExpert.Common;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace TFSExpert
{
    /// <summary>
    ///     A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class ProjectSelectionPage : Page
    {
        private enum States
        {
            NoData,
            ProjectLoadingError,
            LoginError,
            Default
        }

        private NavigationHelper _navigationHelper;
        private ObservableDictionary _defaultViewModel = new ObservableDictionary();

        /// <summary>
        ///     NavigationHelper is used on each page to aid in navigation and
        ///     process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        /// <summary>
        ///     This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return _defaultViewModel; }
        }

        public ProjectSelectionPage()
        {
            InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Enabled;
            _navigationHelper = new NavigationHelper(this);
            _navigationHelper.LoadState += navigationHelper_LoadState;
            _navigationHelper.SaveState += navigationHelper_SaveState;
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
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // Force settings flyout to close
            Focus(FocusState.Programmatic);
            VisualStateManager.GoToState(this, States.Default.ToString(), true);

            if (((App) Application.Current).DataSource != null)
            {
                // if online version is used --> Do Authentication
                await Authenticator.Login();
                await Authenticator.BasicLogin();
            }

            if (e.PageState != null && e.PageState.ContainsKey("Projects"))
            {
                // we do not need to load data
                DefaultViewModel["Projects"] = e.PageState["Projects"];
            }
            else
            {
                bool success;
                if (((App) Application.Current).DataSource != null)
                {
                    // if online version is used --> Do Authentication
                    success = await Authenticator.Login();
                    success = await Authenticator.BasicLogin();
                }
                else
                {
                    // if demo mode --> succeed without login
                    success = true;
                }
                if (success)
                {
                    ((App) Application.Current).DataSource.LoadProjectDataAsync();
                    ((App) Application.Current).DataSource.ProjectsLoaded += OnProjectsLoaded;

                    DefaultViewModel["Projects"] = ((App) Application.Current).DataSource.TeamProjects;
                }
                else
                {
                    // logon not successful
                    VisualStateManager.GoToState(this, States.LoginError.ToString(), true);
                }
            }
        }

        /// <summary>
        ///     Preserves state associated with this page in case the application is suspended or the
        ///     page is discarded from the navigation cache.  Values must conform to the serialization
        ///     requirements of <see cref="SuspensionManager.SessionState" />.
        /// </summary>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["Projects"] = ((App) Application.Current).DataSource.TeamProjects;
        }


        /// <summary>
        ///     Invoked when the project data loading is finished.
        /// </summary>
        /// <param name="bSuccess">True if loading was successful, false otherwise.</param>
        private async void OnProjectsLoaded(bool bSuccess)
        {
            if (bSuccess)
            {
                // if no project found --> show no data error
                if (((App) Application.Current).DataSource.TeamProjects.Count < 1)
                {
                    VisualStateManager.GoToState(this, States.NoData.ToString(), true);
                }
                    // else show data
                else
                {
                    VisualStateManager.GoToState(this, States.Default.ToString(), true);

                    // if we did not find any project supported by the app --> show a message box
                    if (
                        ((App) Application.Current).DataSource.TeamProjects.FirstOrDefault(
                            n => n.IsProjectSupportedByApp) != null)
                    {
                        return;
                    }

                    if (Authenticator.IsBasicAuthDataDefined)
                    {
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            // Create the message dialog and set its content
                            var messageDialog =
                                new MessageDialog(
                                    "All of your projects are not supported by the app.\nPlease check your alternative credentials in the settings pane.",
                                    "Projects not supported");

                            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
                            messageDialog.Commands.Add(new UICommand(
                                "OK",
                                SwitchToSettings));

                            // Show the message dialog
                            messageDialog.ShowAsync();
                        });
                    }
                    else
                    {
                        await Authenticator.BasicLogin();
                    }
                }
            }
            else
            {
                VisualStateManager.GoToState(this, States.ProjectLoadingError.ToString(), true);
            }
        }

        private void SwitchToSettings(IUICommand command)
        {
            SettingsPane.Show();
        }

        /// <summary>
        ///     Invoked when login button is clicked.
        /// </summary>
        private void OnLoginClicked(object sender, RoutedEventArgs e)
        {
            // Switch state
            VisualStateManager.GoToState(this, States.Default.ToString(), true);

            // Reload page again
            Frame.Navigate(typeof (ProjectSelectionPage));
        }

        private void OnReloadClicked(object sender, RoutedEventArgs e)
        {
            ((App) Application.Current).DataSource.LoadProjectDataAsync();
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
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}