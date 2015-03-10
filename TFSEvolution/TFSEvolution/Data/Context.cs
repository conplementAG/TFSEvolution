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

using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace TFSExpert.Data
{
    public class Context
    {
        private const string CUSTOM_REST_SERVICE_SETTING = "CustomRestService";
        private const string DEFAULT_SERVICE_URL = "oss-tfs.azurewebsites.net";

        private static string _additionalRestServer;

        public static bool IsServerDefined
        {
            get { return !string.IsNullOrEmpty(AdditionalRestServer); }
        }

        public static bool IsOwnServerDefined
        {
            get { return !string.IsNullOrEmpty(AdditionalRestServer) && AdditionalRestServer != DEFAULT_SERVICE_URL; }
        }

        public static string AdditionalRestServer
        {
            get
            {
                if (string.IsNullOrEmpty(_additionalRestServer))
                {
                    var setting = ApplicationData.Current.LocalSettings.Values[CUSTOM_REST_SERVICE_SETTING];
                    _additionalRestServer = setting != null ? setting.ToString() : DEFAULT_SERVICE_URL;
                }

                return _additionalRestServer;
            }
        }

        public static async Task<bool> SaveServerUrlAsync(string serverUri)
        {
            string oldUri = _additionalRestServer;

            _additionalRestServer = serverUri;
            bool success = await ((App) Application.Current).DataSource.TestConnectionToCustomRest();
            if (success)
            {
                _additionalRestServer = serverUri;
                ApplicationData.Current.LocalSettings.Values[CUSTOM_REST_SERVICE_SETTING] = _additionalRestServer;
                return true;
            }

            _additionalRestServer = oldUri;
            return false;
        }

        internal static async void ResetServiceUrl()
        {
            await SaveServerUrlAsync(DEFAULT_SERVICE_URL);
        }
    }
}