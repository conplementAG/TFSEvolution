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
using TFSDataAccessPortable;
using TFSExpert.ViewModels;

namespace TFSExpert.Common
{
    public class EventTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BuildSuccessTemplate { get; set; }
        public DataTemplate BuildFailedTemplate { get; set; }
        public DataTemplate BuildStoppedTemplate { get; set; }
        public DataTemplate BuildDefaultTemplate { get; set; }
        public DataTemplate BuildPartiallyTemplate { get; set; }
        public DataTemplate CheckinTemplate { get; set; }

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
            if (item is BuildEventItem)
            {
                var build = item as BuildEventItem;
                DataTemplate buildTemplateReturner;
                switch (build.BuildStatus)
                {
                    case BuildStatus.Succeeded:
                        buildTemplateReturner = BuildSuccessTemplate;
                        break;

                    case BuildStatus.Failed:
                        buildTemplateReturner = BuildFailedTemplate;
                        break;

                    case BuildStatus.PartiallySucceeded:
                        buildTemplateReturner = BuildPartiallyTemplate;
                        break;

                    case BuildStatus.Stopped:
                        buildTemplateReturner = BuildStoppedTemplate;
                        break;

                    default:
                        buildTemplateReturner = BuildDefaultTemplate;
                        break;
                }

                return buildTemplateReturner;
            }
            if (item is CheckinEventItem)
            {
                return CheckinTemplate;
            }

            return BuildFailedTemplate;
        }
    }
}