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

        /// <summary>
        /// Method creates Master Solution Record
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solution">CRM Solution</param>
        public static void CreateSolutionDetail(IOrganizationService service, syed_solutiondetail syed_Solutiondetail, syed_sourcecontrolqueue syed_Sourcecontrolqueue)
        {
            syed_solutiondetail solutiondetail = new syed_solutiondetail();
            solutiondetail.syed_CRMSolutionsId = syed_Solutiondetail.syed_CRMSolutionsId;
            solutiondetail.syed_ListofSolutionId = new EntityReference(syed_Sourcecontrolqueue.LogicalName.ToString(), syed_Sourcecontrolqueue.Id);
            solutiondetail.syed_friendlyname = syed_Solutiondetail.syed_friendlyname;
            solutiondetail.syed_Publisher = syed_Solutiondetail.syed_Publisher;
            solutiondetail.syed_SolutionInstalledOn = syed_Solutiondetail.syed_SolutionInstalledOn;
            solutiondetail.syed_Version = syed_Solutiondetail.syed_Version;
            solutiondetail.syed_IsManaged = syed_Solutiondetail.syed_IsManaged;
            solutiondetail.syed_SolutionId = syed_Solutiondetail.syed_SolutionId;
            solutiondetail.syed_ListofSolutions = syed_Solutiondetail.syed_ListofSolutions;
            solutiondetail.syed_ExportAs = syed_Solutiondetail.syed_ExportAs;
            solutiondetail.syed_Order = syed_Solutiondetail.syed_Order;
            solutiondetail.syed_name = syed_Solutiondetail.syed_name;
            service.Create(solutiondetail);
        }

        /// <summary>
        /// Method creates Master Solution Record
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solution">CRM Solution</param>
        public static Guid CreateDynamicsSourceControl(IOrganizationService service, string solutionId, string mode)
        {
            syed_sourcecontrolqueue sourcecontrolqueue = new syed_sourcecontrolqueue();
            sourcecontrolqueue.syed_Status = "Draft";
            sourcecontrolqueue.syed_CheckInBySolution = true;
            sourcecontrolqueue.syed_CheckInBySolutionId = solutionId;
            sourcecontrolqueue.syed_name = "SOL-" + DateTime.Now.ToString();
            sourcecontrolqueue.syed_Comment = mode + DateTime.Now.ToString();
            sourcecontrolqueue.syed_overwritesolutionstxt = new OptionSetValue(433710000);
            sourcecontrolqueue.syed_CheckIn = true;
            if (mode == "Release")
            {
                sourcecontrolqueue.syed_IncludeInRelease = true;
            }
            else
            {
                sourcecontrolqueue.syed_IncludeInRelease = false;
            }

            Guid Id = service.Create(sourcecontrolqueue);
            return Id;
        }
    }
}
