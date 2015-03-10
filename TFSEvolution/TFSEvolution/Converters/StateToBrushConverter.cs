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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using TFSDataAccessPortable;

namespace TFSExpert.Converters
{
    public class StateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush returner;
            WorkItemType thisWorkItemType = (WorkItemType) value;
            switch (thisWorkItemType)
            {
                case WorkItemType.ProductBacklogItem:
                    returner = Application.Current.Resources["PBIBrush"] as SolidColorBrush;
                    break;

                case WorkItemType.Bug:
                    returner = Application.Current.Resources["BugBrush"] as SolidColorBrush;
                    break;

                case WorkItemType.Task:
                    returner = Application.Current.Resources["TaskBrush"] as SolidColorBrush;
                    break;

                case WorkItemType.Impediment:
                    returner = Application.Current.Resources["ImpedimentBrush"] as SolidColorBrush;
                    break;

                case WorkItemType.TestCase:
                    returner = Application.Current.Resources["TestcaseBrush"] as SolidColorBrush;
                    break;

                case WorkItemType.Feature:
                    returner = Application.Current.Resources["FeatureBrush"] as SolidColorBrush;
                    break;

                default:
                    returner = Application.Current.Resources["DefaultBrush"] as SolidColorBrush;
                    break;
            }

            return returner;
        }


        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}