//-----------------------------------------------------------------------
// <copyright file="ConfigurationSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
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
        /// <returns>returns entity collection</returns>
        public virtual EntityCollection GetConfigurationSettings()
        {
            return new EntityCollection();
        }

        /// <summary>
        /// Method set crm constant properties
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
