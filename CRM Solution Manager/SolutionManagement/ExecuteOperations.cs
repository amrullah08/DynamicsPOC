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

        /// <summary>
        /// Method to get comma separated values obtained by converting list of solutions to comma separated list 
        /// and these indicates list of solutions to be merged.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="associatedRecordList">Merge Solution Records</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        /// <returns>returns comma separated list of solutions to be merged</returns>
        public static string GetCommaSeparatedListofSolution(IOrganizationService service, EntityCollection associatedRecordList, ITracingService tracingService)
        {
            string uniqueName = string.Empty;
            ArrayList uniqueArray = new ArrayList();

            if (associatedRecordList.Entities.Count > 0)
            {
                foreach (syed_mergesolutions mergesolutions in associatedRecordList.Entities)
                {
                    if (mergesolutions.syed_UniqueName != null)
                    {
                        uniqueArray.Add(mergesolutions.syed_UniqueName.ToString());
                    }
                }
            }

            uniqueName = string.Join(",", uniqueArray.ToArray());
            return uniqueName;
        }

        /// <summary>
        /// Method to get comma separated values obtained by converting list of master to comma separated list, 
        /// and these indicates list of master to be merged.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="associatedRecordList">Solution Details Records</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        /// <returns>returns comma separated master solutions to be merged</returns>
        public static string GetCommaSeparatedListofMaster(IOrganizationService service, EntityCollection associatedRecordList, ITracingService tracingService)
        {
            string uniqueName = string.Empty;
            ArrayList uniqueArray = new ArrayList();

            if (associatedRecordList.Entities.Count > 0)
            {
                foreach (syed_solutiondetail solutiondetail in associatedRecordList.Entities)
                {
                    if (solutiondetail.syed_ListofSolutions != null)
                    {
                        uniqueArray.Add(solutiondetail.syed_ListofSolutions.ToString());
                    }
                }
            }

            uniqueName = string.Join(",", uniqueArray.ToArray());
            return uniqueName;
        }
    }
}
