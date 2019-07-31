//-----------------------------------------------------------------------
// <copyright file="DynamicSourceControlOperationsHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Jaiyanthi</author>
//-----------------------------------------------------------------------

namespace SolutionManagement
{
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using SolutionConstants;

    /// <summary>
    /// Class that contains operations for Dynamics Source Control
    /// </summary>
    public class DynamicSourceControlOperationsHelper : DevOpsBusinessBase, IPluginHelper
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="DynamicSourceControlOperationsHelper" /> class.
        /// </summary>
        /// <param name="crmService">Organization service</param>
        /// <param name="crmInitiatingUserService">Initiating User Service</param>
        /// <param name="crmContext">Plugin Execution Context</param>
        /// <param name="crmTracingService">Tracing Service</param>
        public DynamicSourceControlOperationsHelper(IOrganizationService crmService, IOrganizationService crmInitiatingUserService, IPluginExecutionContext crmContext, ITracingService crmTracingService) : base(crmService, crmInitiatingUserService, crmContext, crmTracingService)
        {
        }

        /// <summary>
        /// To delete Merge Solution records.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="sourceControlQueue">Dynamic Source Control entity GUID</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public static void CreateSolutionDetail(IOrganizationService service, syed_sourcecontrolqueue sourceControlQueue, ITracingService tracingService)
        {
            EntityCollection copyTemplate = SolutionHelper.RetrieveDynamicsSourceControlTemplate(service, tracingService);
            foreach (syed_solutiondetail solutionDetail in copyTemplate.Entities)
            {
                ExecuteOperations.CreateSolutionDetail(service, solutionDetail, sourceControlQueue);
                break;
            }

            EntityCollection crmSolution = SolutionHelper.RetrieveMasterSolutionById(service, sourceControlQueue.syed_CheckInBySolutionId, tracingService);
            if (crmSolution.Entities.Count > 0)
            {
                foreach (syed_mastersolutions mastersolutions in crmSolution.Entities)
                {
                    ExecuteOperations.CreateSolutionDetail(service, mastersolutions, sourceControlQueue);
                    break;
                }
            }
            else
            {
                EntityCollection solutionCollection = SolutionHelper.RetrieveSolutionById(service, new Guid(sourceControlQueue.syed_CheckInBySolutionId), tracingService);
                foreach (Solution sol in solutionCollection.Entities)
                {
                    Guid id = ExecuteOperations.CreateMasterSolution(service, sol);
                    syed_mastersolutions syed_Mastersolutions = service.Retrieve(syed_mastersolutions.EntityLogicalName.ToString(), id, new ColumnSet(true)).ToEntity<syed_mastersolutions>();
                    ExecuteOperations.CreateSolutionDetail(service, syed_Mastersolutions, sourceControlQueue);
                    break;
                }
            }
        }

        /// <summary>
        /// To create Dynamics source control and create associated solution details.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solutionId">CRM Solution id</param>
        /// <param name="checkIn">check In</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public static void CreateDynmicsSourceControl(IOrganizationService service, string solutionId, string checkIn, ITracingService tracingService)
        {
            Guid id = ExecuteOperations.CreateDynamicsSourceControl(service, solutionId, checkIn);
            syed_sourcecontrolqueue syed_Sourcecontrolqueue = service.Retrieve(syed_sourcecontrolqueue.EntityLogicalName.ToString(), id, new ColumnSet(true)).ToEntity<syed_sourcecontrolqueue>();
            CreateSolutionDetail(service, syed_Sourcecontrolqueue, tracingService);
        }

        /// <summary>
        ///  Dynamics Source Control Operations
        /// </summary>
        public void Plugin()
        {
            object objSolutionId = null;
            object objCheckIn = null;

            if (CrmContext.InputParameters != null)
            {
                if (!CrmContext.InputParameters.TryGetValue("SolutionId", out objSolutionId))
                {
                    CrmTracingService.Trace("SolutionId- Missing");
                    CrmContext.OutputParameters["success"] = false;
                    throw new InvalidPluginExecutionException("SolutionId- Missing");
                }

                if (!CrmContext.InputParameters.TryGetValue("CheckIn", out objCheckIn))
                {
                    CrmTracingService.Trace("CheckIn/Release - Missing");
                    CrmContext.OutputParameters["success"] = false;
                    throw new InvalidPluginExecutionException("SolutionId- Missing");
                }

                string solutionId = (string)objSolutionId;
                string checkIn = (string)objCheckIn;
                CreateDynmicsSourceControl(this.CrmService, solutionId, checkIn, this.CrmTracingService);
                CrmContext.OutputParameters["success"] = true;
            }
        }
    }
}
