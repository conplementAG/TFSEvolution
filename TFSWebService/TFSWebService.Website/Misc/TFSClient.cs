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
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;
using Microsoft.TeamFoundation.Server;

namespace TFSWebService
{
    public static class TFSClient
    {
        #region Sprints

        /// <summary>
        ///     Gets all sprints defined for the given project.
        /// </summary>
        /// <param name="tfsuri">Url of the TFS.</param>
        /// <param name="user">Username for accessing the TFS.</param>
        /// <param name="pwd">Password for the given user.</param>
        /// <param name="projectCollection">The project collection to project is part of.</param>
        /// <param name="project">Project name.</param>
        /// <returns>A list of Sprint objects.</returns>
        internal static IEnumerable<Sprint> GetSprintsByProject(string tfsuri, string user, string pwd,
            string projectCollection, string project)
        {
            try
            {
                TfsTeamProjectCollection tfs =
                    TfsTeamProjectCollectionFactory.GetTeamProjectCollection(
                        new Uri(string.Format("{0}{1}", tfsuri, projectCollection)));

                // authenticate
                Authenticate(tfs, user, pwd);

                // get sprints
                var sprints = GetSprints(tfs, project);
                return sprints;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Exception while loading sprint data", ex);
            }
        }

        /// <summary>
        ///     Get the sprints of the given project
        /// </summary>
        /// <param name="tfs">Team project collection.</param>
        /// <param name="project">Team project.</param>
        /// <returns>A list of sprints defined for the given team project.</returns>
        private static IEnumerable<Sprint> GetSprints(TfsTeamProjectCollection tfs, string project)
        {
            ICommonStructureService4 css = tfs.GetService<ICommonStructureService4>();
            ProjectInfo projectInfo = css.GetProjectFromName(project);

            // Get iterations defined for the team project 
            var iterations = css.GetIterations(projectInfo.Uri);

            // extract only those Iterations that are Sprints (second level of iteration)
            var sprints = GetSprintsOutOfIterations(iterations);
            return sprints;
        }

        /// <summary>
        ///     Gets the sprints out of a list of iterations.
        /// </summary>
        /// <param name="iterations">The list of iterations.</param>
        /// <returns>The list of sprints.</returns>
        private static IEnumerable<Sprint> GetSprintsOutOfIterations(IEnumerable<Iteration> iterations)
        {
            List<Sprint> cobjSprints = new List<Sprint>();
            foreach (Iteration iteration in iterations)
            {
                if (iteration.Path.Count(f => f == '\\') == 2)
                {
                    cobjSprints.Add(new Sprint(iteration));
                }
            }

            return cobjSprints;
        }

        #endregion

        #region Team informations

        /// <summary>
        ///     Gets informations about all teams of the project like name, current iteration, areas, etc.
        /// </summary>
        /// <param name="tfsuri">Url of the TFS.</param>
        /// <param name="user">Username for accessing the TFS.</param>
        /// <param name="pwd">Password for the given user.</param>
        /// <param name="projectCollection">The project collection to project is part of.</param>
        /// <param name="project">Project name.</param>
        /// <returns>The list of TeamInfo objects, containing one team information for each team.</returns>
        internal static IEnumerable<TeamInfo> GetTeamInformationsByProject(string tfsuri, string user, string pwd,
            string projectCollection, string project)
        {
            try
            {
                TfsTeamProjectCollection tfs =
                    TfsTeamProjectCollectionFactory.GetTeamProjectCollection(
                        new Uri(string.Format("{0}{1}", tfsuri, projectCollection)));

                // authenticate
                Authenticate(tfs, user, pwd);

                var teamInfos = GetTeamInformations(tfs, project);
                return teamInfos;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Exception while loading team info", ex);
            }
        }

        private static IEnumerable<TeamInfo> GetTeamInformations(TfsTeamProjectCollection tfs, string projectName)
        {
            List<TeamInfo> teams = new List<TeamInfo>();

            ICommonStructureService4 css = tfs.GetService<ICommonStructureService4>();
            ProjectInfo project = css.GetProjectFromName(projectName);

            var configSvc = tfs.GetService<TeamSettingsConfigurationService>();
            var configs = configSvc.GetTeamConfigurationsForUser(new[] {project.Uri});

            // iterate over team configurations
            foreach (TeamConfiguration config in configs)
            {
                TeamInfo teamInfo = new TeamInfo();

                // get team basic informations
                teamInfo.TeamName = config.TeamName;
                teamInfo.TeamId = config.TeamId;
                teamInfo.IsDefaultTeam = config.IsDefaultTeam;

                // Access the actual configuration settings.
                TeamSettings ts = config.TeamSettings;

                // get iteratin informations
                teamInfo.CurrentIterationInfo = ts.CurrentIterationPath;
                teamInfo.ActiveIterations = ts.IterationPaths;

                // get area paths
                teamInfo.Areas = new List<Area>();
                foreach (TeamFieldValue tfv in ts.TeamFieldValues)
                {
                    Area area = new Area();
                    area.Name = tfv.Value;
                    area.IncludeChildren = tfv.IncludeChildren;

                    teamInfo.Areas.Add(area);
                }

                teams.Add(teamInfo);
            }

            return teams;
        }

        #endregion

        /// <summary>
        ///     Authenticate the user with the given password at the team project collection
        /// </summary>
        /// <param name="tfs">Team project collection.</param>
        /// <param name="user">Username.</param>
        /// <param name="pwd">Password of the user.</param>
        private static void Authenticate(TfsTeamProjectCollection tfs, string user, string pwd)
        {
            NetworkCredential netCred = new NetworkCredential(user, pwd);
            BasicAuthCredential basicCred = new BasicAuthCredential(netCred);
            TfsClientCredentials tfsCred = new TfsClientCredentials(basicCred);
            tfsCred.AllowInteractive = false;

            tfs.ClientCredentials = tfsCred;
            tfs.Authenticate();
        }
    }
}