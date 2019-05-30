//-----------------------------------------------------------------------
// <copyright file="SolutionSortHelper.cs" company="Microsoft">
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
    /// Class that assist in updating HTML WebResource
    /// </summary>
    public class SolutionSortHelper : DevOpsBusinessBase, IPluginHelper
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="SolutionSortHelper" /> class.
        /// </summary>
        ///  <param name="crmService">Organization service</param>
        /// <param name="crmInitiatingUserService">Initiating User Service</param>
        /// <param name="crmContext">Plugin Execution Context</param>
        /// <param name="crmTracingService">Tracing Service</param>
        public SolutionSortHelper(IOrganizationService crmService, IOrganizationService crmInitiatingUserService, IPluginExecutionContext crmContext, ITracingService crmTracingService) : base(crmService, crmInitiatingUserService, crmContext, crmTracingService)
        {
        }

        /// <summary>
        /// To update comma separated solution list in Dynamic Source Control entity.
        /// </summary>
        public void Plugin()
        {
            Entity solutionDetail = null;
            EntityReference sourceControl = null;
            if (CrmContext.InputParameters != null)
            {
                if (CrmContext.PostEntityImages != null && CrmContext.PostEntityImages.Contains(CRMConstant.PluginPostImage))
                {
                    solutionDetail = CrmContext.PostEntityImages[CRMConstant.PluginPostImage];
                }

                if (CrmContext.PreEntityImages != null && CrmContext.PreEntityImages.Contains(CRMConstant.PluginPreImage))
                {
                    solutionDetail = CrmContext.PreEntityImages[CRMConstant.PluginPreImage];
                }

                if (solutionDetail != null)
                {
                    if (solutionDetail.LogicalName == syed_solutiondetail.EntityLogicalName)
                    {
                        if (solutionDetail.Attributes.Contains("syed_listofsolutionid") && solutionDetail.Attributes["syed_listofsolutionid"] != null)
                        {
                            sourceControl = (EntityReference)solutionDetail.Attributes["syed_listofsolutionid"];
                            UpdateListToSourceControl(this.CrmService, sourceControl.Id, this.CrmTracingService);
                        }
                    }

                    if (solutionDetail.LogicalName == syed_mergesolutions.EntityLogicalName)
                    {
                        if (solutionDetail.Attributes.Contains("syed_listofsolution") && solutionDetail.Attributes["syed_listofsolution"] != null)
                        {
                            sourceControl = (EntityReference)solutionDetail.Attributes["syed_listofsolution"];
                            UpdateListToSourceControl(this.CrmService, sourceControl.Id, this.CrmTracingService);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// To update comma separated solution list in Dynamic Source Control entity.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="sourceControlId">Dynamic Source Control entity GUID</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        private static void UpdateListToSourceControl(IOrganizationService service, Guid sourceControlId, ITracingService tracingService)
        {
            EntityCollection associatedRecordList = null;
            string listOfSolution = string.Empty;
            string solutionName = string.Empty;
            associatedRecordList = SolutionHelper.RetrieveSolutionDetailsToBeMergedByListOfSolutionId(service, sourceControlId, tracingService);
            listOfSolution = ExecuteOperations.GetCommaSeparatedListofSolution(service, associatedRecordList, tracingService);
            associatedRecordList = SolutionHelper.RetrieveMasterSolutionDetailsByListOfSolutionId(service, sourceControlId, tracingService);
            solutionName = ExecuteOperations.GetCommaSeparatedListofMaster(service, associatedRecordList, tracingService);
            syed_sourcecontrolqueue sourceControlQueue = new syed_sourcecontrolqueue();
            sourceControlQueue.syed_sourcecontrolqueueId = sourceControlId;
            sourceControlQueue.syed_SourcenSolutions = listOfSolution;
            sourceControlQueue.syed_SolutionName = solutionName;
            service.Update(sourceControlQueue);
        }
    }
}
