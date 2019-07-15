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
                    List<EntityCollection> componentDependency = this.GetDependentComponents(serviceProxy, new Guid(solutionFile.MasterSolutionId), solutionFile.SolutionUniqueName);


                    SolutionManager sol = new SolutionManager(serviceProxy);

                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<br><br><table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'><tr><th style='background-color: #B8DBFD;border: 1px solid #ccc'>Dependent Components in Source Instance</th><th style='background-color: #B8DBFD;border: 1px solid #ccc'>Required Components</th></tr>");

                    if (componentDependency.Count > 0)
                    {
                        foreach (var comDependency in componentDependency)
                        {
                            if (comDependency != null && comDependency.Entities != null && comDependency.Entities.Count > 0)
                            {
                                foreach (Entity dependency in comDependency.Entities)
                                {
                                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<tr>");
                                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:#FFCC99;border: 1px solid #ccc'>");
                                    sol.GetComponentDetails(null, null, dependency, ((OptionSetValue)dependency.Attributes["dependentcomponenttype"]).Value, (Guid)dependency.Attributes["dependentcomponentobjectid"], "dependentcomponenttype");
                                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:#FFCC99;border: 1px solid #ccc'>");
                                    sol.GetComponentDetails(null, null, dependency, ((OptionSetValue)dependency.Attributes["requiredcomponenttype"]).Value, (Guid)dependency.Attributes["requiredcomponentobjectid"], "requiredcomponenttype");
                                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</tr>");
                                }
                            }

                        }
                    }
                    else
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<tr>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:#FFCC99;border: 1px solid #ccc'>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("There is no missing dependent component to display");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:#FFCC99;border: 1px solid #ccc'>");                        
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("----");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</tr>");
                    }
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</table><br><br>");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'><tr><th style='background-color: #B8DBFD;border: 1px solid #ccc'> Missing Dependent Components in Target Instance</th><th style='background-color: #B8DBFD;border: 1px solid #ccc'>Components Details</th></tr>");

                    if (componentDependency.Count > 0)
                    {
                        foreach (var comDependency in componentDependency)
                        {
                            CheckTarget = this.CheckDependency(targetserviceProxy, comDependency, sol, CheckTarget, serviceProxy);
                        }
                    }
                    else
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<tr>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:tomato;border: 1px solid #ccc'>");                       
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("All dependent components are present in target instance");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");                       
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:tomato;border: 1px solid #ccc'>");                      
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("----");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</tr>");
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
        private List<EntityCollection> GetDependentComponents(OrganizationServiceProxy serviceProxy, Guid masterSolutionId, string solutionUniqueName)
        {
            List<EntityCollection> dependentDetails = new List<EntityCollection>();

            RetrieveMissingDependenciesRequest missingDependenciesRequest = new RetrieveMissingDependenciesRequest
            {
                SolutionUniqueName = solutionUniqueName
            };

            RetrieveMissingDependenciesResponse missingDependenciesResponse = (RetrieveMissingDependenciesResponse)serviceProxy.Execute(missingDependenciesRequest);

            if (missingDependenciesResponse != null && missingDependenciesResponse.EntityCollection != null && missingDependenciesResponse.EntityCollection.Entities.Count > 0)
            {
                dependentDetails.Add(missingDependenciesResponse.EntityCollection);

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
        private bool CheckDependency(OrganizationServiceProxy serviceProxy, EntityCollection dependencyComponents, SolutionManager sol, bool checkTarget, OrganizationServiceProxy sourceServiceProxy)
        {

            foreach (Entity component in dependencyComponents.Entities)
            {
                try
                {
                    GetComponentDetails(component, ((OptionSetValue)component.Attributes["requiredcomponenttype"]).Value, (Guid)component.Attributes["requiredcomponentobjectid"], "requiredcomponenttype", serviceProxy, sourceServiceProxy);
                }
                catch (Exception ex)
                {
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<tr>");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:tomato;border: 1px solid #ccc'>");
                    Console.WriteLine("The below component was not present in Target");
                    sol.GetComponentDetails(null, null, component, ((OptionSetValue)component.Attributes["dependentcomponenttype"]).Value, (Guid)component.Attributes["dependentcomponentobjectid"], "dependentcomponenttype");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:tomato;border: 1px solid #ccc'>");
                    sol.GetComponentDetails(null, null, component, ((OptionSetValue)component.Attributes["requiredcomponenttype"]).Value, (Guid)component.Attributes["requiredcomponentobjectid"], "requiredcomponenttype");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</tr>");
                    checkTarget = true;
                }

            }
            return checkTarget;
        }

        public void QueryTargetComponents(OrganizationServiceProxy serviceProxy, RetrieveResponse retrieveResponse, string type)
        {
            var qe = new QueryExpression("plugintype")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                                {
                                    new ConditionExpression("name", ConditionOperator.Equal, retrieveResponse.Entity.Attributes["name"].ToString()),
                                }
                }
            };
            EntityCollection solutionComponents = serviceProxy.RetrieveMultiple(qe);
            Entity solutionCom = solutionComponents.Entities[0];

        }
        public void GetComponentDetails(Entity component, int componentType, Guid componentId, string componentDetails, OrganizationServiceProxy serviceProxy, OrganizationServiceProxy sourceServiceProxy)
        {
            switch (componentType)
            {
                case Constants.Entity:
                    var entityReq = new RetrieveEntityRequest();
                    entityReq.MetadataId = componentId;
                    var retrievedEntity = (RetrieveEntityResponse)sourceServiceProxy.Execute(entityReq);

                    var targetEntityReq = new RetrieveEntityRequest();
                    targetEntityReq.LogicalName = retrievedEntity.EntityMetadata.LogicalName;
                    var targetRetrievedEntity = (RetrieveEntityResponse)serviceProxy.Execute(targetEntityReq);
                    break;

                case Constants.WebResources:
                    var webresource = new RetrieveRequest();
                    webresource.Target = new EntityReference("webresource", componentId);
                    webresource.ColumnSet = new ColumnSet(true);
                    var retrievedWebresource = (RetrieveResponse)sourceServiceProxy.Execute(webresource);
                    QueryTargetComponents(serviceProxy, retrievedWebresource, "webresource");
                    break;

                case Constants.Attribute:
                    var attributeReq = new RetrieveAttributeRequest();
                    attributeReq.MetadataId = componentId;
                    var retrievedAttribute = (RetrieveAttributeResponse)sourceServiceProxy.Execute(attributeReq);
                    var targetAttributeReq = new RetrieveAttributeRequest();
                    targetAttributeReq.EntityLogicalName = retrievedAttribute.AttributeMetadata.EntityLogicalName;
                    targetAttributeReq.LogicalName = retrievedAttribute.AttributeMetadata.LogicalName;
                    var targetRetrievedAttribute = (RetrieveAttributeResponse)serviceProxy.Execute(targetAttributeReq);
                    break;

                case Constants.Relationship:
                    var relationshipReq = new RetrieveRelationshipRequest();
                    relationshipReq.MetadataId = componentId;
                    var retrievedrelationshipReq = (RetrieveRelationshipResponse)sourceServiceProxy.Execute(relationshipReq);

                    var targetRelationshipReq = new RetrieveRelationshipRequest();
                    targetRelationshipReq.Name = retrievedrelationshipReq.RelationshipMetadata.SchemaName;
                    var targetRetrievedrelationshipReq = (RetrieveRelationshipResponse)serviceProxy.Execute(targetRelationshipReq);
                    break;

                case Constants.DisplayString:
                    var displayStringRequest = new RetrieveRequest();
                    displayStringRequest.Target = new EntityReference("displaystring", componentId);
                    displayStringRequest.ColumnSet = new ColumnSet(true);
                    var retrievedDisplayString = (RetrieveResponse)sourceServiceProxy.Execute(displayStringRequest);
                    QueryTargetComponents(serviceProxy, retrievedDisplayString, "displaystring");
                    break;

                case Constants.SavedQuery:
                    var savedQueryRequest = new RetrieveRequest();
                    savedQueryRequest.Target = new EntityReference("savedquery", componentId);
                    savedQueryRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSavedQuery = (RetrieveResponse)sourceServiceProxy.Execute(savedQueryRequest);
                    QueryTargetComponents(serviceProxy, retrievedSavedQuery, "savedquery");
                    break;

                case Constants.SavedQueryVisualization:
                    var savedQueryVisualizationRequest = new RetrieveRequest();
                    savedQueryVisualizationRequest.Target = new EntityReference("savedqueryvisualization", componentId);
                    savedQueryVisualizationRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSavedQueryVisualization = (RetrieveResponse)sourceServiceProxy.Execute(savedQueryVisualizationRequest);
                    QueryTargetComponents(serviceProxy, retrievedSavedQueryVisualization, "savedqueryvisualization");
                    break;

                case Constants.SystemForm:
                    var systemFormRequest = new RetrieveRequest();
                    systemFormRequest.Target = new EntityReference("systemform", componentId);
                    systemFormRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSystemForm = (RetrieveResponse)sourceServiceProxy.Execute(systemFormRequest);
                    QueryTargetComponents(serviceProxy, retrievedSystemForm, "systemform");
                    break;

                case Constants.HierarchyRule:
                    var hierarchyRuleRequest = new RetrieveRequest();
                    hierarchyRuleRequest.Target = new EntityReference("hierarchyrule", componentId);
                    hierarchyRuleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedHierarchyRule = (RetrieveResponse)sourceServiceProxy.Execute(hierarchyRuleRequest);
                    QueryTargetComponents(serviceProxy, retrievedHierarchyRule, "hierarchyrule");
                    break;

                case Constants.SiteMap:
                    var siteMapRequest = new RetrieveRequest();
                    siteMapRequest.Target = new EntityReference("sitemap", componentId);
                    siteMapRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSiteMap = (RetrieveResponse)sourceServiceProxy.Execute(siteMapRequest);
                    QueryTargetComponents(serviceProxy, retrievedSiteMap, "sitemap");
                    break;

                case Constants.PluginAssembly:
                    var pluginAssemblyRequest = new RetrieveRequest();
                    pluginAssemblyRequest.Target = new EntityReference("pluginassembly", componentId);
                    pluginAssemblyRequest.ColumnSet = new ColumnSet(true);
                    var retrievedPluginAssembly = (RetrieveResponse)sourceServiceProxy.Execute(pluginAssemblyRequest);
                    QueryTargetComponents(serviceProxy, retrievedPluginAssembly, "pluginassembly");
                    break;

                case Constants.PluginType:
                    var pluginTypeRequest = new RetrieveRequest();
                    pluginTypeRequest.Target = new EntityReference("plugintype", componentId);
                    pluginTypeRequest.ColumnSet = new ColumnSet(true);
                    var retrievedPluginTypeRequest = (RetrieveResponse)sourceServiceProxy.Execute(pluginTypeRequest);
                    QueryTargetComponents(serviceProxy, retrievedPluginTypeRequest, "plugintype");
                    break;

                case Constants.SDKMessageProcessingStep:
                    var sdkMessageProcessingStepRequest = new RetrieveRequest();
                    sdkMessageProcessingStepRequest.Target = new EntityReference("sdkmessageprocessingstep", componentId);
                    sdkMessageProcessingStepRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSDKMessageProcessingStep = (RetrieveResponse)sourceServiceProxy.Execute(sdkMessageProcessingStepRequest);
                    QueryTargetComponents(serviceProxy, retrievedSDKMessageProcessingStep, "sdkmessageprocessingstep");
                    break;

                case Constants.ServiceEndpoint:
                    var serviceEndpointRequest = new RetrieveRequest();
                    serviceEndpointRequest.Target = new EntityReference("serviceendpoint", componentId);
                    serviceEndpointRequest.ColumnSet = new ColumnSet(true);
                    var retrievedServiceEndpoint = (RetrieveResponse)sourceServiceProxy.Execute(serviceEndpointRequest);
                    QueryTargetComponents(serviceProxy, retrievedServiceEndpoint, "serviceendpoint");
                    break;

                case Constants.Report:
                    var reportRequest = new RetrieveRequest();
                    reportRequest.Target = new EntityReference("report", componentId);
                    reportRequest.ColumnSet = new ColumnSet(true);
                    var retrievedReport = (RetrieveResponse)sourceServiceProxy.Execute(reportRequest);
                    QueryTargetComponents(serviceProxy, retrievedReport, "report");
                    break;

                case Constants.Role:
                    var roleRequest = new RetrieveRequest();
                    roleRequest.Target = new EntityReference("role", componentId);
                    roleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedRole = (RetrieveResponse)sourceServiceProxy.Execute(roleRequest);
                    QueryTargetComponents(serviceProxy, retrievedRole, "role");
                    break;

                case Constants.FieldSecurityProfile:
                    var fieldSecurityProfileRequest = new RetrieveRequest();
                    fieldSecurityProfileRequest.Target = new EntityReference("fieldsecurityprofile", componentId);
                    fieldSecurityProfileRequest.ColumnSet = new ColumnSet(true);
                    var retrievedFieldSecurityProfile = (RetrieveResponse)sourceServiceProxy.Execute(fieldSecurityProfileRequest);
                    QueryTargetComponents(serviceProxy, retrievedFieldSecurityProfile, "fieldsecurityprofile");
                    break;

                case Constants.ConnectionRole:
                    var connectionRoleRequest = new RetrieveRequest();
                    connectionRoleRequest.Target = new EntityReference("connectionrole", componentId);
                    connectionRoleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedConnectionRole = (RetrieveResponse)sourceServiceProxy.Execute(connectionRoleRequest);
                    QueryTargetComponents(serviceProxy, retrievedConnectionRole, "connectionrole");
                    break;

                case Constants.Workflow:
                    var workflowRequest = new RetrieveRequest();
                    workflowRequest.Target = new EntityReference("workflow", componentId);
                    workflowRequest.ColumnSet = new ColumnSet(true);
                    var retrievedWorkflow = (RetrieveResponse)sourceServiceProxy.Execute(workflowRequest);
                    QueryTargetComponents(serviceProxy, retrievedWorkflow, "workflow");
                    break;

                case Constants.KBArticleTemplate:
                    var articleTemplateRequest = new RetrieveRequest();
                    articleTemplateRequest.Target = new EntityReference("kbarticletemplate", componentId);
                    articleTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedKBArticleTemplate = (RetrieveResponse)sourceServiceProxy.Execute(articleTemplateRequest);
                    QueryTargetComponents(serviceProxy, retrievedKBArticleTemplate, "kbarticletemplate");
                    break;

                case Constants.MailMergeTemplate:
                    var mailMergeTemplateRequest = new RetrieveRequest();
                    mailMergeTemplateRequest.Target = new EntityReference("mailmergetemplate", componentId);
                    mailMergeTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedMailMergeTemplate = (RetrieveResponse)sourceServiceProxy.Execute(mailMergeTemplateRequest);
                    QueryTargetComponents(serviceProxy, retrievedMailMergeTemplate, "mailmergetemplate");
                    break;

                case Constants.ContractTemplate:
                    var contractTemplateRequest = new RetrieveRequest();
                    contractTemplateRequest.Target = new EntityReference("contracttemplate", componentId);
                    contractTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedContractTemplate = (RetrieveResponse)sourceServiceProxy.Execute(contractTemplateRequest);
                    QueryTargetComponents(serviceProxy, retrievedContractTemplate, "contracttemplate");
                    break;

                case Constants.EmailTemplate:
                    var emailTemplateRequest = new RetrieveRequest();
                    emailTemplateRequest.Target = new EntityReference("template", componentId);
                    emailTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedEmailTemplate = (RetrieveResponse)sourceServiceProxy.Execute(emailTemplateRequest);
                    QueryTargetComponents(serviceProxy, retrievedEmailTemplate, "template");
                    break;

                case Constants.SLA:
                    var slaRequest = new RetrieveRequest();
                    slaRequest.Target = new EntityReference("sla", componentId);
                    slaRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSLA = (RetrieveResponse)sourceServiceProxy.Execute(slaRequest);
                    QueryTargetComponents(serviceProxy, retrievedSLA, "sla");
                    break;

                case Constants.ConvertRule:
                    var convertRuleRequest = new RetrieveRequest();
                    convertRuleRequest.Target = new EntityReference("convertrule", componentId);
                    convertRuleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedConvertRule = (RetrieveResponse)sourceServiceProxy.Execute(convertRuleRequest);
                    QueryTargetComponents(serviceProxy, retrievedConvertRule, "convertrule");
                    break;

                default:
                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine("Unable to copy component type: " + component.FormattedValues[componentDetails] + " and objectID: " + componentId.ToString());
                    break;
            }
        }

    }
}
