//-----------------------------------------------------------------------
// <copyright file="SolutionManagement.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Jaiyanthi</author>
//-----------------------------------------------------------------------

namespace SolutionManagement
{
    using System;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Class that assist in Solution Management
    /// </summary>
    public class SolutionManagement : IPlugin
    {
        /// <summary>
        /// To sync Master Solution and CRM Solutions.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public static void CheckSolutionDetails(IOrganizationService service, ITracingService tracingService)
        {
            EntityCollection solutionlist = RetrieveSolutions.CRMSolutions(service, tracingService);
            if (solutionlist.Entities.Count > 0)
            {
                foreach (Entity solution in solutionlist.Entities)
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
        public static void CreateSolutionRecords(IOrganizationService service, Entity solution, ITracingService tracingService)
        {
            bool isAlreadyExist = IsSolutionAvaialable(service, solution.Id, tracingService);
            if (isAlreadyExist == false)
            {
                ExecuteOperations.CreateRecords(service, solution);
            }
        }

        /// <summary>
        /// To check CRM Solutions is available if not found send for Delete Master Solution record.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solutionId"> CRM Solutions Guid</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        /// <returns>returns boolean value</returns>
        public static bool IsSolutionAvaialable(IOrganizationService service, Guid solutionId, ITracingService tracingService)
        {
            bool isAvailable = false;

            EntityCollection masterRecords = RetrieveSolutions.RetrieveMasterSolutionById(service, solutionId, tracingService);
            if (masterRecords.Entities.Count > 0)
            {
                isAvailable = true;
            }

            return isAvailable;
        }

        /// <summary>
        /// To check for Delete.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solutionId">CRM Solution Id</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        /// <returns>returns boolean value</returns>
        public static bool CheckForDelete(IOrganizationService service, string solutionId, ITracingService tracingService)
        {
            bool isDeletable = true;
            Guid solutionGuid = new Guid(solutionId);
            EntityCollection solutionlist = RetrieveSolutions.RetrieveSolutionById(service, solutionGuid, tracingService);
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
            EntityCollection masterSolution = RetrieveSolutions.RetrieveMasterSolution(service, tracingService);
            if (masterSolution.Entities.Count > 0)
            {
                foreach (Entity item in masterSolution.Entities)
                {
                    if (item.Attributes.Contains("syed_solutionid") && item.Attributes["syed_solutionid"] != null)
                    {
                        string solutionID = item.Attributes["syed_solutionid"].ToString();
                        bool isdeletable = CheckForDelete(service, solutionID, tracingService);

                        if (isdeletable == false)
                        {
                            Guid masterSolitionID = new Guid(item.Attributes["syed_mastersolutionsid"].ToString());
                            service.Delete("syed_mastersolutions", masterSolitionID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Plugin Solution Management.
        /// </summary>
        /// <param name="serviceProvider">serviceProvider to connect CRM</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Get a reference to the Organization service.
            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            if (context.InputParameters != null)
            {
                CheckSolutionDetails(service, tracingService);
            }
        }
    }
}
