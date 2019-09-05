﻿//-----------------------------------------------------------------------
// <copyright file="SolutionFileInfo.cs" company="Microsoft">
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
    using System.Reflection;
    using System.Text;
    using CliWrap;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;

    /// <summary>
    /// solution file info
    /// </summary>
    public class SolutionFileInfo
    {
        /// <summary>
        /// web jobs log
        /// </summary>
        private StringBuilder webJobsLog = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFileInfo" /> class without parameter
        /// </summary>
        public SolutionFileInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFileInfo" /> class
        /// </summary>
        /// <param name="organizationServiceProxy">organization service proxy</param>
        public SolutionFileInfo(Microsoft.Xrm.Sdk.Client.OrganizationServiceProxy organizationServiceProxy)
        {
            this.OrganizationServiceProxy = organizationServiceProxy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFileInfo" /> class
        /// </summary>
        /// <param name="solution">solution entity</param>
        /// <param name="organizationServiceProxy">organization service proxy</param>
        /// <param name="solutionDetail">solution detail</param>
        public SolutionFileInfo(Entity solution, OrganizationServiceProxy organizationServiceProxy, Entity solutionDetail)
        {
            this.OrganizationServiceProxy = organizationServiceProxy;
            this.SolutionsToBeMerged = new List<string>();
            this.SolutionUniqueName = solutionDetail.GetAttributeValue<string>("syed_listofsolutions");
            this.Repository = solution.GetAttributeValue<OptionSetValue>(Constants.SourceControlQueueAttributeNameForRepository).Value;
            ////solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForSolutionName);
            this.Message = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForComment);
            this.OwnerName = solution.GetAttributeValue<EntityReference>(Constants.SourceControlQueueAttributeNameForOwnerId).Name;
            this.IncludeInRelease = solution.GetAttributeValue<bool>(Constants.SourceControlQueueAttributeNameForIncludeInRelease);
            this.CheckInSolution = solution.GetAttributeValue<bool>(Constants.SourceControlQueueAttributeNameForCheckinSolution);
            ////this.MergeSolution = solution.GetAttributeValue<bool>(Constants.SourceControlQueueAttributeNameForMergeSolution);
            this.ExportAsManaged = solutionDetail.GetAttributeValue<bool>("syed_exportas");
            this.SolutionsTxt = solution.GetAttributeValue<OptionSetValue>(Constants.SourceControlQueueAttributeNameForOverwriteSolutionsTxt)?.Value ?? 0;
            this.RemoteName = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForRemote);
            this.GitRepoUrl = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForRepositoryUrl);
            EntityCollection retrieveSolutionsToBeMerged = Singleton.CrmSolutionHelperInstance.RetrieveSolutionsToBeMergedByListOfSolutionId(organizationServiceProxy, solutionDetail.Id);
            this.MasterSolutionId = solutionDetail.GetAttributeValue<string>("syed_solutionid");
            this.RepoHTMLFolder = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForRepositoryHTMLFolder);
            this.RepoImagesFolder = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForRepositoryImageFolder);
            this.RepoSolutionFolder = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForRepositorySolutionFolder);
            this.RepoJSFolder = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForRepositoryJsFolder);

            if (this.CheckInSolution)
            {
                this.SolutionExtractionPath = Path.GetTempPath() + this.SolutionUniqueName;
                this.BranchName = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForBranch);
                Singleton.CrmSolutionHelperInstance.CreateEmptyFolder(this.SolutionExtractionPath);
            }

            if (retrieveSolutionsToBeMerged.Entities.Count > 0)
            {
                foreach (Entity solutionsToBeMerged in retrieveSolutionsToBeMerged.Entities)
                {
                    this.SolutionsToBeMerged.Add(solutionsToBeMerged.GetAttributeValue<string>("syed_uniquename"));
                }
            }

            this.Solution = solution;
        }

        public int Repository { get; set; }

        /// <summary>
        /// Gets or sets master solution id
        /// </summary>
        public string MasterSolutionId { get; set; }

        /// <summary>
        /// Gets or sets where unmanaged solution is downloaded
        /// </summary>
        public string SolutionFilePath { get; set; }

        /// <summary>
        /// Gets or sets where managed solution is downloaded
        /// </summary>
        public string SolutionFilePathManaged { get; set; }

        /// <summary>
        /// Gets or sets Unique solution name
        /// </summary>
        public string SolutionUniqueName { get; set; }

        /// <summary>
        /// Gets or sets solution entity
        /// </summary>
        public Entity Solution { get; set; }

        /// <summary>
        /// Gets or sets repo url
        /// </summary>
        public string GitRepoUrl { get; set; }

        /// <summary>
        /// Gets or sets Repo Solution Folder
        /// </summary>
        public string RepoSolutionFolder { get; set; }

        /// <summary>
        /// Gets or sets Repo HTML Folder
        /// </summary>
        public string RepoHTMLFolder { get; set; }

        /// <summary>
        /// Gets or sets Repo JS Folder
        /// </summary>
        public string RepoJSFolder { get; set; }

        /// <summary>
        /// Gets or sets Repo Images Folder
        /// </summary>
        public string RepoImagesFolder { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether Overwrite solutions.txt needs to be overwritten or not
        /// </summary>
        public int SolutionsTxt { get; set; }

        /// <summary>
        /// Gets or sets remote name
        /// </summary>
        public string RemoteName { get; set; }

        /// <summary>
        /// Gets or sets branch name
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// Gets or sets message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets owner name
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether solution needs to be included in Release or not
        /// </summary>
        public bool IncludeInRelease { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether solution needs to be exported as managed or not
        /// </summary>
        public bool ExportAsManaged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether solution needs to be checked in
        /// </summary>
        public bool CheckInSolution { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether solution needs to be Merged
        /// </summary>
        public bool MergeSolution { get; set; }

        /// <summary>
        /// Gets or sets solutions to be merged
        /// </summary>
        public List<string> SolutionsToBeMerged { get; set; }

        /// <summary>
        /// Gets or sets value of Organization service
        /// </summary>
        public Microsoft.Xrm.Sdk.Client.OrganizationServiceProxy OrganizationServiceProxy { get; set; }

        /// <summary>
        /// Gets value of solution extraction path
        /// </summary>
        public string SolutionExtractionPath { get; private set; }

        /// <summary>
        /// Gets web jobs log
        /// </summary>
        public StringBuilder WebJobsLog
        {
            get
            {
                if (this.webJobsLog == null)
                {
                    this.webJobsLog = new StringBuilder();
                }

                return this.webJobsLog;
            }
        }

        /// <summary>
        /// Gets Unique solution file zip name
        /// </summary>
        public string SolutionFileZipName
        {
            get
            {
                if (string.IsNullOrEmpty(this.SolutionUniqueName))
                {
                    return null;
                }
                else if (this.ExportAsManaged)
                {
                    return this.SolutionUniqueName + "_managed_.zip";
                }

                return this.SolutionUniqueName + "_.zip";
            }
        }

        /// <summary>
        /// Method returns web jobs log
        /// </summary>
        /// <returns>returns web jobs log as string</returns>
        public string WebJobs()
        {
            string text = Singleton.SolutionFileInfoInstance.WebJobsLog.ToString();
            return text;
        }

        /// <summary>
        /// Method creates annotation record for web jobs log as notes in dynamics source control 
        /// </summary>
        /// <param name="service">organization service</param>
        /// <param name="dynamicsSourceControl">dynamic source control</param>
        public void UploadFiletoDynamics(IOrganizationService service, Entity dynamicsSourceControl)
        {
            string strMessage = Singleton.SolutionFileInfoInstance.WebJobsLog.ToString();
            strMessage = System.Text.RegularExpressions.Regex.Replace(strMessage, "<.*?>", string.Empty);
            byte[] filename = Encoding.ASCII.GetBytes(strMessage);
            string encodedData = System.Convert.ToBase64String(filename);
            Entity annotation = new Entity("annotation");
            annotation.Attributes["objectid"] = new EntityReference(dynamicsSourceControl.LogicalName, dynamicsSourceControl.Id);
            annotation.Attributes["objecttypecode"] = dynamicsSourceControl.LogicalName;
            annotation.Attributes["subject"] = dynamicsSourceControl.Attributes["syed_name"] + "_Log_" + DateTime.Now.ToString();
            annotation.Attributes["documentbody"] = encodedData;
            annotation.Attributes["mimetype"] = @"text/plain";
            annotation.Attributes["notetext"] = dynamicsSourceControl.Attributes["syed_name"] + DateTime.Now.ToString();
            annotation.Attributes["filename"] = dynamicsSourceControl.Attributes["syed_name"] + DateTime.Now.ToString() + ".txt";
            service.Create(annotation);
        }

        /// <summary>
        ///  Method returns solution info based on unique solution name
        /// </summary>
        /// <param name="solution">solution entity</param>
        /// <param name="organizationServiceProxy">organization proxy</param>
        /// <returns>returns list of solution file info by splitting source solution by comma</returns>
        public List<SolutionFileInfo> GetSolutionFileInfo(Entity solution, OrganizationServiceProxy organizationServiceProxy)
        {
            List<SolutionFileInfo> solutionFileInfos = new List<SolutionFileInfo>();
            EntityCollection retrievedMasterSolution = Singleton.CrmSolutionHelperInstance.RetrieveMasterSolutionDetailsByListOfSolutionId(organizationServiceProxy, solution.Id);
            if (retrievedMasterSolution.Entities.Count > 0)
            {
                foreach (Entity solutiondetail in retrievedMasterSolution.Entities)
                {
                    var solutionFile = new SolutionFileInfo(solution, organizationServiceProxy, solutiondetail);
                    solutionFileInfos.Add(solutionFile);
                }
            }

            return solutionFileInfos;

            ////foreach (var s in solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForSolutionName).Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries))
            ////{
            ////    var solutionFile = new SolutionFileInfo(solution, organizationServiceProxy, s);
            ////    solutionFileInfos.Add(solutionFile);
            ////}

            ////return solutionFileInfos;
        }

        /// <summary>
        /// Method commits the changes done to the solution object
        /// </summary>
        public void Update()
        {
            this.OrganizationServiceProxy.Update(this.Solution);
        }

        /// <summary>
        /// Method extracts solution zip file using solution packager
        /// </summary>
        /// <param name="solutionPackagerPath"> solution packager path</param>
        public void ProcessSolutionZipFile(string solutionPackagerPath)
        {
            try
            {
                var tempSolutionPackagerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Singleton.CrmConstantsInstance.SolutionPackagerRelativePath);
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Solution Packager Path " + tempSolutionPackagerPath + "<br>");
                Console.WriteLine("Solution Packager Path " + tempSolutionPackagerPath);

                if (File.Exists(tempSolutionPackagerPath))
                {
                    solutionPackagerPath = tempSolutionPackagerPath;
                }
            }
            catch (Exception ex)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                Console.WriteLine(ex.Message);

                throw;
            }

            if (!File.Exists(solutionPackagerPath))
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + "SolutionPackager.exe doesnot exists in the specified location : " + solutionPackagerPath + "<br>");
                Console.WriteLine("SolutionPackager.exe doesnot exists in the specified location : " + solutionPackagerPath);
                return;
            }

            var result = Cli.Wrap(solutionPackagerPath)
                            .SetArguments("/action:Extract /zipfile:\"" + this.SolutionFilePath + "\" /folder:\"" + this.SolutionExtractionPath + "\"")
                           .SetStandardOutputCallback(l => Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine($"StdOut> {l}" + "<br>")) // triggered on every line in stdout
                           .SetStandardErrorCallback(l => Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine($"StdErr> {l}" + "<br>")) // triggered on every line in stderr
                           .Execute();
        }
    }
}