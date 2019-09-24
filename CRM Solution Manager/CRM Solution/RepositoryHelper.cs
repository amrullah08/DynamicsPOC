//-----------------------------------------------------------------------
// <copyright file="RepositoryHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;
    using Microsoft.VisualStudio.Services.Common;

    /// <summary>
    /// Repository helper
    /// </summary>
    public class RepositoryHelper
    {
        /// <summary>
        /// Method tries to configure repository
        /// </summary>
        /// <param name="solutionFiles">solution files</param>
        /// <param name="committerName">committer name</param>
        /// <param name="committerEmail">committer email</param>
        /// <param name="authorEmail">author email</param>
        /// <param name="solutionFilePath">solution file path</param>
        /// <returns> returns repository manager</returns>
        public GitDeploy.GitRepositoryManager ConfigureRepository(SolutionFileInfo solutionFiles, string committerName, string committerEmail, string authorEmail, string solutionFilePath)
        {
            int timeOut = Convert.ToInt32(Singleton.CrmConstantsInstance.SleepTimeoutInMillis);
            GitDeploy.GitRepositoryManager gitRepositoryManager = null;
            try
            {
                // todo: enable solutions file clear from crm portal
                this.PopulateHashset(solutionFilePath, new HashSet<string>());
                gitRepositoryManager = this.GetRepositoryManager(committerName, committerEmail, authorEmail, solutionFiles);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                solutionFiles.Solution[Constants.SourceControlQueueAttributeNameForStatus] = "Error +" + ex.Message;
                solutionFiles.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
                solutionFiles.Update();
                Singleton.SolutionFileInfoInstance.UploadFiletoDynamics(Singleton.CrmConstantsInstance.ServiceProxy, solutionFiles.Solution);
                System.Threading.Thread.Sleep(timeOut);
            }

            return gitRepositoryManager;
        }

        /// <summary>
        /// Method tries to push it to repository
        /// </summary>
        /// <param name="solutionFiles">solution files</param>
        /// <param name="committerName">committer name</param>
        /// <param name="committerEmail">committer email</param>
        /// <param name="authorEmail">author email</param>
        /// <param name="solutionFilePath">solution file path</param>
        /// <param name="gitRepositoryManager"> repository manager</param>
        public void PushRepository(List<SolutionFileInfo> solutionFiles, string committerName, string committerEmail, string authorEmail, string solutionFilePath, GitDeploy.GitRepositoryManager gitRepositoryManager)
        {
            foreach (var solutionFile in solutionFiles)
            {
                try
                {
                    if (solutionFile.CheckInSolution)
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Clear();
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append(solutionFile.Solution.GetAttributeValue<string>("syed_webjobs"));
                        HashSet<string> hashSet = new HashSet<string>();
                        this.TryPushToRepository(committerName, committerEmail, authorEmail, solutionFile, solutionFilePath, hashSet, gitRepositoryManager);
                        Singleton.SolutionFileInfoInstance.UploadFiletoDynamics(Singleton.CrmConstantsInstance.ServiceProxy, solutionFile.Solution);
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                    solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = "Error +" + ex.Message;
                    solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
                    solutionFile.Update();
                    Singleton.SolutionFileInfoInstance.UploadFiletoDynamics(Singleton.CrmConstantsInstance.ServiceProxy, solutionFile.Solution);
                }
            }
        }

        /// <summary>
        /// Method tries to update repository
        /// </summary>
        /// <param name="solutionUniqueName">unique solution name</param>
        /// <param name="committerName">committer name</param>
        /// <param name="committerEmail">committer email</param>
        /// <param name="authorEmail">author email</param>
        public void TryUpdateToRepository(string solutionUniqueName, string committerName, string committerEmail, string authorEmail, string mode)
        {
            try
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
                var solutionFiles = crmSolutionHelper.DownloadSolutionFile(solutionUniqueName, mode);
                if (!crmSolutionHelper.CanPush)
                {
                    System.Threading.Thread.Sleep(timeOut);
                    //// continue;
                }

                if (solutionFiles.Count > 0)
                {
                    solutionFilePath = Singleton.RepositoryConfigurationConstantsInstance.LocalDirectory + "solutions.txt";

                    GitDeploy.GitRepositoryManager gitRepositoryManager = this.ConfigureRepository(solutionFiles[0], committerName, committerEmail, authorEmail, solutionFilePath);
                    this.PushRepository(solutionFiles, committerName, committerEmail, authorEmail, solutionFilePath, gitRepositoryManager);
                    System.Threading.Thread.Sleep(timeOut);
                }
            }
            catch (Exception ex)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(ex.Message);
                Console.WriteLine(ex.Message);
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
                                                     Singleton.RepositoryConfigurationConstantsInstance.TFSUserName,
                                                    Singleton.RepositoryConfigurationConstantsInstance.TFSPassword,
                                                 solutionFile.GitRepoUrl ?? Singleton.RepositoryConfigurationConstantsInstance.RepositoryUrl,
                                                    solutionFile.RemoteName ?? Singleton.RepositoryConfigurationConstantsInstance.RepositoryRemoteName,
                                                   solutionFile.BranchName ?? Singleton.RepositoryConfigurationConstantsInstance.BranchName,
                                                    Convert.ToBoolean(Singleton.RepositoryConfigurationConstantsInstance.CloneRepositoryAlways),
                                                    Singleton.RepositoryConfigurationConstantsInstance.LocalDirectory,
                                         solutionFile.RepoJSFolder != null ? Singleton.RepositoryConfigurationConstantsInstance.LocalDirectory + solutionFile.RepoJSFolder : Singleton.RepositoryConfigurationConstantsInstance.JsDirectory,
                                               solutionFile.RepoHTMLFolder != null ? Singleton.RepositoryConfigurationConstantsInstance.LocalDirectory + solutionFile.RepoHTMLFolder : Singleton.RepositoryConfigurationConstantsInstance.HtmlDirectory,
                                             solutionFile.RepoImagesFolder != null ? Singleton.RepositoryConfigurationConstantsInstance.LocalDirectory + solutionFile.RepoImagesFolder : Singleton.RepositoryConfigurationConstantsInstance.ImagesDirectory,
                                             solutionFile.RepoSolutionFolder != null ? Singleton.RepositoryConfigurationConstantsInstance.LocalDirectory + solutionFile.RepoSolutionFolder : Singleton.RepositoryConfigurationConstantsInstance.SolutionFolder,
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
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message);
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
        /// <param name="gitRepositoryManager">object initialization for class RepositoryManager</param>
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

            string solutionCheckerPath = string.Empty;
            string timeTriggerPath = string.Empty;

            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemPushingToStatus;
            solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
            solutionFile.Update();
            Workspace workspace = null;
            ////GitDeploy.GitRepositoryManager gitRepositoryManager = GetRepositoryManager(committerName, committerEmail, authorEmail, solutionFile);

            if (solutionFile.Repository != Constants.SourceControlAzureDevOpsServer)
            {
                gitRepositoryManager.UpdateRepository();
            }

            if (solutionFile.Repository == Constants.SourceControlAzureDevOpsServer)
            {
                gitRepositoryManager.ConnectTFSMap(solutionFile, solutionFilePath, hashSet);
            }
            else
            {
                ////433710000 value for Yes
                if (solutionFile.CheckInSolution == true && solutionFile.IncludeInRelease == true)
                {
                    solutionFilePath = Singleton.RepositoryConfigurationConstantsInstance.SolutionTextRelease;
                    solutionCheckerPath = Singleton.RepositoryConfigurationConstantsInstance.SolutionCheckerPath;
                    timeTriggerPath = Singleton.RepositoryConfigurationConstantsInstance.TimeTriggerPath;

                    if (solutionFile.SolutionsTxt == 433710000 && File.Exists(solutionFilePath))
                    {
                        File.WriteAllText(solutionCheckerPath, string.Empty);
                        File.WriteAllText(timeTriggerPath, string.Empty);
                    }
                    File.WriteAllText(solutionCheckerPath, solutionFile.Solution.Id.ToString());
                    File.WriteAllText(timeTriggerPath, DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                }
                else if (solutionFile.CheckInSolution == true && solutionFile.IncludeInRelease == false)
                {
                    solutionFilePath = Singleton.RepositoryConfigurationConstantsInstance.SolutionText;
                }

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

                gitRepositoryManager.CommitAllChanges(solutionFile, solutionFilePath, null, solutionCheckerPath,timeTriggerPath);
                gitRepositoryManager.PushCommits();
            }



            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemPushToRepositorySuccessStatus;
            solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
            solutionFile.Update();
        }
    }
}
