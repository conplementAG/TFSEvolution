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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TFSDataAccessPortable
{
    public class TFSClient
    {
        private const string API_VERSION = "1.0";

        #region builds

        public async Task<ReadOnlyCollection<BuildEventData>> GetAllBuildsAsync(AuthenticationData authData,
            string serverAddress, string projectCollection, string projectName)
        {
            return await GetBuildsAsync(authData, serverAddress, projectCollection, projectName, null);
        }

        public async Task<ReadOnlyCollection<BuildEventData>> GetLatestBuildsAsync(AuthenticationData authData,
            string serverAddress, string projectCollection, string projectName, int count)
        {
            return await GetBuildsAsync(authData, serverAddress, projectCollection, projectName, count);
        }

        private async Task<ReadOnlyCollection<BuildEventData>> GetBuildsAsync(AuthenticationData authData,
            string serverAddress, string projectCollection, string projectName, int? count)
        {
            #region build filter for query

            string query = String.Empty;

            // add filter for api version
            Helper.AddQueryValue(ref query, "api-version", API_VERSION);

            // add filter for status as we want to ignore some states
            string buildStatus = string.Empty;
            foreach (var value in Enum.GetValues(typeof (BuildStatus)).Cast<BuildStatus>())
            {
                if (value != BuildStatus.None)
                {
                    buildStatus += value + ",";
                }
            }
            buildStatus = buildStatus.Remove(buildStatus.Length - 1, 1);
            Helper.AddQueryValue(ref query, "status", buildStatus);

            // add filter to only get latest builds if specified
            if (count != null)
            {
                Helper.AddQueryValue(ref query, "$top", count.ToString());
            }

            #endregion

            using (var httpClient = new HttpClient())
            {
                Authenticate(httpClient, authData);

                var responseStream =
                    await
                        httpClient.GetStreamAsync(
                            new Uri(string.Format("{0}{1}/{2}/_apis/build/builds{3}", serverAddress, projectCollection,
                                projectName, query)));
                DataContractJsonSerializer serializer =
                    new DataContractJsonSerializer(typeof (DefaultListContract<BuildContract>));
                List<BuildContract> cobjBuilds =
                    ((DefaultListContract<BuildContract>) serializer.ReadObject(responseStream)).Value;

                var result = cobjBuilds.Select(elem => new BuildEventData(elem)).ToList();
                return new ReadOnlyCollection<BuildEventData>(result);
            }
        }

        #endregion

        #region checkins

        public async Task<ReadOnlyCollection<CheckinEventData>> GetAllCheckinsAsync(AuthenticationData authData,
            string serverAddress, string projectCollection, string projectName)
        {
            return await GetCheckinsAsync(authData, serverAddress, projectCollection, projectName, null);
        }

        public async Task<ReadOnlyCollection<CheckinEventData>> GetLatestCheckinsAsync(AuthenticationData authData,
            string serverAddress, string projectCollection, string projectName, int count)
        {
            return await GetCheckinsAsync(authData, serverAddress, projectCollection, projectName, count);
        }

        private async Task<ReadOnlyCollection<CheckinEventData>> GetCheckinsAsync(AuthenticationData authData,
            string serverAddress, string projectCollection, string projectName, int? count)
        {
            #region build filter for query

            string query = String.Empty;

            // add filter for team project
            if (!string.IsNullOrEmpty(projectName))
            {
                Helper.AddQueryValue(ref query, "itemPath", string.Format("$/{0}", projectName));
            }

            // add filter to only get latest builds if specified
            if (count != null)
            {
                Helper.AddQueryValue(ref query, "$top", count.ToString());
            }

            #endregion

            using (var httpClient = new HttpClient())
            {
                Authenticate(httpClient, authData);

                var responseStream =
                    await
                        httpClient.GetStreamAsync(
                            new Uri(string.Format("{0}{1}/_apis/tfvc/changesets{2}", serverAddress, projectCollection,
                                query)));
                DataContractJsonSerializer serializer =
                    new DataContractJsonSerializer(typeof (DefaultListContract<ChangesetContract>));
                List<ChangesetContract> cobjChangeSetContracts =
                    ((DefaultListContract<ChangesetContract>) serializer.ReadObject(responseStream)).Value;

                // Get changesetdetails for the resulting changeset
                foreach (var changesetContract in cobjChangeSetContracts)
                {
                    var changeSetResponseStream =
                        await
                            httpClient.GetStreamAsync(
                                new Uri(string.Format("{0}{1}/_apis/tfvc/changesets/{2}?includeWorkItems=true",
                                    serverAddress, projectCollection, changesetContract.ChangesetID)));
                    serializer = new DataContractJsonSerializer(typeof (ChangesetContract));
                    ChangesetContract changeSetDetails =
                        ((ChangesetContract) serializer.ReadObject(changeSetResponseStream));

                    changesetContract.LinkedWorkItems = changeSetDetails.LinkedWorkItems;
                }

                var result = cobjChangeSetContracts.Select(elem => new CheckinEventData(elem)).ToList();

                return new ReadOnlyCollection<CheckinEventData>(result);
            }
        }

        #endregion

        #region connection tests

        public async Task<bool> TestAlternativeCredentials(AuthenticationData authData, string tfsUri,
            string projectCollection)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    Authenticate(httpClient, authData);

                    HttpRequestMessage request = new HttpRequestMessage
                    {
                        Method = new HttpMethod("GET"),
                        RequestUri = new Uri(string.Format("{0}{1}", tfsUri, projectCollection))
                    };

                    using (HttpResponseMessage response = httpClient.SendAsync(request).Result)
                    {
                        await response.Content.ReadAsStringAsync();
                        response.EnsureSuccessStatusCode();
                    }
                }

                return true;
            }
            catch
            {
            }

            return false;
        }

        public async Task<bool> TestConnectionToAlternativeRest(string serverUri)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage
                    {
                        Method = new HttpMethod("GET"),
                        RequestUri = new Uri(string.Format("https://{0}/api/values", serverUri))
                    };

                    using (HttpResponseMessage response = httpClient.SendAsync(request).Result)
                    {
                        await response.Content.ReadAsStringAsync();
                        response.EnsureSuccessStatusCode();
                    }
                }

                return true;
            }
            catch
            {
            }

            return false;
        }

        #endregion

        #region logged in user profile

        public async Task<UserProfileData> GetLoggedInUserAsync(AuthenticationData authData)
        {
            using (var httpClient = new HttpClient())
            {
                Authenticate(httpClient, authData);

                var responseStream =
                    await
                        httpClient.GetStreamAsync(new Uri("https://app.vssps.visualstudio.com/_apis/profile/profiles/me"));

                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof (ProfileContract));
                ProfileContract profileContract = (ProfileContract) serializer.ReadObject(responseStream);

                return new UserProfileData(profileContract);
            }
        }

        public static async Task<byte[]> GetAvatarOfProfileAsync(AuthenticationData basicAuthData, PersonData person)
        {
            using (var httpClient = new HttpClient())
            {
                Authenticate(httpClient, basicAuthData);

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                var result = await httpClient.GetByteArrayAsync(person.ImageUrl);
                return result;
            }
        }

        public async Task<ReadOnlyCollection<AccountData>> GetAccountsOfProfileAsync(AuthenticationData authData,
            Guid profileID)
        {
            using (var httpClient = new HttpClient())
            {
                Authenticate(httpClient, authData);

                // Get accounts which profile is member of (includes the owning ones)
                var memberStream =
                    await
                        httpClient.GetStreamAsync(
                            new Uri(
                                string.Format(
                                    "https://app.vssps.visualstudio.com/_apis/accounts?memberid={0}&api-version={1}",
                                    profileID, API_VERSION)));
                DataContractJsonSerializer serializer =
                    new DataContractJsonSerializer(typeof (DefaultListContract<AccountContract>));
                List<AccountContract> memberAccounts =
                    ((DefaultListContract<AccountContract>) serializer.ReadObject(memberStream)).Value;

                var result = memberAccounts.Select(elem => new AccountData(elem)).ToList();

                return new ReadOnlyCollection<AccountData>(result);
            }
        }

        #endregion

        # region projects and teams

        public async Task<ReadOnlyCollection<ProjectData>> GetTeamProjectsAsync(AuthenticationData authData,
            string serverAddress, string projectCollection)
        {
            using (var httpClient = new HttpClient())
            {
                Authenticate(httpClient, authData);

                var resultList = new List<ProjectData>();

                var responseObject =
                    await
                        httpClient.GetDynamicAsync(
                            new Uri(string.Format("{0}{1}/_apis/projects?api-version={2}", serverAddress,
                                projectCollection, API_VERSION)));

                var projectList = (JArray) responseObject.value;

                foreach (dynamic project in projectList)
                {
                    string projectID = project.id;

                    var responseStream =
                        await
                            httpClient.GetStreamAsync(
                                new Uri(
                                    string.Format("{0}{1}/_apis/projects/{2}?api-version={3}&includecapabilities=true",
                                        serverAddress, projectCollection, projectID, API_VERSION)));

                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof (ProjectContract));
                    ProjectContract projectContract = ((ProjectContract) serializer.ReadObject(responseStream));

                    resultList.Add(new ProjectData(projectContract));
                }

                return new ReadOnlyCollection<ProjectData>(resultList);
            }
        }

        public async Task<ReadOnlyCollection<TeamTFSData>> GetTeaminfosOfProjectAsync(AuthenticationData authData,
            string serverAddress, string tfsServerAdress, string projectCollection, string projectName)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var results = new List<TeamTFSData>();
                var teamInfos =
                    await
                        httpClient.GetDynamicArrayAsync(
                            new Uri(
                                string.Format(
                                    "https://{0}/api/teaminfo?tfsuri={1}&user={2}&pwd={3}&projectCollection={4}&project={5}",
                                    serverAddress, WebUtility.UrlEncode(tfsServerAdress),
                                    WebUtility.UrlEncode(authData.UserName), WebUtility.UrlEncode(authData.Password),
                                    WebUtility.UrlEncode(projectCollection), WebUtility.UrlEncode(projectName))));

                if (teamInfos == null)
                {
                    return null;
                }

                foreach (var teamInfo in teamInfos)
                {
                    results.Add(new TeamTFSData(teamInfo));
                }

                return new ReadOnlyCollection<TeamTFSData>(results);
            }
        }

        public async Task<ReadOnlyCollection<PersonData>> GetMembersOfTeamAsync(AuthenticationData authData,
            AuthenticationData basicAuthData, string serverAddress, string projectCollection, string projectID,
            string teamID)
        {
            using (var httpClient = new HttpClient())
            {
                Authenticate(httpClient, authData);

                var responseStream =
                    await
                        httpClient.GetStreamAsync(
                            new Uri(string.Format("{0}{1}/_apis/projects/{2}/teams/{3}/members?api-version={4}",
                                serverAddress, projectCollection, projectID, teamID, API_VERSION)));
                DataContractJsonSerializer serializer =
                    new DataContractJsonSerializer(typeof (DefaultListContract<PersonContract>));
                List<PersonContract> projectContracts =
                    ((DefaultListContract<PersonContract>) serializer.ReadObject(responseStream)).Value;

                var result = projectContracts.Select(elem => new PersonData(elem)).ToList();

                foreach (var person in result)
                {
                    try
                    {
                        person.ImageBytes = await GetAvatarOfProfileAsync(basicAuthData, person);
                    }
                    catch
                    {
                        // no image is better than no teammembers
                    }
                }

                return new ReadOnlyCollection<PersonData>(result);
            }
        }

        #endregion

        #region Workitems

        public async Task<ReadOnlyCollection<WorkItemData>> GetSprintWorkItemsOfProjectAsync(
            AuthenticationData authData, string serverAddress, string projectCollection, string projectName,
            List<TeamTFSData.AreaData> areas, string iterationPath)
        {
            using (var httpClient = new HttpClient())
            {
                // define which query to use when getting workitems
                JObject query = JObject.FromObject(new
                {
                    query =
                        string.Format(
                            "SELECT [System.Id],[System.WorkItemType],[System.Title],[System.AssignedTo],[System.State],[System.Tags] " +
                            "FROM WorkItemLinks " +
                            "WHERE ([Source].[System.TeamProject] = @project " +
                            "AND ({0}) " + // source area
                            "AND ({1}) " + // target area
                            "AND [Source].[System.IterationPath] = '{2}' AND [Target].[System.IterationPath] = '{2}' " +
                            "AND [Source].[System.WorkItemType] <> '' " +
                            "AND [Source].[System.State] <> '' AND [Source].[System.State] <> 'Removed') " +
                            "mode(MayContain)",
                            GetAreaPartForQuery(projectName, areas, "[Source]"),
                            GetAreaPartForQuery(projectName, areas, "[Target]"), iterationPath)
                });

                dynamic queryResult =
                    await GetWorkItemIDs(authData, serverAddress, projectCollection, projectName, httpClient, query);

                // Get string of workitem IDs (split in groups of 200 ids)
                var workItemStrings = BuildWorkItemStrings(queryResult.workItemRelations);

                // Get data of all workitems returned by query
                string fields =
                    string.Format(
                        "fields=System.Title,System.AreaPath,System.IterationPath,System.WorkItemType,System.State,System.AssignedTo,Microsoft.VSTS.Scheduling.Effort,microsoft.vsts.scheduling.remainingwork,System.Description");
                List<WorkItemData> allWorkItemData =
                    await GetWorkItemData(httpClient, serverAddress, projectCollection, workItemStrings, fields);

                // Create relations (parent, children, related) between workitem objects
                List<WorkItemData> relatedWorkItems = CreateWorkItemRelations(allWorkItemData,
                    queryResult.workItemRelations);


                return new ReadOnlyCollection<WorkItemData>(relatedWorkItems);
            }
        }

        public async Task<ReadOnlyCollection<WorkItemData>> GetTaskboardItemsOfProjectAsync(AuthenticationData authData,
            string serverAddress, string projectCollection, string projectName, List<TeamTFSData.AreaData> areas,
            string iterationPath)
        {
            using (var httpClient = new HttpClient())
            {
                // define which query to use when getting workitems
                JObject query = JObject.FromObject(new
                {
                    query =
                        string.Format(
                            "SELECT [System.Id],[System.WorkItemType],[System.Title],[System.AssignedTo],[System.State],[System.Tags] " +
                            "FROM WorkItemLinks " +
                            "WHERE ([Source].[System.TeamProject] = @project " +
                            "AND ({0}) " + // source area
                            "AND ({1}) " + // target area
                            "AND [Source].[System.IterationPath] = '{2}' AND [Target].[System.IterationPath] = '{2}' " +
                            "AND ([Source].[System.WorkItemType] = 'Task' OR [Source].[System.WorkItemType] = 'Bug' OR [Source].[System.WorkItemType] = 'Product Backlog Item')" +
                            // for task board we only need PBI, Task, Bug 
                            "AND ([Target].[System.WorkItemType] = 'Task' OR [Target].[System.WorkItemType] = 'Bug' OR [Target].[System.WorkItemType] = 'Product Backlog Item')" +
                            // for task board we only need PBI, Task, Bug 
                            "AND [Source].[System.State] <> '' AND [Source].[System.State] <> 'Removed') " +
                            "mode(MayContain)",
                            GetAreaPartForQuery(projectName, areas, "[Source]"),
                            GetAreaPartForQuery(projectName, areas, "[Target]"), iterationPath)
                });

                dynamic queryResult =
                    await GetWorkItemIDs(authData, serverAddress, projectCollection, projectName, httpClient, query);

                // Get string of workitem IDs (split in groups of 200 ids)
                var workItemStrings = BuildWorkItemStrings(queryResult.workItemRelations);

                // Get data of all workitems returned by query
                string fields =
                    string.Format(
                        "fields=System.Title,System.AreaPath,System.IterationPath,System.WorkItemType,System.State,System.AssignedTo,Microsoft.VSTS.Scheduling.Effort,microsoft.vsts.scheduling.remainingwork,System.Description");
                List<WorkItemData> allWorkItemData =
                    await GetWorkItemData(httpClient, serverAddress, projectCollection, workItemStrings, fields);

                // Create relations (parent, children, related) between workitem objects
                List<WorkItemData> relatedWorkItems = CreateWorkItemRelations(allWorkItemData,
                    queryResult.workItemRelations);

                return new ReadOnlyCollection<WorkItemData>(relatedWorkItems);
            }
        }


        private List<WorkItemData> CreateWorkItemRelations(List<WorkItemData> allWorkItemData, JArray workItemRelations)
        {
            List<WorkItemData> result = new List<WorkItemData>();

            // NOTE: query result is a hierarchy of workitem relations in a source - target notation
            // parse json string in objects
            List<JObject> workitemlinks = workItemRelations.ToObject<List<JObject>>();

            foreach (var linkobj in workitemlinks)
            {
                // find out which properties the link object has
                var linkProperties = linkobj.Properties().Select(p => p.Name).ToList();

                // we are only interested in relations
                if (linkProperties.Contains("rel"))
                {
                    dynamic link = linkobj;
                    var source = allWorkItemData.GetWorkItemByID((int) link.source.id);
                    var target = allWorkItemData.GetWorkItemByID((int) link.target.id);

                    // for all parent-child relations add child and parent vice versa
                    if (link.rel == "System.LinkTypes.Hierarchy-Forward")
                    {
                        source.ChildWorkItems.Add(target);
                        target.ParentWorkItem = source;
                    }
                        // for related workitems
                        // NOTE a relation between two workitems is contained in original list twice (for both directions)
                        // --> add target item to source
                    else if (link.rel == "System.LinkTypes.Related")
                    {
                        source.RelatedWorkItems.Add(target);
                    }
                    else if (link.rel == "Microsoft.VSTS.Common.TestedBy-Forward")
                    {
                        source.RelatedWorkItems.Add(target);
                    }
                }
            }

            // add all workitems that do not have a parent to the result collection
            result.AddRange(allWorkItemData.FindAll(x => x.ParentWorkItem == null));

            return result;
        }

        private async Task<List<WorkItemData>> GetWorkItemData(HttpClient httpClient, string serverAddress,
            string projectCollection, List<string> workItemStrings, string fields = "")
        {
            List<WorkItemData> allWorkItems = new List<WorkItemData>();

            foreach (var workItems in workItemStrings)
            {
                string uriString;
                if (!String.IsNullOrEmpty(fields))
                {
                    uriString = string.Format("{0}{1}/_apis/wit/workitems?ids={2}&{3}", serverAddress, projectCollection,
                        workItems, fields);
                }
                else
                {
                    uriString = string.Format("{0}{1}/_apis/wit/workitems?ids={2}", serverAddress, projectCollection,
                        workItems);
                }

                dynamic wiContracts = await httpClient.GetDynamicAsync(new Uri(uriString));

                foreach (var item in wiContracts.value)
                {
                    allWorkItems.Add(new WorkItemData(item));
                }
            }
            return allWorkItems;
        }

        private static List<string> BuildWorkItemStrings(JArray cobjFoundWorkItems)
        {
            // NOTE: We can only get 200 items at once
            // --> Split List in smaller lists
            // --> build workitem string for each list
            // --> return list of workitem strings

            // get ids of source workitems
            List<JToken> workitems = cobjFoundWorkItems.Values("source").Values("id").Distinct().ToList();
            // get target workitems and add them
            workitems.AddRange(cobjFoundWorkItems.Values("target").Values("id").Distinct().ToList());

            var cobjWorkItemLists = workitems.Distinct().ToList().SplitList(200);

            List<string> result = new List<string>();

            Parallel.ForEach(cobjWorkItemLists, wiList =>
            {
                string strIDsToGet = string.Empty;

                foreach (var foundWI in wiList)
                {
                    if (!string.IsNullOrEmpty(strIDsToGet))
                    {
                        strIDsToGet += ",";
                    }
                    strIDsToGet += foundWI.Value<int>();
                }

                result.Add(strIDsToGet);
            });
            return result;
        }

        private static List<string> BuildWorkItemStringsFromFlat(JArray cobjFoundWorkItems)
        {
            // NOTE: We can only get 200 items at once
            // --> Split List in smaller lists
            // --> build workitem string for each list
            // --> return list of workitem strings

            // get ids of workitems
            List<JToken> workitems = cobjFoundWorkItems.Values("id").Distinct().ToList();

            var cobjWorkItemLists = workitems.Distinct().ToList().SplitList(200);

            List<string> result = new List<string>();

            Parallel.ForEach(cobjWorkItemLists, wiList =>
            {
                string strIDsToGet = string.Empty;

                foreach (var foundWI in wiList)
                {
                    if (!string.IsNullOrEmpty(strIDsToGet))
                    {
                        strIDsToGet += ",";
                    }
                    strIDsToGet += foundWI.Value<int>();
                }

                result.Add(strIDsToGet);
            });
            return result;
        }


        public async Task<int> GetWorkSprintBacklogCountAsync(AuthenticationData authData, string serverAddress,
            string projectCollection, string projectName, List<TeamTFSData.AreaData> areas, string iterationPath)
        {
            using (var httpClient = new HttpClient())
            {
                // define which query to use when getting workitems
                JObject wiql = JObject.FromObject(new
                {
                    query =
                        string.Format(
                            "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.IterationPath] UNDER '{0}' AND ({1})",
                            iterationPath, GetAreaPartForQuery(projectName, areas, null))
                });

                // Get WorkitemIDs
                dynamic queryResult =
                    await GetWorkItemIDs(authData, serverAddress, projectCollection, projectName, httpClient, wiql);

                JArray workitemIDs = queryResult.results;

                if (workitemIDs != null)
                {
                    return workitemIDs.Count();
                }

                return 0;
            }
        }

        public async Task<int> GetWorkProductBacklogCountAsync(AuthenticationData authData, string serverAddress,
            string projectCollection, string projectName, List<TeamTFSData.AreaData> areas)
        {
            using (var httpClient = new HttpClient())
            {
                string query = String.Empty;

                // add filter for project
                if (!string.IsNullOrEmpty(projectName))
                {
                    Helper.AddQueryValue(ref query, "@project", projectName);
                }

                // define which query to use when getting workitems
                JObject wiql = JObject.FromObject(new
                {
                    wiql =
                        string.Format(
                            "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project AND ({0}) AND [System.WorkItemType] IN GROUP 'Microsoft.RequirementCategory'",
                            GetAreaPartForQuery(projectName, areas, null))
                });

                Authenticate(httpClient, authData);
                // Get workitems
                dynamic queryResult =
                    await
                        httpClient.PushAndGetDynamicAsync(wiql,
                            new Uri(string.Format("{0}{1}/_apis/wit/queryresults{2}", serverAddress, projectCollection,
                                query)));

                JArray workitemIDs = queryResult.results;

                if (workitemIDs != null)
                {
                    return workitemIDs.Count();
                }

                return 0;
            }
        }

        public async Task<int> GetWorkInProgressCountAsync(AuthenticationData authData, string serverAddress,
            string projectCollection, string projectName, List<TeamTFSData.AreaData> areas, string iterationPath)
        {
            using (var httpClient = new HttpClient())
            {
                // define which query to use when getting workitems
                JObject wiql = JObject.FromObject(new
                {
                    query =
                        string.Format(
                            "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.IterationPath] UNDER '{0}' AND ({1}) AND [System.WorkItemType] IN GROUP 'Microsoft.TaskCategory' AND [System.State] = 'In Progress'",
                            iterationPath, GetAreaPartForQuery(projectName, areas, null))
                });

                dynamic queryResult =
                    await GetWorkItemIDs(authData, serverAddress, projectCollection, projectName, httpClient, wiql);

                JArray workitemIDs = queryResult.results;

                if (workitemIDs != null)
                {
                    return workitemIDs.Count();
                }

                return 0;
            }
        }

        public async Task<List<WorkitemChangeData>> GetWorkitemUpdatesAsync(string serverAddress, Guid projectID,
            DateTime dtLastRequest)
        {
            using (var httpClient = new HttpClient())
            {
                //var response = await httpClient.GetDynamicArrayAsync(new Uri(string.Format("https://{0}/api/event?projectID={0}&dtLastRequestTime={1}", projectID, dtLastRequest)));

                var responseStream =
                    await
                        httpClient.GetStreamAsync(
                            new Uri(string.Format("https://{0}/api/event?projectID={1}&dtLastRequestTime={2}",
                                serverAddress, projectID, dtLastRequest)));

                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof (List<WorkitemChangeData>));
                List<WorkitemChangeData> cobjChanges =
                    ((List<WorkitemChangeData>) serializer.ReadObject(responseStream));

                return cobjChanges;
            }
        }

        #endregion

        #region UPDATE WORKITEMS

        public async Task<WorkItemData> SetRemainingWorkOfWI(WorkItemData workItem, double newRemainingWork,
            AuthenticationData authData, string serverAddress, string projectCollection)
        {
            using (var httpClient = new HttpClient())
            {
                // Define the update data to send to VSO
                JArray updateData = new JArray(
                    JObject.FromObject(
                        new
                        {
                            op = "test",
                            path = "/rev",
                            value = workItem.Revision
                        }
                        ),
                    JObject.FromObject(
                        new
                        {
                            op = "add",
                            path = "/fields/microsoft.vsts.scheduling.remainingwork",
                            value = newRemainingWork
                        })
                    );

                return
                    await UpdateWorkItem(authData, serverAddress, projectCollection, workItem, httpClient, updateData);
            }
        }

        public async Task<WorkItemData> SetAssignedToOfWI(WorkItemData workItem, string assigneeFullName,
            AuthenticationData authData, string serverAddress, string projectCollection)
        {
            using (var httpClient = new HttpClient())
            {
                JArray updateData;
                if (!string.IsNullOrEmpty(assigneeFullName))
                {
                    // Define the update data to send to VSO
                    updateData = new JArray(
                        JObject.FromObject(
                            new
                            {
                                op = "test",
                                path = "/rev",
                                value = workItem.Revision
                            }
                            ),
                        JObject.FromObject(
                            new
                            {
                                op = "add",
                                path = "/fields/System.AssignedTo",
                                value = assigneeFullName
                            })
                        );
                }
                else
                {
                    updateData = new JArray(
                        JObject.FromObject(
                            new
                            {
                                op = "test",
                                path = "/rev",
                                value = workItem.Revision
                            }
                            ),
                        JObject.FromObject(
                            new
                            {
                                op = "remove",
                                path = "/fields/System.AssignedTo",
                            })
                        );
                }

                return
                    await UpdateWorkItem(authData, serverAddress, projectCollection, workItem, httpClient, updateData);
            }
        }

        public async Task<WorkItemData> SetStateOfTask(WorkItemData workItem, TaskState newState,
            double? newRemainingWork, AuthenticationData authData, string serverAddress, string projectCollection)
        {
            if (workItem.WorkItemState == WorkItemState.Done)
            {
                // Reopen the task
                if (newRemainingWork == null)
                {
                    // error
                    throw new ArgumentException("Remaining work must be given if task is currently in state 'Done'");
                }

                return
                    await
                        ReopenTask(authData, serverAddress, projectCollection, workItem,
                            newState == TaskState.InProgress, newRemainingWork.Value);
            }

            if (newState == TaskState.Done)
            {
                // Close task
                return await CloseTask(authData, serverAddress, projectCollection, workItem);
            }

            // must be another state change
            return
                await
                    SimpleStateChangeOfTask(authData, serverAddress, projectCollection, workItem,
                        newState == TaskState.InProgress);
        }

        /// <summary>
        ///     Changes the State of a WorkItem from Closed to ToDo or InProgress
        /// </summary>
        /// <param name="authData">The Authentication data</param>
        /// <param name="serverAddress">Server adresse to use</param>
        /// <param name="projectCollection">project collection to use</param>
        /// <param name="workItem">Work item object</param>
        /// <param name="toInProgress">True if new state should be InProgress, false if it should be ToDo.</param>
        /// <returns>True if state transition was successful, false otherwise.</returns>
        private async Task<WorkItemData> SimpleStateChangeOfTask(AuthenticationData authData, string serverAddress,
            string projectCollection, WorkItemData workItem, bool toInProgress)
        {
            // Check that current workitem state is not done
            if (workItem.WorkItemState == WorkItemState.Done)
            {
                return null;
            }

            using (var httpClient = new HttpClient())
            {
                JArray updateData = new JArray(
                    JObject.FromObject(
                        new
                        {
                            op = "test",
                            path = "/rev",
                            value = workItem.Revision
                        }),
                    JObject.FromObject(
                        new
                        {
                            op = "add",
                            path = "/fields/System.State",
                            value = toInProgress ? "In Progress" : "To Do"
                        })
                    );

                return
                    await UpdateWorkItem(authData, serverAddress, projectCollection, workItem, httpClient, updateData);
            }
        }

        /// <summary>
        ///     Changes the State of a WorkItem from Closed to ToDo or InProgress
        /// </summary>
        /// <param name="authData">The Authentication data</param>
        /// <param name="serverAddress">Server adresse to use</param>
        /// <param name="projectCollection">project collection to use</param>
        /// <param name="workItem">Work item object</param>
        /// <param name="toInProgress">True if new state should be InProgress, false if it should be ToDo.</param>
        /// <param name="newRemainingWork">The value for remaining work to set.</param>
        /// <returns>True if state transition was successful, false otherwise.</returns>
        private async Task<WorkItemData> ReopenTask(AuthenticationData authData, string serverAddress,
            string projectCollection, WorkItemData workItem, bool toInProgress, double newRemainingWork)
        {
            using (var httpClient = new HttpClient())
            {
                JArray updateData = new JArray(
                    JObject.FromObject(
                        new
                        {
                            op = "test",
                            path = "/rev",
                            value = workItem.Revision
                        }),
                    JObject.FromObject(
                        new
                        {
                            op = "add",
                            path = "/fields/System.State",
                            value = toInProgress ? "In Progress" : "To Do"
                        }),
                    JObject.FromObject(
                        new
                        {
                            op = "add",
                            path = "/fields/microsoft.vsts.scheduling.remainingwork",
                            value = newRemainingWork
                        }),
                    JObject.FromObject(
                        new

                        {
                            op = "remove",
                            path = "/fields/Microsoft.VSTS.Common.ClosedDate"
                        })
                    );

                return
                    await UpdateWorkItem(authData, serverAddress, projectCollection, workItem, httpClient, updateData);
            }
        }

        /// <summary>
        ///     Changes the State of a WorkItem to Done
        /// </summary>
        /// <param name="authData">The Authentication data</param>
        /// <param name="serverAddress">Server adresse to use</param>
        /// <param name="projectCollection">project collection to use</param>
        /// <param name="workItem">Work item object</param>
        /// <returns>True if state transition was successful, false otherwise.</returns>
        private async Task<WorkItemData> CloseTask(AuthenticationData authData, string serverAddress,
            string projectCollection, WorkItemData workItem)
        {
            using (var httpClient = new HttpClient())
            {
                JArray updateData = new JArray(
                    JObject.FromObject(
                        new
                        {
                            op = "test",
                            path = "/rev",
                            value = workItem.Revision
                        }),
                    JObject.FromObject(
                        new
                        {
                            op = "add",
                            path = "/fields/System.State",
                            value = "Done"
                        }),
                    JObject.FromObject(
                        new
                        {
                            op = "remove",
                            path = "/fields/microsoft.vsts.scheduling.remainingwork"
                        }),
                    JObject.FromObject(
                        new

                        {
                            op = "remove",
                            path = "/fields/Microsoft.VSTS.CMMI.Blocked"
                        }),
                    JObject.FromObject(
                        new

                        {
                            op = "add",
                            path = "/fields/Microsoft.VSTS.Common.ClosedDate",
                            value = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff''Z")
                        })
                    );

                return
                    await UpdateWorkItem(authData, serverAddress, projectCollection, workItem, httpClient, updateData);
            }
        }

        private static async Task<WorkItemData> UpdateWorkItem(AuthenticationData authData, string serverAddress,
            string projectCollection, WorkItemData workItem, HttpClient httpClient, JArray updateData)
        {
            Authenticate(httpClient, authData);

            try
            {
                dynamic response =
                    await
                        httpClient.PatchArrayAsync(updateData,
                            new Uri(string.Format("{0}{1}/_apis/wit/workitems/{2}?api-version={3}", serverAddress,
                                projectCollection, workItem.ID, API_VERSION)));
                return new WorkItemData(response);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region burndown

        public async Task<ReadOnlyCollection<BurnDownPointData>> GetBurndownDataAsync(AuthenticationData authData,
            string serverAddress, string projectCollection, string projectName, SprintData sprint,
            List<TeamTFSData.AreaData> areas)
        {
            var burndownData =
                await
                    GetBurndownDataAsync(authData, serverAddress, projectCollection, projectName, sprint.Path,
                        sprint.StartDate.Value, sprint.EndDate.Value, areas);

            return new ReadOnlyCollection<BurnDownPointData>(burndownData);
        }

        public async Task<BurnDownPointData[]> GetBurndownDataAsync(AuthenticationData authData, string serverAddress,
            string projectCollection, string projectName, string iterationPath, DateTime startDate, DateTime endDate,
            List<TeamTFSData.AreaData> areas)
        {
            string querystring = GetQueryText(projectName, iterationPath, areas);

            var datesOfSprint = new List<DateTime>();
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                datesOfSprint.Add(date);
            }

            // Create a query that, when executed, returns a collection of tasks.
            IEnumerable<Task<BurnDownPointData>> cobjGetDataTasksQuery =
                from date in datesOfSprint
                select
                    GetBurnDownDataForDayAsync(authData, serverAddress, projectCollection, projectName, querystring,
                        date);

            // Use ToList to execute the query and start the tasks. 
            List<Task<BurnDownPointData>> cobjGetDataTasks = cobjGetDataTasksQuery.ToList();

            var burndownData = await Task.WhenAll(cobjGetDataTasks);

            return burndownData;
        }

        public async Task<BurnDownPointData> GetBurnDownDataForDayAsync(AuthenticationData authData,
            string serverAddress, string projectCollection, string projectName, string querystring, DateTime date)
        {
            double? work = null;

            if (date <= DateTime.Today)
            {
                string asof;

                if (date < DateTime.Today)
                {
                    asof = date.AddDays(1).AddMilliseconds(-1).ToString("u");
                        //"2014-06-05t11:40:09.84z";//"2014-06-05 13:35:00 PM";//
                }
                else
                {
                    asof = DateTime.Now.ToString("u");
                }

                string fullQuery = string.Format("{0} ASOF '{1}'", querystring, asof);


                using (var httpClient = new HttpClient())
                {
                    // define which query to use when getting workitems
                    JObject wiql = JObject.FromObject(new
                    {
                        query = fullQuery
                    });

                    dynamic queryResult =
                        await GetWorkItemIDs(authData, serverAddress, projectCollection, projectName, httpClient, wiql);

                    // Get string of workitem IDs (split in groups of 200 ids)
                    var workItemStrings = BuildWorkItemStringsFromFlat(queryResult.workItems);
                    // Get workitem data of day
                    string whereClause =
                        string.Format("fields=system.id,microsoft.vsts.scheduling.remainingwork&asof={0}", asof);
                    List<WorkItemData> allWorkItemData =
                        await
                            GetWorkItemData(httpClient, serverAddress, projectCollection, workItemStrings, whereClause);

                    foreach (WorkItemData wi in allWorkItemData)
                    {
                        if (wi.RemainingWork != null)
                        {
                            work = work == null ? wi.RemainingWork : work + wi.RemainingWork;
                        }
                    }
                }
            }

            return new BurnDownPointData
            {
                Date = date,
                RemainingWork = work
            };
        }

        private static async Task<dynamic> GetWorkItemIDs(AuthenticationData authData, string serverAddress,
            string projectCollection, string projectName, HttpClient httpClient, JObject wiql)
        {
            Authenticate(httpClient, authData);

            // Get workitemids of sprint of day
            dynamic queryResult =
                await
                    httpClient.PushAndGetDynamicAsync(wiql,
                        new Uri(string.Format("{0}{1}/{2}/_apis/wit/wiql?api-version={3}", serverAddress,
                            projectCollection, projectName, API_VERSION)));

            return queryResult;
        }

        private static string GetQueryText(string projectName, string iterationPath, List<TeamTFSData.AreaData> areas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT [System.Id], [Microsoft.VSTS.Scheduling.RemainingWork] FROM WorkItems WHERE (");
            sb.AppendFormat("({0})", GetAreaPartForQuery(projectName, areas, null));
            sb.AppendFormat(" AND ([System.IterationPath] = '{0}')", iterationPath);
            sb.Append(
                " AND ([System.WorkItemType] IN ('Task')) AND ([System.State] IN ('To Do', 'In Progress')) AND ([Microsoft.VSTS.Scheduling.RemainingWork] >= 0))");

            return sb.ToString();
        }

        private static string GetAreaPartForQuery(string projectName, List<TeamTFSData.AreaData> areas,
            string fieldprefix)
        {
            string areaField = "[System.AreaPath]";
            if (!String.IsNullOrEmpty(fieldprefix))
            {
                areaField = areaField.Insert(0, fieldprefix + ".");
            }

            if (areas != null)
            {
                StringBuilder areaString = new StringBuilder();
                foreach (var area in areas)
                {
                    if (areaString.Length > 0)
                    {
                        areaString.Append(" AND ");
                    }


                    if (area.IncludeChildren)
                    {
                        areaString.AppendFormat("{0} UNDER '{1}'", areaField, area.AreaName);
                    }
                    else
                    {
                        areaString.AppendFormat("{0} = '{1}'", areaField, area.AreaName);
                    }
                }

                return areaString.ToString();
            }
            return string.Format("{0} UNDER '{1}'", areaField, projectName);
        }

        #endregion

        #region sprint data

        public async Task<ReadOnlyCollection<SprintData>> GetSprintsOfProjectAsync(AuthenticationData authData,
            string serverAddress, string tfsServerAdress, string projectCollection, string projectName)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var results = new List<SprintData>();
                var sprints =
                    await
                        httpClient.GetDynamicArrayAsync(
                            new Uri(
                                string.Format(
                                    "https://{0}/api/sprints?tfsuri={1}&user={2}&pwd={3}&projectCollection={4}&project={5}",
                                    serverAddress, WebUtility.UrlEncode(tfsServerAdress),
                                    WebUtility.UrlEncode(authData.UserName), WebUtility.UrlEncode(authData.Password),
                                    WebUtility.UrlEncode(projectCollection), WebUtility.UrlEncode(projectName))));

                foreach (var sprint in sprints)
                {
                    results.Add(new SprintData(sprint));
                }

                return new ReadOnlyCollection<SprintData>(results);
            }
        }

        #endregion

        #region private helper methods

        /// <summary>
        ///     Serialize Person object to Json string
        /// </summary>
        /// <param name="objectToSerialize">Person object instance</param>
        /// <returns>return Json String</returns>
        public string SerializeObjectToJson(object objectToSerialize)
        {
            if (objectToSerialize == null)
            {
                throw new ArgumentException("objectToSerialize must not be null");
            }
            MemoryStream ms;

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(objectToSerialize.GetType());
            ms = new MemoryStream();
            serializer.WriteObject(ms, objectToSerialize);
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);
            return sr.ReadToEnd();
        }

        private static void Authenticate(HttpClient httpClient, AuthenticationData authData)
        {
            if (String.IsNullOrEmpty(authData.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization = CreateBasicHeader(authData.UserName, authData.Password);
            }
            else
            {
                // Assign the authentication headers
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData.Token);
            }
        }

        private static AuthenticationHeaderValue CreateBasicHeader(string username, string password)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(username + ":" + password);

            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        #endregion
    }
}