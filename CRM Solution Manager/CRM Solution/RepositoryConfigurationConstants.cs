//-----------------------------------------------------------------------
// <copyright file="RepositoryConfigurationConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    using System.Configuration;

    /// <summary>
    /// constants file for repository configurations
    /// </summary>
    internal class RepositoryConfigurationConstants
    {
        /// <summary>
        /// Gets Repository local directory
        /// </summary>
        public static string LocalDirectory
        {
            get
            {
                return ConfigurationManager.AppSettings["RepositoryLocalDirectory"];
            }
        }

        /// <summary>
        /// Gets repository script directory
        /// </summary>
        public static string JsDirectory
        {
            get
            {
                return ConfigurationManager.AppSettings["RepositoryJsDirectory"];
            }
        }

        /// <summary>
        /// Gets repository html directory
        /// </summary>
        public static string HtmlDirectory
        {
            get
            {
                return ConfigurationManager.AppSettings["RepositoryHtmlDirectory"];
            }
        }

        /// <summary>
        /// Gets repository Images directory
        /// </summary>
        public static string ImagesDirectory
        {
            get
            {
                return ConfigurationManager.AppSettings["RepositoryImagesDirectory"];
            }
        }
    }
}
