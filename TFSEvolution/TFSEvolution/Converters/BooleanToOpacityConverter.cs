﻿#region License and Terms

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
using Windows.UI.Xaml.Data;

namespace TFSExpert.Converters
{
    /// <summary>
    ///     Value converter that translates false to a opacity givin in the parameter or default 0.8 and true to 1.
    /// </summary>
    public sealed class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && !(bool) value)
            {
                double opacity = 0.5;

                if (parameter != null)
                {
                    try
                    {
                        opacity = System.Convert.ToDouble(parameter);
                    }
                    catch
                    {
                    }
                }

                return opacity;
            }

            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (double) value != 1.0;
        }
    }
}