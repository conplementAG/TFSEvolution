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

namespace TFSDataAccessPortable
{
    public class PersonData
    {
        private PersonContract _contract;

        public Guid ID
        {
            get { return _contract.ID; }
        }

        public string UniqueName
        {
            get { return _contract.UniqueName; }
        }

        public string DisplayName
        {
            get { return _contract.DisplayName; }
        }

        public Uri ImageUrl
        {
            get { return _contract.ImageUrl; }
        }

        public byte[] ImageBytes { get; internal set; }

        public string ImagePath { get; set; }

        internal PersonData(PersonContract contract)
        {
            _contract = contract;
        }

        public PersonData(string name)
        {
            _contract = new PersonContract {DisplayName = name, UniqueName = name};
        }
    }
}