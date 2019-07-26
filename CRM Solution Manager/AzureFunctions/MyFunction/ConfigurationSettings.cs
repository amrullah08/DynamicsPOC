//-----------------------------------------------------------------------
// <copyright file="ConfigurationSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace MyFunction
{
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Abstract class for configuration settings
    /// </summary>
    public abstract class ConfigurationSettings
    {
        /// <summary>
        /// Methods gets all configuration settings record
        /// </summary>
        /// <param name="orgUrl">org url</param>
        /// <param name="clientId">client id</param>
        /// <param name="clientSecret">client secret</param>       
        /// <param name="tenantId">tenant id</param>
        /// <returns>returns entity collection</returns>
        public virtual EntityCollection GetConfigurationSettings(string orgUrl, string clientId, string clientSecret, string tenantId)
        {
            return new EntityCollection();
        }

        /// <summary>
        /// Method set CRM constant properties
        /// </summary>
        /// <param name="entityCollection">entity collection</param>
        public virtual void SetCrmProperties(EntityCollection entityCollection)
        {
        }

        /// <summary>
        /// Method sets repository configuration constant properties
        /// </summary>
        /// <param name="entityCollection">entity collection</param>
        public virtual void SetRepositoryConfigurationProperties(EntityCollection entityCollection)
        {
        }
    }
}
