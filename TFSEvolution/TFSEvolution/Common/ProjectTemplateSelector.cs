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
using TFSExpert.ViewModels;

namespace TFSExpert.Common
{
    public class ProjectTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Template { get; set; }

        private const string BACKGROUND_IMAGE1 = "ms-appx:///Images/ProjectBackGreen.png";
        private const string BACKGROUND_IMAGE2 = "ms-appx:///Images/ProjectBackDarkBlue.png";
        private const string BACKGROUND_IMAGE3 = "ms-appx:///Images/ProjectBackGrey.png";
        private const string BACKGROUND_IMAGE4 = "ms-appx:///Images/ProjectBackBlue.png";
        private const string BACKGROUND_IMAGE5 = "ms-appx:///Images/ProjectBackPurple.png";

        /// <summary>
        ///     The select template core.
        /// </summary>
        /// <param name="item">
        ///     The item.
        /// </param>
        /// <param name="container">
        ///     The container.
        /// </param>
        /// <returns>
        ///     The <see cref="DataTemplate" />.
        /// </returns>
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var project = item as TeamProject;
            if (project != null)
            {
                int index = ((App) Application.Current).DataSource.TeamProjects.IndexOf(project);

                switch (index%5)
                {
                    default:
                        project.BackgroundImage = BACKGROUND_IMAGE1;
                        project.IsBackgroundDark = false;
                        break;
                    case 1:
                        project.BackgroundImage = BACKGROUND_IMAGE2;
                        project.IsBackgroundDark = true;
                        break;
                    case 2:
                        project.BackgroundImage = BACKGROUND_IMAGE3;
                        project.IsBackgroundDark = true;
                        break;
                    case 3:
                        project.BackgroundImage = BACKGROUND_IMAGE4;
                        project.IsBackgroundDark = false;
                        break;
                    case 4:
                        project.BackgroundImage = BACKGROUND_IMAGE5;
                        project.IsBackgroundDark = true;
                        break;
                }
            }

            return Template;
        }
    }
}