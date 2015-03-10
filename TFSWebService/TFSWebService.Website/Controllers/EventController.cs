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
using System.Configuration;
using System.Linq;
using System.Web.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using TFSWebService.Website.Helpers;

namespace TFSWebService.Website.Controllers
{
    public class EventController : ApiController
    {
        #region POST for Service hook

        /// <summary>
        ///     Method used by the service hook to post all events, which the web service is registered
        ///     for, to the webservice.
        ///     API Call:
        ///     POST: api/event
        /// </summary>
        /// <param name="obj">The JSON-Object from the service hook that contains the event information.</param>
        public void PostEvent(JObject obj)
        {
            dynamic jsonObject = obj;

            string eventType = jsonObject.eventType;

            switch (eventType)
            {
                    // if we have a workitem.updated event we want to do something
                case "workitem.updated":
                    OnWorkitemChanged(jsonObject);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        private static bool OnWorkitemChanged(dynamic jsonObject)
        {
            try
            {
                // Get values from jsonObject
                // Project ID
                Guid projectID;
                bool success = Guid.TryParse(jsonObject.resourceContainers.project.id.ToString(), out projectID);
                if (success)
                {
                    // Changed Date
                    DateTime dtChanged = DateTime.Parse(jsonObject.createdDate.ToString());

                    // WorkitemID
                    // Currently we need to use this workarround as the WI ID is not available in any property
                    string workitemUri = jsonObject.resource._links.parent.href.Value;
                    int indexStart = workitemUri.LastIndexOf("/") + 1;
                    int workitemID = Convert.ToInt32(workitemUri.Substring(indexStart, workitemUri.Length - indexStart));

                    // workitem field changes
                    List<FieldChange> cobjChanges = new List<FieldChange>();
                    foreach (var item in jsonObject.resource.fields)
                    {
                        #region parse fields

                        var properties = ((JObject) item.Value).Properties().Select(p => p.Name).ToList();

                        object oldValue = null;
                        object newValue = null;
                        if (properties.Contains("oldValue"))
                        {
                            oldValue = item.Value.oldValue.Value;
                        }
                        if (properties.Contains("newValue"))
                        {
                            newValue = item.Value.newValue.Value;
                        }
                        cobjChanges.Add(new FieldChange
                        {
                            FieldName = item.Name,
                            OldValue = oldValue,
                            NewValue = newValue
                        });

                        #endregion
                    }

                    // Save work item changes in azure
                    return SaveWorkitemChangeInAzure(projectID, dtChanged, workitemID, cobjChanges);
                }
            }
            catch
            {
                // todo: add errorhandling
            }

            return false;
        }

        /// <summary>
        ///     Saves the work item change in azure
        /// </summary>
        /// <param name="projectID">The team project.</param>
        /// <param name="changedDate">Date where the change happend.</param>
        /// <param name="workitemID">Work item ID.</param>
        /// <param name="workitemChanges">Changes of the work item fields.</param>
        /// <returns>True if Ssaving was successful, false otherwise.</returns>
        private static bool SaveWorkitemChangeInAzure(Guid projectID, DateTime changedDate, int workitemID,
            IEnumerable<FieldChange> workitemChanges)
        {
            try
            {
                // parse data from input values


                WorkitemChangeEntity entity = new WorkitemChangeEntity(projectID, changedDate, workitemID,
                    workitemChanges,
                    new EncryptionHelper(
                        SerializationService.ConvertToByteArray(ConfigurationManager.AppSettings["enc_key"]),
                        SerializationService.ConvertToByteArray(ConfigurationManager.AppSettings["enc_iv"])));


                if (entity != null)
                {
                    CloudStorageAccount storageAccount =
                        CloudStorageAccount.Parse(
                            ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

                    // Create table client
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                    // Create table if it does not exist
                    CloudTable table = tableClient.GetTableReference("WorkitemChanges");
                    table.CreateIfNotExists();


                    // Create the TableOperation that inserts the customer entity.
                    TableOperation insertOperation = TableOperation.Insert(entity);

                    // Execute the insert operation.
                    TableResult result = table.Execute(insertOperation);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region GET last work item changes

        /// <summary>
        ///     Gets the last workitem changes of the given project since the given timestamp.
        ///     API Call:
        ///     GET: api/event?projectID=<projectID>&dtLastRequestTime=<dtLastRequestTime>
        /// </summary>
        /// <param name="projectID">The project ID.</param>
        /// <param name="dtLastRequestTime">Timestamp since when the changes should be returned.</param>
        /// <returns>An array of workitem changes.</returns>
        public JArray GetEvent(Guid projectID, DateTime dtLastRequestTime)
        {
            try
            {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount =
                    CloudStorageAccount.Parse(
                        ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Create the CloudTable object that represents the "WorkitemChanges" table.
                CloudTable table = tableClient.GetTableReference("WorkitemChanges");

                // Construct the query operation for all customer entities where changedDate after dtLastRequestTime
                TableQuery<WorkitemChangeEntity> query =
                    new TableQuery<WorkitemChangeEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey",
                        QueryComparisons.Equal, projectID.ToString()));

                JArray array = new JArray();
                foreach (WorkitemChangeEntity entity in table.ExecuteQuery(query))
                {
                    if (entity.UpdateTime > dtLastRequestTime)
                    {
                        array.Add(JObject.FromObject(new
                        {
                            workitemID = entity.WorkitemID,
                            changeDate = entity.UpdateTime,
                            changes = entity.WorkitemChangeData
                        }));
                    }
                }

                return array;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}