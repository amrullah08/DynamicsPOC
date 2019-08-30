//-----------------------------------------------------------------------
// <copyright file="CrmConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.ServiceModel.Description;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;

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
        /// client credentials
        /// </summary>
        private ClientCredentials clientCredentials;

        /// <summary>
        /// service proxy
        /// </summary>
        private OrganizationServiceProxy serviceProxy;

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
                return ConfigurationManager.AppSettings["OrgServiceUrl"];
            }
        }

        /// <summary>
        /// Gets Dynamics User Name
        /// </summary>
        public string DynamicsUserName
        {
            get
            {
                return ConfigurationManager.AppSettings["DynamicsUserName"];
            }
        }

        /// <summary>
        /// Gets or Sets Dynamics password
        /// </summary>
        public string DynamicsPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["DynamicsPassword"];
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
        public OrganizationServiceProxy ServiceProxy
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
            this.serviceProxy = this.InitializeOrganizationService();

            EntityCollection retrievedConfigurationSettingsList = this.RetrieveConfigurationSettings(this.serviceProxy);

            return retrievedConfigurationSettingsList;
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
        private EntityCollection RetrieveConfigurationSettings(OrganizationServiceProxy serviceProxy)
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

        /// <summary>
        /// Method returns new instance of organization service
        /// </summary>
        /// <returns>returns organization service proxy</returns>
        private OrganizationServiceProxy InitializeOrganizationService()
        {

            //Security Protocol as TLS12
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            OrganizationServiceProxy organizationServiceProxy = new OrganizationServiceProxy(new Uri(this.OrgServiceUrl), null, this.clientCredentials, null);
            organizationServiceProxy.Timeout = new TimeSpan(0, 30, 0);
            return organizationServiceProxy;
        }
    }
}
