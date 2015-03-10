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
using System.Threading.Tasks;
using Windows.UI.Xaml;
using TFSDataAccessPortable;
using TFSExpert.Common;
using TFSExpert.Controls;
using TFSExpert.Misc;

namespace TFSExpert.ViewModels
{
    public abstract class WorkItem : BindableBase, IResizable
    {
        protected WorkItemData _objModel;
        private ObservableCollection<WorkItem> _objChilds;
        private ObservableCollection<WorkItem> _objRelatedWIs;
        private ObservableCollection<WorkItem> _cobjChildsTodo = new ObservableCollection<WorkItem>();
        private ObservableCollection<WorkItem> _cobjChildsInWork = new ObservableCollection<WorkItem>();
        private ObservableCollection<WorkItem> _cobjChildsDone = new ObservableCollection<WorkItem>();


        //Event for changing state from "done" to anything else
        public delegate void ChangingStateFromDone(TaskState newState, WorkItem changingItem);

        public event ChangingStateFromDone DoneStateChanged;

        public delegate void ChangingRemainingWork(WorkItem firedItem);

        public event ChangingRemainingWork RemainingWorkChanged;

        public delegate void ChangingWorkItemState(WorkItem sender, WorkItemState newState);

        public event ChangingWorkItemState WorkItemStateChanging;

        #region Methods for the interface IResizable

        public int Columns
        {
            get
            {
                const int basicValue = 6;

                return _objModel.RemainingWork <= 10 ? Convert.ToInt16((basicValue + _objModel.RemainingWork)) : 16;
            }
            set { }
        }

        public int Rows
        {
            get
            {
                const int basicValue = 6;
                return _objModel.RemainingWork <= 10 ? Convert.ToInt16((basicValue + _objModel.RemainingWork)) : 16;
            }
            set { }
        }

        #endregion

        public string AreaPath
        {
            get { return _objModel.AreaPath; }
        }

        public string IterationPath
        {
            get { return _objModel.IterationPath; }
        }

        public WorkItemState WorkItemState
        {
            get { return _objModel.WorkItemState; }
        }

        public WorkItemType WorkItemType
        {
            get { return _objModel.WorkItemType; }
        }

        public string AssignedTo
        {
            get { return _objModel.AssignedToName; }
        }

        public string Title
        {
            get { return _objModel.Title; }
        }

        public string Description
        {
            get
            {
                if (_objModel.Description != null)
                {
                    return _objModel.Description.RemoveHTMLFormatting();
                }
                return "";
            }
        }

        public int Revision
        {
            get { return _objModel.Revision; }
        }

        public int ID
        {
            get { return _objModel.ID; }
        }

        public double? WorkRemaining
        {
            get
            {
                if (_objModel.RemainingWork != null)
                {
                    return _objModel.RemainingWork;
                }
                if (_objModel.Effort != null)
                {
                    return _objModel.Effort;
                }

                return 0.0;
            }
        }


        public ObservableCollection<WorkItem> ChildWorkItems
        {
            get
            {
                if (_objChilds == null)
                {
                    _objChilds = _objModel.ChildWorkItems.ConvertToWorkitemCollection();
                }
                return _objChilds;
            }
        }

        public ObservableCollection<WorkItem> ChildsTodo
        {
            get
            {
                RefreshChildsTodo();
                return _cobjChildsTodo;
            }
        }

        public int ChildsTodoCount
        {
            get
            {
                RefreshChildsTodo();
                return _cobjChildsTodo.Count;
            }
        }


        public ObservableCollection<WorkItem> ChildsInWork
        {
            get
            {
                RefreshChildsInWork();
                return _cobjChildsInWork;
            }
        }

        public int ChildsInWorkCount
        {
            get
            {
                RefreshChildsInWork();
                return _cobjChildsInWork.Count;
            }
        }

        public ObservableCollection<WorkItem> ChildsDone
        {
            get
            {
                RefreshChildsDone();
                return _cobjChildsDone;
            }
        }

        public int ChildsDoneCount
        {
            get
            {
                RefreshChildsDone();
                return _cobjChildsDone.Count();
            }
        }


        public ObservableCollection<WorkItem> RelatedWorkItems
        {
            get
            {
                if (_objRelatedWIs == null)
                {
                    _objRelatedWIs = _objModel.RelatedWorkItems.ConvertToWorkitemCollection();
                }

                return _objRelatedWIs;
            }
        }

        public WorkItem ParentWorkItem
        {
            get
            {
                if (_objModel.ParentWorkItem != null)
                {
                    return
                        ((App) Application.Current).DataSource.WorkItemCollection.FirstOrDefault(
                            n => n.ID == _objModel.ParentWorkItem.ID);
                }
                return null;
            }
        }

        public async Task<bool> SetAssignedToAsync(string memberFullName)
        {
            WorkItem updatedWI =
                await ((App) Application.Current).DataSource.SetAssignedToOfWI(_objModel, memberFullName);
            if (updatedWI != null)
            {
                // Update data of WI
                Update(updatedWI);
                return true;
            }
            return false;
        }

        public async Task<bool> SetRemainingWorkAsync(double remainingWork)
        {
            WorkItem updatedWI =
                await ((App) Application.Current).DataSource.SetRemainingWorkOfWI(_objModel, remainingWork);
            if (updatedWI != null)
            {
                // Update data of WI
                Update(updatedWI);
                //Send Update-Event to View
                RemainingWorkChanged(this);
                return true;
            }
            return false;
        }

        public void ChangeState(WorkItem source, WorkItemState newState)
        {
            WorkItemStateChanging(source, newState);
        }

        public async Task<bool> SetTaskStateAsync(TaskState newState, double? remainingWork = null)
        {
            //init edit in event for view
            WorkItem updatedWI =
                await ((App) Application.Current).DataSource.SetStateOfTask(_objModel, newState, remainingWork);
            if (updatedWI != null)
            {
                // Update data of WI
                Update(updatedWI);
                // Update child collections (ToDo, InProgress, Done) of parent WI
                UpdateParentWI();
                return true;
            }
            return false;
            //finish edit in event for view
        }

        private void Update(WorkItem updatedWI)
        {
            _objModel.UpdateData(updatedWI._objModel);

            OnPropertyChanged("WorkItemState");
            OnPropertyChanged("AssignedTo");
            OnPropertyChanged("WorkRemaining");
        }

        public void UpdateParentWI()
        {
            ParentWorkItem.OnPropertyChanged("ChildsInWork");
            ParentWorkItem.OnPropertyChanged("ChildsTodo");
            ParentWorkItem.OnPropertyChanged("ChildsDone");
        }

        private void RefreshChildsTodo()
        {
            var items =
                ChildWorkItems.Where(
                    n =>
                        n.WorkItemState == WorkItemState.New || n.WorkItemState == WorkItemState.Open ||
                        n.WorkItemState == WorkItemState.ToDo);
            _cobjChildsTodo.RefreshCollection(items);
        }

        private void RefreshChildsInWork()
        {
            var items = ChildWorkItems.Where(n => n.WorkItemState == WorkItemState.InProgress);
            _cobjChildsInWork.RefreshCollection(items);
        }

        private void RefreshChildsDone()
        {
            var items = ChildWorkItems.Where(n => n.WorkItemState == WorkItemState.Done);
            _cobjChildsDone.RefreshCollection(items);
        }

        internal void UpdateProperties(IEnumerable<WorkitemChangeData.FieldChange> changes)
        {
            _objModel.UpdateData(changes);
            OnPropertyChanged("AreaPath");
            OnPropertyChanged("IterationPath");
            OnPropertyChanged("WorkItemState");
            OnPropertyChanged("AssignedTo");
            OnPropertyChanged("Title");
            OnPropertyChanged("Description");
            OnPropertyChanged("WorkRemaining");

            // to get state changes synced
            UpdateParentWI();
        }

        public void ChangeStateFromDone(TaskState newState, WorkItem changedItem)
        {
            DoneStateChanged(newState, changedItem);
        }

        internal void RemoveEventHandlers()
        {
            if (DoneStateChanged != null)
            {
                foreach (Delegate d in DoneStateChanged.GetInvocationList())
                {
                    DoneStateChanged -= (ChangingStateFromDone) d;
                }
            }

            if (RemainingWorkChanged != null)
            {
                foreach (Delegate d in RemainingWorkChanged.GetInvocationList())
                {
                    RemainingWorkChanged -= (ChangingRemainingWork) d;
                }
            }

            if (WorkItemStateChanging != null)
            {
                foreach (Delegate d in WorkItemStateChanging.GetInvocationList())
                {
                    WorkItemStateChanging -= (ChangingWorkItemState) d;
                }
            }
        }
    }
}