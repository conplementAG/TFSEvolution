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

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TFSExpert.Auth;
using TFSExpert.DataModel;
using TFSExpert.Navigation;
using TFSExpert.Views;

namespace TFSExpert
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public DataSource DataSource { get; private set; }
        public INavigationService Navigator { get; private set; }

        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        public void InitDataSource()
        {
            DataSource = new DataSource();
        }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            SettingsPane.GetForCurrentView().CommandsRequested += App_CommandsRequested;
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += App_AccountCommandsRequested;

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame {Language = ApplicationLanguages.Languages[0]};
                // Set the default language

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            Navigator = new Navigator(rootFrame);

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter

                ((App) Current).InitDataSource();

                rootFrame.Navigate(typeof (ProjectSelectionPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void App_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            try
            {
                // Show the “Accounts” command when the “Settings” pane is displayed
                args.Request.ApplicationCommands.Add(SettingsCommand.AccountsCommand);
                args.Request.ApplicationCommands.Add(new SettingsCommand(
                    "Service Url", "Service Url", handler => ShowServerSettingFlyout()));
            }
            catch
            {
            }
        }

        private static void ShowServerSettingFlyout()
        {
            var setting = new ServerSettingsFlyout();
            setting.Show();
        }

        private void App_AccountCommandsRequested(AccountsSettingsPane sender,
            AccountsSettingsPaneCommandsRequestedEventArgs args)
        {
            // Get the Deferral object if async operations are required.          
            var deferral = args.GetDeferral();

            // Handler for the delete credential command.    
            var credDeletedHandler = new CredentialCommandCredentialDeletedHandler(OnCredentialDeleted);

            // Get credentials to display in the Settings pane.
            var credential = Authenticator.GetCredentialForSettings();

            if (credential == null)
            {
                args.HeaderText = "No alternative credentials found.";
            }
            else
            {
                args.HeaderText = "Alternative credentials for your VSO account:";

                try
                {
                    var credCommand = new CredentialCommand(credential, credDeletedHandler);

                    // Delete command is invoked after the system deletes the credential
                    args.CredentialCommands.Add(credCommand);
                }
                catch (Exception)
                {
                    // Handle error.
                }
            }
            // Complete the Deferral.
            deferral.Complete();
        }

        private void OnCredentialDeleted(CredentialCommand command)
        {
            Authenticator.BasicLogout();

            // switch to project selection page if we are not on start page
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null)
            {
                rootFrame.Navigate(typeof (ProjectSelectionPage), true);
            }
        }

        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}