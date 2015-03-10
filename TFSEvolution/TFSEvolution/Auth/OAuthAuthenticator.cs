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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Newtonsoft.Json.Linq;
using TFSDataAccessPortable;

namespace TFSExpert.Auth
{
    public class OAuthAuthenticator : IAuthenticator
    {
        /// <summary>
        ///     The app id.
        /// </summary>
        private const string APP_ID = "<Enter your app id here";
            // App ID from https://app.vssps.visualstudio.com

        private const string APP_SECRET = "<Enter Your app secret here>";

        // Authorized Scopes from https://app.vssps.visualstudio.com
        private const string SCOPE =
            "vso.build_execute%20vso.chat_manage%20vso.code_manage%20vso.test_write%20vso.work_write";

        /// <summary>
        ///     The redirect uri.
        /// </summary>
        private const string REDIRECT_URI = "<enter your azure website here>";

        /// <summary>
        ///     The authentication uri
        /// </summary>
        private const string AUTH_URI =
            "https://app.vssps.visualstudio.com/oauth2/authorize?client_id={0}&response_type=Assertion&scope={1}&redirect_uri={2}";

        private const string REQUEST_TOKEN_URI = "https://app.vssps.visualstudio.com/oauth2/token";

        private const string REQUEST_TOKEN_POST =
            "client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion={1}&redirect_uri={2}";

        private const string REFRESH_TOKEN_POST =
            "client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=refresh_token&assertion={1}&redirect_uri={2}";

        // resource string for saving credentials
        private const string CREDENTIALS_RESOURCE = "https://app.vssps.visualstudio.com";
        private const string CREDENTIALS_USER = "TFSExpert";

        private AuthenticationToken _authToken;

        private AuthenticationToken AuthToken
        {
            get { return _authToken; }
            set
            {
                _authToken = value;
                // Save access token
                if (value != null)
                {
                    PasswordVault vault = new PasswordVault();
                    vault.Add(new PasswordCredential(CREDENTIALS_RESOURCE, CREDENTIALS_USER, value.RefreshToken));
                }
            }
        }

        public async Task<bool> Login()
        {
            PasswordVault vault = new PasswordVault();
            PasswordCredential credit = null;
            try
            {
                credit = vault.Retrieve(CREDENTIALS_RESOURCE, "TFSExpert");
            }
            catch
            {
                // No Credential in password vault --> no problem: get new credentials
            }

            if (credit != null)
            {
                // Refresh Token and set it as authentication Token
                AuthToken = await RefreshAuthenticationTokenAsync(credit.Password);
            }
            else
            {
                // Get new Token and save it
                AuthToken = await AuthenticateAsync();
            }

            if (AuthToken != null)
            {
                return true;
            }

            return false;
        }

        public async Task<AuthenticationData> GetValidAuthData()
        {
            if (AuthToken != null && AuthToken.IsValid)
            {
                return new AuthenticationData(AuthToken.AccessToken);
            }

            if (AuthToken != null)
            {
                var newToken = await RefreshAuthenticationTokenAsync(AuthToken.RefreshToken);
                AuthToken.RefreshData(newToken);
                // Do this to make new token saved in password vault
                AuthToken = AuthToken;
                return new AuthenticationData(AuthToken.AccessToken);
            }

            return null;
        }

        /// <summary>
        ///     The authenticate async.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        private async Task<AuthenticationToken> AuthenticateAsync()
        {
            try
            {
                var startUri = new Uri(string.Format(AUTH_URI, APP_ID, SCOPE, REDIRECT_URI), UriKind.Absolute);
                var endUri = new Uri(REDIRECT_URI, UriKind.Absolute);

                var result =
                    await WebAuthenticationBroker.AuthenticateAsync(
                        WebAuthenticationOptions.None,
                        startUri,
                        endUri);

                if (result.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    string[] resultValues =
                        result.ResponseData.Substring(result.ResponseData.IndexOf('?') + 1).Split('&');
                    string code = resultValues[0].Split('=')[1];

                    // Get access token
                    if (!String.IsNullOrEmpty(code))
                    {
                        return await GetAuthenticationTokenAsync(code);
                    }

                    // error = "Issue: Empty authorization code";
                    throw new Exception("User canceled login");
                }

                if (result.ResponseStatus == WebAuthenticationStatus.UserCancel)
                {
                    // error canceled by user

                    throw new Exception("User canceled login");
                }

                throw new SecurityException(string.Format("Authentication failed: {0}", result.ResponseErrorDetail));
            }
            catch
            {
                return null;
            }
        }

        private async Task<AuthenticationToken> GetAuthenticationTokenAsync(string code)
        {
            AuthenticationToken accessToken = null;

            using (var httpClient = new HttpClient())
            {
                string strPostBody = string.Format(REQUEST_TOKEN_POST, APP_SECRET, code, REDIRECT_URI);


                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                try
                {
                    var response =
                        await
                            httpClient.PostAsync(REQUEST_TOKEN_URI,
                                new StringContent(strPostBody, Encoding.UTF8, "application/x-www-form-urlencoded"));
                    if (response.IsSuccessStatusCode)
                    {
                        var strResponse = await response.Content.ReadAsStringAsync();
                        // get token
                        accessToken = CreateAuthenticationTokenFromResponse(strResponse);
                    }
                }
                catch (WebException)
                {
                    // error = "Request Issue: " + wex.Message.ToString();
                }
                catch (Exception)
                {
                    // error = "Issue: " + ex.Message.ToString();
                }
                return accessToken;
            }
        }

        private async Task<AuthenticationToken> RefreshAuthenticationTokenAsync(string refreshToken)
        {
            AuthenticationToken accessToken = null;

            using (var httpClient = new HttpClient())
            {
                string strPostBody = string.Format(REFRESH_TOKEN_POST, APP_SECRET, refreshToken, REDIRECT_URI);


                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                try
                {
                    var response =
                        await
                            httpClient.PostAsync(REQUEST_TOKEN_URI,
                                new StringContent(strPostBody, Encoding.UTF8, "application/x-www-form-urlencoded"));
                    if (response.IsSuccessStatusCode)
                    {
                        var strResponse = await response.Content.ReadAsStringAsync();
                        // get token
                        accessToken = CreateAuthenticationTokenFromResponse(strResponse);
                    }
                    else
                    {
                        throw new Exception(string.Format("Unable to retrieve authentication token"));
                    }
                }
                catch (WebException)
                {
                    // log error = "Request Issue: " + wex.Message.ToString();
                }
                catch (Exception)
                {
                    // log error = "Issue: " + ex.Message.ToString();
                }

                if (accessToken == null)
                {
                    // Something went wrong during refresh --> Get a new access token by showing logon mask
                    accessToken = await AuthenticateAsync();
                }

                return accessToken;
            }
        }

        private AuthenticationToken CreateAuthenticationTokenFromResponse(string strResponse)
        {
            dynamic result = JObject.Parse(strResponse);

            double expiresIn = result.expires_in;
            string access = result.access_token;
            string refresh = result.refresh_token;

            return new AuthenticationToken(access, DateTime.Now.AddSeconds(expiresIn), refresh);
        }

        public void Logout()
        {
            // Remove token from vault
            PasswordVault vault = new PasswordVault();
            try
            {
                PasswordCredential credit = vault.Retrieve(CREDENTIALS_RESOURCE, CREDENTIALS_USER);
                vault.Remove(credit);
            }
            catch
            {
                // No Credential in password vault --> no problem
            }

            // Clean Application data and start it again
            AuthToken = null;
            ((App) Application.Current).DataSource.ResetDataSource();
        }
    }
}