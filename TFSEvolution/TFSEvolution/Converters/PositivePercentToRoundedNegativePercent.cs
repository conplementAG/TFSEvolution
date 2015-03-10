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
    public sealed class PositivePercentToRoundedNegativePercent : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double returner = 0;
            if (value != "")
            {
                string valueString = value.ToString().Substring(0, value.ToString().Length - 2);
                double valuedouble = System.Convert.ToDouble(valueString);
                returner = (int) (100 - valuedouble);
            }
            return returner + " %";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return 0.0;
        }
    }
}