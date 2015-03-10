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
    internal class PersonContract
    {
        //   "id": "d291b0c4-a05c-4ea6-8df1-4b41d5f39eff",
        [DataMember(Name = "id")]
        public Guid ID { get; set; }

        //  "uniqueName": "fabrikamfiber4@hotmail.com","
        [DataMember(Name = "uniqueName")]
        public string UniqueName { get; set; }

        // "displayName": "Jamal Hartnett",
        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        //  "url": "https://fabrikam-fiber-inc.vssps.visualstudio.com/_apis/Identities/d291b0c4-a05c-4ea6-8df1-4b41d5f39eff"
        [DataMember(Name = "url")]
        public Uri Url { get; set; }

        // "imageUrl": "https://fabrikam.visualstudio.com/DefaultCollection/_api/_common/identityImage?id=3b5f0c34-4aec-4bf4-8708-1d36f0dbc468"
        [DataMember(Name = "imageUrl")]
        public Uri ImageUrl { get; set; }
    }
}