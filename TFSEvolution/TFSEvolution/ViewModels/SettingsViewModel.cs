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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TFSExpert.Common;
using TFSExpert.Data;

namespace TFSExpert.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        public bool IsOwnServiceConfigured { get; set; }

        public string ServiceUrl
        {
            get { return string.Format("https://{0}", Context.AdditionalRestServer); }
        }

        public string NewServiceUrl { get; set; }

        public RelayCommand ChangeService { get; private set; }
        public RelayCommand ChangeServiceUrl { get; private set; }
        public RelayCommand SwitchToEdit { get; private set; }
        public RelayCommand CancelEdit { get; private set; }

        public bool IsEditMode { get; private set; }

        public bool IsError { get; set; }

        public bool IsChecking { get; set; }

        public bool ShowEditButton
        {
            get { return !IsEditMode && IsOwnServiceConfigured; }
        }

        public SettingsViewModel()
        {
            IsOwnServiceConfigured = Context.IsOwnServerDefined;
            ChangeService = new RelayCommand(OnServiceSourceChanged);
            ChangeServiceUrl = new RelayCommand(OnServiceUrlChanged);
            SwitchToEdit = new RelayCommand(OnSwitchToEdit);
            CancelEdit = new RelayCommand(OnCancelEdit);
        }

        private void OnSwitchToEdit()
        {
            IsEditMode = true;
            OnPropertyChanged("IsEditMode");
            OnPropertyChanged("ShowEditButton");
        }

        private void OnCancelEdit()
        {
            IsEditMode = false;
            OnPropertyChanged("IsEditMode");
            OnPropertyChanged("ShowEditButton");
        }

        private void OnServiceSourceChanged()
        {
            IsError = false;
            OnPropertyChanged("IsError");

            // user switched to predefined service url
            if (!IsOwnServiceConfigured)
            {
                IsEditMode = false;
                Context.ResetServiceUrl();
                OnPropertyChanged("ServiceUrl");
                OnPropertyChanged("IsEditMode");
                OnPropertyChanged("ShowEditButton");
            }
                // user switched to self defined service url
            else
            {
                IsEditMode = true;
                OnPropertyChanged("IsEditMode");
                OnPropertyChanged("ShowEditButton");
            }
        }

        private async void OnServiceUrlChanged()
        {
            IsChecking = true;
            OnPropertyChanged("IsChecking");

            if (await Context.SaveServerUrlAsync(NewServiceUrl))
            {
                IsChecking = false;
                OnPropertyChanged("IsChecking");

                IsEditMode = false;
                NewServiceUrl = string.Empty;
                OnPropertyChanged("NewServiceUrl");
                OnPropertyChanged("ServiceUrl");
                OnPropertyChanged("IsEditMode");
                OnPropertyChanged("ShowEditButton");

                // switch to project selection page if we are not on start page
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame != null)
                {
                    rootFrame.Navigate(typeof (ProjectSelectionPage), true);
                }
            }
            else
            {
                IsChecking = false;
                OnPropertyChanged("IsChecking");

                IsEditMode = false;
                IsOwnServiceConfigured = Context.IsOwnServerDefined;
                NewServiceUrl = string.Empty;
                OnPropertyChanged("NewServiceUrl");
                OnPropertyChanged("IsOwnServiceConfigured");
                OnPropertyChanged("IsEditMode");
                OnPropertyChanged("ShowEditButton");
                OnPropertyChanged("ServiceUrl");
                IsError = true;
                OnPropertyChanged("IsError");
            }
        }
    }
}