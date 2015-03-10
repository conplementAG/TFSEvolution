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
    internal class BuildContract
    {
        [DataContract]
        internal class BuildDefinitionContract
        {
            //  "definitionType": "xaml",
            [DataMember(Name = "definitionType")]
            public string DefinitionType { get; set; }

            //  "id": 2,
            [DataMember(Name = "id")]
            public int ID { get; set; }

            //  "name": "MyWebSite CI",
            [DataMember(Name = "name")]
            public string Name { get; set; }

            //  "url": "https://fabrikam-fiber-inc.visualstudio.com/DefaultCollection/_apis/build/Definitions/2"
            [DataMember(Name = "url")]
            public Uri Url { get; set; }
        }

        [DataContract]
        internal class BuildQueueContract
        {
            //"queueType": "buildController",        
            [DataMember(Name = "queueType")]
            public string QueueType { get; set; }

            //  "id": 274,
            [DataMember(Name = "id")]
            public int ID { get; set; }

            //  "name": "Hosted Build Controller",
            [DataMember(Name = "name")]
            public string Name { get; set; }

            //  "url": "https://fabrikam-fiber-inc.visualstudio.com/DefaultCollection/_apis/build/Queues/274"
            [DataMember(Name = "url")]
            public Uri Url { get; set; }
        }


        [DataContract]
        internal class LastChangedByContract
        {
            //  "id": "e43c66ab-e1f6-4686-980e-adca43527f9a",
            [DataMember(Name = "id")]
            public Guid ID { get; set; }

            //  "displayName": "Elastic Build (Fabrikam-Fiber-Inc)",
            [DataMember(Name = "displayName")]
            public string DisplayName { get; set; }

            //  "uniqueName": "LOCAL AUTHORITY\\Elastic Build (Fabrikam-Fiber-Inc)"
            [DataMember(Name = "uniqueName")]
            public string UniqueName { get; set; }
        }

        [DataContract]
        internal class RequestContract
        {
            //  "id": 11,
            [DataMember(Name = "id")]
            public int ID { get; set; }

            //  "url": "https://fabrikam-fiber-inc.visualstudio.com/DefaultCollection/_apis/build/Requests/11",
            [DataMember(Name = "url")]
            public Uri Url { get; set; }

            //  "requestedFor"
            [DataMember(Name = "requestedFor")]
            public PersonContract RequestedFor { get; set; }
        }

        // "uri": "vstfs:///Build/Build/11"
        [DataMember(Name = "uri")]
        public Uri Uri { get; set; }

        // "id": 11
        [DataMember(Name = "id")]
        public int ID { get; set; }

        //"buildNumber": "11"
        [DataMember(Name = "buildNumber")]
        public string BuildNumber { get; set; }

        //"url": "https://fabrikam-fiber-inc.visualstudio.com/DefaultCollection/_apis/build/Builds/11",
        [DataMember(Name = "url")]
        public Uri Url { get; set; }

        //"startTime": "2014-03-13T21:12:47.11Z",
        [DataMember(Name = "startTime")]
        public string StartTime { get; set; }

        //"finishTime": "2014-03-13T21:13:25.387Z",
        [DataMember(Name = "finishTime")]
        public string FinishTime { get; set; }

        //"reason": "manual",
        [DataMember(Name = "reason")]
        public string Reason { get; set; }

        //"status": "notStarted",
        [DataMember(Name = "status")]
        public string Status { get; set; }

        //"dropLocation": "#/2124/drop",
        [DataMember(Name = "dropLocation")]
        public string DropLocation { get; set; }

        //"sourceGetVersion": "T",
        [DataMember(Name = "sourceGetVersion")]
        public string SourceGetVersion { get; set; }

        //"lastChangedBy": {
        [DataMember(Name = "lastChangedBy")]
        public LastChangedByContract LastChangedBy { get; set; }

        //"retainIndefinitely": false,
        [DataMember(Name = "retainIndefinitely")]
        public bool RetainIndefinitely { get; set; }

        //"hasDiagnostics": true,
        [DataMember(Name = "hasDiagnostics")]
        public bool HasDiagnostics { get; set; }

        //"definition": {
        [DataMember(Name = "definition")]
        public BuildDefinitionContract Definition { get; set; }

        //"queue": {
        [DataMember(Name = "queue")]
        public BuildQueueContract Queue { get; set; }

        //"requests": [
        [DataMember(Name = "requests")]
        public List<RequestContract> Requests { get; set; }
    }
}