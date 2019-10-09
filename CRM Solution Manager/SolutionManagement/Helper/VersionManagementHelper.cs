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
        /// web jobs log
        /// </summary>
        public StringBuilder webJobsLog = null;

        /// <summary>
        /// Gets web jobs log
        /// </summary>
        public StringBuilder WebJobsLog
        {
            get
            {
                if (this.webJobsLog == null)
                {
                    this.webJobsLog = new StringBuilder();
                }

                return this.webJobsLog;
            }
        }

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

        public void UpdateVersionNumber(syed_solutiondetail syed_Solutiondetail, Guid solId)
        {
            string versionNumber = string.Empty;
            int increasedVersion = 0;
            string parentSolutionName = string.Empty;
            List<int> versionUpdate = null;
            Solution solution = CrmService.Retrieve(Solution.EntityLogicalName, solId, new ColumnSet(true)).ToEntity<Solution>();

            if (solution != null)
            {
                versionUpdate = solution.Version.Split('.').Select(int.Parse).ToList();
                parentSolutionName = solution.UniqueName;
            }
            if (versionUpdate.Count > 0)
            {
                versionNumber = string.Empty;
                for (int version = 1; version <= 4; version++)
                {
                    if (version == 4)
                    {
                        if (versionUpdate.Count == 2)
                        {
                            versionNumber = versionNumber + "1";
                        }
                        else
                        {
                            increasedVersion = versionUpdate[version - 1] + 1;
                            versionNumber = versionNumber + increasedVersion.ToString();
                        }
                    }
                    else if (version == 2 || version == 3)
                    {
                        if (versionUpdate.Count == 2)
                        {
                            versionNumber = versionNumber + "0" + ".";
                        }
                        else
                        {
                            versionNumber = versionNumber + versionUpdate[version - 1].ToString() + ".";
                        }
                    }
                    else
                    {
                        versionNumber = versionNumber + versionUpdate[version - 1].ToString() + ".";
                    }
                }
            }

            if (syed_Solutiondetail.syed_NewVersion != null && syed_Solutiondetail.syed_NewVersion != string.Empty)
            {
                solution.Version = syed_Solutiondetail.syed_NewVersion;
                syed_Solutiondetail.syed_NewVersion = versionNumber;
            }
            else
            {
                solution.Version = versionNumber;
                syed_Solutiondetail.syed_NewVersion = versionNumber;
            }
            CrmService.Update(solution);
            CrmService.Update(syed_Solutiondetail);
            syed_mastersolutions syed_Mastersolutions = CrmService.Retrieve(syed_mastersolutions.EntityLogicalName.ToString(), syed_Solutiondetail.syed_CRMSolutionsId.Id, new ColumnSet(true)).ToEntity<syed_mastersolutions>();
            ExecuteOperations.UpdateMasterSolution(CrmService, solution, syed_Mastersolutions);
            WebJobsLog.AppendLine("Version Updated - sucessfully " + syed_Solutiondetail.syed_ListofSolutions);
            UpdateExceptionDetails(syed_Solutiondetail.syed_ListofSolutionId.Id.ToString(), WebJobsLog.ToString(), "Queued");
        }
        public void ValidateVersionMemberForClone(syed_solutiondetail syed_Solutiondetail, Guid solId)
        {
            string versionNumber = string.Empty;
            int increasedVersion = 0;
            string parentSolutionName = string.Empty;
            List<int> versionUpdate = null;
            Solution solution = CrmService.Retrieve(Solution.EntityLogicalName, solId, new ColumnSet(true)).ToEntity<Solution>();
            if (solution != null)
            {
                versionUpdate = solution.Version.Split('.').Select(int.Parse).ToList();
                parentSolutionName = solution.UniqueName;
            }

            if (versionUpdate.Count > 0)
            {
                versionNumber = string.Empty;
                for (int version = 1; version <= 4; version++)
                {
                    if (version == 2)
                    {
                        increasedVersion = versionUpdate[version - 1] + 1;
                        versionNumber = versionNumber + increasedVersion.ToString() + ".";
                    }
                    else if (version == 4)
                    {
                        versionNumber = versionNumber + "0";
                    }
                    else if (version == 3)
                    {
                        versionNumber = versionNumber + "0" + ".";
                    }
                    else
                    {
                        versionNumber = versionNumber + versionUpdate[version - 1].ToString() + ".";
                    }
                }
            }
            this.CloneASolution(versionNumber, parentSolutionName, syed_Solutiondetail);
        }

        public void ValidateVersionMember(syed_solutiondetail syed_Solutiondetail, Guid solId)
        {
            string versionNumber = string.Empty;
            int increasedVersion = 0;
            string parentSolutionName = string.Empty;
            List<int> versionUpdate = null;
            EntityCollection verionCollection = SolutionHelper.RetrieveParentSolutionById(CrmService, solId, CrmTracingService);
            if (verionCollection != null && verionCollection.Entities.Count > 0)
            {
                foreach (Solution sol in verionCollection.Entities)
                {
                    versionUpdate = sol.Version.Split('.').Select(int.Parse).ToList();
                    EntityCollection solutionCollection = SolutionHelper.RetrieveSolutionById(CrmService, sol.ParentSolutionId.Id, CrmTracingService);
                    foreach (Solution solution in solutionCollection.Entities)
                    {
                        parentSolutionName = solution.UniqueName;
                        break;
                    }
                    break;
                }
            }
            else
            {
                EntityCollection solutionCollection = SolutionHelper.RetrieveSolutionById(CrmService, solId, CrmTracingService);
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
                for (int version = 1; version <= 4; version++)
                {
                    if (version == 3)
                    {
                        if (versionUpdate.Count < version)
                        {
                            versionNumber = versionNumber + "1" + ".";
                        }
                        else
                        {
                            increasedVersion = versionUpdate[version - 1] + 1;
                            versionNumber = versionNumber + increasedVersion.ToString() + ".";
                        }
                    }
                    else if (version == 4)
                    {
                        if (versionUpdate.Count < version)
                        {
                            versionNumber = versionNumber + "0";
                        }
                        else
                        {
                            versionNumber = versionNumber + versionUpdate[version - 1].ToString();
                        }
                    }
                    else if (version == 2 || version == 1)
                    {

                        if (versionUpdate.Count < version)
                        {
                            versionNumber = versionNumber + "0" + ".";
                        }
                        else
                        {
                            versionNumber = versionNumber + versionUpdate[version - 1].ToString() + ".";
                        }
                    }
                }
            }
            this.CloneAPatch(versionNumber, parentSolutionName, syed_Solutiondetail);
        }

        /// <summary>
        /// Clone a Solution
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="versionNumber">Version Number</param>
        /// <param name="parentSolutionName">Parent Solution Name</param>
        /// <param name="syed_Solutiondetail">Solution Detail entity</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public void CloneASolution(string versionNumber, string parentSolutionName, syed_solutiondetail syed_Solutiondetail)
        {
            try
            {
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
                    CloneAsSolutionResponse cloneAsSolutionResponse = (CloneAsSolutionResponse)CrmService.Execute(cloneAsSolutionRequest);
                    EntityCollection solutionCollection = SolutionHelper.RetrieveSolutionById(CrmService, cloneAsSolutionResponse.SolutionId, CrmTracingService);
                    foreach (Solution sol in solutionCollection.Entities)
                    {
                        syed_mastersolutions syed_Mastersolutions = CrmService.Retrieve(syed_mastersolutions.EntityLogicalName.ToString(), syed_Solutiondetail.syed_CRMSolutionsId.Id, new ColumnSet(true)).ToEntity<syed_mastersolutions>();
                        ExecuteOperations.UpdateMasterSolution(CrmService, sol, syed_Mastersolutions);
                        ExecuteOperations.UpdateSolutionDetail(CrmService, syed_Mastersolutions, syed_Solutiondetail);
                        break;
                    }

                    WebJobsLog.AppendLine("Cloned solution - sucessfully " + syed_Solutiondetail.syed_ListofSolutions);
                    UpdateExceptionDetails(syed_Solutiondetail.syed_ListofSolutionId.Id.ToString(), WebJobsLog.ToString(), "Queued");
                }
            }
            catch (Exception ex)
            {
                WebJobsLog.AppendLine("Exception occured in clone solution- " + syed_Solutiondetail.syed_ListofSolutions + ", " + ex.Message);
                UpdateExceptionDetails(syed_Solutiondetail.syed_ListofSolutionId.Id.ToString(), WebJobsLog.ToString(), "Draft");
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        /// <summary>
        /// Clone a Patch
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="versionNumber">Version Number</param>
        /// <param name="parentSolutionName">Parent Solution Name</param>
        /// <param name="syed_Solutiondetail">Solution Detail entity</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public void CloneAPatch(string versionNumber, string parentSolutionName, syed_solutiondetail syed_Solutiondetail)
        {
            try
            {
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

                    CloneAsPatchResponse cloneAsPatchResponse = (CloneAsPatchResponse)CrmService.Execute(cloneAsPatchRequest);
                    syed_Solutiondetail.syed_CRMSolutionsId = new EntityReference(syed_mastersolutions.EntityLogicalName, cloneAsPatchResponse.SolutionId);

                    EntityCollection solutionCollection = SolutionHelper.RetrieveSolutionById(CrmService, cloneAsPatchResponse.SolutionId, CrmTracingService);
                    foreach (Solution sol in solutionCollection.Entities)
                    {
                        Guid id = ExecuteOperations.CreateMasterSolution(CrmService, sol);
                        syed_mastersolutions syed_Mastersolutions = CrmService.Retrieve(syed_mastersolutions.EntityLogicalName.ToString(), id, new ColumnSet(true)).ToEntity<syed_mastersolutions>();
                        ExecuteOperations.UpdateSolutionDetail(CrmService, syed_Mastersolutions, syed_Solutiondetail);
                        break;
                    }
                    WebJobsLog.AppendLine("Patch created - sucessfully " + syed_Solutiondetail.syed_ListofSolutions);
                    UpdateExceptionDetails(syed_Solutiondetail.syed_ListofSolutionId.Id.ToString(), WebJobsLog.ToString(), "Queued");
                }
            }
            catch (Exception ex)
            {
                WebJobsLog.AppendLine("Exception occured in clone a patch- " + syed_Solutiondetail.syed_ListofSolutions + ", " + ex.Message);
                UpdateExceptionDetails(syed_Solutiondetail.syed_ListofSolutionId.Id.ToString(), WebJobsLog.ToString(), "Draft");
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        /// <summary>
        /// To create Dynamics source control and create associated solution details.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solutionId">Dynamics Source Control id</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public void CheckMasterSolutionForCloneRequest(string solutionId)
        {
            try
            {
                EntityCollection masterSolutions = SolutionHelper.RetrieveMasterSolutionBySolutionOptions(CrmService, solutionId, CrmTracingService);
                foreach (syed_solutiondetail syed_Solutiondetail in masterSolutions.Entities)
                {

                    syed_mastersolutions crmSolution = CrmService.Retrieve(syed_mastersolutions.EntityLogicalName, syed_Solutiondetail.syed_CRMSolutionsId.Id, new ColumnSet("syed_solutionid")).ToEntity<syed_mastersolutions>();
                    if (crmSolution != null)
                    {
                        if (syed_Solutiondetail.syed_SolutionOptions.Value == 433710001)
                        {
                            this.ValidateVersionMember(syed_Solutiondetail, new Guid(crmSolution.syed_SolutionId));
                        }
                        else if (syed_Solutiondetail.syed_SolutionOptions.Value == 433710002)
                        {
                            this.ValidateVersionMemberForClone(syed_Solutiondetail, new Guid(crmSolution.syed_SolutionId));
                        }
                        else if (syed_Solutiondetail.syed_SolutionOptions.Value == 433710000)
                        {
                            this.UpdateVersionNumber(syed_Solutiondetail, new Guid(crmSolution.syed_SolutionId));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        public void UpdateExceptionDetails(string solutionId, string message, string status)
        {
            syed_sourcecontrolqueue syed_Sourcecontrolqueue = CrmService.Retrieve(syed_sourcecontrolqueue.EntityLogicalName, new Guid(solutionId), new ColumnSet(true)).ToEntity<syed_sourcecontrolqueue>();
            syed_Sourcecontrolqueue.syed_ExceptionDetails = syed_Sourcecontrolqueue.syed_ExceptionDetails + message;
            syed_Sourcecontrolqueue.syed_Status = status;
            syed_Sourcecontrolqueue.Id = new Guid(solutionId);
            CrmService.Update(syed_Sourcecontrolqueue);
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
                    WebJobsLog.AppendLine("SolutionId- Missing");
                    throw new InvalidPluginExecutionException("SolutionId- Missing");
                }

                string dynamicsSourceControlId = (string)objDynamicsSourceControlId;
                UpdateExceptionDetails(dynamicsSourceControlId, string.Empty, "Draft");
                this.CheckMasterSolutionForCloneRequest(dynamicsSourceControlId);
                CrmContext.OutputParameters["ErrorMessage"] = WebJobsLog.ToString();
            }
        }
    }
}
