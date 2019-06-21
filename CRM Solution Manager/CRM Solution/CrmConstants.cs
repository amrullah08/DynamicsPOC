//-----------------------------------------------------------------------
// <copyright file="CrmConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Query;
    using System;
    using System.Configuration;
    using System.ServiceModel.Description;

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
        /// Gets organization service url
        /// </summary>
        public static string OrgServiceUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["OrgServiceUrl"];
            }
        }

        /// <summary>
        /// Gets Dynamics User Name
        /// </summary>
        public static string DynamicsUserName
        {
            get
            {
                return ConfigurationManager.AppSettings["DynamicsUserName"];
            }
        }

        /// <summary>
        /// Gets Dynamics password
        /// </summary>
        public static string DynamicsPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["DynamicsPassword"];
            }
        }

        /// <summary>
        /// Gets solution packager path
        /// </summary>
        public static string SolutionPackagerPath
        {
            get
            {
                return solutionPackagerPath;
            }
            set { }
        }

        /// <summary>
        /// Gets MultilpleSolutionsImport PS path
        /// </summary>
        public static string MultilpleSolutionsImport
        {
            get
            {
                return multilpleSolutionsImport;
            }
            set { }
        }

        /// <summary>
        /// Gets SolutionToBeImported PS path
        /// </summary>
        public static string SolutionToBeImported
        {
            get
            {
                return solutionToBeImported;
            }
            set { }
        }

        /// <summary>
        /// Gets solution packager relative path
        /// </summary>
        public static string SolutionPackagerRelativePath
        {
            get
            {
                return solutionPackagerRelativePath;
            }
            set { }
        }


        /// <summary>
        /// Gets sleep timeout in millis
        /// </summary>
        public static string SleepTimeoutInMillis
        {
            get
            {
                return sleepTimeoutInMillis;
            }
            set { }
        }

        /// <summary>
        /// Method returns configuration settings entity collection list
        /// </summary>
        public override EntityCollection GetConfigurationSettings()
        {
            this.clientCredentials = new ClientCredentials();
            clientCredentials.UserName.UserName = DynamicsUserName;
            clientCredentials.UserName.Password = DynamicsPassword;
            var serviceProxy = this.InitializeOrganizationService();
            EntityCollection retrievedConfigurationSettingsList = RetrieveConfigurationSettings(serviceProxy);

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
                    case "SolutionPackagerPath":
                        solutionPackagerPath = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case "MultilpleSolutionsImport":
                        multilpleSolutionsImport = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case "SolutionToBeImported":
                        solutionToBeImported = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case "SolutionPackagerRelativePath":
                        solutionPackagerRelativePath = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case "SleepTimeoutInMillis":
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
        /// <returns></returns>
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
        /// <returns></returns>
        private OrganizationServiceProxy InitializeOrganizationService()
        {
            return new OrganizationServiceProxy(new Uri(OrgServiceUrl), null, this.clientCredentials, null);
        }
    }
}
