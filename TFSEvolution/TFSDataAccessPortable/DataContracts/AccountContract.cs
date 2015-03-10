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
    internal class AccountContract
    {
        // "accountId": "c9593673-efb8-4e01-a52e-0ccacb2d4471",
        [DataMember(Name = "accountId")]
        public Guid AccountId { get; set; }

        //"accountUri":"https://ddf.vssps.visualstudio.com/"
        [DataMember(Name = "accountUri")]
        public Uri AccountUri { get; set; }

        // "accountName": "Fabrikam-Fiber",
        [DataMember(Name = "accountName")]
        public string AccountName { get; set; }

        // "organizationName": "Fabrikam-Fiber",
        [DataMember(Name = "organizationName")]
        public string OrganizationName { get; set; }

        // "accountType":"personal"
        [DataMember(Name = "accountType")]
        public string AccountType { get; set; }

        // "accountOwner": "0092e3da-9254-46dc-ba5a-6346b107780d",
        [DataMember(Name = "accountOwner")]
        public Guid AccountOwner { get; set; }

        // "createdBy": "0092e3da-9254-46dc-ba5a-6346b107780d",
        [DataMember(Name = "createdBy")]
        public Guid CreatedBy { get; set; }

        // "createdDate": "2013-05-09T14:49:23.93Z",
        [DataMember(Name = "createdDate")]
        public string CreatedDate { get; set; }

        // "accountStatus":"enabled"
        [DataMember(Name = "accountStatus")]
        public string AccountStatus { get; set; }

        // "lastUpdatedBy":"3c1e3f94-47d6-4d1a-89a3-9d48a916812d"
        [DataMember(Name = "lastUpdatedBy")]
        public Guid LastUpdatedBy { get; set; }

        // "lastUpdatedDate":"2013-01-05T16:16:44.707Z"
        [DataMember(Name = "lastUpdatedDate")]
        public string LastUpdatedDate { get; set; }
    }
}