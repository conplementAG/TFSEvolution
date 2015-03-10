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
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using TFSDataAccessPortable;
using TFSExpert.Auth;
using TFSExpert.Common;
using TFSExpert.Data;
using TFSExpert.Misc;
using TFSExpert.Navigation;
using TFSExpert.ViewModels;

namespace TFSExpert.DataModel
{
    public delegate void DataLoading();

    public delegate void LoadingMapItemsDone(bool success, ObservableCollection<WorkItem> results);

    public delegate void LoadingTaskboardItemsDone(bool success, ObservableCollection<WorkItem> results);

    public delegate void LoadingProjectsDone(bool success);

    public delegate void LoadingBuildsDone(bool success);

    public delegate void LoadingCheckinsDone(bool success, ObservableCollection<CheckinEventItem> checkins);

    public delegate void LoadingSprintDataDone(bool success, Sprint sprint);

    public class DataSource : BindableBase
    {
        private static TFSClient _tfsClient;
        internal DateTime LastUpdateRequest { get; set; }

        private DispatcherTimer _updateWITimer = new DispatcherTimer();

        #region events

        public event LoadingTaskboardItemsDone TaskboardItemsLoaded;

        private void OnTaskboardDataLoaded(bool bSuccess, ObservableCollection<WorkItem> results)
        {
            if (TaskboardItemsLoaded != null)
            {
                TaskboardItemsLoaded(bSuccess, results);
            }

            LastUpdateRequest = DateTime.UtcNow;
            _updateWITimer.Start();
        }

        public event LoadingProjectsDone ProjectsLoaded;

        private void OnProjectsLoaded(bool bSuccess)
        {
            if (ProjectsLoaded != null)
            {
                ProjectsLoaded(bSuccess);
            }
        }

        public event LoadingBuildsDone BuildsLoaded;

        private void OnBuildsLoaded(bool bSuccess)
        {
            if (BuildsLoaded != null)
            {
                BuildsLoaded(bSuccess);
            }
        }

        public event LoadingCheckinsDone CheckinsLoaded;

        private void OnCheckinsLoaded(bool bSuccess, ObservableCollection<CheckinEventItem> checkins)
        {
            if (CheckinsLoaded != null)
            {
                CheckinsLoaded(bSuccess, checkins);
            }
        }

        public event LoadingSprintDataDone SprintDataLoaded;
        public event DataLoading SprintDataLoading;

        private void OnSprintDataLoading()
        {
            if (SprintDataLoading != null)
            {
                SprintDataLoading();
            }
        }

        private void OnSprintDataLoaded(bool bSuccess, Sprint sprintData)
        {
            if (SprintDataLoaded != null)
            {
                SprintDataLoaded(bSuccess, sprintData);
            }
        }

        #endregion

        public ObservableCollection<TeamProject> TeamProjects { get; set; }

        public ObservableCollection<BuildEventItem> LatestBuildEvents { get; set; }

        public ObservableCollection<CheckinEventItem> LatestCheckinEvents { get; set; }

        public ObservableCollection<WorkItem> WorkItemCollection { get; set; }

        public ObservableCollection<WorkItem> WorkItemTree { get; set; }

        public UserInformation CurrentUser { get; private set; }

        public Sprint Sprint { get; private set; }

        public DataSource()
        {
            InitDatasource();
            _tfsClient = new TFSClient();

            _updateWITimer.Interval = new TimeSpan(0, 0, 5);
            _updateWITimer.Tick += CheckForWorkItemUpdates;
        }

        private void InitDatasource()
        {
            LatestBuildEvents = new ObservableCollection<BuildEventItem>();
            LatestCheckinEvents = new ObservableCollection<CheckinEventItem>();
            TeamProjects = new ObservableCollection<TeamProject>();
            WorkItemCollection = new ObservableCollection<WorkItem>();
            WorkItemTree = new ObservableCollection<WorkItem>();
            CurrentUser = UserInformation.Empty;
            Sprint = Sprint.Empty;
        }


        public void ResetDataSource()
        {
            LatestBuildEvents.Clear();
            LatestCheckinEvents.Clear();
            TeamProjects.Clear();
            WorkItemCollection.Clear();
            WorkItemTree.Clear();
            CurrentUser.Logout();
            Sprint.Reset();
        }

        public async void LoadProjectDataAsync()
        {
            // clean collections
            TeamProjects.Clear();

            UserProfileData objProfile = null;

            try
            {
                objProfile = await GetLoggedInUser();
            }
            catch
            {
                //raise event and results
                OnProjectsLoaded(false);
            }

            if (objProfile != null)
            {
                try
                {
                    // get accounts the current user is member of
                    var cobjAccounts =
                        await
                            _tfsClient.GetAccountsOfProfileAsync(await Authenticator.GetValidAuthData(), objProfile.ID);

                    // Create a query that, when executed, returns a collection of tasks.
                    IEnumerable<Task> getTeamProjectTasksQuery =
                        from account in cobjAccounts select GetTeamProjectsOfAccount(account);

                    // Use ToList to execute the query and start the tasks. 
                    List<Task> cobjGetTeamProjectTasks = getTeamProjectTasksQuery.ToList();

                    await Task.WhenAll(cobjGetTeamProjectTasks);

                    //raise event and results
                    OnProjectsLoaded(true);
                }
                catch
                {
                    //raise event and results
                    OnProjectsLoaded(false);
                }
            }
        }

        internal async Task<UserProfileData> GetLoggedInUser()
        {
            UserProfileData objProfile = await _tfsClient.GetLoggedInUserAsync(await Authenticator.GetValidAuthData());
            CurrentUser.LoggedInUser = new User(objProfile);
            return objProfile;
        }

        public async Task<bool> TestAlternativeCredentials()
        {
            // this does not work, cause we don't now the tfsaccount
            return
                await
                    _tfsClient.TestAlternativeCredentials(await Authenticator.GetValidBasicAuthData(),
                        Navigator.CurrentServer.ServerAddress, Navigator.CurrentServer.ProjectCollection);
            //CurrentUser.LoggedInUser = new User(objProfile);
            //return objProfile;
        }

        public async Task<bool> TestConnectionToCustomRest()
        {
            return await _tfsClient.TestConnectionToAlternativeRest(Context.AdditionalRestServer);
            //CurrentUser.LoggedInUser = new User(objProfile);
            //return objProfile;
        }

        private async Task GetTeamProjectsOfAccount(AccountData elem)
        {
            // get team projects
            ServerInfo objServerInfo = new ServerInfo(elem.ProjectAddress.ToString());
            var cobjTeamProjects =
                await
                    _tfsClient.GetTeamProjectsAsync(await Authenticator.GetValidAuthData(), objServerInfo.ServerAddress,
                        objServerInfo.ProjectCollection);

            // Create a query that, when executed, returns a collection of tasks.
            IEnumerable<Task<TeamProject>> getProjectDetailsTasksQuery =
                from project in cobjTeamProjects select GetDetailsOfTeamProjectAsync(elem, objServerInfo, project);

            // Use ToList to execute the query and start the tasks. 
            List<Task<TeamProject>> cobjProjectDetailsTasks = getProjectDetailsTasksQuery.ToList();

            // Add a loop to process the tasks one at a time until none remain.
            while (cobjProjectDetailsTasks.Count > 0)
            {
                // Identify the first task that completes.
                Task<TeamProject> firstFinishedTask = await Task.WhenAny(cobjProjectDetailsTasks);

                // Remove the selected task from the list so that you don't
                // process it more than once.
                cobjProjectDetailsTasks.Remove(firstFinishedTask);

                // Await the completed task.
                TeamProject teamProject = await firstFinishedTask;
                TeamProjects.Add(teamProject);
            }
        }

        private async Task<TeamProject> GetDetailsOfTeamProjectAsync(AccountData elem, ServerInfo objServerInfo,
            ProjectData item)
        {
            item.Account = elem;
            TeamProject project = new TeamProject(item);

            // #### get team infos via webservice
            var cobjTeamInfos =
                await
                    _tfsClient.GetTeaminfosOfProjectAsync(await Authenticator.GetValidBasicAuthData(),
                        Context.AdditionalRestServer, objServerInfo.ServerAddress, objServerInfo.ProjectCollection,
                        item.Name);

            if (cobjTeamInfos != null)
            {
                foreach (var team in cobjTeamInfos)
                {
                    IEnumerable<PersonData> cobjTeamMembers = null;
                    // #### get team members
                    try
                    {
                        cobjTeamMembers =
                            await
                                _tfsClient.GetMembersOfTeamAsync(await Authenticator.GetValidAuthData(),
                                    await Authenticator.GetValidBasicAuthData(), objServerInfo.ServerAddress,
                                    objServerInfo.ProjectCollection, item.Name, team.TeamName);
                        foreach (var member in cobjTeamMembers)
                        {
                            member.ImagePath = await SaveMemberImageToFile(member);
                        }
                    }
                    catch
                    {
                    }

                    if (team.IsDefaultTeam)
                    {
                        // Add default team at first position!
                        project.Teams.Insert(0, new Team(team, cobjTeamMembers));
                    }
                    else
                    {
                        project.Teams.Add(new Team(team, cobjTeamMembers));
                    }
                }
            }

            // #### set default team
            Team defaultTeam = project.Teams.FirstOrDefault(x => x.IsDefaultTeam);
            project.SwitchTeam(defaultTeam);

            return project;
        }

        private async Task<string> SaveMemberImageToFile(PersonData person)
        {
            var destinationFolder = ApplicationData.Current.LocalFolder;
            var pictureFilename = person.ID + ".png";

            StorageFile file =
                await destinationFolder.CreateFileAsync(pictureFilename, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteBytesAsync(file, person.ImageBytes);

            return file.Path;
        }

        public async void LoadDashboardDataAsync()
        {
            // fire loading events
            OnSprintDataLoading();

            // this is needed to avoid concurrency problems when reloading all data with invalid token
            await Authenticator.GetValidAuthData();

            // clean collections
            LatestBuildEvents.Clear();
            LatestCheckinEvents.Clear();
            Sprint.Reset();

            // get burndown data
            LoadSprintData();

            // get latest builds
            LoadLatestBuilds();

            // get latest checkins
            LoadLatestCheckins();

            // we need the current sprint workitems to react on workitem changes
            LoadTaskboardDataAsync();
        }

        public async void LoadSprintData()
        {
            SprintData currentSprint = null;
            try
            {
                var sprints =
                    await
                        _tfsClient.GetSprintsOfProjectAsync(await Authenticator.GetValidBasicAuthData(),
                            Context.AdditionalRestServer, Navigator.CurrentServer.ServerAddress,
                            Navigator.CurrentServer.ProjectCollection, Navigator.CurrentTeamProject.ProjectName);
                if (sprints != null)
                {
                    currentSprint =
                        sprints.FirstOrDefault(
                            x => x.Path == Navigator.CurrentTeamProject.CurrentTeam.CurrentIterationPath);
                    if (currentSprint != null)
                    {
                        Sprint.SetSprintData(currentSprint);
                    }
                }
            }
            catch
            {
            }

            LoadBurndownDataForCurrentSprintAsync(currentSprint);
        }

        private async void LoadLatestCheckins()
        {
            try
            {
                switch (Navigator.CurrentTeamProject.VersionControl)
                {
                    case VersionControl.Tfvc:

                        var cobjLatestCheckins =
                            await
                                _tfsClient.GetLatestCheckinsAsync(await Authenticator.GetValidAuthData(),
                                    Navigator.CurrentServer.ServerAddress, Navigator.CurrentServer.ProjectCollection,
                                    Navigator.CurrentTeamProject.ProjectName, 4);
                        foreach (var elem in cobjLatestCheckins)
                        {
                            LatestCheckinEvents.Add(new CheckinEventItem(elem));
                        }

                        OnCheckinsLoaded(true, LatestCheckinEvents);

                        break;
                    case VersionControl.Git:
                        OnCheckinsLoaded(false, null);
                        break;
                    default:
                        OnCheckinsLoaded(false, null);
                        break;
                }
            }
            catch
            {
                // fire event for UI
                OnCheckinsLoaded(false, null);
            }
        }

        private async void LoadLatestBuilds()
        {
            try
            {
                var cobjlatestBuildModels =
                    await
                        _tfsClient.GetLatestBuildsAsync(await Authenticator.GetValidAuthData(),
                            Navigator.CurrentServer.ServerAddress, Navigator.CurrentServer.ProjectCollection,
                            Navigator.CurrentTeamProject.ProjectName, 4);
                foreach (var elem in cobjlatestBuildModels)
                {
                    LatestBuildEvents.Add(new BuildEventItem(elem));
                }

                OnBuildsLoaded(true);
            }
            catch
            {
                // fire event for UI
                OnBuildsLoaded(false);
            }
        }

        private async void LoadBurndownDataForCurrentSprintAsync(SprintData sprint)
        {
            if (sprint != null)
            {
                try
                {
                    if (sprint.StartDate.HasValue && sprint.EndDate.HasValue)
                    {
                        var cobjBurnDownData =
                            await
                                _tfsClient.GetBurndownDataAsync(await Authenticator.GetValidAuthData(),
                                    Navigator.CurrentServer.ServerAddress, Navigator.CurrentServer.ProjectCollection,
                                    Navigator.CurrentTeamProject.ProjectName, sprint,
                                    Navigator.CurrentTeamProject.CurrentTeam.Areas);

                        if (cobjBurnDownData != null)
                        {
                            Sprint.BurndownData = new ObservableCollection<BurnDownPointData>(cobjBurnDownData);
                        }
                        else
                        {
                            Sprint.BurndownData = null;
                        }

                        Sprint.CalculateTrendlines();

                        OnSprintDataLoaded(true, Sprint);
                    }
                }
                catch
                {
                    OnSprintDataLoaded(false, Sprint);
                }
            }
            else
            {
                OnSprintDataLoaded(true, null);
            }
        }

        public async void UpdateBurndownDataForCurrentSprintAsync(DateTime updateSince)
        {
            if (updateSince > Sprint.EndDateTime)
            {
                // Do nothing
                return;
            }

            // update data for all days that need to be updated
            var cobjBurnDownData =
                await
                    _tfsClient.GetBurndownDataAsync(await Authenticator.GetValidAuthData(),
                        Navigator.CurrentServer.ServerAddress, Navigator.CurrentServer.ProjectCollection,
                        Navigator.CurrentTeamProject.ProjectName, Sprint.SprintPath, updateSince, DateTime.Now.Date,
                        Navigator.CurrentTeamProject.CurrentTeam.Areas);

            if (cobjBurnDownData != null)
            {
                // update burndowndata objects
                foreach (BurnDownPointData item in cobjBurnDownData)
                {
                    BurnDownPointData origin = Sprint.BurndownData.FirstOrDefault(p => p.Date == item.Date);

                    if (origin != null)
                    {
                        //update original remaining work   
                        origin.RemainingWork = item.RemainingWork;
                    }
                }

                // Recalculate trendlines
                Sprint.CalculateTrendlines();

                // Call on property changed methods for burndowndata
                Sprint.RefreshBurndownData();
            }
        }

        public async Task LoadTaskboardDataAsync()
        {
            try
            {
                WorkItemTree.Clear();
                WorkItemCollection.Clear();

                // this is needed to avoid concurrency problems when reloading all data with invalid token
                await Authenticator.GetValidAuthData();

                // get workitems
                var cobjWorkItems =
                    await
                        _tfsClient.GetTaskboardItemsOfProjectAsync(await Authenticator.GetValidAuthData(),
                            Navigator.CurrentServer.ServerAddress, Navigator.CurrentServer.ProjectCollection,
                            Navigator.CurrentTeamProject.ProjectName, Navigator.CurrentTeamProject.CurrentTeam.Areas,
                            Navigator.CurrentTeamProject.CurrentTeam.CurrentIterationPath);

                // build workitem collection from tree
                // to have every workitem object only created one time!
                AddToWorkItemCollection(cobjWorkItems);

                // add only workitems without a parent to the top level tree
                WorkItemTree.RefreshCollection(WorkItemCollection.Where(n => n.ParentWorkItem == null));

                //raise event and results
                OnTaskboardDataLoaded(true, WorkItemTree);
            }
            catch
            {
                OnTaskboardDataLoaded(false, null);
            }
        }

        internal async Task SyncWIChangesToDataAsync(DateTime lastUpdateRequest)
        {
            try
            {
                var wiChanges =
                    await
                        _tfsClient.GetWorkitemUpdatesAsync(Context.AdditionalRestServer, Navigator.CurrentTeamProject.Id,
                            lastUpdateRequest);

                bool isBurndownUpdateNeeded = false;
                // Update Data!
                foreach (var change in wiChanges)
                {
                    // set last update time
                    LastUpdateRequest = DateTime.UtcNow;

                    // find item in collection
                    WorkItem item = WorkItemCollection.FirstOrDefault(p => p.ID == change.WorkitemID);

                    // only update if current revision is different then the one in change
                    // Because we also get the changes done by the app here
                    int newRevision = Convert.ToInt32(change.Changes.First(p => p.FieldName == "System.Rev").NewValue);
                    if (newRevision > item.Revision)
                    {
                        // Update properties of Workitem
                        item.UpdateProperties(change.Changes);

                        // if the remaining work has changed we need to update the burndown data
                        if (
                            change.Changes.FirstOrDefault(p => p.FieldName == "Microsoft.VSTS.Scheduling.RemainingWork") !=
                            null)
                        {
                            isBurndownUpdateNeeded = true;
                        }
                    }
                }

                // update burndown date if neccessary
                if (isBurndownUpdateNeeded && Sprint.BurndownData != null)
                {
                    UpdateBurndownDataForCurrentSprintAsync(lastUpdateRequest.Date);
                }
            }
            catch
            {
            }
        }


        private void AddToWorkItemCollection(IEnumerable<WorkItemData> tree)
        {
            foreach (WorkItemData item in tree)
            {
                if (item == null)
                {
                    continue;
                }
                WorkItem objItem = item.ConvertToWorkitem();

                if (objItem != null)
                {
                    // add workitem itself
                    WorkItemCollection.Add(objItem);

                    // add children of workitem
                    AddToWorkItemCollection(item.ChildWorkItems);
                }
            }
        }

        public async Task<WorkItem> SetAssignedToOfWI(WorkItemData item, string memberFullName)
        {
            WorkItemData wiData =
                await
                    _tfsClient.SetAssignedToOfWI(item, memberFullName, await Authenticator.GetValidAuthData(),
                        Navigator.CurrentServer.ServerAddress, Navigator.CurrentServer.ProjectCollection);

            if (wiData != null)
            {
                return wiData.ConvertToWorkitem();
            }

            return null;
        }

        public async Task<WorkItem> SetRemainingWorkOfWI(WorkItemData item, double newRemainingWork)
        {
            WorkItemData wiData =
                await
                    _tfsClient.SetRemainingWorkOfWI(item, newRemainingWork, await Authenticator.GetValidAuthData(),
                        Navigator.CurrentServer.ServerAddress, Navigator.CurrentServer.ProjectCollection);

            if (wiData != null)
            {
                return wiData.ConvertToWorkitem();
            }

            return null;
        }

        public async Task<WorkItem> SetStateOfTask(WorkItemData item, TaskState newState,
            double? newRemainingWork = null)
        {
            WorkItemData wiData =
                await
                    _tfsClient.SetStateOfTask(item, newState, newRemainingWork, await Authenticator.GetValidAuthData(),
                        Navigator.CurrentServer.ServerAddress, Navigator.CurrentServer.ProjectCollection);

            if (wiData != null)
            {
                return wiData.ConvertToWorkitem();
            }

            return null;
        }

        private async void CheckForWorkItemUpdates(object sender, object e)
        {
            await SyncWIChangesToDataAsync(LastUpdateRequest);
        }
    }
}