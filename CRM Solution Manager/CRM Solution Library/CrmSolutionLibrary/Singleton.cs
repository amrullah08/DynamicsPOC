//-----------------------------------------------------------------------
// <copyright file="Singleton.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolutionLibrary
{
    /// <summary>
    /// Class that handles object creation for sub classes
    /// </summary>
    public sealed class Singleton
    {
        /// <summary>
        /// Repository Configuration Constants Instance
        /// </summary>
        private static RepositoryConfigurationConstants repositoryConfigurationConstantsInstance = null;

        /// <summary>
        /// CRM Constants Instance
        /// </summary>
        private static CrmConstants crmConstantsInstance = null;

        /// <summary>
        /// CRM Solution Helper Instance
        /// </summary>
        private static CrmSolutionHelper crmSolutionHelperInstance = null;

        /// <summary>
        /// Solution File Info Instance
        /// </summary>
        private static SolutionFileInfo solutionFileInfoInstance = null;

        /// <summary>
        /// Constants Instance
        /// </summary>
        private static Constants constantsInstance = null;

        /// <summary>
        /// Repository Helper Instance
        /// </summary>
        private static RepositoryHelper repositoryHelperInstance = null;

        /// <summary>
        /// Gets object of RepositoryConfigurationConstants class
        /// </summary>
        public static RepositoryConfigurationConstants RepositoryConfigurationConstantsInstance
        {
            get
            {
                if (repositoryConfigurationConstantsInstance == null)
                {
                    repositoryConfigurationConstantsInstance = new RepositoryConfigurationConstants();
                }

                return repositoryConfigurationConstantsInstance;
            }
        }

        /// <summary>
        /// Gets object of CRM Constants class
        /// </summary>
        public static CrmConstants CrmConstantsInstance
        {
            get
            {
                if (crmConstantsInstance == null)
                {
                    crmConstantsInstance = new CrmConstants();
                }

                return crmConstantsInstance;
            }
        }

        /// <summary>
        /// Gets object of CRM SolutionHelper class
        /// </summary>
        public static CrmSolutionHelper CrmSolutionHelperInstance
        {
            get
            {
                if (crmSolutionHelperInstance == null)
                {
                    crmSolutionHelperInstance = new CrmSolutionHelper();
                }

                return crmSolutionHelperInstance;
            }
        }

        /// <summary>
        /// Gets object of SolutionFileInfo class
        /// </summary>
        public static SolutionFileInfo SolutionFileInfoInstance
        {
            get
            {
                if (solutionFileInfoInstance == null)
                {
                    solutionFileInfoInstance = new SolutionFileInfo();
                }

                return solutionFileInfoInstance;
            }
        }

        /// <summary>
        /// Gets object of Constants class
        /// </summary>
        public static Constants ConstantsInstance
        {
            get
            {
                if (constantsInstance == null)
                {
                    constantsInstance = new Constants();
                }

                return constantsInstance;
            }
        }

        /// <summary>
        /// Gets object of RepositoryHelper class
        /// </summary>
        public static RepositoryHelper RepositoryHelperInstance
        {
            get
            {
                if (repositoryHelperInstance == null)
                {
                    repositoryHelperInstance = new RepositoryHelper();
                }

                return repositoryHelperInstance;
            }
        }
    }
}
