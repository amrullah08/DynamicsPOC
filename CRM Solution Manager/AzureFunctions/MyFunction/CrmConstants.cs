//-----------------------------------------------------------------------
// <copyright file="CrmConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace MyFunction
{
    using System;
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
        /// Client Id
        /// </summary>
        private string clientId;

        /// <summary>
        /// Client Secret
        /// </summary>
        private string clientSecret;

        /// <summary>
        /// Tenant Id
        /// </summary>
        private string tenantId;

        /// <summary>
        /// Org Url
        /// </summary>
        private string orgUrl;

        /// <summary>
        /// SDK Service
        /// </summary>
        private OrganizationWebProxyClient sdkService;

        /// <summary>
        /// org service
        /// </summary>
        private IOrganizationService service;

        /// <summary>
        /// Gets client id
        /// </summary>
        public string ClientId
        {
            get
            {
                return this.clientId;
            }
        }

        /// <summary>
        /// Gets client secret
        /// </summary>
        public string ClientSecret
        {
            get
            {
                return this.clientSecret;
            }
        }

        /// <summary>
        /// Gets tenant id
        /// </summary>
        public string TenantId
        {
            get
            {
                return this.tenantId;
            }
        }

        /// <summary>
        /// Gets org url
        /// </summary>
        public string OrgUrl
        {
            get
            {
                return this.orgUrl;
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
                return this.service;
            }

            set
            {
            }
        }

        /// <summary>
        /// Method returns configuration settings entity collection list
        /// </summary>
        /// <param name="orgUrl">org url</param>
        /// <param name="clientId">client id</param>
        /// <param name="clientSecret">client secret</param>
        /// <param name="tenantId">tenant id</param>
        /// <returns>returns entity collection</returns>
        public override EntityCollection GetConfigurationSettings(string orgUrl, string clientId, string clientSecret, string tenantId)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.tenantId = tenantId;
            this.orgUrl = orgUrl;

            Task<string> callTask = Task.Run(() => this.AccessTokenGenerator());
            callTask.Wait();
            string token = callTask.Result;
            Uri serviceUrl = new Uri(this.orgUrl + @"/xrmservices/2011/organization.svc/web?SdkClientVersion=8.2");

            using (this.sdkService = new OrganizationWebProxyClient(serviceUrl, false))
            {
                this.sdkService.HeaderToken = token;
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                this.service = (IOrganizationService)this.sdkService != null ? (IOrganizationService)this.sdkService : null;
            }
            
            EntityCollection retrievedConfigurationSettingsList = this.RetrieveConfigurationSettings(this.service);

            return retrievedConfigurationSettingsList;
        }       

        /// <summary>
        /// Method sets CRM constant property values
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
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Method generated access token to authenticate dynamics CRM
        /// </summary>
        /// <returns>returns access token</returns>
        private async Task<string> AccessTokenGenerator()
        {
            string authority = "https://login.microsoftonline.com/" + this.tenantId;

            var credentials = new ClientCredential(this.clientId, this.clientSecret);
            var authContext = new AuthenticationContext(authority);
            var result = await authContext.AcquireTokenAsync(this.orgUrl, credentials);
            return result.AccessToken;
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
                throw new Exception(ex.Message.ToString(), ex);
            }
        }      
    }
}
