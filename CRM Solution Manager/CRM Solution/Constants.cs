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
        /// Value for Missing Components in Target
        /// </summary>
        public const string SourceControlQueueMissingComponents = "Target Instance missing required components";

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
        ///  multiple solutions import
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
        ///  repository java script directory
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
        ///  Entity value
        /// </summary>
        public const int Entity = 1;

        /// <summary>
        ///  WebResources value
        /// </summary>
        public const int WebResources = 61;

        /// <summary>
        ///  Attribute value
        /// </summary>
        public const int Attribute = 2;

        /// <summary>
        ///  Relationship value
        /// </summary>
        public const int Relationship = 10;

        /// <summary>
        ///  Entity Key value
        /// </summary>
        public const int EntityKey = 14;

        /// <summary>
        ///  Display String value
        /// </summary>
        public const int DisplayString = 22;

        /// <summary>
        ///  Saved Query value
        /// </summary>
        public const int SavedQuery = 26;

        /// <summary>
        ///  Saved Query Visualization value
        /// </summary>
        public const int SavedQueryVisualization = 59;

        /// <summary>
        ///  System Form value
        /// </summary>
        public const int SystemForm = 60;

        /// <summary>
        ///  Hierarchy Rule value
        /// </summary>
        public const int HierarchyRule = 65;

        /// <summary>
        ///  SiteMap value
        /// </summary>
        public const int SiteMap = 62;

        /// <summary>
        ///  Ribbon Customization value
        /// </summary>
        public const int RibbonCustomization = 50;

        /// <summary>
        ///  OptionSet value
        /// </summary>
        public const int OptionSet = 9;

        /// <summary>
        ///  Plugin Assembly value
        /// </summary>
        public const int PluginAssembly = 91;

        /// <summary>
        ///  SDK Message Processing Step value
        /// </summary>
        public const int SDKMessageProcessingStep = 92;

        /// <summary>
        ///  Service End point value
        /// </summary>
        public const int ServiceEndpoint = 95;

        /// <summary>
        ///  Report value
        /// </summary>
        public const int Report = 31;

        /// <summary>
        ///  Role value
        /// </summary>
        public const int Role = 20;

        /// <summary>
        ///  Field Security Profile value
        /// </summary>
        public const int FieldSecurityProfile = 70;

        /// <summary>
        ///  ConnectionRole value
        /// </summary>
        public const int ConnectionRole = 63;

        /// <summary>
        ///  Work flow value
        /// </summary>
        public const int Workflow = 29;

        /// <summary>
        ///  KB Article Template value
        /// </summary>
        public const int KBArticleTemplate = 38;

        /// <summary>
        /// Mail Merge Template value
        /// </summary>
        public const int MailMergeTemplate = 39;

        /// <summary>
        /// Contract Template value
        /// </summary>
        public const int ContractTemplate = 37;

        /// <summary>
        /// Email Template value
        /// </summary>
        public const int EmailTemplate = 36;

        /// <summary>
        /// Routing Rule value
        /// </summary>
        public const int RoutingRule = 150;

        /// <summary>
        /// SLA value
        /// </summary>
        public const int SLA = 152;

        /// <summary>
        /// Plugin Type value
        /// </summary>
        public const int PluginType = 90;

        /// <summary>
        /// Convert Rule value
        /// </summary>
        public const int ConvertRule = 154;

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
    }
}