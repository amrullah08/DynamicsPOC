using Microsoft.Xrm.Sdk;

namespace CrmSolution
{
    /// <summary>
    /// Abstract class for configuration settings
    /// </summary>
    abstract class ConfigurationSettings
    {
        /// <summary>
        /// Methods gets all configuration settings record
        /// </summary>
        /// <returns></returns>
        public virtual EntityCollection GetConfigurationSettings()
        {
            return new EntityCollection();
        }

        /// <summary>
        /// Method set crm constant properties
        /// </summary>
        /// <param name="entityCollection">entity collection</param>
        public virtual void SetCrmProperties(EntityCollection entityCollection)
        { }

        /// <summary>
        /// Method sets repository configuration constant properties
        /// </summary>
        /// <param name="entityCollection">entity collection</param>
        public virtual void SetRepositoryConfigurationProperties(EntityCollection entityCollection)
        { }
    }
}
