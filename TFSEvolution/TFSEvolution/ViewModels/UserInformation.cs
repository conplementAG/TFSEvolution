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

using TFSExpert.Common;

namespace TFSExpert.ViewModels
{
    public class UserInformation : BindableBase
    {
        private bool _bIsUserLoaded;
        private User _loggedInUser;

        public bool IsUserLoaded
        {
            get { return _bIsUserLoaded; }
            private set
            {
                _bIsUserLoaded = value;
                OnPropertyChanged("IsUserLoaded");
            }
        }

        public User LoggedInUser
        {
            get { return _loggedInUser; }
            set
            {
                _loggedInUser = value;
                OnPropertyChanged("LoggedInUser");
                if (value != null)
                {
                    IsUserLoaded = true;
                }
            }
        }

        public static UserInformation Empty
        {
            get { return new UserInformation(); }
        }

        internal void Logout()
        {
            IsUserLoaded = false;
            LoggedInUser = null;
        }
    }
}