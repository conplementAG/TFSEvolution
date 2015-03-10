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
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TFSExpert.Converters
{
    public sealed class PositivePercentToBackShape : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ImageSource returner = null;
            if (value != "")
            {
                string valueString = value.ToString().Substring(0, value.ToString().Length - 2);
                double valuedouble = System.Convert.ToDouble(valueString);
                int percentValue = (int) (valuedouble);
                if (percentValue > 0)
                {
                    returner = Application.Current.Resources["GoodShape"] as ImageSource;
                }
                else
                {
                    returner = Application.Current.Resources["BadShape"] as ImageSource;
                }
            }
            return returner;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public sealed class PercentToBackShape : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ImageSource returner = null;
            if (value != "")
            {
                string valueString = value.ToString().Substring(0, value.ToString().Length - 2);
                double valuedouble = System.Convert.ToDouble(valueString);
                int percentValue = (int) (100 - valuedouble);
                if (percentValue > 0)
                {
                    returner = Application.Current.Resources["GoodShape"] as ImageSource;
                }
                else
                {
                    returner = Application.Current.Resources["BadShape"] as ImageSource;
                }
            }
            return returner;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public sealed class PercentToStrokeColors : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush returner = new SolidColorBrush(Colors.Transparent);
            if (value != "")
            {
                string valueString = value.ToString().Substring(0, value.ToString().Length - 2);
                double valuedouble = System.Convert.ToDouble(valueString);
                int percentValue = (int) (100 - valuedouble);
                if (percentValue > 0)
                {
                    returner = Application.Current.Resources["GreenBrush"] as SolidColorBrush;
                }
                else
                {
                    returner = Application.Current.Resources["BugBrush"] as SolidColorBrush;
                }
            }
            return returner;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return new SolidColorBrush(Colors.Transparent);
        }
    }


    public sealed class PositivePercentToStrokeColors : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush returner = new SolidColorBrush(Colors.Transparent);
            if (value != "")
            {
                string valueString = value.ToString().Substring(0, value.ToString().Length - 2);
                double valuedouble = System.Convert.ToDouble(valueString);
                int percentValue = (int) (valuedouble);
                if (percentValue > 0)
                {
                    returner = Application.Current.Resources["GreenBrush"] as SolidColorBrush;
                }
                else
                {
                    returner = Application.Current.Resources["BugBrush"] as SolidColorBrush;
                }
            }
            return returner;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return new SolidColorBrush(Colors.Transparent);
        }
    }
}