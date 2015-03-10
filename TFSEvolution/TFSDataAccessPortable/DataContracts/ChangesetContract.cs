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
using System.Runtime.Serialization;

namespace TFSDataAccessPortable
{
    [DataContract]
    internal class ChangesetContract
    {
        [DataContract]
        internal class WorkItemContract
        {
            //  "webUrl": "https://fabrikam-fiber-inc.visualstudio.com/web/wi.aspx?pcguid=d81542e4-cdfa-4333-b082-1ae2d6c3ad16&id=22",
            [DataMember(Name = "webUrl")]
            public Uri WebUrl { get; set; }

            //  "id": 22,
            [DataMember(Name = "id")]
            public int ID { get; set; }

            //  "title": "Wrong output",
            [DataMember(Name = "title")]
            public string Title { get; set; }

            // "workItemType": "Bug",
            [DataMember(Name = "workItemType")]
            public string WorkItemType { get; set; }

            //  "state": "Done"
            [DataMember(Name = "state")]
            public string State { get; set; }
        }

        // "changesetId": 16
        [DataMember(Name = "changesetId")]
        public int ChangesetID { get; set; }

        // "author": {
        [DataMember(Name = "author")]
        public PersonContract Author { get; set; }

        // "checkedInBy": {
        [DataMember(Name = "checkedInBy")]
        public PersonContract CheckedInBy { get; set; }

        // "createdDate": "2014-03-24T20:21:02.727Z"
        [DataMember(Name = "createdDate")]
        public string CreatedDate { get; set; }

        // "comment": "Checking in new Team Foundation Build Automation files."
        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        // "workItems": [
        [DataMember(Name = "workItems")]
        public List<WorkItemContract> LinkedWorkItems { get; set; }
    }
}