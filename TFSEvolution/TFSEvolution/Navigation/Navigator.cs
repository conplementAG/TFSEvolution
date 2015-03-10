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
using Windows.UI.Xaml.Controls;
using TFSExpert.ViewModels;

namespace TFSExpert.Navigation
{
    public class Navigator : INavigationService
    {
        private Frame _rootFrame;

        public Navigator(Frame rootFrame)
        {
            _rootFrame = rootFrame;
        }

        public static ServerInfo CurrentServer { get; set; }
        public static TeamProject CurrentTeamProject { get; set; }

        public static void SetCurrentProject(string serveraddress, string projectcollection, TeamProject teamproject)
        {
            CurrentServer = new ServerInfo(serveraddress, projectcollection);
            CurrentTeamProject = teamproject;
        }

        public static void SetCurrentProject(string serveraddress, TeamProject teamproject)
        {
            CurrentServer = new ServerInfo(serveraddress);
            CurrentTeamProject = teamproject;
        }

        public void Navigate(Type page, object param)
        {
            _rootFrame.Navigate(page, param);
        }

        public void Navigate(Type page)
        {
            _rootFrame.Navigate(page);
        }
    }
}