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
        public static void TryUpdateToRepository(string solutionUniqueName, string committerName, string committerEmail, string authorEmail)
        {
            ICrmSolutionHelper crmSolutionHelper = new CrmSolutionHelper(
                            RepositoryConfigurationConstants.RepositoryUrl,
                            RepositoryConfigurationConstants.BranchName,
                            CrmConstants.OrgServiceUrl,
                            CrmConstants.DynamicsUserName,
                            CrmConstants.DynamicsPassword,
                            CrmConstants.SolutionPackagerPath);

            int timeOut = Convert.ToInt32(CrmConstants.SleepTimeoutInMillis);
            
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

                    ////if (!Directory.Exists(ConfigurationManager.AppSettings["RepositoryLocalDirectory"]))
                    ////{
                    ////    Console.WriteLine("Repository local directory doesnt exists " + ConfigurationManager.AppSettings["RepositoryLocalDirectory"]);
                    ////}
                    ////else
                    {
                        string solutionFilePath = RepositoryConfigurationConstants.LocalDirectory + "solutions.txt";

                        // todo: enable solutions file clear from crm portal
                        PopulateHashset(solutionFilePath, new HashSet<string>());
                        foreach (var solutionFile in solutionFiles)
                        {
                            if (solutionFile.CheckInSolution)
                            {
                                TryPushToRepository(committerName, committerEmail, authorEmail, solutionFile, solutionFilePath, hashSet);
                            }
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
        private static GitDeploy.GitRepositoryManager GetRepositoryManager(string committerName, string committerEmail, string authorEmail, SolutionFileInfo solutionFile)
        {
            return new GitDeploy.GitRepositoryManager(
                                                    RepositoryConfigurationConstants.GitUserName,
                                                    RepositoryConfigurationConstants.GitUserPassword,
                                                    RepositoryConfigurationConstants.RepositoryUrl,
                                                    RepositoryConfigurationConstants.RepositoryRemoteName,
                                                    solutionFile.BranchName ?? RepositoryConfigurationConstants.BranchName,
                                                    Convert.ToBoolean(RepositoryConfigurationConstants.CloneRepositoryAlways),
                                                    RepositoryConfigurationConstants.LocalDirectory,
                                                    RepositoryConfigurationConstants.JsDirectory,
                                                    RepositoryConfigurationConstants.HtmlDirectory,
                                                    RepositoryConfigurationConstants.ImagesDirectory,
                                                    RepositoryConfigurationConstants.SolutionFolder,
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
        private static void PopulateHashset(string solutionFilePath, HashSet<string> hashSet)
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
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// method saves Hash set to the specified file
        /// </summary>
        /// <param name="solutionFilePath">path of file that contains list of solution to be released</param>
        /// <param name="hashSet">hash set to store release solution</param>
        private static void SaveHashSet(string solutionFilePath, HashSet<string> hashSet)
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
        private static void TryPushToRepository(
                                                string committerName, 
                                                string committerEmail, 
                                                string authorEmail,
                                                SolutionFileInfo solutionFile, 
                                                string solutionFilePath, 
                                                HashSet<string> hashSet)
        {
            RepositoryConfigurationConstants.ResetLocalDirectory();

            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemPushingToStatus;
            solutionFile.Update();

            GitDeploy.GitRepositoryManager gitRepositoryManager = GetRepositoryManager(committerName, committerEmail, authorEmail, solutionFile);

            gitRepositoryManager.UpdateRepository();

            PopulateHashset(solutionFilePath, hashSet);

            if (!hashSet.Contains(solutionFile.SolutionFileZipName) && solutionFile.IncludeInRelease)
            {
                hashSet.Add(solutionFile.SolutionFileZipName);
            }

            SaveHashSet(solutionFilePath, hashSet);
            gitRepositoryManager.CommitAllChanges(solutionFile, solutionFilePath);

            gitRepositoryManager.PushCommits();

            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemPushToRepositorySuccessStatus;
            solutionFile.Update();
        }
    }
}