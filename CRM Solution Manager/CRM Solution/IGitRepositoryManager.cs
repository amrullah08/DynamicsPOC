//-----------------------------------------------------------------------
// <copyright file="IGitRepositoryManager.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    /// <summary>
    /// interface for repository manager
    /// </summary>
    public interface IGitRepositoryManager
    {
        /// <summary>
        /// Method commits all changes
        /// </summary>
        /// <param name="solutionFileInfo">solution file info</param>
        /// <param name="solutionFilePath">path of file having release solution list</param>
        void CommitAllChanges(SolutionFileInfo solutionFileInfo, string solutionFilePath);

        /// <summary>
        /// Method pushes commits to the repository
        /// </summary>
        void PushCommits();

        /// <summary>
        /// Method fetches or clones the repository
        /// </summary>
        void UpdateRepository();
    }
}
