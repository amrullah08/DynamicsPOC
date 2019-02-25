//-----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    using System.Collections.Generic;

    /// <summary>
    /// class for keeping constants
    /// </summary>
    internal class Constants
    {
        /// <summary>
        /// Entity Source Control Queue
        /// </summary>
        public const string SounceControlQueue = "syed_sourcecontrolqueue";

        /// <summary>
        /// status column
        /// </summary>
        public const string SourceControlQueueAttributeNameForStatus = "syed_status";

        /// <summary>
        /// Value for completed status
        /// </summary>
        public const string SourceControlQueueCompletedStatus = "Completed";

        /// <summary>
        /// Value for completed status
        /// </summary>
        public const string SourceControlQueueQueuedStatus = "Queued";

        /// <summary>
        /// Value for Exporting status
        /// </summary>
        public const string SourceControlQueueExportStatus = "Exporting";

        /// <summary>
        /// Value for Export successful status
        /// </summary>
        public const string SourceControlQueueExportSuccessful = "Export Successful";

        /// <summary>
        /// Value for Merging solutions status
        /// </summary>
        public const string SourceControlQueuemMergingStatus = "Merging Solutions";

        /// <summary>
        /// Value for Merge Successful status
        /// </summary>
        public const string SourceControlQueuemMergingSuccessfulStatus = "Merge Successful";

        /// <summary>
        /// Value for Pushing to repository status
        /// </summary>
        public const string SourceControlQueuemPushingToStatus = "Pushing to Repository";

        /// <summary>
        /// Value for successful push to repo status
        /// </summary>
        public const string SourceControlQueuemPushToRepositorySuccessStatus = "Successfully Pushed To Repository";

        /// <summary>
        /// repository location column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForRepositoryUrl = "syed_repositoryurl";

        /// <summary>
        /// branch name column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForBranch = "syed_branch";

        /// <summary>
        /// solution column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForSolutionName = "syed_solutionname";

        /// <summary>
        /// comment column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForComment = "syed_comment";

        /// <summary>
        /// comment column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForCommitIds = "syed_commitids";

        /// <summary>
        /// owner id column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForOwnerId = "ownerid";

        /// <summary>
        /// include in release column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForIncludeInRelease = "syed_includeinrelease";

        /// <summary>
        /// check-in column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForCheckinSolution = "syed_checkin";

        /// <summary>
        ///  merge solution column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForMergeSolution = "syed_mergesolutions";

        /// <summary>
        ///  source solution column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForSourceSolutions = "syed_sourcensolutions";

        /// <summary>
        /// Gets the component types
        /// </summary>
        public static List<int> ComponentTypes
        {
            get => new List<int>
            {
                1, // entities
                    2, // Attribut
                    10, // Relationship
                    14, // Key
                    22, // Display string
                    24, // Form
                    26, // View
                    59, // Chart
                    60, // System Form
                    65, // Hierarchical rules

                    62, // sitempas
                    50, // ribbon
                    9, // Global optionsets
                    91, // plugin assemblies
                    92, // Sdk message processing steps
                    95, // service endpoints
                    31, // Reports
                    20, // Roles
                    70, // field security profile
                    63, // connection roles
                    61, // web resources
                    29, // work flows
                    38, // kb article templates
                    39, // mail merge templates
                    37, // contract templates
                    36, // email templates
                    60, // Dashboards
                    150, // routing rules
                    152, // slas
                    154 // convert rules
                };
        }
    }
}