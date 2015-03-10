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
using System.Runtime.Serialization;

namespace TFSDataAccessPortable
{
    [DataContract]
    internal class ProjectContract
    {
        [DataContract]
        internal class ProjectCollectionContract
        {
            // "id": "d81542e4-cdfa-4333-b082-1ae2d6c3ad16",
            [DataMember(Name = "id")]
            public Guid ID { get; set; }

            // "name": "DefaultCollection",
            [DataMember(Name = "name")]
            public string Name { get; set; }

            // "url": "https://fabrikam-fiber-inc.visualstudio.com/_apis/projectCollections/d81542e4-cdfa-4333-b082-1ae2d6c3ad16",
            [DataMember(Name = "url")]
            public Uri Url { get; set; }

            // "collectionUrl": "https://fabrikam-fiber-inc.visualstudio.com/DefaultCollection"
            [DataMember(Name = "collectionUrl")]
            public Uri CollectionUrl { get; set; }
        }

        [DataContract]
        internal class TeamContract
        {
            // "id": "66df9be7-3586-467b-9c5f-425b29afedfd",
            [DataMember(Name = "id")]
            public Guid ID { get; set; }

            // "name": "Fabrikam-Fiber-TFVC Team",
            [DataMember(Name = "name")]
            public string Name { get; set; }

            // "url": "https://fabrikam-fiber-inc.visualstudio.com/DefaultCollection/_apis/projects/eb6e4656-77fc-42a1-9181-4c6d8e9da5d1/teams/66df9be7-3586-467b-9c5f-425b29afedfd"
            [DataMember(Name = "url")]
            public Uri Url { get; set; }
        }

        [DataContract]
        internal class ProjectCapabilities
        {
            //  "versioncontrol": {
            [DataMember(Name = "versioncontrol")]
            public VersionControl VersionControl { get; set; }

            // "processTemplate": {
            [DataMember(Name = "processTemplate")]
            public ProcessTemplate ProcessTemplate { get; set; }
        }

        [DataContract]
        internal class VersionControl
        {
            //    "sourceControlType": "Tfvc"
            [DataMember(Name = "sourceControlType")]
            public string SourceControlType { get; set; }
        }

        [DataContract]
        internal class ProcessTemplate
        {
            //    "templateName": "Microsoft Visual Studio Scrum 2013"
            [DataMember(Name = "templateName")]
            public string Name { get; set; }
        }

        // "id": "eb6e4656-77fc-42a1-9181-4c6d8e9da5d1",
        [DataMember(Name = "id")]
        public Guid ID { get; set; }

        // "name": "Fabrikam-Fiber-TFVC",
        [DataMember(Name = "name")]
        public string Name { get; set; }

        // "url": "https://fabrikam-fiber-inc.visualstudio.com/DefaultCollection/_apis/projects/eb6e4656-77fc-42a1-9181-4c6d8e9da5d1",
        [DataMember(Name = "url")]
        public Uri Url { get; set; }

        // "description": "Team Foundation Version Control projects",
        [DataMember(Name = "description")]
        public string Description { get; set; }

        // "state": "WellFormed",
        [DataMember(Name = "state")]
        public string State { get; set; }

        //// "collection": {
        //[DataMember(Name = "collection")]
        //public ProjectCollectionContract Collection { get; set; }

        // "defaultTeam": {
        [DataMember(Name = "defaultTeam")]
        public TeamContract DefaultTeam { get; set; }

        //      "capabilities": {
        [DataMember(Name = "capabilities")]
        public ProjectCapabilities Capabilities { get; set; }
    }
}