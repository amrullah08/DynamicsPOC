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


        public static void ValidateVersionMemberForClone(IOrganizationService service, syed_solutiondetail syed_Solutiondetail, ITracingService tracingService, Guid solId)
        {
            string versionNumber = string.Empty;
            int increasedVersion = 0;
            string parentSolutionName = string.Empty;
            List<int> versionUpdate = null;
            EntityCollection verionCollection = SolutionHelper.RetrieveParentSolutionById(service, solId, tracingService);
            if (verionCollection != null && verionCollection.Entities.Count > 0)
            {
                foreach (Solution sol in verionCollection.Entities)
                {
                    versionUpdate = sol.Version.Split('.').Select(int.Parse).ToList();
                    parentSolutionName = sol.ParentSolutionId.Name;
                    break;
                }
            }
            else
            {
                EntityCollection solutionCollection = SolutionHelper.RetrieveSolutionById(service, solId, tracingService);
                foreach (Solution sol in solutionCollection.Entities)
                {
                    versionUpdate = sol.Version.Split('.').Select(int.Parse).ToList();
                    parentSolutionName = sol.UniqueName;
                    break;
                }
            }

            if (versionUpdate.Count > 0)
            {
                versionNumber = string.Empty;
                for (int version = 1; version <= versionUpdate.Count; version++)
                {
                    if (version == 2)
                    {
                        increasedVersion = versionUpdate[version - 1] + 1;
                        versionNumber = versionNumber + increasedVersion.ToString();
                    }
                    else
                    {
                        versionNumber = versionNumber + versionUpdate[version - 1].ToString() + ".";
                    }
                }
            }

            if (!string.IsNullOrEmpty(versionNumber) && !string.IsNullOrEmpty(parentSolutionName))
            {
                CloneAsSolutionRequest cloneAsSolutionRequest = new CloneAsSolutionRequest();
                cloneAsSolutionRequest.ParentSolutionUniqueName = parentSolutionName;
                cloneAsSolutionRequest.DisplayName = parentSolutionName;

                if (syed_Solutiondetail.syed_NewVersion != null && syed_Solutiondetail.syed_NewVersion != string.Empty)
                {
                    cloneAsSolutionRequest.VersionNumber = syed_Solutiondetail.syed_NewVersion;
                }
                else
                {
                    cloneAsSolutionRequest.VersionNumber = versionNumber;
                }
                CloneAsSolutionResponse cloneAsSolutionResponse = (CloneAsSolutionResponse)service.Execute(cloneAsSolutionRequest);
                syed_Solutiondetail.syed_CRMSolutionsId = new EntityReference(syed_mastersolutions.EntityLogicalName, cloneAsSolutionResponse.SolutionId);
                EntityCollection solutionCollection = SolutionHelper.RetrieveSolutionById(service, cloneAsSolutionResponse.SolutionId, tracingService);
                foreach (Solution sol in solutionCollection.Entities)
                {
                    Guid id = ExecuteOperations.CreateMasterSolution(service, sol);
                    syed_mastersolutions syed_Mastersolutions = service.Retrieve(syed_mastersolutions.EntityLogicalName.ToString(), id, new ColumnSet(true)).ToEntity<syed_mastersolutions>();
                    ExecuteOperations.UpdateSolutionDetail(service, syed_Mastersolutions, syed_Solutiondetail);
                    break;
                }
            }
        }

        public static void ValidateVersionMember(IOrganizationService service, syed_solutiondetail syed_Solutiondetail, ITracingService tracingService, Guid solId)
        {
            string versionNumber = string.Empty;
            int increasedVersion = 0;
            string parentSolutionName = string.Empty;
            List<int> versionUpdate = null;
            EntityCollection verionCollection = SolutionHelper.RetrieveParentSolutionById(service, solId, tracingService);
            if (verionCollection != null && verionCollection.Entities.Count > 0)
            {
                foreach (Solution sol in verionCollection.Entities)
                {
                    versionUpdate = sol.Version.Split('.').Select(int.Parse).ToList();
                    parentSolutionName = sol.ParentSolutionId.Name;
                    break;
                }
            }
            else
            {
                EntityCollection solutionCollection = SolutionHelper.RetrieveSolutionById(service, solId, tracingService);
                foreach (Solution sol in solutionCollection.Entities)
                {
                    versionUpdate = sol.Version.Split('.').Select(int.Parse).ToList();
                    parentSolutionName = sol.UniqueName;
                    break;
                }
            }

            if (versionUpdate.Count > 0)
            {
                versionNumber = string.Empty;
                for (int version = 1; version <= versionUpdate.Count; version++)
                {

                    if (version == (versionUpdate.Count))
                    {
                        increasedVersion = versionUpdate[version - 1] + 1;
                        versionNumber = versionNumber + increasedVersion.ToString();
                        break;
                    }
                    else
                    {
                        versionNumber = versionNumber + versionUpdate[version - 1].ToString() + ".";
                    }
                }
            }

            if (!string.IsNullOrEmpty(versionNumber) && !string.IsNullOrEmpty(parentSolutionName))
            {
                CloneAsPatchRequest cloneAsPatchRequest = new CloneAsPatchRequest();
                cloneAsPatchRequest.DisplayName = parentSolutionName;
                cloneAsPatchRequest.ParentSolutionUniqueName = parentSolutionName;

                if (syed_Solutiondetail.syed_NewVersion != null && syed_Solutiondetail.syed_NewVersion != string.Empty)
                {
                    cloneAsPatchRequest.VersionNumber = syed_Solutiondetail.syed_NewVersion;
                }
                else
                {
                    cloneAsPatchRequest.VersionNumber = versionNumber;
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
        }

        /// <summary>
        /// To create Dynamics source control and create associated solution details.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solutionId">CRM Solution id</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public void CheckMasterSolutionForCloneRequest(IOrganizationService service, string solutionId, ITracingService tracingService)
        {
            EntityCollection masterSolutions = SolutionHelper.RetrieveMasterSolutionBySolutionOptions(service, solutionId, tracingService);
            foreach (syed_solutiondetail syed_Solutiondetail in masterSolutions.Entities)
            {
                try
                {
                    syed_mastersolutions crmSolution = service.Retrieve(syed_mastersolutions.EntityLogicalName, syed_Solutiondetail.syed_CRMSolutionsId.Id, new ColumnSet("syed_solutionid")).ToEntity<syed_mastersolutions>(); ;
                    if (syed_Solutiondetail.syed_SolutionOptions.Value == 433710001)
                    {
                        if (crmSolution != null)
                        {
                            ValidateVersionMember(service, syed_Solutiondetail, tracingService, new Guid(crmSolution.syed_SolutionId));
                        }
                    }
                    else if (syed_Solutiondetail.syed_SolutionOptions.Value == 433710002)
                    {
                        if (crmSolution != null)
                        {
                            ValidateVersionMember(service, syed_Solutiondetail, tracingService, new Guid(crmSolution.syed_SolutionId));
                        }
                    }

                }
                catch (Exception ex)
                {
                    CrmContext.OutputParameters["ErrorMessage"] = ex.Message;
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
                    CrmContext.OutputParameters["ErrorMessage"] = false;
                    throw new InvalidPluginExecutionException("SolutionId- Missing");
                }

                string dynamicsSourceControlId = (string)objDynamicsSourceControlId;
                CheckMasterSolutionForCloneRequest(this.CrmService, dynamicsSourceControlId, this.CrmTracingService);
                CrmContext.OutputParameters["ErrorMessage"] = "Success";
            }
        }
    }
}
