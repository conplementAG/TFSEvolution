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
using System.Security;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Credentials.UI;
using Windows.UI.Xaml;
using TFSDataAccessPortable;

namespace TFSExpert.Auth
{
    public class BasicAuthenticator : IAuthenticator
    {
        // resource string for saving credentials
        private const string CREDENTIALS_RESOURCE = "TFSExpertBasic";

        private AuthenticationData AuthData { get; set; }

        public bool IsBasicAuthDataDefined
        {
            get { return AuthData != null; }
        }

        public BasicAuthenticator()
        {
            PasswordCredential credit = null;
            try
            {
                credit = GetCredential();
            }
            catch
            {
            }

            if (credit != null)
            {
                AuthData = new AuthenticationData(credit.UserName, credit.Password);
            }
        }

        public async Task<bool> Login()
        {
            if (AuthData == null)
            {
                // Get new Data and save it
                AuthData = await AuthenticateAsync(null);
            }

            if (AuthData != null)
            {
                return true;
            }
            return false;
        }

        public async Task<AuthenticationData> GetValidAuthData()
        {
            if (AuthData != null && AuthData.IsValidBasicData)
            {
                return AuthData;
            }

            if (await Login())
            {
                return AuthData;
            }
            return null;
        }

        public PasswordCredential GetCredentialForSettings()
        {
            return GetCredential();
        }

        private PasswordCredential GetCredential()
        {
            PasswordCredential passwordCredential = null;
            PasswordVault passwordVault = new PasswordVault();

            try
            {
                passwordCredential = passwordVault.FindAllByResource(CREDENTIALS_RESOURCE).FirstOrDefault();

                if (passwordCredential != null)
                {
                    passwordCredential.Password =
                        passwordVault.Retrieve(CREDENTIALS_RESOURCE, passwordCredential.UserName).Password;
                }
            }
            catch
            {
            }
            return passwordCredential;
        }

        private void SaveCredential(AuthenticationData authData)
        {
            PasswordVault passwordVault = new PasswordVault();
            PasswordCredential passwordCredential = new PasswordCredential(CREDENTIALS_RESOURCE, authData.UserName,
                authData.Password);

            passwordVault.Add(passwordCredential);
        }

        /// <summary>
        ///     The authenticate async.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        private async Task<AuthenticationData> AuthenticateAsync(uint? errorCode)
        {
            try
            {
                CredentialPickerOptions options = new CredentialPickerOptions
                {
                    AuthenticationProtocol = AuthenticationProtocol.Basic,
                    CredentialSaveOption = CredentialSaveOption.Selected,
                    CallerSavesCredential = true,
                    Caption = "Login to use " + Application.Current.Resources["AppName"],
                    Message = "Please enter the alternative credentials of your VSO account.",
                    TargetName = "."
                };

                if (errorCode.HasValue)
                {
                    options.ErrorCode = errorCode.Value;
                }

                CredentialPickerResults results = await CredentialPicker.PickAsync(options);

                if (results.ErrorCode == 0)
                {
                    // check credentials
                    string username = results.CredentialUserName;
                    string password = results.CredentialPassword;

                    AuthData = new AuthenticationData(username, password);

                    if (AuthData.IsValidBasicData)
                    {
                        // save credentials if they are correct and user wants to.
                        if (results.CredentialSaveOption == CredentialSaveOption.Selected)
                        {
                            SaveCredential(AuthData);
                        }

                        return AuthData;
                    }

                    // error and try again
                    // http://msdn.microsoft.com/en-us/library/cc231199.aspx
                    return await AuthenticateAsync(0x00000421);
                }
                // user aborted
                if (results.ErrorCode == 2147943623)
                {
                    throw new Exception("User canceled login");
                }

                throw new SecurityException("Authentication failed");
            }
            catch
            {
                return null;
            }
        }

        public void Logout()
        {
            // Remove password from vault
            PasswordVault vault = new PasswordVault();
            try
            {
                PasswordCredential credit = GetCredential();
                vault.Remove(credit);
            }
            catch
            {
                // No Credential in password vault --> no problem
            }

            // Clean Application data and start it again
            AuthData = null;
            ((App)Application.Current).DataSource.ResetDataSource();
        }
    }
}