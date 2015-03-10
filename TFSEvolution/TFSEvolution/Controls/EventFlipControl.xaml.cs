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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TFSExpert.Misc;

namespace TFSExpert
{
    public sealed partial class EventFlipControl : UserControl
    {
        // Event-Slideshow members
        private DispatcherTimer _mySlideshowTimer = new DispatcherTimer();

        public EventFlipControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            EventView.SelectionChanged += EventView_SelectionChanged;
            Button1.IsChecked = true;
        }

        #region slideshow for events

        /// <summary>
        ///     Eventhandler for the Loaded-Event of the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Start Slideshow of Events
            try
            {
                //init the Dispatcher-Timer 
                _mySlideshowTimer = new DispatcherTimer();
                _mySlideshowTimer.Interval = new TimeSpan(0, 0, 10);
                _mySlideshowTimer.Tick += SlideshowTimer_Tick;
                _mySlideshowTimer.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SlideshowTimer_Tick(object sender, object e)
        {
            //Move the Slider based on the actual position
            EventView.SwitchToNextItem();
            _mySlideshowTimer.Start();
        }

        #endregion

        private void OnFlipViewChanged(object sender, RoutedEventArgs e)
        {
            var radioBtn = sender as RadioButton;

            if (radioBtn != null)
            {
                EventView.SwitchToItem(radioBtn);
                _mySlideshowTimer.Start();
            }
        }

        private void EventView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (EventView.SelectedIndex)
            {
                case 0:
                    Button1.IsChecked = true;
                    break;
                case 1:
                    Button2.IsChecked = true;
                    break;
                case 2:
                    Button3.IsChecked = true;
                    break;
            }
        }
    }
}