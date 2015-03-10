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
    internal class ProfileContract
    {
        // "id": "f9a757fc-0f11-4e1b-8b5c-6feb2b50efc0"
        [DataMember(Name = "id")]
        public Guid ID { get; set; }

        // "displayName": "John Smith",
        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        // "publicAlias": "johnsmith0808",
        [DataMember(Name = "publicAlias")]
        public string PublicAlias { get; set; }

        // "uniqueName": "\\northamerica\johnsmith",
        [DataMember(Name = "uniqueName")]
        public string UniqueName { get; set; }

        // "emailAddress": "jsmith@hotmail.com",
        [DataMember(Name = "emailAddress")]
        public string EmailAddress { get; set; }

        // "avatar": {
        //     "value": "/9j/DGQV1Idz/9k="
        //     },
        [DataMember(Name = "avatar")]
        public DefaultValueContract<string> Avatar { get; set; }

        // "url": "http://app.vssps.visualstudio.com/_apis/profile/profiles/f9a757fc-0f11-4e1b-8b5c-6feb2b50efc0",
        [DataMember(Name = "url")]
        public Uri Url { get; set; }

        // "updatedDate": "2013-08-09T13:32:08.553+00:00",
        [DataMember(Name = "updatedDate")]
        public string UpdatedDate { get; set; }

        // "coreRevision": 102,
        [DataMember(Name = "coreRevision")]
        public string CoreRevision { get; set; }

        // "revision": 102
        [DataMember(Name = "revision")]
        public string Revision { get; set; }
    }
}