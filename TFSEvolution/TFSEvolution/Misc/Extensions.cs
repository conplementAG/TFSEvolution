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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TFSDataAccessPortable;
using TFSExpert.ViewModels;

namespace TFSExpert.Misc
{
    internal static class Extensions
    {
        internal static string GetReadableTimespan(this TimeSpan ts)
        {
            // formats and its cutoffs based on totalseconds
            var cutoff = new SortedDictionary<long, string>
            {
                {60, "{3:S}"},
                {60*60, "{2:M} and {3:S}"},
                {24*60*60, "{1:H} and {2:M}"},
                {Int64.MaxValue, "{0:D} and {1:H}"}
            };

            // find nearest best match
            var find = cutoff.Keys.ToList()
                .BinarySearch((long) ts.TotalSeconds);

            // negative values indicate a nearest match
            var near = find < 0 ? Math.Abs(find) - 1 : find;

            // use custom formatter to get the string
            return String.Format(
                new HMSFormatter(),
                cutoff[cutoff.Keys.ElementAt(near)],
                ts.Days,
                ts.Hours,
                ts.Minutes,
                ts.Seconds);
        }

        // formatter for plural/singular forms of
        // seconds/hours/days
        internal class HMSFormatter : ICustomFormatter, IFormatProvider
        {
            private readonly string _plural;
            private readonly string _singular;

            public HMSFormatter()
            {
            }

            private HMSFormatter(string plural, string singular)
            {
                _plural = plural;
                _singular = singular;
            }

            public object GetFormat(Type formatType)
            {
                return formatType == typeof (ICustomFormatter) ? this : null;
            }

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (arg == null)
                {
                    return format;
                }

                switch (format)
                {
                    case "S": // second
                        return String.Format(new HMSFormatter("{0} seconds", "{0} second"), "{0}", arg);
                    case "M": // minute
                        return String.Format(new HMSFormatter("{0} minutes", "{0} minute"), "{0}", arg);
                    case "H": // hour
                        return String.Format(new HMSFormatter("{0} hours", "{0} hour"), "{0}", arg);
                    case "D": // day
                        return String.Format(new HMSFormatter("{0} days", "{0} day"), "{0}", arg);
                    default:
                        // plural/ singular             
                        return String.Format((int) arg > 1 ? _plural : _singular, arg);
                }
            }
        }

        internal static void SwitchToNextItem(this FlipView flipView)
        {
            if (flipView != null)
            {
                int elementCount = flipView.Items.Count;
                int currentIndex = flipView.SelectedIndex;

                // if it is the last item --> navigate to first
                if (currentIndex == elementCount - 1)
                {
                    flipView.SelectedIndex = 0;
                    return;
                }

                // navigate to next item
                flipView.SelectedIndex++;
            }
        }

        internal static void SwitchToItem(this FlipView flipView, RadioButton radioBtn)
        {
            if (radioBtn != null)
            {
                try
                {
                    int index = Convert.ToInt32(radioBtn.Tag);

                    flipView.SelectedIndex = index;
                }
                catch
                {
                }
            }
        }

        internal static void SwitchToItem(this FlipView flipView, int index)
        {
            try
            {
                flipView.SelectedIndex = index;
            }
            catch
            {
            }
        }

        internal static WorkItem ConvertToWorkitem(this WorkItemData workitemData)
        {
            switch (workitemData.WorkItemType)
            {
                case WorkItemType.ProductBacklogItem:
                    return new ProductBacklogItem(workitemData);
                case WorkItemType.Task:
                    return new TaskItem(workitemData);
                case WorkItemType.Bug:
                    return new BugItem(workitemData);
                case WorkItemType.Feature:
                    return new FeatureItem(workitemData);
                case WorkItemType.Impediment:
                    return new ImpedimentItem(workitemData);
                case WorkItemType.TestCase:
                    return new TestCaseItem(workitemData);
                default:
                    return null;
            }
        }

        internal static ObservableCollection<WorkItem> ConvertToWorkitemCollection(this List<WorkItemData> workitems)
        {
            var result = new ObservableCollection<WorkItem>();

            foreach (var data in workitems)
            {
                WorkItem item =
                    ((App) Application.Current).DataSource.WorkItemCollection.FirstOrDefault(n => n.ID == data.ID);

                if (item != null)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        internal static void RefreshCollection(this ObservableCollection<WorkItem> oldWorkitems,
            IEnumerable<WorkItem> newWorkItems)
        {
            oldWorkitems.Clear();

            foreach (var item in newWorkItems)
            {
                oldWorkitems.Add(item);
            }
        }

        internal static string RemoveHTMLFormatting(this string stringWithHtml)
        {
            return Regex.Replace(stringWithHtml, "<.*?>", string.Empty);
        }
    }
}