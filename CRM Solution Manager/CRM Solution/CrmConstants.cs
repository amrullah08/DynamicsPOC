//-----------------------------------------------------------------------
// <copyright file="CrmConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    using System.Configuration;

    /// <summary>
    /// class contains constants for dynamics
    /// </summary>
    internal class CrmConstants
    {
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
                return ConfigurationManager.AppSettings["SolutionPackagerPath"];
            }
        }

        /// <summary>
        /// Gets MultilpleSolutionsImport PS path
        /// </summary>
        public static string MultilpleSolutionsImport
        {
            get
            {
                return ConfigurationManager.AppSettings["MultilpleSolutionsImport"];
            }
        }

        /// <summary>
        /// Gets SolutionToBeImported PS path
        /// </summary>
        public static string SolutionToBeImported
        {
            get
            {
                return ConfigurationManager.AppSettings["SolutionToBeImported"];
            }
        }

        /// <summary>
        /// Gets solution packager relative path
        /// </summary>
        public static string SolutionPackagerRelativePath
        {
            get
            {
                return ConfigurationManager.AppSettings["SolutionPackagerRelativePath"];
            }
        }
        
        /// <summary>
        /// Gets sleep timeout in millis
        /// </summary>
        public static string SleepTimeoutInMillis
        {
            get
            {
                return ConfigurationManager.AppSettings["SleepTimeoutInMillis"];
            }
        }
    }
}
