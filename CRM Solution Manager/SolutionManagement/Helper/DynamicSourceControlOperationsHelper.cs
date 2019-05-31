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
            if (sourceControlQueue.syed_MergeSolutions != null)
            {
                bool mergeSolutions = sourceControlQueue.syed_MergeSolutions.Value;
                if (mergeSolutions == false)
                {
                    EntityCollection associatedRecordList = SolutionHelper.RetrieveSolutionDetailsToBeMergedByListOfSolutionId(service, sourceControlQueue.Id, tracingService);
                    if (associatedRecordList.Entities.Count > 0)
                    {
                        foreach (syed_mergesolutions syed_Mergesolutions in associatedRecordList.Entities)
                        {
                            service.Delete(syed_Mergesolutions.LogicalName, syed_Mergesolutions.Id);
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
                if (CrmContext.MessageName.ToLower() == CRMConstant.PluginUpdate)
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
