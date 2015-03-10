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
using System.ComponentModel;

namespace TFSDataAccessPortable
{
    public enum WorkItemType
    {
        ProductBacklogItem,
        Task,
        Bug,
        Feature,
        Impediment,
        TestCase,
        None
    }

    public enum TaskState
    {
        [Description("In Progress")] InProgress,
        [Description("To Do")] ToDo,
        Done
    }

    public enum WorkItemState
    {
        New,
        Approved,
        Committed,
        [Description("In Progress")] InProgress,
        Open,
        Design,
        Done,
        [Description("To Do")] ToDo,
        Removed,
        None
    }

    public class WorkItemData
    {
        private dynamic _contract;

        public int Revision { get; private set; }

        public string AreaPath { get; private set; }

        public string IterationPath { get; private set; }

        public WorkItemType WorkItemType { get; private set; }

        public WorkItemState WorkItemState { get; private set; }

        public string AssignedTo { get; private set; }

        public string AssignedToName
        {
            get
            {
                if (AssignedTo != null && AssignedTo.Contains("<"))
                {
                    return AssignedTo.Split('<')[0].Trim();
                }

                return AssignedTo;
            }
        }

        public double? RemainingWork { get; private set; }

        public double? Effort { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public int ID { get; private set; }

        public WorkItemData ParentWorkItem { get; internal set; }

        public List<WorkItemData> ChildWorkItems { get; internal set; }

        public List<WorkItemData> RelatedWorkItems { get; internal set; }

        internal WorkItemData(dynamic contract)
        {
            _contract = contract;

            InitWorkItemFieldData();

            ChildWorkItems = new List<WorkItemData>();
            RelatedWorkItems = new List<WorkItemData>();
        }

        private void InitWorkItemFieldData()
        {
            ID = _contract.id;
            Revision = _contract.rev;

            foreach (dynamic field in _contract.fields)
            {
                string fieldName = field.Name;
                switch (fieldName)
                {
                    case "System.Title":
                        Title = field.Value.Value;
                        break;
                    case "System.AreaPath":
                        AreaPath = field.Value.Value;
                        break;
                    case "System.IterationPath":
                        IterationPath = field.Value.Value;
                        break;
                    case "System.WorkItemType":
                        WorkItemType type;
                        if (Enum.TryParse(field.Value.Value.ToString().Replace(" ", ""), true, out type))
                        {
                            // parsing succeeded
                            WorkItemType = type;
                        }
                        else
                        {
                            WorkItemType = WorkItemType.None;
                        }
                        break;
                    case "System.State":
                        WorkItemState status;

                        if (Enum.TryParse(field.Value.Value.ToString().Replace(" ", ""), true, out status))
                        {
                            // parsing succeeded
                            WorkItemState = status;
                        }
                        else
                        {
                            WorkItemState = WorkItemState.None;
                        }
                        break;
                    case "System.AssignedTo":
                        AssignedTo = field.Value.Value;
                        break;
                    case "Microsoft.VSTS.Scheduling.Effort":
                        Effort = field.Value.Value;
                        break;
                    case "Microsoft.VSTS.Scheduling.RemainingWork":
                        RemainingWork = field.Value.Value;
                        break;
                    case "System.Description":
                        Description = field.Value.Value;
                        break;
                }
            }
        }

        public void UpdateData(WorkItemData workItemData)
        {
            Revision = workItemData.Revision;
            RemainingWork = workItemData.RemainingWork;
            AssignedTo = workItemData.AssignedTo;
            WorkItemState = workItemData.WorkItemState;
        }

        public void UpdateData(IEnumerable<WorkitemChangeData.FieldChange> changes)
        {
            foreach (var prop in changes)
            {
                switch (prop.FieldName)
                {
                    case "System.Rev":
                        Revision = Convert.ToInt32(prop.NewValue);
                        break;
                    case "System.Title":
                        Title = prop.NewValue != null ? prop.NewValue.ToString() : string.Empty;
                        break;
                    case "System.AreaPath":
                        AreaPath = prop.NewValue != null ? prop.NewValue.ToString() : string.Empty;
                        break;
                    case "System.IterationPath":
                        IterationPath = prop.NewValue != null ? prop.NewValue.ToString() : string.Empty;
                        break;
                    case "System.State":
                        WorkItemState status;

                        if (Enum.TryParse(prop.NewValue.ToString().Replace(" ", ""), true, out status))
                        {
                            // parsing succeeded
                            WorkItemState = status;
                        }
                        else
                        {
                            WorkItemState = WorkItemState.None;
                        }
                        break;
                    case "System.AssignedTo":
                        AssignedTo = prop.NewValue != null ? prop.NewValue.ToString() : string.Empty;
                        break;
                    case "Microsoft.VSTS.Scheduling.Effort":
                        if (prop.NewValue != null)
                        {
                            Effort = Convert.ToDouble(prop.NewValue);
                        }
                        else
                        {
                            Effort = null;
                        }
                        break;
                    case "Microsoft.VSTS.Scheduling.RemainingWork":
                        if (prop.NewValue != null)
                        {
                            RemainingWork = Convert.ToDouble(prop.NewValue);
                        }
                        else
                        {
                            RemainingWork = null;
                        }
                        break;
                    case "System.Description":
                        Description = prop.NewValue != null ? prop.NewValue.ToString() : string.Empty;
                        break;
                }
            }
        }

        internal void SetAssignedTo(string assigneeFullName)
        {
            AssignedTo = assigneeFullName;
            Revision++;
        }

        internal void SetRemainingWork(double newRemainingWork)
        {
            RemainingWork = newRemainingWork;
            Revision++;
        }

        internal void SetState(WorkItemState workItemState)
        {
            WorkItemState = workItemState;
            Revision++;
        }

        internal void SetState(WorkItemState workItemState, double? newRemainingWork)
        {
            WorkItemState = workItemState;
            RemainingWork = newRemainingWork;
            Revision++;
        }
    }
}