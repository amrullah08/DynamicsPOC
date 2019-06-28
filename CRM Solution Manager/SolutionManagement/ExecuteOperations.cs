//-----------------------------------------------------------------------
// <copyright file="ExecuteOperations.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Jaiyanthi</author>
//-----------------------------------------------------------------------

namespace SolutionManagement
{
    using System;
    using System.Collections;
    using Microsoft.Xrm.Sdk;
    using SolutionConstants;

    /// <summary>
    /// Class that contains execute functions in CRM
    /// </summary>
    public class ExecuteOperations
    {
        /// <summary>
        /// Method creates Master Solution Record
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solution">CRM Solution</param>
        public static void CreateMasterSolution(IOrganizationService service, Solution solution)
        {
            syed_mastersolutions masterSolutionUpdate = new syed_mastersolutions();
            masterSolutionUpdate.syed_name = solution.FriendlyName;
            masterSolutionUpdate.syed_FriendlyName = solution.FriendlyName;
            masterSolutionUpdate.syed_Publisher = solution.PublisherId.Name;
            masterSolutionUpdate.syed_ListofSolutions = solution.UniqueName.ToString();
            masterSolutionUpdate.syed_SolutionId = solution.Id.ToString();
            masterSolutionUpdate.syed_SolutionInstalledOn = solution.InstalledOn;
            masterSolutionUpdate.syed_Version = solution.Version;
            masterSolutionUpdate.syed_IsManaged = solution.IsManaged;
            service.Create(masterSolutionUpdate);
        }
    }
}
