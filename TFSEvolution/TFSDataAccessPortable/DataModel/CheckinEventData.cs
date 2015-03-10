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

namespace TFSDataAccessPortable
{
    public class CheckinEventData
    {
        private ChangesetContract _contract;

        internal CheckinEventData(ChangesetContract contract)
        {
            _contract = contract;
        }

        public DateTime CreationDate
        {
            get { return Convert.ToDateTime(_contract.CreatedDate); }
        }

        public PersonData CheckedInBy
        {
            get { return new PersonData(_contract.CheckedInBy); }
        }

        public int ChangeSetID
        {
            get { return _contract.ChangesetID; }
        }

        public string Comment
        {
            get { return _contract.Comment; }
        }

        public List<int> LinkedWorkItemIDs
        {
            get
            {
                List<int> cobjResults = new List<int>();
                if (_contract.LinkedWorkItems != null)
                {
                    foreach (var workitem in _contract.LinkedWorkItems)
                    {
                        cobjResults.Add(workitem.ID);
                    }
                }
                return cobjResults;
            }
        }
    }
}