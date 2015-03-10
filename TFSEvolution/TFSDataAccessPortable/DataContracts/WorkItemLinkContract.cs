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

using System.Runtime.Serialization;

namespace TFSDataAccessPortable
{
    internal enum WorkItemLinkType
    {
        HierarchyForward, // source = parent, target = child
        HierarchyReverse, // source = child, parent = target
        Relation, // any other relation
        None // item has no relation to nothing
    }

    [DataContract]
    internal class WorkItemLinkContract
    {
        // "sourceId": 20,
        [DataMember(Name = "sourceId")]
        public int SourceId { get; set; }

        // "linkType": "System.LinkTypes.Hierarchy-Forward",
        [DataMember(Name = "linkType")]
        public string LinkTypeString { get; set; }

        // "targetId": 10        
        [DataMember(Name = "targetId")]
        public int TargetId { get; set; }

        public WorkItemLinkType LinkType
        {
            get { return ParseTypeFromString(LinkTypeString); }
        }

        private WorkItemLinkType ParseTypeFromString(string linkType)
        {
            switch (linkType)
            {
                case "System.LinkTypes.Hierarchy-Forward":
                    return WorkItemLinkType.HierarchyForward;
                case "System.LinkTypes.Hierarchy-Reverse":
                    return WorkItemLinkType.HierarchyReverse;
                case null:
                    return WorkItemLinkType.None;
                default:
                    return WorkItemLinkType.Relation;
            }
        }
    }
}