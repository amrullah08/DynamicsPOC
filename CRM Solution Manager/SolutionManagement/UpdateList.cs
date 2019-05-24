//-----------------------------------------------------------------------
// <copyright file="UpdateList.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Jaiyanthi</author>
//-----------------------------------------------------------------------

namespace SolutionManagement
{
    using System;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Class that assist in maintaining sort order in Solution Management
    /// </summary>
    public class UpdateList : IPlugin
    {
        /// <summary>
        /// To update comma separated solution list in Dynamic Source Control entity.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="sourceControlQueue">Dynamic Source Control entity GUID</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public static void UpdateListToSourceControl(IOrganizationService service, Entity sourceControlQueue, ITracingService tracingService)
        {
            EntityReference sourceControl = null;
            if (sourceControlQueue.Attributes.Contains("syed_listofsolutionid") && sourceControlQueue.Attributes["syed_listofsolutionid"] != null)
            {
                sourceControl = (EntityReference)sourceControlQueue.Attributes["syed_listofsolutionid"];
                ExecuteOperations.AddSolutionToList(service, sourceControl.Id, tracingService);
                ExecuteOperations.AddSolutionToMaster(service, sourceControl.Id, tracingService);
            }
        }

        /// <summary>
        /// Plugin for maintaining sort order in Solution Management.
        /// </summary>
        /// <param name="serviceProvider">serviceProvider to connect CRM</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            Entity solutionDetail = null;

            if (context.InputParameters != null)
            {
                if (context.MessageName.ToLower() == "update" || context.MessageName.ToLower() == "create")
                {
                    if (context.PostEntityImages != null && context.PostEntityImages.Contains("PostImage"))
                    {
                        solutionDetail = (Entity)context.PostEntityImages["PostImage"];

                        if (solutionDetail != null)
                        {
                            if (solutionDetail.LogicalName == "syed_solutiondetail")
                            {
                                UpdateListToSourceControl(service, solutionDetail, tracingService);
                            }
                        }
                    }
                }

                if (context.MessageName.ToLower() == "delete")
                {
                    if (context.PreEntityImages != null && context.PreEntityImages.Contains("PreImage"))
                    {
                        solutionDetail = (Entity)context.PreEntityImages["PreImage"];

                        if (solutionDetail != null)
                        {
                            if (solutionDetail.LogicalName == "syed_solutiondetail")
                            {
                                UpdateListToSourceControl(service, solutionDetail, tracingService);
                            }
                        }
                    }
                }
            }
        }
    }
}