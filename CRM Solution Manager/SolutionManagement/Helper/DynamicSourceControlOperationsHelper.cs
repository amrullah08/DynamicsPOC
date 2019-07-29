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
        public static void DeleteSolutionDetail(IOrganizationService service, syed_sourcecontrolqueue sourceControlQueue, ITracingService tracingService)
        {
            if (sourceControlQueue.syed_CheckInBySolution != null && sourceControlQueue.syed_CheckInBySolutionId != null)
            {
                bool checkInBySolution = sourceControlQueue.syed_CheckInBySolution.Value;
                string checkSolutionId = sourceControlQueue.syed_CheckInBySolutionId.ToString();
                if (checkInBySolution == true && checkSolutionId != string.Empty)
                {
                    EntityCollection crmSolution = SolutionHelper.RetrieveMasterSolutionById(service, checkSolutionId, tracingService);
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
                        EntityCollection solutionCollection = SolutionHelper.RetrieveSolutionById(service, new Guid(checkSolutionId), tracingService);
                        foreach (Solution sol in solutionCollection.Entities)
                        {
                            Guid Id = ExecuteOperations.CreateMasterSolution(service, sol);
                            syed_mastersolutions syed_Mastersolutions = service.Retrieve(syed_mastersolutions.EntityLogicalName.ToString(), Id, new ColumnSet(true)).ToEntity<syed_mastersolutions>(); ;
                            ExecuteOperations.CreateSolutionDetail(service, syed_Mastersolutions, sourceControlQueue);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Dynamics Source Control Operations
        /// </summary>
        public void Plugin()
        {
            syed_sourcecontrolqueue postSourceControlQueue = null;
            if (CrmContext.InputParameters != null)
            {
                if (CrmContext.MessageName.ToLower() == CRMConstant.PluginCreate)
                {
                    if (CrmContext.PostEntityImages != null && CrmContext.PostEntityImages.Contains(CRMConstant.PluginPostImage))
                    {
                        postSourceControlQueue = CrmContext.PostEntityImages[CRMConstant.PluginPostImage].ToEntity<syed_sourcecontrolqueue>();
                    }

                    if (postSourceControlQueue.LogicalName == syed_sourcecontrolqueue.EntityLogicalName)
                    {
                        DeleteSolutionDetail(this.CrmService, postSourceControlQueue, this.CrmTracingService);
                    }
                }
            }
        }
    }
}
