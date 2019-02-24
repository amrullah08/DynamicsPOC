//-----------------------------------------------------------------------
// <copyright file="CrmSolutionHelper.cs" company="Microsoft">
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
    using System.ServiceModel.Description;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Metadata;
    using Microsoft.Xrm.Sdk.Query;
    using MsCrmTools.SolutionComponentsMover.AppCode;

    /// <summary>
    /// Class that assist management of source control queues
    /// </summary>
    internal class CrmSolutionHelper : ICrmSolutionHelper
    {
        /// <summary>
        /// Gets or sets client credentials
        /// </summary>
        private readonly ClientCredentials clientCredentials;

        /// <summary>
        /// Organization service uri
        /// </summary>
        private Uri serviceUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrmSolutionHelper" /> class.
        /// </summary>
        /// <param name="repositoryUrl">repository url</param>
        /// <param name="branch">repository branch</param>
        /// <param name="organizationServiceUrl">organization service url</param>
        /// <param name="userName">user name</param>
        /// <param name="password">password token</param>
        public CrmSolutionHelper(string repositoryUrl, string branch, string organizationServiceUrl, string userName, string password)
        {
            this.RepositoryUrl = repositoryUrl;
            this.Branch = branch;

            this.serviceUri = new Uri(organizationServiceUrl);
            this.clientCredentials = new ClientCredentials();
            this.clientCredentials.UserName.UserName = userName;
            this.clientCredentials.UserName.Password = password;
            this.InitializeOrganizationService();
        }

        /// <summary>
        /// Gets or sets Repository url
        /// </summary>
        public string RepositoryUrl { get; set; }

        /// <summary>
        /// Gets or sets Repository branch
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pushing code changes is required or not
        /// </summary>
        public bool CanPush { get; set; }

        /// <summary>
        /// Gets or sets solution file info
        /// </summary>
        public List<SolutionFileInfo> SolutionFileInfos { get; set; }

        /// <summary>
        /// Method downloads unique solution name
        /// </summary>
        /// <param name="solutionUnqiueName">unique solution name</param>
        /// <returns>returns list of solution file info</returns>
        public List<SolutionFileInfo> DownloadSolutionFile(string solutionUnqiueName)
        {
            this.InitializeOrganizationService();
            this.CanPush = false;
            List<SolutionFileInfo> solutionFileInfos = new List<SolutionFileInfo>();
            Console.WriteLine("Connecting to the " + this.serviceUri.OriginalString);
            var serviceProxy = this.InitializeOrganizationService();
            EntityCollection querySampleSolutionResults = FetchSourceControlQueues(serviceProxy);

            for (int i = 0; i < querySampleSolutionResults.Entities.Count; i++)
            {
                SolutionFileInfo solutioninfo = null;
                try
                {
                    solutioninfo = this.ExportSolution(serviceProxy, querySampleSolutionResults.Entities[i]);
                    solutionFileInfos.Add(solutioninfo);

                    if (solutioninfo.CheckInSolution)
                    {
                        this.CanPush = true;
                    }
                }
                catch (Exception ex)
                {
                    solutioninfo.Solution[Constants.SourceControlQueueAttributeNameForStatus] = "Error +" + ex.Message;
                    solutioninfo.Update();
                }
            }

            return solutionFileInfos;
        }

        /// <summary>
        /// Method fetches source control queues
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <returns>returns entity collection</returns>
        private static EntityCollection FetchSourceControlQueues(OrganizationServiceProxy serviceProxy)
        {
            QueryExpression querySampleSolution = new QueryExpression
            {
                EntityName = Constants.SounceControlQueue,
                ColumnSet = new ColumnSet() { AllColumns = true },
                Criteria = new FilterExpression()
            };

            querySampleSolution.Criteria.AddCondition(Constants.SourceControlQueueAttributeNameForStatus, ConditionOperator.Equal, Constants.SourceControlQueueQueuedStatus);

            Console.WriteLine("Fetching Solutions to be copied to Repository ");
            EntityCollection querySampleSolutionResults = serviceProxy.RetrieveMultiple(querySampleSolution);
            return querySampleSolutionResults;
        }

        /// <summary>
        /// returns new instance of organization service
        /// </summary>
        /// <returns>returns organization service</returns>
        private OrganizationServiceProxy InitializeOrganizationService()
        {
            return new OrganizationServiceProxy(this.serviceUri, null, this.clientCredentials, null);
        }

        /// <summary>
        /// Method merges solution
        /// </summary>
        /// <param name="solutionFileInfo">solution file info</param>
        /// <param name="organizationServiceProxy">organization service proxy</param>
        private void MergeSolutions(SolutionFileInfo solutionFileInfo, OrganizationServiceProxy organizationServiceProxy)
        {
            SolutionManager solutionManager = new SolutionManager(organizationServiceProxy);
            solutionManager.CopyComponents(solutionFileInfo);
        }

        /// <summary>
        /// Method exports solution
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solution">entity solution</param>
        /// <returns>returns solution file info</returns>
        private SolutionFileInfo ExportSolution(OrganizationServiceProxy serviceProxy, Entity solution)
        {
            SolutionFileInfo solutionFile = new SolutionFileInfo(solution, serviceProxy);
            if (solutionFile.SolutionsToBeMerged.Count > 0)
            {
                this.MergeSolutions(solutionFile, serviceProxy);
            }

            if (!solutionFile.CheckInSolution)
            {
                return solutionFile;
            }

            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForRepositoryUrl] = this.RepositoryUrl;
            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForBranch] = this.Branch;
            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueueExportStatus;
            solutionFile.Update();

            string tempFolder = Path.GetTempFileName();
            ExportSolutionRequest exportRequest = new ExportSolutionRequest
            {
                Managed = true,
                SolutionName = solutionFile.SolutionUniqueName
            };

            Console.WriteLine("Downloading Solutions");
            ExportSolutionResponse exportResponse = (ExportSolutionResponse)serviceProxy.Execute(exportRequest);

            // Handles the response
            byte[] downloadedSolutionFile = exportResponse.ExportSolutionFile;
            solutionFile.SolutionFilePath = Path.GetTempFileName();
            File.WriteAllBytes(solutionFile.SolutionFilePath, downloadedSolutionFile);

            string solutionExport = string.Format("Solution Successfully Exported to {0}", solutionFile.SolutionUniqueName);
            Console.WriteLine(solutionExport);

            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueueExportSuccessful;
            solutionFile.Update();

            return solutionFile;
        }
    }
}