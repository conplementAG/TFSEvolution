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

using TFSDataAccessPortable;

namespace TFSExpert.ViewModels
{
    public class CheckinEventItem
    {
        #region members

        // the underlying DataModel
        private CheckinEventData _objEventModel;

        #endregion

        #region properties

        public string CheckedInBy
        {
            get { return _objEventModel.CheckedInBy.DisplayName; }
        }

        public string CheckinDate
        {
            get { return _objEventModel.CreationDate.ToString(); }
        }

        public string ChangeSetID
        {
            get { return _objEventModel.ChangeSetID.ToString(); }
        }

        public string LinkedWorkItemsString
        {
            get
            {
                if (_objEventModel.LinkedWorkItemIDs != null && _objEventModel.LinkedWorkItemIDs.Count > 0)
                {
                    if (_objEventModel.LinkedWorkItemIDs.Count > 1)
                    {
                        return string.Format("linked with work items {0}",
                            string.Join(", ", _objEventModel.LinkedWorkItemIDs));
                    }

                    return string.Format("linked with work item {0}", _objEventModel.LinkedWorkItemIDs[0]);
                }

                return string.Empty;
            }
        }

        public string EventTitle
        {
            get
            {
                return string.Format("Changeset {0} {1} successfully checked in.", ChangeSetID, LinkedWorkItemsString);
            }
        }

        #endregion

        public CheckinEventItem(CheckinEventData model)
        {
            _objEventModel = model;
        }
    }
}