//-----------------------------------------------------------------------
// <copyright file="SolutionManagementHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Jaiyanthi</author>
//-----------------------------------------------------------------------

namespace SolutionManagement
{
    using System;
    using Microsoft.Xrm.Sdk;
    using SolutionConstants;

    /// <summary>
    /// Class that assist in Solution Management
    /// </summary>
    public class SolutionManagementHelper : DevOpsBusinessBase, IPluginHelper
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="SolutionManagementHelper" /> class.
        /// </summary>
        /// <param name="crmService">Organization service</param>
        /// <param name="crmInitiatingUserService">Initiating User Service</param>
        /// <param name="crmContext">Plugin Execution Context</param>
        /// <param name="crmTracingService">Tracing Service</param>
        public SolutionManagementHelper(IOrganizationService crmService, IOrganizationService crmInitiatingUserService, IPluginExecutionContext crmContext, ITracingService crmTracingService) : base(crmService, crmInitiatingUserService, crmContext, crmTracingService)
        {
        }

        /// <summary>
        /// To sync Master Solution and CRM Solutions.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public static void SyncMasterSolutiontoCRMSolution(IOrganizationService service, ITracingService tracingService)
        {
            EntityCollection solutionlist = SolutionHelper.FetchCrmSolutions(service, tracingService);
            if (solutionlist.Entities.Count > 0)
            {
                foreach (Solution solution in solutionlist.Entities)
                {
                    CreateSolutionRecords(service, solution, tracingService);
                }
            }

            DeleteSolution(service, tracingService);
        }

        /// <summary>
        /// To create Master Solution and CRM Solutions.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solution"> CRM Solutions</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public static void CreateSolutionRecords(IOrganizationService service, Solution solution, ITracingService tracingService)
        {
            bool isAlreadyExist = IsSolutionAvaialable(service, solution.Id, tracingService);
            if (isAlreadyExist == false)
            {
                ExecuteOperations.CreateMasterSolution(service, solution);
            }
        }

        /// <summary>
        /// Method check if there are CRM Solutions is available by Master Solution Id, if available then returns true.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solutionId"> CRM Solutions Guid</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        /// <returns>returns boolean value</returns>
        public static bool IsSolutionAvaialable(IOrganizationService service, Guid solutionId, ITracingService tracingService)
        {
            bool isAvailable = false;

            EntityCollection masterRecords = SolutionHelper.RetrieveMasterSolutionById(service, solutionId, tracingService);
            if (masterRecords.Entities.Count > 0)
            {
                isAvailable = true;
            }

            return isAvailable;
        }

        /// <summary>
        /// Method check if there are master solutions available by solutions id and if present returns true.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solutionId">CRM Solution Id</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        /// <returns>returns boolean value</returns>
        public static bool IsDeletable(IOrganizationService service, string solutionId, ITracingService tracingService)
        {
            bool isDeletable = true;
            Guid solutionGuid = new Guid(solutionId);
            EntityCollection solutionlist = SolutionHelper.RetrieveSolutionById(service, solutionGuid, tracingService);
            if (solutionlist.Entities.Count == 0)
            {
                isDeletable = false;
            }

            return isDeletable;
        }

        /// <summary>
        /// To Delete Master Solution.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public static void DeleteSolution(IOrganizationService service, ITracingService tracingService)
        {
            EntityCollection masterSolution = SolutionHelper.RetrieveMasterSolutions(service, tracingService);
            if (masterSolution.Entities.Count > 0)
            {
                foreach (syed_mastersolutions mastersolutions in masterSolution.Entities)
                {
                    if (mastersolutions.syed_SolutionId != null)
                    {
                        string solutionID = mastersolutions.syed_SolutionId.ToString();
                        bool isdeletable = IsDeletable(service, solutionID, tracingService);

                        if (isdeletable == false)
                        {
                            service.Delete(mastersolutions.LogicalName, mastersolutions.syed_mastersolutionsId.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Solution Management
        /// </summary>
        public void Plugin()
        {
            if (CrmContext.InputParameters != null)
            {
                SyncMasterSolutiontoCRMSolution(this.CrmService, this.CrmTracingService);
            }
        }
    }
}