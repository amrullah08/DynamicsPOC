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
        public static Guid CreateMasterSolution(IOrganizationService service, Solution solution)
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
            Guid Id = service.Create(masterSolutionUpdate);
            return Id;
        }

        /// <summary>
        /// Method creates Master Solution Record
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solution">CRM Solution</param>
        public static void CreateSolutionDetail(IOrganizationService service, syed_mastersolutions mastersolutions, syed_sourcecontrolqueue syed_Sourcecontrolqueue)
        {
            syed_solutiondetail solutiondetail = new syed_solutiondetail();
            solutiondetail.syed_CRMSolutionsId = new EntityReference(mastersolutions.LogicalName.ToString(), mastersolutions.Id);
            solutiondetail.syed_ListofSolutionId = new EntityReference(syed_Sourcecontrolqueue.LogicalName.ToString(), syed_Sourcecontrolqueue.Id);
            solutiondetail.syed_friendlyname = mastersolutions.syed_FriendlyName;
            solutiondetail.syed_Publisher = mastersolutions.syed_Publisher;
            solutiondetail.syed_SolutionInstalledOn = mastersolutions.syed_SolutionInstalledOn;
            solutiondetail.syed_Version = mastersolutions.syed_Version;
            solutiondetail.syed_IsManaged = mastersolutions.syed_IsManaged;
            solutiondetail.syed_SolutionId = mastersolutions.syed_SolutionId;
            solutiondetail.syed_ListofSolutions = mastersolutions.syed_ListofSolutions;
            solutiondetail.syed_ExportAs = false;
            solutiondetail.syed_Order = 0;
            solutiondetail.syed_name = mastersolutions.syed_FriendlyName;
            service.Create(solutiondetail);

            syed_sourcecontrolqueue sourcecontrolqueue = new syed_sourcecontrolqueue();
            sourcecontrolqueue.Id = syed_Sourcecontrolqueue.Id;
            sourcecontrolqueue.syed_Status = "Queued";
            service.Update(sourcecontrolqueue);
        }
    }
}
