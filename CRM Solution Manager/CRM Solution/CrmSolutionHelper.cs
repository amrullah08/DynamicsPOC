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
    using System.ServiceModel.Description;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Messages;
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
        private readonly Uri serviceUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrmSolutionHelper" /> class.
        /// </summary>
        /// <param name="repositoryUrl">repository url</param>
        /// <param name="branch">repository branch</param>
        /// <param name="organizationServiceUrl">organization service url</param>
        /// <param name="userName">user name</param>
        /// <param name="password">password token</param>
        /// <param name="solutionPackagerPath">solution package path</param>
        public CrmSolutionHelper(string repositoryUrl, string branch, string organizationServiceUrl, string userName, string password, string solutionPackagerPath)
        {
            this.RepositoryUrl = repositoryUrl;
            this.Branch = branch;
            this.SolutionPackagerPath = solutionPackagerPath;
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
        /// Gets or sets solution packager path
        /// </summary>
        public string SolutionPackagerPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pushing code changes is required or not
        /// </summary>
        public bool CanPush { get; set; }

        /// <summary>
        /// Gets or sets solution file info
        /// </summary>
        public List<SolutionFileInfo> SolutionFileInfos { get; set; }

        /// <summary>
        /// Empties folder
        /// </summary>
        /// <param name="directory">folder to be emptied</param>
        public static void CreateEmptyFolder(string directory)
        {
            if (Directory.Exists(directory))
            {
                DeleteDirectory(directory);
            }

            Directory.CreateDirectory(directory);
        }

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
                try
                {
                    var infos = SolutionFileInfo.GetSolutionFileInfo(querySampleSolutionResults.Entities[i], serviceProxy);
                    this.ExportListOfSolutionsToBeMerged(serviceProxy, infos[0]);
                    foreach (var info in infos)
                    {
                        try
                        {
                            this.ExportMasterSolution(serviceProxy, info);
                            solutionFileInfos.Add(info);
                            if (info.CheckInSolution)
                            {
                                this.CanPush = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            info.Solution[Constants.SourceControlQueueAttributeNameForStatus] = "Error +" + ex.Message;
                            info.Update();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    querySampleSolutionResults.Entities[i][Constants.SourceControlQueueAttributeNameForStatus] = "Error +" + ex.Message;
                    serviceProxy.Update(querySampleSolutionResults.Entities[i]);
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
        /// Method fetches MasterSolutionDetails records
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <returns>returns entity collection</returns>
        public static EntityCollection RetrieveMasterSolutionDetailsByListOfSolutionId(OrganizationServiceProxy service, Guid sourceControlId)
        {
            try
            {
                string fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='syed_solutiondetail'>
                                        <attribute name='syed_solutiondetailid' />
                                        <attribute name='syed_name' />
                                        <attribute name='createdon' />
                                        <attribute name='syed_order' />
                                        <attribute name='syed_solutionid' />
                                        <attribute name='syed_exportas' />
                                        <attribute name='syed_ismaster' />
                                        <attribute name='syed_listofsolutions' />
                                        <order attribute='syed_order' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='syed_listofsolutionid' operator='eq'  uitype='syed_sourcecontrolqueue'  value='" + sourceControlId + @"' />
                                        </filter>
                                      </entity>
                                    </fetch>";

                EntityCollection associatedRecordList = service.RetrieveMultiple(new FetchExpression(fetchXML));
                return associatedRecordList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString(), ex);
            }
        }

        /// <summary>
        /// Method fetches MergeSolution records
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <returns>returns entity collection</returns>
        public static EntityCollection RetrieveSolutionsToBeMergedByListOfSolutionId(OrganizationServiceProxy service, Guid sourceControlId)
        {
            try
            {
                string fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                        <entity name='syed_mergesolutions'>
                                        <attribute name='syed_mergesolutionsid' />
                                        <attribute name='syed_name' />
                                        <attribute name='syed_uniquename' />
                                        <attribute name='syed_order' />
                                        <order attribute='syed_order' descending='false' />
                                        <filter type='and'>
                                            <condition attribute='syed_listofsolution' operator='eq' uitype='syed_sourcecontrolqueue' value='" + sourceControlId + @"' />
                                        </filter>
                                        </entity>
                                    </fetch>";

                EntityCollection associatedRecordList = service.RetrieveMultiple(new FetchExpression(fetchXML));
                return associatedRecordList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString(), ex);
            }
        }

        /// <summary>
        /// Deletes directory
        /// </summary>
        /// <param name="path">folder path</param>
        private static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                // Delete all files from the Directory
                foreach (string file in Directory.GetFiles(path))
                {
                    File.Delete(file);
                }

                // Delete all child Directories
                foreach (string directory in Directory.GetDirectories(path))
                {
                    DeleteDirectory(directory);
                }

                // Delete a Directory
                Directory.Delete(path);
            }
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
        /// Method merges solution components into Master solution and exports it alongwith unzip file
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionFile">solution file info</param>
        private void ExportMasterSolution(OrganizationServiceProxy serviceProxy, SolutionFileInfo solutionFile)
        {
            if (solutionFile.SolutionsToBeMerged.Count > 0)
            {
                this.MergeSolutions(solutionFile, serviceProxy);
            }

            if (!solutionFile.CheckInSolution)
            {
                solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueueExportStatus;
                solutionFile.Update();
                this.ExportSolution(serviceProxy, solutionFile, solutionFile.SolutionUniqueName, "Downloading Master Solution: ", solutionFile.ExportAsManaged);
                solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueueExportSuccessful;
                solutionFile.Update();
                this.GetDeploymentInstance(serviceProxy, solutionFile);
            }
            else
            {
                solutionFile.Solution[Constants.SourceControlQueueAttributeNameForRepositoryUrl] = this.RepositoryUrl;
                solutionFile.Solution[Constants.SourceControlQueueAttributeNameForBranch] = this.Branch;
                solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueueExportStatus;
                solutionFile.Update();

                this.ExportSolution(serviceProxy, solutionFile, solutionFile.SolutionUniqueName, "Downloading Unmanaged Master Solution: ", false);
                this.ExportSolution(serviceProxy, solutionFile, solutionFile.SolutionUniqueName, "Downloading Managed Master Solution: ", true);

                solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueueExportSuccessful;
                solutionFile.Update();
                solutionFile.ProcessSolutionZipFile(this.SolutionPackagerPath);
            }
        }

        /// <summary>
        /// Method exports each of solution to be merged
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionFile">solution file info</param>
        private void ExportListOfSolutionsToBeMerged(OrganizationServiceProxy serviceProxy, SolutionFileInfo solutionFile)
        {
            if (solutionFile.SolutionsToBeMerged.Count > 0)
            {
                foreach (string solutionNAme in solutionFile.SolutionsToBeMerged)
                {
                    ExportSolution(serviceProxy, solutionFile, solutionNAme, "Downloading solutions to be merged: ", false);
                }
            }
        }

        /// <summary>
        /// Method gets deployment instance record
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionFile">solution file info</param>        
        private void GetDeploymentInstance(OrganizationServiceProxy serviceProxy, SolutionFileInfo solutionFile)
        {
            Entity sourceControl = solutionFile.Solution;
            EntityCollection deploymentInstance = FetchDeplopymentInstance(serviceProxy, sourceControl.Id);

            if (deploymentInstance.Entities.Count > 0)
            {
                foreach (Entity instance in deploymentInstance.Entities)
                {
                    ClientCredentials clientCredentials = new ClientCredentials();
                    clientCredentials.UserName.UserName = instance.Attributes["syed_name"].ToString();
                    clientCredentials.UserName.Password = instance.Attributes["syed_password"].ToString();
                    OrganizationServiceProxy client = new OrganizationServiceProxy(new Uri(instance.Attributes["syed_instanceurl"].ToString()), null, clientCredentials, null);
                    ImportSolution(client, solutionFile.SolutionFilePath ?? solutionFile.SolutionFilePathManaged);
                }
            }
        }

        /// <summary>
        /// Method import master solution to deployment instance
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionImportPath">solution import path</param>
        public static void ImportSolution(OrganizationServiceProxy serviceProxy, string solutionImportPath)
        {
            byte[] fileBytes = File.ReadAllBytes(solutionImportPath);

            ImportSolutionRequest impSolReq = new ImportSolutionRequest()
            {
                CustomizationFile = fileBytes,
                ImportJobId = Guid.NewGuid(),
                OverwriteUnmanagedCustomizations = true,
                SkipProductUpdateDependencies = true,
                PublishWorkflows = false,
            };
            ExecuteAsyncRequest importRequest = new ExecuteAsyncRequest()
            {
                Request = impSolReq
            };
            ExecuteAsyncResponse importRequestResponse = (ExecuteAsyncResponse)serviceProxy.Execute(importRequest);

            Console.WriteLine("Started importing solution");

            // put in sleep for every 30 seconds to check the status of import using asyncoperation entity
            string solutionImportResult = null;
            while (solutionImportResult == null)
            {
                System.Threading.Thread.Sleep(30000);
                Console.WriteLine("still importing......");
                Guid asyncJobId = importRequestResponse.AsyncJobId;
                Entity job = (Entity)serviceProxy.Retrieve("asyncoperation", asyncJobId, new ColumnSet(new System.String[] { "asyncoperationid", "statuscode", "message" }));
                int jobStatusCode = ((OptionSetValue)job["statuscode"]).Value;
                switch (jobStatusCode)
                {
                    // Success
                    case 30:
                        solutionImportResult = "success";
                        Console.WriteLine("Solution imported successfully");
                        break;
                    //Pausing  
                    case 21:
                        break;
                    //Canceling
                    case 22:
                        break;
                    //Failed
                    case 31:
                        break;
                    //Canceled
                    case 32:
                        throw new Exception(string.Format("Solution Import Failed: {0}{1}", jobStatusCode, job["message"]));
                    default:
                        break;
                }
            }

            if (solutionImportResult == "success")
                PublishAllCustomizationChanges(serviceProxy);
        }

        /// <summary>
        /// Method publish all the customization
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        public static void PublishAllCustomizationChanges(OrganizationServiceProxy serviceProxy)
        {
            PublishAllXmlRequest publishAllXmlRequest = new PublishAllXmlRequest();
            serviceProxy.Execute(publishAllXmlRequest);
            Console.WriteLine("Successfully published solution components.");
        }

        /// <summary>
        /// Method retrieves associated Deployment Instance for Dynamic Source Control.
        /// </summary>
        /// <param name="serviceProxy">service proxy</param>
        /// <param name="sourceControlId">source control id</param>
        /// <returns></returns>
        public static EntityCollection FetchDeplopymentInstance(IOrganizationService serviceProxy, Guid sourceControlId)
        {
            try
            {
                string fetchSolutions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='syed_deploymentinstance'>
                                                <attribute name='syed_deploymentinstanceid' />
                                                <attribute name='syed_name' />
                                                <attribute name='createdon' />
                                                <attribute name='syed_password' />
                                                <attribute name='syed_instanceurl' />
                                                <order attribute='syed_name' descending='false' />
                                                <filter type='and'>
                                                  <condition attribute='syed_dynamicssourcecontrol' operator='eq' uitype='syed_sourcecontrolqueue' value='" + sourceControlId + @"' />
                                                </filter>
                                              </entity>
                                            </fetch>";

                EntityCollection solutionlist = serviceProxy.RetrieveMultiple(new FetchExpression(fetchSolutions));
                return solutionlist;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString(), ex);
            }
        }

        /// <summary>
        /// Method exports solution
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionFile">solution file info</param>
        /// <param name="solutionName">solution name</param>
        /// <param name="message">message to be printed on console</param>
        private void ExportSolution(OrganizationServiceProxy serviceProxy, SolutionFileInfo solutionFile, string solutionName, string message, bool IsManaged)
        {
            try
            {
                ExportSolutionRequest exportRequest = new ExportSolutionRequest
                {
                    Managed = IsManaged,
                    SolutionName = solutionName
                };

                Console.WriteLine(message + solutionName);
                ExportSolutionResponse exportResponse = (ExportSolutionResponse)serviceProxy.Execute(exportRequest);

                // Handles the response
                byte[] downloadedSolutionFile = exportResponse.ExportSolutionFile;
                if(IsManaged)
                {
                    solutionFile.SolutionFilePathManaged = Path.GetTempFileName();
                    File.WriteAllBytes(solutionFile.SolutionFilePathManaged, downloadedSolutionFile);
                }
                else
                {
                    solutionFile.SolutionFilePath = Path.GetTempFileName();
                    File.WriteAllBytes(solutionFile.SolutionFilePath, downloadedSolutionFile);
                }

                string solutionExport = string.Format("Solution Successfully Exported to {0}", solutionName);
                Console.WriteLine(solutionExport);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}