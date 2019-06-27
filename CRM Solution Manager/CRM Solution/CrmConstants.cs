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
    using System.ServiceModel.Description;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// class contains constants for dynamics
    /// </summary>
    internal class CrmConstants : ConfigurationSettings
    {
        private ClientCredentials clientCredentials;
        private static string solutionPackagerPath;
        private static string multilpleSolutionsImport;
        private static string solutionToBeImported;
        private static string solutionPackagerRelativePath;
        private static string sleepTimeoutInMillis;

        /// <summary>
        /// client credentials
        /// </summary>
        private ClientCredentials clientCredentials;

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
                return solutionPackagerPath;
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
                return multilpleSolutionsImport;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets SolutionToBeImported PS path
        /// </summary>
        public string SolutionToBeImported
        {
            get
            {
                return solutionToBeImported;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets solution packager relative path
        /// </summary>
        public string SolutionPackagerRelativePath
        {
            get
            {
                return solutionPackagerRelativePath;
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
                return sleepTimeoutInMillis;
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
            this.clientCredentials.UserName.UserName = DynamicsUserName;
            this.clientCredentials.UserName.Password = DynamicsPassword;
            var serviceProxy = this.InitializeOrganizationService();
            EntityCollection retrievedConfigurationSettingsList = this.RetrieveConfigurationSettings(serviceProxy);

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
                        solutionPackagerPath = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.MultilpleSolutionsImport:
                        multilpleSolutionsImport = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.SolutionToBeImported:
                        solutionToBeImported = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.SolutionPackagerRelativePath:
                        solutionPackagerRelativePath = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.SleepTimeoutInMillis:
                        sleepTimeoutInMillis = setting.GetAttributeValue<string>("syed_value");
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
            return new OrganizationServiceProxy(new Uri(OrgServiceUrl), null, this.clientCredentials, null);
        }
    }
}
