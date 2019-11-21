//-----------------------------------------------------------------------
// <copyright file="CrmConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolutionLibrary
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.ServiceModel.Description;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Sdk.WebServiceClient;

    /// <summary>
    /// class contains constants for dynamics
    /// </summary>
    public class CrmConstants : ConfigurationSettings
    {
        ///Naresh
        ///

        /// Solution Folder
        /// </summary>
        private string solutionFolder;

        /// <summary>
        /// JavaScript Directory
        /// </summary>
        private string javaScriptDirectory;

        /// <summary>
        /// Html Directory
        /// </summary>
        private string htmlDirectory;

        /// <summary>
        /// Images Directory
        /// </summary>
        private string imagesDirectory;

        /// <summary>
        /// Repository Url
        /// </summary>
        private string repositoryUrl;


        /// <summary>
        /// Repository Remote Name
        /// </summary>
        private string repositoryRemoteName;

        /// <summary>
        /// Branch Name
        /// </summary>
        private string branchName;

        /// <summary>
        /// solution text file 
        /// </summary>
        private string solutionText;

        /// <summary>
        /// Time Trigger text file 
        /// </summary>
        private string timeTriggerPath;

        /// <summary>
        /// solution text file 
        /// </summary>
        private string solutionCheckerPath;

        /// <summary>
        /// solution text file 
        /// </summary>
        private string solutionTextRelease;

        /// <summary>
        /// Gets or sets repository release directory containing CRM Solutions
        /// </summary>
        public string BranchName
        {
            get
            {
                return this.branchName;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository release directory containing CRM Solutions
        /// </summary>
        public string SolutionFolder
        {
            get
            {
                return this.solutionFolder;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository script directory
        /// </summary>
        public string JsDirectory
        {
            get
            {
                return this.javaScriptDirectory;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository solution text file
        /// </summary>
        public string SolutionText
        {
            get
            {
                return this.solutionText;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets solution checker path file
        /// </summary>
        public string SolutionCheckerPath
        {
            get
            {
                return this.solutionCheckerPath;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets solution checker path file
        /// </summary>
        public string TimeTriggerPath
        {
            get
            {
                return this.timeTriggerPath;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository solution text file for Release
        /// </summary>
        public string SolutionTextRelease
        {
            get
            {
                return this.solutionTextRelease;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository html directory
        /// </summary>
        public string HtmlDirectory
        {
            get
            {
                return this.htmlDirectory;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository Images directory
        /// </summary>
        public string ImagesDirectory
        {
            get
            {
                return this.imagesDirectory;
            }

            set
            {
            }
        }


        /// <summary>
        /// Gets or sets repository url
        /// </summary>
        public string RepositoryUrl
        {
            get
            {
                return this.repositoryUrl;
            }

            set
            {
            }
        }

        //Naresh

        /// <summary>
        /// Solution Packager Path
        /// </summary>
        private string solutionPackagerPath;

        /// <summary>
        /// Multiple Solutions Import
        /// </summary>
        private string multilpleSolutionsImport;

        /// <summary>
        /// Solution To Be Imported
        /// </summary>
        private string solutionToBeImported;

        /// <summary>
        /// Solution Packager Relative Path
        /// </summary>
        private string solutionPackagerRelativePath;

        /// <summary>
        /// Sleep Timeout In Millis
        /// </summary>
        private string sleepTimeoutInMillis;

        /// <summary>
        /// client credentials
        /// </summary>
        private ClientCredentials clientCredentials;

        /// <summary>
        /// service proxy
        /// </summary>
        private IOrganizationService serviceProxy;

        /// <summary>
        /// power apps checker azure client app id
        /// </summary>
        private string powerAppsCheckerAzureClientAppId;

        /// <summary>
        /// power apps checker azure tenant id
        /// </summary>
        private string powerAppsCheckerAzureTenantId;

        /// <summary>
        /// Gets organization service url
        /// </summary>
        public string OrgServiceUrl
        {
            get
            {
                //  return ConfigurationManager.AppSettings["CRMSourceInstanceUrl"]; // if using from webjob

                return ConfigurationManager.AppSettings["CRMSourceServiceUrl"]; // if using Azure function with secrets and cleintid
            }
        }

        /// <summary>
        /// Gets Dynamics User Name
        /// </summary>
        public string DynamicsUserName
        {
            get
            {
                return ConfigurationManager.AppSettings["CRMSourceUserName"];
            }
        }

        /// <summary>
        /// Gets or Sets Dynamics password
        /// </summary>
        public string DynamicsPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["CRMSourcePassword"];
            }
        }

        /// <summary>
        /// Gets or sets solution packager path
        /// </summary>
        public string SolutionPackagerPath
        {
            get
            {
                return this.solutionPackagerPath;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets MultipleSolutionsImport PS path
        /// </summary>
        public string MultilpleSolutionsImport
        {
            get
            {
                return this.multilpleSolutionsImport;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets SolutionToBeImported PS path
        /// </summary>
        public string SolutionToBeImported
        {
            get
            {
                return this.solutionToBeImported;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets solution packager relative path
        /// </summary>
        public string SolutionPackagerRelativePath
        {
            get
            {
                return this.solutionPackagerRelativePath;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets sleep timeout in millis
        /// </summary>
        public string SleepTimeoutInMillis
        {
            get
            {
                return this.sleepTimeoutInMillis;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets Organization Service Proxy
        /// </summary>
        public IOrganizationService ServiceProxy
        {
            get
            {
                return this.serviceProxy;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets power apps checker azure client app id
        /// </summary>
        public string PowerAppsCheckerAzureClientAppId
        {
            get
            {
                return this.powerAppsCheckerAzureClientAppId;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets power apps checker azure tenant id
        /// </summary>
        public string PowerAppsCheckerAzureTenantId
        {
            get
            {
                return this.powerAppsCheckerAzureTenantId;
            }

            set
            {
            }
        }

                    
        /// <summary>
        /// Method returns configuration settings entity collection list
        /// </summary>
        /// <returns>returns entity collection</returns>
        public override EntityCollection GetConfigurationSettings()
        {
            this.clientCredentials = new ClientCredentials();

            this.clientCredentials.UserName.UserName = this.DynamicsUserName;
            this.clientCredentials.UserName.Password = this.DynamicsPassword;

            //if (this.DynamicsUserName == "CRMSourceUserName" && this.DynamicsPassword == "CRMSourcePassword") // TODO : Naresh // User Name and password won't work and client id and secret need to use 
            //{
            Task<string> callTask = Task.Run(() => this.AccessTokenGenerator());
                callTask.Wait();
                string token = callTask.Result;
                //Uri serviceUrl = new Uri(ConfigurationManager.AppSettings["CRMSourceInstanceUrl"] + @"/xrmservices/2011/organization.svc/web?SdkClientVersion=8.2"); // if calling from web job 
                Uri serviceUrl = new Uri(ConfigurationManager.AppSettings["CRMSourceInstanceUrl"] + @"/xrmservices/2011/organization.svc/web?SdkClientVersion=8.2"); // if calling from azure function
                OrganizationWebProxyClient sdkService = null;
                using (sdkService = new OrganizationWebProxyClient(serviceUrl, false))
                {
                    sdkService.HeaderToken = token;
                    sdkService.InnerChannel.OperationTimeout = new TimeSpan(1, 30, 0);
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    this.serviceProxy = (IOrganizationService)sdkService != null ? (IOrganizationService)sdkService : null;
                }
            //} // TODO : Naresh // User Name and password won't work and client id and secret need to use 
            //else
            //{
            //    this.serviceProxy = this.InitializeOrganizationService();
            //}

            EntityCollection retrievedConfigurationSettingsList = this.RetrieveConfigurationSettings(this.serviceProxy);
            return retrievedConfigurationSettingsList;
        }

        /// <summary>
        /// Method generated access token to authenticate dynamics CRM
        /// </summary>
        /// <returns>returns access token</returns>
        private async Task<string> AccessTokenGenerator()
        {
            string authority = "https://login.microsoftonline.com/" + ConfigurationManager.AppSettings["TenantId"];
            var credentials = new ClientCredential(ConfigurationManager.AppSettings["SolutionCheckerAppClientId"], ConfigurationManager.AppSettings["ClientApplicationSecret"]);
            var authContext = new AuthenticationContext(authority);
            var result = await authContext.AcquireTokenAsync(ConfigurationManager.AppSettings["CRMSourceInstanceUrl"], credentials).ConfigureAwait(true); // if calling from webjob
            //var result = await authContext.AcquireTokenAsync(ConfigurationManager.AppSettings["CRMSourceServiceUrl"], credentials).ConfigureAwait(true); // if calling azure function 
            return result.AccessToken;
        }


        /// <summary>
        /// Method sets crm constant property values
        /// </summary>
        /// <param name="retrievedConfigurationSettingsList">entity collection</param>
        public override void SetCrmProperties(EntityCollection retrievedConfigurationSettingsList)
        {
            foreach (Entity setting in retrievedConfigurationSettingsList.Entities)
            {
                string key = setting.GetAttributeValue<string>("syed_name");

                switch (key)
                {
                    case Constants.SolutionPackagerPath:
                        this.solutionPackagerPath = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.MultilpleSolutionsImport:
                        this.multilpleSolutionsImport = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.SolutionToBeImported:
                        this.solutionToBeImported = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.SolutionPackagerRelativePath:
                        this.solutionPackagerRelativePath = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.SleepTimeoutInMillis:
                        this.sleepTimeoutInMillis = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.PowerAppsCheckerAzureClientAppId:
                        this.powerAppsCheckerAzureClientAppId = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.PowerAppsCheckerAzureTenantId:
                        this.powerAppsCheckerAzureTenantId = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositorySolutionFolder:
                        this.solutionFolder = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.SolutionTextPath:
                        this.solutionText = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.SolutionCheckerPath:
                        this.solutionCheckerPath = setting.GetAttributeValue<string>("syed_value");
                        break;

                    case Constants.TimeTriggerPath:
                        this.timeTriggerPath = setting.GetAttributeValue<string>("syed_value");
                        break;

                    case Constants.SolutionTextPathForRelease:
                        this.solutionTextRelease = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositoryJsDirectory:
                        this.javaScriptDirectory = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositoryHtmlDirectory:
                        this.htmlDirectory = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositoryImagesDirectory:
                        this.imagesDirectory = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositoryUrl:
                        this.repositoryUrl = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RemoteName:
                        this.repositoryRemoteName = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.BranchName:
                        this.branchName = setting.GetAttributeValue<string>("syed_value");
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Method retrieves active configuration settings record
        /// </summary>
        /// <param name="serviceProxy">organization service proxy</param>
        /// <returns>returns entity collection</returns>
        private EntityCollection RetrieveConfigurationSettings(IOrganizationService serviceProxy)
        {
            try
            {
                string fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                    <entity name = 'syed_configurationsettings'>
                                    <attribute name = 'syed_name' />
                                    <attribute name = 'syed_value' />
                                    <attribute name = 'syed_configurationsettingsid' />
                                    <order descending = 'false' attribute = 'syed_name'/>
                                    <filter type = 'and'>
                                    <condition attribute = 'statecode' value = '0' operator= 'eq' />
                                    </filter>
                                    </entity>
                                    </fetch>";

                EntityCollection configurationSettingsList = serviceProxy.RetrieveMultiple(new FetchExpression(fetchXML));
                return configurationSettingsList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Method returns new instance of organization service
        /// </summary>
        /// <returns>returns organization service proxy</returns>
        private IOrganizationService InitializeOrganizationService()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            OrganizationServiceProxy organizationServiceProxy = new OrganizationServiceProxy(new Uri(this.OrgServiceUrl), null, this.clientCredentials, null);
            organizationServiceProxy.Timeout = new TimeSpan(1, 30, 0);
            IOrganizationService organizationService = (IOrganizationService)organizationServiceProxy;
            return organizationService;
        }
    }
}
