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

using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TFSExpert.ViewModels;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TFSExpert
{
    public sealed partial class BurndownControl : UserControl
    {
        internal enum States
        {
            NoSprint,
            NoData,
            Loading,
            Loaded,
            Error
        }

        public BurndownControl()
        {
            InitializeComponent();

            ((App) Application.Current).DataSource.SprintDataLoaded += DataSource_SprintDataLoaded;
            DataContextChanged += BurndownControl_DataContextChanged;
        }

        private void BurndownControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var burndownChart = FindName("BurnDownChart") as Chart;
            if (burndownChart != null)
            {
                burndownChart.DataContext = null;
                burndownChart.DataContext = args.NewValue;
                if (args.NewValue == null)
                {
                    burndownChart.UpdateLayout();
                }
            }
        }

        private void DataSource_SprintDataLoaded(bool success, Sprint sprint)
        {
            if (success)
            {
                sprint.PropertyChanged += BurndownControl_PropertyChanged;
            }
        }

        private void BurndownControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BurndownData")
            {
                var burndownChart = FindName("BurnDownChart") as Chart;
                if (burndownChart != null)
                {
                    burndownChart.DataContext = null;
                    DataContext = ((App) Application.Current).DataSource.Sprint;
                    burndownChart.DataContext = DataContext;
                }
            }
        }
    }
}