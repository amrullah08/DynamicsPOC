//-----------------------------------------------------------------------
// <copyright file="UpdateHTML.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Jaiyanthi</author>
//-----------------------------------------------------------------------

namespace SolutionManagement
{
    using System;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Class that assist in updating HTML WebResource
    /// </summary>
    public class UpdateHTML : IPlugin
    {
        /// <summary>
        /// To update CRM HTML Web Resource.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="sourceControlQueue">Dynamic Source Control entity</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public static void UpdateHTMLWebResource(IOrganizationService service, Entity sourceControlQueue, ITracingService tracingService)
        {
            string commitId = sourceControlQueue.Attributes["syed_commitids"].ToString();
            Entity webResource = RetrieveSolutions.RetrieveHTML(service, tracingService);
            RetrieveSolutions.UpdateHTMLContent(service, webResource, commitId, tracingService);
        }

        /// <summary>
        /// Plugin for updating HTML WebResource.
        /// </summary>
        /// <param name="serviceProvider">serviceProvider to connect CRM</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            Entity sourceControlQueue = null;

            if (context.InputParameters != null)
            {
                if (context.MessageName.ToLower() == "update")
                {
                    // Get Target Entity
                    if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                    {
                        sourceControlQueue = (Entity)context.InputParameters["Target"];
                    }

                    if (sourceControlQueue.LogicalName == "syed_sourcecontrolqueue")
                    {
                        UpdateHTMLWebResource(service, sourceControlQueue, tracingService);
                    }
                }
            }
        }
    }
}
