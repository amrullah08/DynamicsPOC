//-----------------------------------------------------------------------
// <copyright file="RepositoryHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolutionLibrary
{
    using CrmSolutionLibrary.AzureDevopsAPIs.RestClient;
    using CrmSolutionLibrary.AzureDevopsAPIs.Schemas;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml;

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
        /// Method Initiate Request
        /// </summary>
        /// <param name="solutionUniqueName">unique solution name</param>
        /// <param name="committerName">committer name</param>
        /// <param name="committerEmail">committer email</param>
        /// <param name="authorEmail">author email</param>
        /// <param name="mode">mode for flow</param>
        /// public async void TryUpdateToRepository(string solutionUniqueName, string committerName, string committerEmail, string authorEmail, string mode)
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0018:Inline variable declaration", Justification = "<Pending>")]
        public async void InitiateRequest(string solutionUniqueName, string mode)
        {
            try
            {
                string solutionFilePath = string.Empty;
                List<SolutionFileInfo> solutionFiles = ProcessSourceControlRequest(solutionUniqueName, mode);
                if (solutionFiles.Count > 0)
                {
                    string credentials = ConfigurationManager.AppSettings["GitPassword"];

                    //todo: Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", ConfigurationManager.AppSettings["GitPassword"].ToString())));

                    credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", credentials)));
                    HashSet<string> hashSet = new HashSet<string>();
                    string solutionCheckerPath, timeTriggerPath;
                    CreateTempSupportingFiles(out solutionFilePath, out solutionCheckerPath, out timeTriggerPath);
                    try
                    {
                        foreach (var item in solutionFiles)
                        {
                            try
                            {
                                if (item.CheckInSolution)
                                {
                                    string branchName, RepoJSFolder, RepoHTMLFolder, RepoImagesFolder, RepoSolutionFolder, AzureDevopsBaseURL;
                                    ConfigureAzureDevOps(item, out branchName, out RepoJSFolder, out RepoHTMLFolder, out RepoImagesFolder, out RepoSolutionFolder, out AzureDevopsBaseURL);
                                    List<string> BranchesNames = await GetRepositoryDetails.GetBranches(credentials, AzureDevopsBaseURL);
                                    bool BrachExists = BranchesNames.Contains(branchName);
                                    if (!BrachExists)
                                    {
                                        //if invalid branch name given in configuration settings in CRM source instance
                                        Console.WriteLine("Invalid Branch Name" + Singleton.CrmConstantsInstance.BranchName);
                                        item.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.InvalidBranchName;
                                        item.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
                                        item.Update();
                                        return;
                                    }
                                    ChangeRequest changeRequest = new ChangeRequest
                                    {
                                        Comments = item.Message,
                                        SourceBranchName = branchName,
                                        CommitDate = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"),
                                        AutherName = item.OwnerName
                                    };
                                    string lastcomitid = await GetRepositoryDetails.GetLastCommitDetails(credentials, branchName, AzureDevopsBaseURL);
                                    changeRequest.Lastcomitid = lastcomitid;
                                    List<RequestDetails> RequestDetailslist = new List<RequestDetails>();
                                    //Adding solution files to repository 
                                    #region update solutions zip files and etc to repo
                                    RequestDetails requestDetail = await AddSolutionsBeforeCommit(credentials, item, branchName, RepoSolutionFolder, AzureDevopsBaseURL, RequestDetailslist);

                                    #endregion
                                    // Adding html,js,images from web resources folder of extracted solution files to repository.
                                    #region update webResources to repo
                                    requestDetail = await AddWebResourcesBeforeCommit(credentials, item, branchName, RepoJSFolder, RepoHTMLFolder, RepoImagesFolder, AzureDevopsBaseURL, RequestDetailslist, requestDetail);
                                    #endregion
                                    //Adding txt files to repository
                                    #region update txt files in repo
                                    requestDetail = UpdateAdditionalFilesBeforeCommit(solutionFilePath, hashSet, solutionCheckerPath, timeTriggerPath, item, RequestDetailslist);
                                    #endregion
                                    item.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemPushingToStatus;
                                    item.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
                                    item.Update();

                                    changeRequest.RequestDetails = RequestDetailslist;
                                    //Building file input payload to commit
                                    CommitObject cmObject = CreateCommit.FillCommitDetails(changeRequest);
                                    //Calling Api for Commiting changes to repository
                                    string responseContent = await CreateCommit.Commit(credentials, cmObject, new Uri(AzureDevopsBaseURL));
                                    try
                                    {
                                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(responseContent.ToString());
                                        JObject json = string.IsNullOrEmpty(responseContent) ? JObject.Parse(responseContent) : null;
                                        var commitURL = json?["commits"][0]["url"].ToString();
                                        item.UpdateNotes("Commit Url" + commitURL);
                                    }
                                    catch { }
                                    item.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemPushToRepositorySuccessStatus;
                                    item.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
                                    item.Update();
                                }
                            }
                            catch (Exception ex)
                            {
                                item.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.InvalidRepoConfiguration;
                                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(ex.Message);
                                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(ex.StackTrace);
                                item.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
                                Console.WriteLine("Error : " + ex.Message.ToString() + "Stack " + ex.StackTrace);
                                item.Update();
                            }
                        }
                        UpdateItemForRelease(solutionFiles);
                    }
                    catch (Exception ex)
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(ex.Message);
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(ex.StackTrace);
                        Console.WriteLine("Error : " + ex.Message.ToString() + "Stack " + ex.StackTrace);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        private void CreateTempSupportingFiles(out string solutionFilePath, out string solutionCheckerPath, out string timeTriggerPath)
        {
            string TempPath = Path.GetTempPath() + "DD365CICD";
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
            solutionFilePath = TempPath + "/solutions.txt";
            solutionCheckerPath = TempPath + "/config.txt";
            timeTriggerPath = TempPath + "/trigger.txt";
            CreateTempFiles(solutionFilePath, solutionCheckerPath, timeTriggerPath);
        }

        private static List<SolutionFileInfo> ProcessSourceControlRequest(string solutionUniqueName, string mode)
        {
            ICrmSolutionHelper crmSolutionHelper = new CrmSolutionHelper(
                            Singleton.RepositoryConfigurationConstantsInstance.RepositoryUrl,
                            Singleton.RepositoryConfigurationConstantsInstance.BranchName,
                            Singleton.RepositoryConfigurationConstantsInstance.RepositoryRemoteName,
                            Singleton.CrmConstantsInstance.OrgServiceUrl,
                            Singleton.CrmConstantsInstance.DynamicsUserName,
                            Singleton.CrmConstantsInstance.DynamicsPassword,
                            Singleton.CrmConstantsInstance.SolutionPackagerPath
                            );
            var solutionFiles = crmSolutionHelper.DownloadSolutionFile(solutionUniqueName, mode);
            return solutionFiles;
        }

        private RequestDetails UpdateAdditionalFilesBeforeCommit(string solutionFilePath, HashSet<string> hashSet, string solutionCheckerPath, string timeTriggerPath, SolutionFileInfo item, List<RequestDetails> RequestDetailslist)
        {
            RequestDetails requestDetail;
            ////433710000 value for Yes
            if (item.IncludeInRelease == true)
            {
                try
                {
                    File.WriteAllText(solutionCheckerPath, item.Solution.Id.ToString());
                    File.WriteAllText(timeTriggerPath, DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                }
                catch (Exception ex)
                {
                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message);
                    Console.WriteLine(ex.Message);
                }
            }
            if (item.SolutionsTxt == 433710000 && File.Exists(solutionFilePath))
            {
                File.WriteAllText(solutionFilePath, string.Empty);
                hashSet.Clear();
            }
            this.PopulateHashset(solutionFilePath, hashSet);
            if (!hashSet.Contains(item.SolutionFileZipName) && item.IncludeInRelease)
            {
                hashSet.Add(item.SolutionFileZipName);
            }
            this.SaveHashSet(solutionFilePath, hashSet);
            string FileDestinationPath = string.Empty;
            if (item.CheckInSolution == true && item.IncludeInRelease == false)
                FileDestinationPath = Singleton.CrmConstantsInstance.SolutionText.Replace("\\", "/");
            else
                FileDestinationPath = Singleton.CrmConstantsInstance.SolutionTextRelease.Replace("\\", "/");
            requestDetail = new RequestDetails(File.ReadAllText(solutionFilePath), "solutions.txt", FileDestinationPath, "edit", "rawtext");
            RequestDetailslist.Add(requestDetail);

            //if checkin is true and include in release is true then we will update trigger.txt,config.txt in repo release folder
            if (item.CheckInSolution == true && item.IncludeInRelease == true)
            {
                requestDetail = new RequestDetails(File.ReadAllText(timeTriggerPath), "trigger.txt", Singleton.CrmConstantsInstance.TimeTriggerPath.Replace("\\", "/"), "edit", "rawtext");
                RequestDetailslist.Add(requestDetail);

                requestDetail = new RequestDetails(File.ReadAllText(solutionCheckerPath), "config.txt", Singleton.CrmConstantsInstance.SolutionCheckerPath.Replace("\\", "/"), "edit", "rawtext");
                RequestDetailslist.Add(requestDetail);
            }

            return requestDetail;
        }

        private static async Task<RequestDetails> AddWebResourcesBeforeCommit(string credentials, SolutionFileInfo item, string branchName, string RepoJSFolder, string RepoHTMLFolder, string RepoImagesFolder, string AzureDevopsBaseURL, List<RequestDetails> RequestDetailslist, RequestDetails requestDetail)
        {
            string webResources = item.SolutionExtractionPath + "\\WebResources";

            //  string localFolder = Singleton.RepositoryConfigurationConstantsInstance.LocalDirectory;

            try
            {
                if (Directory.Exists(webResources))
                {
                    foreach (var dataFile in Directory.GetFiles(webResources, "*.data.xml", SearchOption.AllDirectories))
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(dataFile);
                        var webResourceType = xmlDoc.SelectSingleNode("//WebResource/WebResourceType").InnerText; // content type of files
                        string webResournceName = xmlDoc.SelectSingleNode("//WebResource/Name").InnerText;
                        string modifiedName = string.Empty;
                        string[] webList = Regex.Split(webResournceName, "/");
                        if (webList.Length != 0)
                        {
                            modifiedName = webList[webList.Length - 1];
                        }
                        if (string.IsNullOrEmpty(modifiedName))
                        {
                            modifiedName = webResournceName;
                        }
                        switch (webResourceType)
                        {
                            case "1":
                                if (!string.IsNullOrEmpty(RepoHTMLFolder))
                                {
                                    requestDetail = new RequestDetails(Convert.ToBase64String(File.ReadAllBytes(webResources + "\\" + webResournceName)), modifiedName, RepoHTMLFolder.Replace("\\", "/"), "", "base64Encoded");
                                    requestDetail.ChangeType = await GetRepositoryDetails.GetItemDetails(credentials, @"/" + requestDetail.FileDestinationPath, @"/" + requestDetail.FileDestinationPath + @"/" + requestDetail.FileName, AzureDevopsBaseURL, branchName);
                                    RequestDetailslist.Add(requestDetail);
                                }
                                break;

                            case "3":
                                if (!string.IsNullOrEmpty(RepoJSFolder))
                                {
                                    requestDetail = new RequestDetails(Convert.ToBase64String(File.ReadAllBytes(webResources + "\\" + webResournceName)), modifiedName, RepoJSFolder.Replace("\\", "/"), "", "base64Encoded");
                                    requestDetail.ChangeType = await GetRepositoryDetails.GetItemDetails(credentials, @"/" + requestDetail.FileDestinationPath, @"/" + requestDetail.FileDestinationPath + @"/" + requestDetail.FileName, AzureDevopsBaseURL, branchName);
                                    RequestDetailslist.Add(requestDetail);
                                }
                                break;

                            case "5":
                            case "6":
                                if (!string.IsNullOrEmpty(RepoImagesFolder))
                                {

                                    requestDetail = new RequestDetails(Convert.ToBase64String(File.ReadAllBytes(webResources + "\\" + webResournceName)), modifiedName, RepoImagesFolder.Replace("\\", "/"), "", "base64Encoded");
                                    requestDetail.ChangeType = await GetRepositoryDetails.GetItemDetails(credentials, @"/" + requestDetail.FileDestinationPath, @"/" + requestDetail.FileDestinationPath + @"/" + requestDetail.FileName, AzureDevopsBaseURL, branchName);
                                    RequestDetailslist.Add(requestDetail);
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                item.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.InvalidRepoConfiguration;
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(ex.Message);
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(ex.StackTrace);
                item.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
                Console.WriteLine("Error : " + ex.Message.ToString() + "Stack " + ex.StackTrace);
                item.Update();
            }

            return requestDetail;
        }

        private static async Task<RequestDetails> AddSolutionsBeforeCommit(string credentials, SolutionFileInfo item, string branchName, string RepoSolutionFolder, string AzureDevopsBaseURL, List<RequestDetails> RequestDetailslist)
        {
            string fileUnmanaged = string.Empty;
            string fileManaged = string.Empty;
            if (item.DoYouWantToCheckInSolutionZipFiles == true)
            {
                fileUnmanaged = item.SolutionFilePath + item.SolutionUniqueName + "_.zip";
                fileManaged = item.SolutionFilePathManaged + item.SolutionUniqueName + "_managed_.zip";
            }

            Console.WriteLine("Committing solutions");

            if (item.DoYouWantToCheckInSolutionZipFiles == true)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Copy managed" + "<br>");
                File.Copy(item.SolutionFilePathManaged, fileManaged, true);
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Copy unmanaged" + "<br>");
                File.Copy(item.SolutionFilePath, fileUnmanaged, true);
            }

            //unmanaged solution zip file
            RequestDetails requestDetail = new RequestDetails(Convert.ToBase64String(File.ReadAllBytes(fileUnmanaged)), item.SolutionUniqueName + "_.zip", RepoSolutionFolder.Replace("\\", "/"), "", "base64Encoded");
            requestDetail.ChangeType = await GetRepositoryDetails.GetItemDetails(credentials, @"/" + requestDetail.FileDestinationPath, @"/" + requestDetail.FileDestinationPath + @"/" + requestDetail.FileName, AzureDevopsBaseURL, branchName);
            RequestDetailslist.Add(requestDetail);

            //managed solution zip file
            requestDetail = new RequestDetails(Convert.ToBase64String(File.ReadAllBytes(fileManaged)), item.SolutionUniqueName + "_managed_.zip", RepoSolutionFolder.Replace("\\", "/"), "", "base64Encoded");
            requestDetail.ChangeType = await GetRepositoryDetails.GetItemDetails(credentials, @"/" + requestDetail.FileDestinationPath, @"/" + requestDetail.FileDestinationPath + @"/" + requestDetail.FileName, AzureDevopsBaseURL, branchName);
            RequestDetailslist.Add(requestDetail);
            return requestDetail;
        }

        private static void ConfigureAzureDevOps(SolutionFileInfo item, out string branchName, out string RepoJSFolder, out string RepoHTMLFolder, out string RepoImagesFolder, out string RepoSolutionFolder, out string AzureDevopsBaseURL)
        {
            var tempUri = item.GitRepoUrl ?? Singleton.CrmConstantsInstance.RepositoryUrl;

            //check below if its setting the Url
            string GitRepoUrl = tempUri.ToString();
            branchName = item.BranchName ?? Singleton.CrmConstantsInstance.BranchName;
            branchName = "refs/heads/" + branchName;
            RepoJSFolder = item.RepoJSFolder ?? Singleton.CrmConstantsInstance.JsDirectory;
            RepoHTMLFolder = item.RepoHTMLFolder ?? Singleton.CrmConstantsInstance.HtmlDirectory;
            RepoImagesFolder = item.RepoImagesFolder ?? Singleton.CrmConstantsInstance.ImagesDirectory;
            RepoSolutionFolder = item.RepoSolutionFolder ?? Singleton.CrmConstantsInstance.SolutionFolder;
            AzureDevopsBaseURL = null;
            if (GitRepoUrl.ToLower().Contains("dev.azure.com"))
                AzureDevopsBaseURL = GitRepoUrl + "/{action}?api-version=5.0";
            else
            {
                //todo: below url to be part of configuration
                AzureDevopsBaseURL = "https://dev.azure.com/{organization}/{project}/_apis/git/repositories/{repositoryName}/{action}?api-version=5.0";
                AzureDevopsBaseURL = AzureDevopsBaseURL.Replace("{organization}", GitRepoUrl.Split('/')[3]);
                AzureDevopsBaseURL = AzureDevopsBaseURL.Replace("{project}", GitRepoUrl.Split('/')[4]);
                AzureDevopsBaseURL = AzureDevopsBaseURL.Replace("{repositoryName}", GitRepoUrl.Split('/')[6]);
            }
        }

        /// <summary>
        /// Method updates the Source control queue with queued for release status
        /// </summary>
        /// <param name="solutionFiles"></param>
        private static void UpdateItemForRelease(List<SolutionFileInfo> solutionFiles)
        {
            foreach (var item in solutionFiles)
            {
                if (item.Solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForStatus) == Constants.SourceControlQueuemPushToRepositorySuccessStatus)
                {
                    HashSet<string> completeHashSet = new HashSet<string>();
                    string id = (item.Solution.Id.ToString());
                    if (!completeHashSet.Contains(id) && item.IncludeInRelease)
                    {
                        item.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.ReadyForRelease;
                        item.Update();
                    }
                }
            }
        }

        /// <summary>
        /// CreateTempFiles
        /// </summary>
        /// <param name="solutionFilePath"></param>
        /// <param name="solutionCheckerPath"></param>
        /// <param name="timeTriggerPath"></param>
        private void CreateTempFiles(string solutionFilePath, string solutionCheckerPath, string timeTriggerPath)
        {
            if (File.Exists(solutionFilePath))
            {
                File.WriteAllText(solutionFilePath, string.Empty);
            }
            else
            {
                File.Create(solutionFilePath).Dispose();
            }
            if (File.Exists(solutionCheckerPath))
            {
                File.WriteAllText(solutionCheckerPath, string.Empty);

            }
            else
            {
                File.Create(solutionCheckerPath).Dispose();
            }
            if (File.Exists(timeTriggerPath))
            {
                File.WriteAllText(timeTriggerPath, string.Empty);
            }
            else
            {
                File.Create(timeTriggerPath).Dispose();
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
            File.WriteAllLines(solutionFilePath, hashSet.Where(cc => !string.IsNullOrEmpty(cc)).ToArray()); // TODO : need to download online file and update with old contnent adn new content.resp api item download
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
          //  Workspace workspace = null;
            // GitDeploy.GitRepositoryManager gitRepositoryManager = GetRepositoryManager(committerName, committerEmail, authorEmail, solutionFile);

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
                gitRepositoryManager.CommitAllChanges(solutionFile, solutionFilePath, null, solutionCheckerPath, timeTriggerPath);
                gitRepositoryManager.PushCommits();
            }

            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemPushToRepositorySuccessStatus;
            solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
            solutionFile.Update();
        }
    }
}
