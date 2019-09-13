using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionManagement
{
    using System;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using SolutionConstants;
    public class VersionManagementHelper : DevOpsBusinessBase, IPluginHelper
    {

        /// <summary>
        ///  Initializes a new instance of the <see cref="VersionManagementHelper" /> class.
        /// </summary>
        /// <param name="crmService">Organization service</param>
        /// <param name="crmInitiatingUserService">Initiating User Service</param>
        /// <param name="crmContext">Plugin Execution Context</param>
        /// <param name="crmTracingService">Tracing Service</param>
        public VersionManagementHelper(IOrganizationService crmService, IOrganizationService crmInitiatingUserService, IPluginExecutionContext crmContext, ITracingService crmTracingService) : base(crmService, crmInitiatingUserService, crmContext, crmTracingService)
        {
        }

        /// <summary>
        /// To create Dynamics source control and create associated solution details.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solutionId">CRM Solution id</param>
        /// <param name="checkIn">check In</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public static void CheckMasterSolutionForCloneRequest(IOrganizationService service, string solutionId, ITracingService tracingService)
        {
            EntityCollection masterSolutions = SolutionHelper.RetrieveMasterSolutionBySolutionOptions(service, solutionId, tracingService);
            foreach (syed_solutiondetail syed_Solutiondetail in masterSolutions.Entities)
            {
                try
                {
                    CloneAsPatchRequest cloneAsPatchRequest = new CloneAsPatchRequest();
                    cloneAsPatchRequest.DisplayName = syed_Solutiondetail.syed_name;
                    cloneAsPatchRequest.ParentSolutionUniqueName = syed_Solutiondetail.syed_ListofSolutions;
                    if (syed_Solutiondetail.syed_NewVersion != null && syed_Solutiondetail.syed_NewVersion != string.Empty)
                    {
                        cloneAsPatchRequest.VersionNumber = syed_Solutiondetail.syed_NewVersion;
                    }
                    CloneAsPatchResponse cloneAsPatchResponse = (CloneAsPatchResponse)service.Execute(cloneAsPatchRequest);
                    syed_Solutiondetail.syed_CRMSolutionsId = new EntityReference(syed_mastersolutions.EntityLogicalName, cloneAsPatchResponse.SolutionId);

                    EntityCollection solutionCollection = SolutionHelper.RetrieveSolutionById(service, cloneAsPatchResponse.SolutionId, tracingService);
                    foreach (Solution sol in solutionCollection.Entities)
                    {
                        Guid id = ExecuteOperations.CreateMasterSolution(service, sol);
                        syed_mastersolutions syed_Mastersolutions = service.Retrieve(syed_mastersolutions.EntityLogicalName.ToString(), id, new ColumnSet(true)).ToEntity<syed_mastersolutions>();
                        ExecuteOperations.UpdateSolutionDetail(service, syed_Mastersolutions, syed_Solutiondetail);
                        break;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        /// <summary>
        ///  Dynamics Source Control Operations
        /// </summary>
        public void Plugin()
        {
            object objDynamicsSourceControlId = null;

            if (CrmContext.InputParameters != null)
            {
                if (!CrmContext.InputParameters.TryGetValue("SourceControlId", out objDynamicsSourceControlId))
                {
                    CrmTracingService.Trace("SolutionId- Missing");
                    CrmContext.OutputParameters["success"] = false;
                    throw new InvalidPluginExecutionException("SolutionId- Missing");
                }

                string dynamicsSourceControlId = (string)objDynamicsSourceControlId;

                CheckMasterSolutionForCloneRequest(this.CrmService, dynamicsSourceControlId, this.CrmTracingService);
                CrmContext.OutputParameters["success"] = true;
            }
        }
    }
}
