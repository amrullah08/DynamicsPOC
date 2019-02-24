//-----------------------------------------------------------------------
// <copyright file="SolutionFileInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    using System.Collections.Generic;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// solution file info
    /// </summary>
    public class SolutionFileInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFileInfo" /> class
        /// </summary>
        /// <param name="organizationServiceProxy">Organization proxy</param>
        public SolutionFileInfo(Microsoft.Xrm.Sdk.Client.OrganizationServiceProxy organizationServiceProxy)
        {
            this.OrganizationServiceProxy = organizationServiceProxy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFileInfo" /> class
        /// </summary>
        /// <param name="solution">solution entity</param>
        /// <param name="organizationServiceProxy">Organization proxy</param>
        public SolutionFileInfo(Entity solution, Microsoft.Xrm.Sdk.Client.OrganizationServiceProxy organizationServiceProxy)
        {
            this.OrganizationServiceProxy = organizationServiceProxy;
            this.SolutionsToBeMerged = new List<string>();
            this.SolutionUniqueName = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForSolutionName);
            this.Message = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForComment);
            this.OwnerName = solution.GetAttributeValue<EntityReference>(Constants.SourceControlQueueAttributeNameForOwnerId).Name;
            this.IncludeInRelease = solution.GetAttributeValue<bool>(Constants.SourceControlQueueAttributeNameForIncludeInRelease);
            this.CheckInSolution = solution.GetAttributeValue<bool>(Constants.SourceControlQueueAttributeNameForCheckinSolution);
            this.MergeSolution = solution.GetAttributeValue<bool>(Constants.SourceControlQueueAttributeNameForMergeSolution);
            var solutions = solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForSourceSolutions);

            if (!string.IsNullOrEmpty(solutions) && this.MergeSolution)
            {
                foreach (var s in solutions.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    this.SolutionsToBeMerged.Add(s);
                }
            }
            
            this.Solution = solution;
        }

        /// <summary>
        /// Gets or sets Downloaded solution file path
        /// </summary>
        public string SolutionFilePath { get; set; }

        /// <summary>
        /// Gets or sets Unique solution name
        /// </summary>
        public string SolutionUniqueName { get; set; }

        /// <summary>
        /// Gets or sets solution entity
        /// </summary>
        public Entity Solution { get; set; }

        /// <summary>
        /// Gets or sets message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets owner name
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether solution needs to be included in Release
        /// </summary>
        public bool IncludeInRelease { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether solution needs to be checked in
        /// </summary>
        public bool CheckInSolution { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether solution needs to be Merged
        /// </summary>
        public bool MergeSolution { get; set; }

        /// <summary>
        /// Gets or sets solutions to be merged
        /// </summary>
        public List<string> SolutionsToBeMerged { get; set; }

        /// <summary>
        /// Gets or sets value of Organization service
        /// </summary>
        public Microsoft.Xrm.Sdk.Client.OrganizationServiceProxy OrganizationServiceProxy { get; set; }
        
        /// <summary>
        /// Method commits the changes done to the solution object
        /// </summary>
        public void Update()
        {
            this.OrganizationServiceProxy.Update(this.Solution);
        }
    }
}