//-----------------------------------------------------------------------
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
    using CliWrap;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;

    /// <summary>
    /// solution file info
    /// </summary>
    public class SolutionFileInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFileInfo" /> class
        /// </summary>
        /// <param name="organizationServiceProxy">Organization proxy</param>
        public SolutionFileInfo(Microsoft.Xrm.Sdk.Client.OrganizationServiceProxy organizationServiceProxy)
        {
            this.OrganizationServiceProxy = organizationServiceProxy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFileInfo" /> class
        /// </summary>
        /// <param name="solution">solution entity</param>
        /// <param name="organizationServiceProxy">Organization proxy</param>
        /// <param name="uniqueSolutionName">unique solution name</param>
        public SolutionFileInfo(Entity solution, OrganizationServiceProxy organizationServiceProxy, Entity solutionDetail)
        {
            this.OrganizationServiceProxy = organizationServiceProxy;
            this.SolutionsToBeMerged = new List<string>();
            this.SolutionUniqueName = solutionDetail.GetAttributeValue<string>("syed_listofsolutions");

            // solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForSolutionName);
            this.Message = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForComment);
            this.OwnerName = solution.GetAttributeValue<EntityReference>(Constants.SourceControlQueueAttributeNameForOwnerId).Name;
            this.IncludeInRelease = solution.GetAttributeValue<bool>(Constants.SourceControlQueueAttributeNameForIncludeInRelease);
            this.CheckInSolution = solution.GetAttributeValue<bool>(Constants.SourceControlQueueAttributeNameForCheckinSolution);
            this.MergeSolution = solution.GetAttributeValue<bool>(Constants.SourceControlQueueAttributeNameForMergeSolution);
            this.ExportAsManaged = solutionDetail.GetAttributeValue<bool>("syed_exportas");
            this.SolutionsTxt = solution.GetAttributeValue<bool>(Constants.SourceControlQueueAttributeNameForOverwriteSolutionsTxt);
            this.RemoteName = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForRemote);
            this.GitRepoUrl = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForRepositoryUrl);
            EntityCollection retrieveSolutionsToBeMerged = CrmSolutionHelper.RetrieveSolutionsToBeMergedByListOfSolutionId(organizationServiceProxy, solution.Id);



            if (this.CheckInSolution)
            {
                this.SolutionExtractionPath = Path.GetTempPath() + this.SolutionUniqueName;
                this.BranchName = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForBranch);
                CrmSolutionHelper.CreateEmptyFolder(this.SolutionExtractionPath);
            }

            if (retrieveSolutionsToBeMerged.Entities.Count > 0 && this.MergeSolution)
            {
                foreach (Entity solutionsToBeMerged in retrieveSolutionsToBeMerged.Entities)
                {
                    this.SolutionsToBeMerged.Add(solutionsToBeMerged.GetAttributeValue<string>("syed_uniquename"));
                }
            }

            this.Solution = solution;
        }

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
        /// Gets or sets solution entity
        /// </summary>
        public Entity Solution { get; set; }

        /// <summary>
        /// Gets or sets repo url
        /// </summary>
        public string GitRepoUrl { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether Overwrite solutions.txt needs to be overwritten or not
        /// </summary>
        public bool SolutionsTxt { get; set; }

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
        ///  Method returns solution info based on unique solution name
        /// </summary>
        /// <param name="solution">solution entity</param>
        /// <param name="organizationServiceProxy">organization proxy</param>
        /// <returns>returns list of solution file info by splitting source solution by comma</returns>
        public static List<SolutionFileInfo> GetSolutionFileInfo(Entity solution, OrganizationServiceProxy organizationServiceProxy)
        {
            List<SolutionFileInfo> solutionFileInfos = new List<SolutionFileInfo>();
            EntityCollection retrievedMasterSolution = CrmSolutionHelper.RetrieveMasterSolutionDetailsByListOfSolutionId(organizationServiceProxy, solution.Id);
            if (retrievedMasterSolution.Entities.Count > 0)
            {
                foreach (Entity solutiondetail in retrievedMasterSolution.Entities)
                {
                    var solutionFile = new SolutionFileInfo(solution, organizationServiceProxy, solutiondetail);
                    solutionFileInfos.Add(solutionFile);
                }
            }
            return solutionFileInfos;
            //foreach (var s in solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForSolutionName).Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries))
            //{
            //    var solutionFile = new SolutionFileInfo(solution, organizationServiceProxy, s);
            //    solutionFileInfos.Add(solutionFile);
            //}

            //return solutionFileInfos;
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
                var tempSolutionPackagerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), CrmConstants.SolutionPackagerRelativePath);
                Console.WriteLine("Solution Packager Path " + tempSolutionPackagerPath);

                if (File.Exists(tempSolutionPackagerPath))
                {
                    solutionPackagerPath = tempSolutionPackagerPath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (!File.Exists(solutionPackagerPath))
            {
                Console.WriteLine("SolutionPackager.exe doesnot exists in the specified location : " + solutionPackagerPath);
                return;
            }

            var result = Cli.Wrap(solutionPackagerPath)
                            .SetArguments("/action:Extract /zipfile:\"" + this.SolutionFilePath + "\" /folder:\"" + this.SolutionExtractionPath + "\"")
                           .SetStandardOutputCallback(l => Console.WriteLine($"StdOut> {l}")) // triggered on every line in stdout
                           .SetStandardErrorCallback(l => Console.WriteLine($"StdErr> {l}")) // triggered on every line in stderr
                           .Execute();
        }
    }
}