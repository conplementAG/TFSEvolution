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
    public enum BuildStatus
    {
        None,
        Failed,
        PartiallySucceeded,
        Stopped,
        Succeeded
    }

    public class BuildEventData
    {
        private BuildContract _contract;

        internal BuildEventData(BuildContract contract)
        {
            _contract = contract;
        }

        public string BuildNumber
        {
            get { return _contract.BuildNumber; }
        }

        public DateTime FinishTime
        {
            get { return Convert.ToDateTime(_contract.FinishTime); }
        }

        public DateTime StartTime
        {
            get { return Convert.ToDateTime(_contract.StartTime); }
        }

        public PersonData Creator
        {
            get { return new PersonData(_contract.Requests[0].RequestedFor); }
        }

        public BuildStatus Status
        {
            get
            {
                BuildStatus status;

                if (Enum.TryParse(_contract.Status, true, out status))
                {
                    // parsing succeeded
                    return status;
                }
                return BuildStatus.None;
            }
        }
    }
}