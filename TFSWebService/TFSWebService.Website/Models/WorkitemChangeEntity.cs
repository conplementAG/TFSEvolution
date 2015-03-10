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
using Microsoft.WindowsAzure.Storage.Table;
using TFSWebService.Website.Helpers;

namespace TFSWebService
{
    /// <summary>
    ///     Class that represents work item changes.
    /// </summary>
    public class WorkitemChangeEntity : TableEntity
    {
        private readonly IEncryptionHelper _encryptionHelper;

        /// <summary>
        ///     Gets a byte array of work item field changes
        /// </summary>
        public byte[] WorkitemChangeDataBytes { get; set; }

        /// <summary>
        ///     Gets or sets the workitem id.
        /// </summary>
        public int WorkitemID { get; set; }

        /// <summary>
        ///     Gets the project id.
        /// </summary>
        internal Guid ProjectID
        {
            get { return Guid.Parse(PartitionKey); }
        }

        /// <summary>
        ///     Gets the time when the workitem was changed.
        /// </summary>
        internal DateTime UpdateTime
        {
            get { return DateTime.Parse(RowKey); }
        }

        /// <summary>
        ///     Gets the list of work item fields changes
        /// </summary>
        internal IEnumerable<FieldChange> WorkitemChangeData
        {
            get
            {
                return
                    SerializationService.ConvertFromByteArray<IEnumerable<FieldChange>>(
                        _encryptionHelper.Decrypt(WorkitemChangeDataBytes));
            }
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="updateTime">Change date.</param>
        /// <param name="workitemId">ID of the wrkitem that has changed.</param>
        /// <param name="wiChangeData">List of workitem field changes.</param>
        /// <param name="encryptionHelper"></param>
        public WorkitemChangeEntity(Guid projectId, DateTime updateTime, int workitemId,
            IEnumerable<FieldChange> wiChangeData, IEncryptionHelper encryptionHelper)
        {
            _encryptionHelper = encryptionHelper;

            PartitionKey = projectId.ToString();
            RowKey = String.Format("{0:s}", updateTime);

            WorkitemID = workitemId;
            WorkitemChangeDataBytes = _encryptionHelper.Encrypt(SerializationService.ConvertToByteArray(wiChangeData));
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public WorkitemChangeEntity()
        {
        }
    }
}