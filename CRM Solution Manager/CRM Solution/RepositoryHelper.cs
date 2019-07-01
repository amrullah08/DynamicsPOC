﻿//-----------------------------------------------------------------------
// <copyright file="RepositoryHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Repository helper
    /// </summary>
    public class RepositoryHelper
    {
        /// <summary>
        /// Method tries to update repository
        /// </summary>
        /// <param name="solutionUniqueName">unique solution name</param>
        /// <param name="committerName">committer name</param>
        /// <param name="committerEmail">committer email</param>
        /// <param name="authorEmail">author email</param>
        public void TryUpdateToRepository(string solutionUniqueName, string committerName, string committerEmail, string authorEmail)
        {
            string solutionFilePath = string.Empty;
            ICrmSolutionHelper crmSolutionHelper = new CrmSolutionHelper(
                            Singleton.RepositoryConfigurationConstantsInstance.RepositoryUrl,
                            Singleton.RepositoryConfigurationConstantsInstance.BranchName,
                            Singleton.RepositoryConfigurationConstantsInstance.RepositoryRemoteName,
                            Singleton.CrmConstantsInstance.OrgServiceUrl,
                            Singleton.CrmConstantsInstance.DynamicsUserName,
                            Singleton.CrmConstantsInstance.DynamicsPassword,
                            Singleton.CrmConstantsInstance.SolutionPackagerPath);

            int timeOut = Convert.ToInt32(Singleton.CrmConstantsInstance.SleepTimeoutInMillis);

            // while (true)
            {
                HashSet<string> hashSet = new HashSet<string>();

                try
                {
                    var solutionFiles = crmSolutionHelper.DownloadSolutionFile(solutionUniqueName);

                    if (!crmSolutionHelper.CanPush)
                    {
                        System.Threading.Thread.Sleep(timeOut);

                        // continue;
                    }

                    solutionFilePath = Singleton.RepositoryConfigurationConstantsInstance.LocalDirectory + "solutions.txt";

                    // todo: enable solutions file clear from crm portal
                    this.PopulateHashset(solutionFilePath, new HashSet<string>());
                    GitDeploy.GitRepositoryManager gitRepositoryManager = this.GetRepositoryManager(committerName, committerEmail, authorEmail, solutionFiles[0]);

                    foreach (var solutionFile in solutionFiles)
                    {
                        if (solutionFile.CheckInSolution)
                        {
                            this.TryPushToRepository(committerName, committerEmail, authorEmail, solutionFile, solutionFilePath, hashSet, gitRepositoryManager);
                            ////Singleton.SolutionFileInfoInstance.UploadFiletoDynamics(Singleton.CrmConstantsInstance.ServiceProxy, solutionFile.Solution);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                    System.Threading.Thread.Sleep(timeOut);
                }

                System.Threading.Thread.Sleep(timeOut);
            }
        }

        /// <summary>
        /// Method gets repository manager instance
        /// </summary>
        /// <param name="committerName">committer name</param>
        /// <param name="committerEmail">committer email</param>
        /// <param name="authorEmail">author email</param>
        /// <param name="solutionFile">solution file info</param>
        /// <returns>returns repository manager</returns>
        private GitDeploy.GitRepositoryManager GetRepositoryManager(string committerName, string committerEmail, string authorEmail, SolutionFileInfo solutionFile)
        {
            return new GitDeploy.GitRepositoryManager(
                                                    Singleton.RepositoryConfigurationConstantsInstance.GitUserName,
                                                    Singleton.RepositoryConfigurationConstantsInstance.GitUserPassword,
                                                    Singleton.RepositoryConfigurationConstantsInstance.RepositoryUrl,
                                                    Singleton.RepositoryConfigurationConstantsInstance.RepositoryRemoteName,
                                                    Singleton.RepositoryConfigurationConstantsInstance.BranchName,
                                                    Convert.ToBoolean(Singleton.RepositoryConfigurationConstantsInstance.CloneRepositoryAlways),
                                                    Singleton.RepositoryConfigurationConstantsInstance.LocalDirectory,
                                                    Singleton.RepositoryConfigurationConstantsInstance.JsDirectory,
                                                    Singleton.RepositoryConfigurationConstantsInstance.HtmlDirectory,
                                                    Singleton.RepositoryConfigurationConstantsInstance.ImagesDirectory,
                                                    Singleton.RepositoryConfigurationConstantsInstance.SolutionFolder,
                                                    solutionFile.OwnerName ?? committerName,
                                                    authorEmail,
                                                    committerName,
                                                    committerEmail);
        }

        /// <summary>
        /// Method populates hash set from source control release file
        /// </summary>
        /// <param name="solutionFilePath">path of file that contains list of solution to be released</param>
        /// <param name="hashSet">hash set to store release solution</param>
        private void PopulateHashset(string solutionFilePath, HashSet<string> hashSet)
        {
            try
            {
                if (File.Exists(solutionFilePath))
                {
                    string[] lines = File.ReadAllLines(solutionFilePath);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            hashSet.Add(line);
                        }
                    }
                }
                else
                {
                    File.Create(solutionFilePath).Dispose();
                }
            }
            catch (Exception ex)
            {
                Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine(".. " + ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// method saves Hash set to the specified file
        /// </summary>
        /// <param name="solutionFilePath">path of file that contains list of solution to be released</param>
        /// <param name="hashSet">hash set to store release solution</param>
        private void SaveHashSet(string solutionFilePath, HashSet<string> hashSet)
        {
            File.WriteAllText(solutionFilePath, string.Empty);
            File.WriteAllLines(solutionFilePath, hashSet.Where(cc => !string.IsNullOrEmpty(cc)).ToArray());
        }

        /// <summary>
        /// method tries to push to repository
        /// </summary>
        /// <param name="committerName">committer name</param>
        /// <param name="committerEmail">committer detail</param>
        /// <param name="authorEmail">author email id</param>
        /// <param name="solutionFile">solution file info</param>
        /// <param name="solutionFilePath">path of file that contains list of solution to be released</param>
        /// <param name="hashSet">hash set to store release solution</param>
        /// <param name="gitRepositoryManager">object initialization for class gitRepositoryManager</param>
        private void TryPushToRepository(
                                                string committerName,
                                                string committerEmail,
                                                string authorEmail,
                                                SolutionFileInfo solutionFile,
                                                string solutionFilePath,
                                                HashSet<string> hashSet,
                                                GitDeploy.GitRepositoryManager gitRepositoryManager)
        {
            ////RepositoryConfigurationConstants.ResetLocalDirectory();

            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemPushingToStatus;
            solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.webJobs();
            solutionFile.Update();

            ////GitDeploy.GitRepositoryManager gitRepositoryManager = GetRepositoryManager(committerName, committerEmail, authorEmail, solutionFile);

            gitRepositoryManager.UpdateRepository();

            ////433710000 value for Yes
            if (solutionFile.SolutionsTxt == 433710000 && File.Exists(solutionFilePath))
            {
                File.WriteAllText(solutionFilePath, string.Empty);
                hashSet.Clear();
            }

            this.PopulateHashset(solutionFilePath, hashSet);

            if (!hashSet.Contains(solutionFile.SolutionFileZipName) && solutionFile.IncludeInRelease)
            {
                hashSet.Add(solutionFile.SolutionFileZipName);
            }

            this.SaveHashSet(solutionFilePath, hashSet);
            gitRepositoryManager.CommitAllChanges(solutionFile, solutionFilePath);

            gitRepositoryManager.PushCommits();

            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemPushToRepositorySuccessStatus;
            solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.webJobs();
            solutionFile.Update();
        }
    }
}