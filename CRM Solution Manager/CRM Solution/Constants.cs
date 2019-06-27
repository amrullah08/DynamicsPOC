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
    public class Constants
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
        /// Value for Import Successful status
        /// </summary>
        public const string SourceControlQueueImportSuccessfulStatus = "Import Successful";

        /// <summary>
        /// Value for Publish Successful status
        /// </summary>
        public const string SourceControlQueuePublishSuccessfulStatus = "Publish all Customizations successful";

        /// <summary>
        /// Value for Pushing to repository status
        /// </summary>
        public const string SourceControlQueuemPushingToStatus = "Pushing to Repository";

        /// <summary>
        /// Value for successful push to repo status
        /// </summary>
        public const string SourceControlQueuemPushToRepositorySuccessStatus = "Successfully Pushed To Repository";

        /// <summary>
        /// Overwrite Solutions.txt column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForOverwriteSolutionsTxt = "syed_overwritesolutionstxt";

        /// <summary>
        /// repository location column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForRepositoryUrl = "syed_repositoryurl";

        /// <summary>
        /// branch name column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForBranch = "syed_branch";

        /// <summary>
        /// remote name column name
        /// </summary>
        public const string SourceControlQueueAttributeNameForRemote = "syed_remotename";

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
        ///  solution packager path
        /// </summary>
        public const string SolutionPackagerPath = "SolutionPackagerPath";

        /// <summary>
        ///  multilple solutions import
        /// </summary>
        public const string MultilpleSolutionsImport = "MultilpleSolutionsImport";

        /// <summary>
        ///  solution to be imported
        /// </summary>
        public const string SolutionToBeImported = "SolutionToBeImported";

        /// <summary>
        ///  solution packager relative path
        /// </summary>
        public const string SolutionPackagerRelativePath = "SolutionPackagerRelativePath";

        /// <summary>
        ///  sleep timeout in millis
        /// </summary>
        public const string SleepTimeoutInMillis = "SleepTimeoutInMillis";

        /// <summary>
        ///  repository solution folder
        /// </summary>
        public const string RepositorySolutionFolder = "RepositorySolutionFolder";

        /// <summary>
        ///  repository js directory
        /// </summary>
        public const string RepositoryJsDirectory = "RepositoryJsDirectory";

        /// <summary>
        ///  repository html directory
        /// </summary>
        public const string RepositoryHtmlDirectory = "RepositoryHtmlDirectory";

        /// <summary>
        ///  repository images directory
        /// </summary>
        public const string RepositoryImagesDirectory = "RepositoryImagesDirectory";

        /// <summary>
        ///  repository url
        /// </summary>
        public const string RepositoryUrl = "RepositoryUrl";

        /// <summary>
        ///  clone repository always
        /// </summary>
        public const string CloneRepositoryAlways = "CloneRepositoryAlways";

        /// <summary>
        ///  remote name
        /// </summary>
        public const string RemoteName = "RemoteName";

        /// <summary>
        ///  branch name
        /// </summary>
        public const string BranchName = "BranchName";

        /// <summary>
        /// Gets the component types
        /// </summary>
        public List<int> ComponentTypes
        {
            get => new List<int>
            {
                Entity, // entities
                    Attribute, // Attribut
                    Relationship, // Relationship
                    EntityKey, // Key
                    DisplayString, // Display string
                    24, // Form
                    SavedQuery, // View
                    SavedQueryVisualization, // Chart
                    SystemForm, // System Form
                    HierarchyRule, // Hierarchical rules
                    SiteMap, // sitempas
                    RibbonCustomization, // ribbon
                    OptionSet, // Global optionsets
                    PluginAssembly, // plugin assemblies
                    SDKMessageProcessingStep, // Sdk message processing steps
                    ServiceEndpoint, // service endpoints
                    Report, // Reports
                    Role, // Roles
                    FieldSecurityProfile, // field security profile
                    ConnectionRole, // connection roles
                    WebResources, // web resources
                    Workflow, // work flows
                    KBArticleTemplate, // kb article templates
                    MailMergeTemplate, // mail merge templates
                    ContractTemplate, // contract templates
                    EmailTemplate, // email templates                    
                    RoutingRule, // routing rules
                    SLA, // slas
                    ConvertRule // convert rules
                };
        }

        public const int Entity = 1;
        public const int WebResources = 61;
        public const int Attribute = 2;
        public const int Relationship = 10;
        public const int EntityKey = 14;
        public const int DisplayString = 22;
        public const int SavedQuery = 26;
        public const int SavedQueryVisualization = 59;
        public const int SystemForm = 60;
        public const int HierarchyRule = 65;
        public const int SiteMap = 62;
        public const int RibbonCustomization = 50;
        public const int OptionSet = 9;
        public const int PluginAssembly = 91;
        public const int SDKMessageProcessingStep = 92;
        public const int ServiceEndpoint = 95;
        public const int Report = 31;
        public const int Role = 20;
        public const int FieldSecurityProfile = 70;
        public const int ConnectionRole = 63;
        public const int Workflow = 29;
        public const int KBArticleTemplate = 38;
        public const int MailMergeTemplate = 39;
        public const int ContractTemplate = 37;
        public const int EmailTemplate = 36;
        public const int RoutingRule = 150;
        public const int SLA = 152;
        public const int ConvertRule = 154;

    }
}