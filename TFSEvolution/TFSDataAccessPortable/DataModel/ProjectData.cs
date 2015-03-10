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
    public enum VersionControl
    {
        None, // Not initialized default
        Git, // Git
        Tfvc // Team Foundation Server
    }

    public class ProjectData
    {
        private ProjectContract _contract;

        public Guid ID
        {
            get { return _contract.ID; }
        }

        public string Name
        {
            get { return _contract.Name; }
        }

        //public Uri CollectionUrl
        //{
        //    get { return _contract.Collection.CollectionUrl; }
        //}

        public string Description
        {
            get { return _contract.Description; }
        }

        public VersionControl VersionControl
        {
            get
            {
                VersionControl versionControl;

                if (Enum.TryParse(_contract.Capabilities.VersionControl.SourceControlType, true, out versionControl))
                {
                    // parsing succeeded
                    return versionControl;
                }
                return VersionControl.None;
            }
        }

        public ProcessTemplate ProcessTemplate
        {
            get
            {
                if (_contract.Capabilities == null || _contract.Capabilities.ProcessTemplate == null ||
                    String.IsNullOrEmpty(_contract.Capabilities.ProcessTemplate.Name))
                {
                    return ProcessTemplate.Undefined;
                }

                string templateName = _contract.Capabilities.ProcessTemplate.Name;

                if (templateName.Contains("Scrum"))
                {
                    return ProcessTemplate.Scrum;
                }
                if (templateName.Contains("Agile"))
                {
                    return ProcessTemplate.Agile;
                }
                if (templateName.Contains("CMMI"))
                {
                    return ProcessTemplate.CMMI;
                }

                return ProcessTemplate.Undefined;
            }
        }

        public AccountData Account { get; set; }

        internal ProjectData(ProjectContract contract)
        {
            _contract = contract;
        }
    }
}