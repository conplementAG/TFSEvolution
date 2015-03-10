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
using TFSDataAccessPortable;
using TFSExpert.ViewModels;

namespace TFSExpert
{
    public sealed partial class TaskboardSingleRowControl : UserControl
    {
        private WorkItem _draggingItem;

        public TaskboardSingleRowControl()
        {
            InitializeComponent();
            DataContextChanged += TaskboardSingleRowControl_DataContextChanged;
        }

        private void TaskboardSingleRowControl_DataContextChanged(FrameworkElement sender,
            DataContextChangedEventArgs args)
        {
            //Adding eventhandling to all taskitems for listening to remainingwork changes
            WorkItem thisData = (WorkItem) DataContext;
            if (thisData != null)
            {
                foreach (WorkItem item in thisData.ChildWorkItems)
                {
                    item.RemainingWorkChanged += Item_RemainingWorkChanged;
                }
            }
        }

        private void Item_RemainingWorkChanged(WorkItem firedItem)
        {
            //Repaint the Container 
            firedItem.UpdateParentWI();
        }

        private void ToDoContainer_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (_draggingItem.WorkItemState != WorkItemState.ToDo)
                {
                    _draggingItem.ChangeState(_draggingItem, WorkItemState.ToDo);
                }
            }
            catch (Exception)
            {
            }
        }

        private void ProgressContainer_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (_draggingItem.WorkItemState != WorkItemState.InProgress)
                {
                    _draggingItem.ChangeState(_draggingItem, WorkItemState.InProgress);
                }
            }
            catch (Exception)
            {
            }
        }

        private void DoneContainer_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (_draggingItem.WorkItemState != WorkItemState.Done)
                {
                    _draggingItem.ChangeState(_draggingItem, WorkItemState.Done);
                }
            }
            catch (Exception)
            {
            }
        }

        private void ProgressContainer_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            _draggingItem = e.Items[0] as WorkItem;
        }

        private void ToDoContainer_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            _draggingItem = e.Items[0] as WorkItem;
        }
    }
}