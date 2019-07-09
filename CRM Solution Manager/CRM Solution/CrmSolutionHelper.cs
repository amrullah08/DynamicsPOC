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
    using Microsoft.Xrm.Sdk.Query;
    using MsCrmTools.SolutionComponentsMover.AppCode;

    /// <summary>
    /// Class that assist management of source control queues
    /// </summary>
    public class CrmSolutionHelper : ICrmSolutionHelper
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
        ///  /// Initializes a new instance of the <see cref="CrmSolutionHelper" /> class without parameter.
        /// </summary>
        public CrmSolutionHelper()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrmSolutionHelper" /> class.
        /// </summary>
        /// <param name="repositoryUrl">repository url</param>
        /// <param name="branch">repository branch</param>
        /// <param name="remoteName">repository remote</param>
        /// <param name="organizationServiceUrl">organization service url</param>
        /// <param name="userName">user name</param>
        /// <param name="password">password token</param>
        /// <param name="solutionPackagerPath">solution package path</param>
        public CrmSolutionHelper(string repositoryUrl, string branch, string remoteName, string organizationServiceUrl, string userName, string password, string solutionPackagerPath)
        {
            this.RepositoryUrl = repositoryUrl;
            this.Branch = branch;
            this.RemoteName = remoteName;
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
        /// Gets or sets Remote name
        /// </summary>
        public string RemoteName { get; set; }

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
        public void CreateEmptyFolder(string directory)
        {
            if (Directory.Exists(directory))
            {
                this.DeleteDirectory(directory);
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
            serviceProxy.EnableProxyTypes();
            EntityCollection querySampleSolutionResults = this.FetchSourceControlQueues(serviceProxy);
            if (querySampleSolutionResults.Entities.Count > 0)
            {
                for (int i = 0; i < querySampleSolutionResults.Entities.Count; i++)
                {
                    try
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Clear();
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Connected to the " + this.serviceUri.OriginalString + "<br>");
                        var infos = Singleton.SolutionFileInfoInstance.GetSolutionFileInfo(querySampleSolutionResults.Entities[i], serviceProxy);
                        foreach (var info in infos)
                        {
                            try
                            {
                                this.ExportListOfSolutionsToBeMerged(serviceProxy, info);
                                this.ExportMasterSolution(serviceProxy, info);
                                solutionFileInfos.Add(info);
                                if (info.CheckInSolution)
                                {
                                    this.CanPush = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                                throw new Exception(ex.InnerException.Message, ex);
                            }
                        }

                        if (!querySampleSolutionResults.Entities[i].GetAttributeValue<bool>("syed_checkin"))
                        {
                            Singleton.SolutionFileInfoInstance.UploadFiletoDynamics(Singleton.CrmConstantsInstance.ServiceProxy, querySampleSolutionResults.Entities[i]);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Authentication Failure")
                        {
                            Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + "Please re-enter password in deployment instances" + "<br>");
                            Console.WriteLine("Please re-enter password in deployment instances");
                            querySampleSolutionResults.Entities[i][Constants.SourceControlQueueAttributeNameForStatus] = "Please re-enter password in deployment instances";
                        }
                        else
                        {
                            Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                            Console.WriteLine(ex.Message);
                            querySampleSolutionResults.Entities[i][Constants.SourceControlQueueAttributeNameForStatus] = "Error +" + ex.Message;
                        }

                        querySampleSolutionResults.Entities[i].Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
                        serviceProxy.Update(querySampleSolutionResults.Entities[i]);
                        Singleton.SolutionFileInfoInstance.UploadFiletoDynamics(serviceProxy, querySampleSolutionResults.Entities[i]);

                        throw new Exception();
                    }
                }
            }
            else
            {
                Console.WriteLine("There are no Dynamic Source Control record to proceed");
            }

            return solutionFileInfos;
        }

        /// <summary>
        /// Method fetches MasterSolutionDetails records
        /// </summary>
        /// <param name="service">organization service proxy</param>
        /// <param name="sourceControlId"> Source Control Id</param>
        /// <returns>returns entity collection</returns>
        public EntityCollection RetrieveMasterSolutionDetailsByListOfSolutionId(OrganizationServiceProxy service, Guid sourceControlId)
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
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message.ToString(), ex);
            }
        }

        /// <summary>
        /// Method fetches MergeSolution records
        /// </summary>
        /// <param name="service">organization service proxy</param>
        /// <param name="masterSolutionId">Master Solution Id</param>
        /// <returns>returns entity collection</returns>
        public EntityCollection RetrieveSolutionsToBeMergedByListOfSolutionId(OrganizationServiceProxy service, Guid masterSolutionId)
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
                                            <condition attribute='syed_mastersolution' operator='eq' uitype='syed_solutiondetail' value='" + masterSolutionId + @"' />
                                        </filter>
                                        </entity>
                                    </fetch>";

                EntityCollection associatedRecordList = service.RetrieveMultiple(new FetchExpression(fetchXML));
                return associatedRecordList;
            }
            catch (Exception ex)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message.ToString(), ex);
            }
        }

        /// <summary>
        /// Method import master solution to deployment instance
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionFile">solution file</param>
        /// <param name="uri">URL of Instance</param>
        public void ImportSolution(OrganizationServiceProxy serviceProxy, SolutionFileInfo solutionFile, Uri uri)
        {
            string solutionImportPath = solutionFile.SolutionFilePathManaged ?? solutionFile.SolutionFilePath;
            Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Started importing solution to Organization " + uri + "<br>");
            Console.WriteLine("Started importing solution to Organization " + uri);

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

            string solutionImportResult = string.Empty;
            while (solutionImportResult == string.Empty)
            {
                Guid asyncJobId = importRequestResponse.AsyncJobId;
                Entity job = (Entity)serviceProxy.Retrieve("asyncoperation", asyncJobId, new ColumnSet(new string[] { "asyncoperationid", "statuscode", "message" }));
                int jobStatusCode = ((OptionSetValue)job["statuscode"]).Value;
                switch (jobStatusCode)
                {
                    ////Success
                    case 30:
                        solutionImportResult = "success";
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Solution imported successfully to the Organization " + uri + "<br>");
                        Console.WriteLine("Solution imported successfully to the Organization " + uri);
                        solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueueImportSuccessfulStatus;
                        solutionFile.Solution["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
                        solutionFile.Update();
                        break;
                    ////Pausing  
                    case 21:
                        solutionImportResult = "pausing";
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(string.Format(" Solution Import Pausing: {0}{1}", jobStatusCode, job["message"]) + "<br>");
                        Console.WriteLine(string.Format("Solution Import Pausing: {0} {1}", jobStatusCode, job["message"]));
                        break;
                    ////Cancelling
                    case 22:
                        solutionImportResult = "cancelling";
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(string.Format("Solution Import Cancelling: {0}{1}", jobStatusCode, job["message"]) + "<br>");
                        Console.WriteLine(string.Format("Solution Import Cancelling: {0}{1}", jobStatusCode, job["message"]));
                        break;
                    ////Failed
                    case 31:
                        solutionImportResult = "failed";
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(string.Format("Solution Import Failed: {0}{1}", jobStatusCode, job["message"]) + "<br>");
                        Console.WriteLine(string.Format("Solution Import Failed: {0}{1}", jobStatusCode, job["message"]));
                        break;
                    ////Cancelled
                    case 32:
                        Console.WriteLine(string.Format("Solution Import Cancelled: {0}{1}", jobStatusCode, job["message"]));
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(string.Format("Solution Import Cancelled: {0}{1}", jobStatusCode, job["message"]));
                        throw new Exception(string.Format("Solution Import Cancelled: {0}{1}", jobStatusCode, job["message"]));
                    default:
                        break;
                }
            }

            if (solutionImportResult == "success")
            {
                this.PublishAllCustomizationChanges(serviceProxy);
                solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuePublishSuccessfulStatus;
            }

            solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
            solutionFile.Update();
        }

        /// <summary>
        /// Method publish all the customization
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        public void PublishAllCustomizationChanges(OrganizationServiceProxy serviceProxy)
        {
            PublishAllXmlRequest publishAllXmlRequest = new PublishAllXmlRequest();
            serviceProxy.Execute(publishAllXmlRequest);
            Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine("Successfully published solution components." + "<br>");
            Console.WriteLine("Successfully published solution components.");
        }

        /// <summary>
        /// Method retrieves associated Deployment Instance for Dynamic Source Control.
        /// </summary>
        /// <param name="serviceProxy">service proxy</param>
        /// <param name="sourceControlId">source control id</param>
        /// <returns> Deployment Instance</returns>
        public EntityCollection FetchDeplopymentInstance(IOrganizationService serviceProxy, Guid sourceControlId)
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
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message.ToString(), ex);
            }
        }

        /// <summary>
        /// Deletes directory
        /// </summary>
        /// <param name="path">folder path</param>
        private void DeleteDirectory(string path)
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
                    this.DeleteDirectory(directory);
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
        /// Method merges solution components into Master solution and exports along with unzip file
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionFile">solution file info</param>
        private void ExportMasterSolution(OrganizationServiceProxy serviceProxy, SolutionFileInfo solutionFile)
        {
            if (solutionFile.SolutionsToBeMerged.Count > 0)
            {
                this.MergeSolutions(solutionFile, serviceProxy);
            }

            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueueExportStatus;
            solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
            solutionFile.Update();

            this.ExportSolution(serviceProxy, solutionFile, solutionFile.SolutionUniqueName, "Downloading Master Solution: ", solutionFile.ExportAsManaged);
            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueueExportSuccessful;
            solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
            solutionFile.Update();
            this.ImportSolutionToTargetInstance(serviceProxy, solutionFile);

            if (solutionFile.CheckInSolution)
            {
                if (string.IsNullOrEmpty(solutionFile.GitRepoUrl))
                {
                    solutionFile.Solution[Constants.SourceControlQueueAttributeNameForRepositoryUrl] = this.RepositoryUrl;
                }

                if (string.IsNullOrEmpty(solutionFile.BranchName))
                {
                    solutionFile.Solution[Constants.SourceControlQueueAttributeNameForBranch] = this.Branch;
                }

                if (string.IsNullOrEmpty(solutionFile.RemoteName))
                {
                    solutionFile.Solution[Constants.SourceControlQueueAttributeNameForRemote] = this.RemoteName;
                }

                solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueueExportStatus;
                solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
                solutionFile.Update();

                this.ExportSolution(serviceProxy, solutionFile, solutionFile.SolutionUniqueName, "Downloading Unmanaged Master Solution: ", false);
                this.ExportSolution(serviceProxy, solutionFile, solutionFile.SolutionUniqueName, "Downloading Managed Master Solution: ", true);

                solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueueExportSuccessful;
                solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
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
                    this.ExportSolution(serviceProxy, solutionFile, solutionNAme, "Downloading solutions to be merged: ", false);
                }
            }
        }

        /// <summary>
        /// Method gets deployment instance record
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionFile">solution file info</param>        
        private void ImportSolutionToTargetInstance(OrganizationServiceProxy serviceProxy, SolutionFileInfo solutionFile)
        {
            Entity sourceControl = solutionFile.Solution;
            EntityCollection deploymentInstance = this.FetchDeplopymentInstance(serviceProxy, sourceControl.Id);
            var CheckTarget = false;
            if (deploymentInstance.Entities.Count > 0)
            {
                foreach (Entity instance in deploymentInstance.Entities)
                {

                    ClientCredentials clientCredentials = new ClientCredentials();
                    clientCredentials.UserName.UserName = instance.Attributes["syed_name"].ToString();
                    clientCredentials.UserName.Password = this.DecryptString(instance.Attributes["syed_password"].ToString());
                    ////Resetting password
                    instance.Attributes["syed_password"] = "Reset_Password";
                    serviceProxy.Update(instance);
                    OrganizationServiceProxy targetserviceProxy = new OrganizationServiceProxy(new Uri(instance.Attributes["syed_instanceurl"].ToString()), null, clientCredentials, null);
                    targetserviceProxy.EnableProxyTypes();
                    List<EntityCollection> componentDependency = this.GetDependentComponents(serviceProxy, new Guid(solutionFile.MasterSolutionId));

                    SolutionManager sol = new SolutionManager(serviceProxy);

                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<br><br><table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'><tr><th style='background-color: #B8DBFD;border: 1px solid #ccc'>Dependent Components in Source Instance</th><th style='background-color: #B8DBFD;border: 1px solid #ccc'>Required Components</th></tr>");
                    foreach (var comDependency in componentDependency)
                    {
                        foreach (Entity dependency in comDependency.Entities)
                        {
                            Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<tr>");
                            Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:tomato;border: 1px solid #ccc'>");
                            sol.GetComponentDetails(null, null, dependency, ((OptionSetValue)dependency.Attributes["dependentcomponenttype"]).Value, (Guid)dependency.Attributes["dependentcomponentobjectid"], "dependentcomponenttype");
                            Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                            Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:tomato;border: 1px solid #ccc'>");
                            sol.GetComponentDetails(null, null, dependency, ((OptionSetValue)dependency.Attributes["requiredcomponenttype"]).Value, (Guid)dependency.Attributes["requiredcomponentobjectid"], "requiredcomponenttype");
                            Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                            Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</tr>");
                        }
                    }

                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</table><br><br>");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'><tr><th style='background-color: #B8DBFD;border: 1px solid #ccc'> Missing Dependent Components in Target Instance</th></tr>");
                    foreach (var comDependency in componentDependency)
                    {
                        CheckTarget = this.CheckDependency(targetserviceProxy, comDependency, sol, CheckTarget);
                    }


                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</table><br><br>");

                    if (!CheckTarget)
                    {
                        this.ImportSolution(targetserviceProxy, solutionFile, new Uri(instance.Attributes["syed_instanceurl"].ToString()));
                    }
                    else
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Target Instance missing Required components.  <br> ");
                        solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueueMissingComponents;
                        solutionFile.Solution["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
                        Singleton.SolutionFileInfoInstance.UploadFiletoDynamics(serviceProxy, solutionFile.Solution);
                        solutionFile.Update();
                    }
                }
            }
        }

        /// <summary>
        /// Methods decrypts the password
        /// </summary>
        /// <param name="encryptString">encrypt string</param>
        /// <returns> Decrypted string </returns>
        private string DecryptString(string encryptString)
        {
            byte[] b = Convert.FromBase64String(encryptString);
            string decrypted = System.Text.ASCIIEncoding.ASCII.GetString(b);

            return decrypted;
        }

        /// <summary>
        /// Method exports solution
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionFile">solution file info</param>
        /// <param name="solutionName">solution name</param>
        /// <param name="message">message to be printed on console</param>
        /// <param name="isManaged">Managed Property</param>
        private void ExportSolution(OrganizationServiceProxy serviceProxy, SolutionFileInfo solutionFile, string solutionName, string message, bool isManaged)
        {
            try
            {
                ExportSolutionRequest exportRequest = new ExportSolutionRequest
                {
                    Managed = isManaged,
                    SolutionName = solutionName
                };

                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + message + solutionName + "<br>");
                Console.WriteLine(message + solutionName);
                ExportSolutionResponse exportResponse = (ExportSolutionResponse)serviceProxy.Execute(exportRequest);

                // Handles the response
                byte[] downloadedSolutionFile = exportResponse.ExportSolutionFile;
                if (isManaged)
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
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + solutionExport + "<br>");
                Console.WriteLine(solutionExport);
            }
            catch (Exception ex)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Method fetches source control queues
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <returns>returns entity collection</returns>
        private EntityCollection FetchSourceControlQueues(OrganizationServiceProxy serviceProxy)
        {
            QueryExpression querySampleSolution = new QueryExpression
            {
                EntityName = Constants.SounceControlQueue,
                ColumnSet = new ColumnSet() { AllColumns = true },
                Criteria = new FilterExpression()
            };

            querySampleSolution.Criteria.AddCondition(Constants.SourceControlQueueAttributeNameForStatus, ConditionOperator.Equal, Constants.SourceControlQueueQueuedStatus);

            Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Fetching Solutions to be copied to Repository " + "<br>");
            Console.WriteLine("Fetching Solutions to be copied to Repository ");
            EntityCollection querySampleSolutionResults = serviceProxy.RetrieveMultiple(querySampleSolution);
            return querySampleSolutionResults;
        }

        /// <summary>
        /// Method gets solution components dependency
        /// </summary>
        /// <param name="serviceProxy">service proxy</param>
        /// <param name="masterSolutionId">master solution id</param>
        /// <returns>returns components dependency entity collection</returns>
        private List<EntityCollection> GetDependentComponents(OrganizationServiceProxy serviceProxy, Guid masterSolutionId)
        {
            List<EntityCollection> dependentDetails = new List<EntityCollection>();
            var solutionComponents = this.RetrieveComponentsFromSolutions(serviceProxy, masterSolutionId);

            foreach (var component in solutionComponents)
            {
                RetrieveDependentComponentsRequest dependentComponentsRequest =
                             new RetrieveDependentComponentsRequest
                             {
                                 ComponentType = component.GetAttributeValue<OptionSetValue>("componenttype").Value,
                                 ObjectId = component.GetAttributeValue<Guid>("objectid")
                             };
                RetrieveDependentComponentsResponse dependentComponentsResponse = (RetrieveDependentComponentsResponse)serviceProxy.Execute(dependentComponentsRequest);
                if (dependentComponentsResponse != null && dependentComponentsResponse.EntityCollection != null && dependentComponentsResponse.EntityCollection.Entities.Count > 0)
                {
                    //Console.WriteLine("Found {0} dependencies for Component {1} of type {2}",
                    //    dependentComponentsResponse.EntityCollection.Entities.Count,
                    //    component.GetAttributeValue<OptionSetValue>("dependentcomponenttype").Value,
                    //    component.GetAttributeValue<Guid>("dependentcomponentobjectid"));
                    dependentDetails.Add(dependentComponentsResponse.EntityCollection);

                }

                RetrieveRequiredComponentsRequest requiredComponentsRequest =
                                new RetrieveRequiredComponentsRequest
                                {
                                    ComponentType = component.GetAttributeValue<OptionSetValue>("componenttype").Value,
                                    ObjectId = component.GetAttributeValue<Guid>("objectid")
                                };
                RetrieveRequiredComponentsResponse requiredComponentsResponse = (RetrieveRequiredComponentsResponse)serviceProxy.Execute(requiredComponentsRequest);
                if (requiredComponentsResponse != null && requiredComponentsResponse.EntityCollection != null && requiredComponentsResponse.EntityCollection.Entities.Count > 0)
                {
                    //Console.WriteLine("Found {0} required dependencies for Component {1} of type {2}",
                    //    requiredComponentsResponse.EntityCollection.Entities.Count, component.GetAttributeValue<OptionSetValue>("dependentcomponenttype").Value,
                    //    component.GetAttributeValue<Guid>("dependentcomponentobjectid"));
                    dependentDetails.Add(requiredComponentsResponse.EntityCollection);
                }
            }

            return dependentDetails;
        }

        /// <summary>
        /// Method retrieves components from the solution
        /// </summary>
        /// <param name="serviceProxy">service proxy</param>
        /// <param name="masterSolutionId">master solution id</param>
        /// <returns>returns entity components</returns>
        private DataCollection<Entity> RetrieveComponentsFromSolutions(OrganizationServiceProxy serviceProxy, Guid masterSolutionId)
        {
            var qe = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("solutionid", ConditionOperator.Equal, masterSolutionId)
                    }
                }
            };

            return serviceProxy.RetrieveMultiple(qe).Entities;
        }

        /// <summary>
        /// Method checks dependency components present in target instance
        /// </summary>
        /// <param name="serviceProxy">service proxy</param>
        /// <param name="dependencyComponents">dependency components</param>
        /// <param name="sol">Solution Manager</param>
        private bool CheckDependency(OrganizationServiceProxy serviceProxy, EntityCollection dependencyComponents, SolutionManager sol, bool checkTarget)
        {
            foreach (var component in dependencyComponents.Entities)
            {
                QueryExpression retrieveTargetDependency = new QueryExpression("dependency");
                retrieveTargetDependency.ColumnSet = new ColumnSet(true);
                retrieveTargetDependency.Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.Or,
                    Conditions =
                            {
                                new ConditionExpression("dependentcomponentobjectid", ConditionOperator.Equal, component.Attributes["dependentcomponentobjectid"]),
                                new ConditionExpression("requiredcomponentobjectid", ConditionOperator.Equal, component.Attributes["requiredcomponentobjectid"]),
                            }
                };
                EntityCollection retrievedTargetDependecy = serviceProxy.RetrieveMultiple(retrieveTargetDependency);
                if (retrievedTargetDependecy.Entities.Count == 0)
                {
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<tr>");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:pink;border: 1px solid #ccc'>");
                    Console.WriteLine("The below component was not present in Target");
                    sol.GetComponentDetails(null, null, component, ((OptionSetValue)component.Attributes["dependentcomponenttype"]).Value, (Guid)component.Attributes["dependentcomponentobjectid"], "dependentcomponenttype");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                    checkTarget = true;
                }
                else
                {
                }
            }
            return checkTarget;
        }
    }
}
