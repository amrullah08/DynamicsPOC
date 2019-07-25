//-----------------------------------------------------------------------
// <copyright file="ICrmSolutionHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace MyFunction
{
    using System.Collections.Generic;

    /// <summary>
    /// interface for solution helper
    /// </summary>
    internal interface ICrmSolutionHelper
    {
        /// <summary>
        /// Gets or sets a value of http repository url
        /// </summary>
        string RepositoryUrl { get; set; }

        /// <summary>
        /// Gets or sets a value of branch
        /// </summary>
        string Branch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether repository push can be done or not
        /// </summary>
        bool CanPush { get; set; }

        /// <summary>
        /// Gets or sets collection of solution file info
        /// </summary>
        List<SolutionFileInfo> SolutionFileInfos { get; set; }

        /// <summary>
        /// Method download solutions file
        /// </summary>
        /// <param name="solutionUnqiueName">unique solution name</param>
        /// <returns>returns list of downloaded solutions</returns>
        List<SolutionFileInfo> DownloadSolutionFile(string solutionUnqiueName);
    }
}
