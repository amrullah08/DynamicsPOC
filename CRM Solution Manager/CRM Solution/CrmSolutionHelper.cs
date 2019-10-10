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
    using System.Management.Automation;
    using System.Security.Cryptography;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Threading.Tasks;
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
            //this.serviceUri = new Uri(organizationServiceUrl);
            //this.clientCredentials = new ClientCredentials();
            //this.clientCredentials.UserName.UserName = userName;
            //this.clientCredentials.UserName.Password = password;
            //this.InitializeOrganizationService();
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
        /// <param name="mode">mode of the flow</param>
        /// <returns>returns list of solution file info</returns>
        public List<SolutionFileInfo> DownloadSolutionFile(string solutionUnqiueName, string mode)
        {
            this.CanPush = false;
            List<SolutionFileInfo> solutionFileInfos = new List<SolutionFileInfo>();
            //Console.WriteLine("Connecting to the " + this.serviceUri.OriginalString);
            var serviceProxy = Singleton.CrmConstantsInstance.ServiceProxy;
            EntityCollection querySampleSolutionResults = this.FetchSourceControlQueues(serviceProxy, mode);
            if (querySampleSolutionResults.Entities.Count > 0)
            {
                for (int i = 0; i < querySampleSolutionResults.Entities.Count; i++)
                {
                    try
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Clear();
                        // Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Connected to the " + this.serviceUri.OriginalString + "<br>");
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
        public EntityCollection RetrieveMasterSolutionDetailsByListOfSolutionId(IOrganizationService service, Guid sourceControlId)
        {
            try
            {
                string fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='syed_solutiondetail'>
                                          <all-attributes />
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
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Method fetches MergeSolution records
        /// </summary>
        /// <param name="service">organization service proxy</param>
        /// <param name="masterSolutionId">Master Solution Id</param>
        /// <returns>returns entity collection</returns>
        public EntityCollection RetrieveSolutionsToBeMergedByListOfSolutionId(IOrganizationService service, Guid masterSolutionId)
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
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Method import master solution to deployment instance
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionFile">solution file</param>
        /// <param name="uri">URL of Instance</param>
        public void ImportSolution(IOrganizationService serviceProxy, SolutionFileInfo solutionFile, Uri uri)
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
            while (string.IsNullOrEmpty(solutionImportResult))
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
                this.PublishAllCustomizationChanges(serviceProxy, solutionFile);
                solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuePublishSuccessfulStatus;
            }

            solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
            solutionFile.Update();
        }

        /// <summary>
        /// Method publish all the customization
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionFile">solution File</param>
        public void PublishAllCustomizationChanges(IOrganizationService serviceProxy, SolutionFileInfo solutionFile)
        {
            PublishAllXmlRequest publishAllXmlRequest = new PublishAllXmlRequest();
            serviceProxy.Execute(publishAllXmlRequest);
            Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine("Successfully published solution components." + "<br>");
            solutionFile.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuePublishSuccessfulStatus;
            solutionFile.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();
            solutionFile.Update();
            Singleton.SolutionFileInfoInstance.UploadFiletoDynamics(Singleton.CrmConstantsInstance.ServiceProxy, solutionFile.Solution);
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
                                                <all-attributes />
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
        /// Method runs in different thread to publish all customization
        /// </summary>
        /// <param name="serviceProxy">service proxy</param>
        /// <param name="solutionFile">solution file</param>
        private async void CallPublishAllCustomizationChanges(IOrganizationService serviceProxy, SolutionFileInfo solutionFile)
        {
            await Task.Run(() => this.PublishAllCustomizationChanges(serviceProxy, solutionFile));
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
        private void MergeSolutions(SolutionFileInfo solutionFileInfo, IOrganizationService organizationServiceProxy)
        {
            SolutionManager solutionManager = new SolutionManager(organizationServiceProxy);
            solutionManager.CopyComponents(solutionFileInfo);
        }

        /// <summary>
        /// Method merges solution components into Master solution and exports along with unzip file
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionFile">solution file info</param>
        private void ExportMasterSolution(IOrganizationService serviceProxy, SolutionFileInfo solutionFile)
        {
            this.MergeSolutions(solutionFile, serviceProxy);

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
        private void ExportListOfSolutionsToBeMerged(IOrganizationService serviceProxy, SolutionFileInfo solutionFile)
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
        private void ImportSolutionToTargetInstance(IOrganizationService serviceProxy, SolutionFileInfo solutionFile)
        {
            Entity sourceControl = solutionFile.Solution;
            EntityCollection deploymentInstance = this.FetchDeplopymentInstance(serviceProxy, sourceControl.Id);
            bool checkDependency = false;
            bool import = false;
            bool clearPassword = false;
            var checkTarget = false;
            if (deploymentInstance.Entities.Count > 0)
            {
                foreach (Entity instance in deploymentInstance.Entities)
                {
                    ClientCredentials clientCredentials = new ClientCredentials();
                    clientCredentials.UserName.UserName = instance.Attributes["syed_name"].ToString();
                    clientCredentials.UserName.Password = this.DecryptString(instance.Attributes["syed_password"].ToString());
                    clearPassword = (bool)instance.Attributes["syed_clearpassword"];
                    ////Resetting password
                    if (clearPassword == true)
                    {
                        instance.Attributes["syed_password"] = "Reset_Password";
                    }

                    checkDependency = (bool)instance.Attributes["syed_checkdependency"];
                    import = (bool)instance.Attributes["syed_import"];
                    serviceProxy.Update(instance);
                    OrganizationServiceProxy targetserviceProxy = new OrganizationServiceProxy(new Uri(instance.Attributes["syed_instanceurl"].ToString()), null, clientCredentials, null);
                    targetserviceProxy.EnableProxyTypes();
                    IOrganizationService targetService = (IOrganizationService)targetserviceProxy;

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
                                    sol.GetComponentDetails(null, null, dependency, ((OptionSetValue)dependency.Attributes["dependentcomponenttype"]).Value, (Guid)dependency.Attributes["dependentcomponentobjectid"], "dependentcomponenttype", null);
                                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:#FFCC99;border: 1px solid #ccc'>");
                                    sol.GetComponentDetails(null, null, dependency, ((OptionSetValue)dependency.Attributes["requiredcomponenttype"]).Value, (Guid)dependency.Attributes["requiredcomponentobjectid"], "requiredcomponenttype", null);
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
                            checkTarget = this.CheckDependency(targetService, comDependency, sol, checkTarget, serviceProxy);
                        }
                    }

                    if (!checkTarget && import == true)
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<tr>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:tomato;border: 1px solid #ccc'>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("All dependent components are present in target instance");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:tomato;border: 1px solid #ccc'>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("----");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</tr>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</table><br><br>");
                        this.ImportSolution(targetService, solutionFile, new Uri(instance.Attributes["syed_instanceurl"].ToString()));
                    }
                    else if (checkDependency)
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
            string encryptionKey = Constants.EncryptionKey;
            encryptString = encryptString.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }

                    encryptString = Encoding.Unicode.GetString(ms.ToArray());
                }
            }

            return encryptString;
        }

        /// <summary>
        /// Method exports solution
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <param name="solutionFile">solution file info</param>
        /// <param name="solutionName">solution name</param>
        /// <param name="message">message to be printed on console</param>
        /// <param name="isManaged">Managed Property</param>
        private void ExportSolution(IOrganizationService serviceProxy, SolutionFileInfo solutionFile, string solutionName, string message, bool isManaged)
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
        /// <param name="mode">web or Scheduled mode</param>
        /// <returns>returns entity collection</returns>
        private EntityCollection FetchSourceControlQueues(IOrganizationService serviceProxy, string mode)
        {
            QueryExpression querySampleSolution = new QueryExpression
            {
                EntityName = Constants.SounceControlQueue,
                ColumnSet = new ColumnSet() { AllColumns = true },
                Criteria = new FilterExpression()
            };
            if (mode == Constants.ArgumentWEB)
            {
                querySampleSolution.Criteria.AddCondition(Constants.SourceControlQueueAttributeNameForStatus, ConditionOperator.Equal, Constants.SourceControlQueueQueuedStatus);
            }
            else
            {
                querySampleSolution.Criteria.AddCondition(Constants.SourceControlQueueAttributeNameForStatus, ConditionOperator.Equal, Constants.SourceControlQueueQueuedStatus);
                querySampleSolution.Criteria.AddCondition(Constants.SourceControlQueueAttributeNameForIsScheduled, ConditionOperator.Equal, true);
            }

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
        /// <param name="solutionUniqueName">solution unique name</param>
        /// <returns>returns components dependency entity collection</returns>
        private List<EntityCollection> GetDependentComponents(IOrganizationService serviceProxy, Guid masterSolutionId, string solutionUniqueName)
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
        private DataCollection<Entity> RetrieveComponentsFromSolutions(IOrganizationService serviceProxy, Guid masterSolutionId)
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
        /// <param name="solutionManager">solution manager</param>
        /// <param name="checkTarget">check target</param>
        /// <param name="sourceServiceProxy">source service proxy</param>
        /// <returns>returns boolean value</returns>
        private bool CheckDependency(IOrganizationService serviceProxy, EntityCollection dependencyComponents, SolutionManager solutionManager, bool checkTarget, IOrganizationService sourceServiceProxy)
        {
            foreach (Entity component in dependencyComponents.Entities)
            {
                try
                {
                    solutionManager.GetComponentDetails(null, null, component, ((OptionSetValue)component.Attributes["requiredcomponenttype"]).Value, (Guid)component.Attributes["requiredcomponentobjectid"], "requiredcomponenttype", serviceProxy);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<tr>");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:tomato;border: 1px solid #ccc'>");
                    Console.WriteLine("The below component was not present in Target");
                    solutionManager.GetComponentDetails(null, null, component, ((OptionSetValue)component.Attributes["dependentcomponenttype"]).Value, (Guid)component.Attributes["dependentcomponentobjectid"], "dependentcomponenttype", null);
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:tomato;border: 1px solid #ccc'>");
                    solutionManager.GetComponentDetails(null, null, component, ((OptionSetValue)component.Attributes["requiredcomponenttype"]).Value, (Guid)component.Attributes["requiredcomponentobjectid"], "requiredcomponenttype", null);
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</tr>");
                    checkTarget = true;
                }
            }

            return checkTarget;
        }
    }
}
