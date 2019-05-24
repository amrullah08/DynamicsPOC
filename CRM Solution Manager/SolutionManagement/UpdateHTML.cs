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
            if (sourceControlQueue.Attributes.Contains("syed_commitids"))
            {
                string commitId = sourceControlQueue.Attributes["syed_commitids"].ToString();
                Entity webResource = RetrieveSolutions.RetrieveHTML(service, tracingService);
                RetrieveSolutions.UpdateHTMLContent(service, webResource, commitId, tracingService);
            }
        }

        /// <summary>
        /// To update comma separated solution list in Dynamic Source Control entity.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="sourceControlQueue">Dynamic Source Control entity GUID</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public static void DeleteSolutionDetail(IOrganizationService service, Entity sourceControlQueue, ITracingService tracingService)
        {
            if (sourceControlQueue.Attributes.Contains("syed_mergesolutions"))
            {
                bool mergeSolutions = (bool)sourceControlQueue.Attributes["syed_mergesolutions"];
                if (mergeSolutions == false)
                {
                    EntityCollection associatedRecordList = RetrieveSolutions.AddListToSolution(service, sourceControlQueue.Id, tracingService);
                    if (associatedRecordList.Entities.Count > 0)
                    {
                        foreach (Entity item in associatedRecordList.Entities)
                        {
                            Guid solutionID = new Guid(item.Attributes["syed_solutiondetailid"].ToString());
                            service.Delete("syed_solutiondetail", solutionID);
                        }
                    }
                }
            }
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

            Entity postSourceControlQueue = null;

            if (context.InputParameters != null)
            {
                if (context.MessageName.ToLower() == "update")
                {
                    if (context.PostEntityImages != null && context.PostEntityImages.Contains("PostImage"))
                    {
                        postSourceControlQueue = (Entity)context.PostEntityImages["PostImage"];
                    }

                    if (postSourceControlQueue.LogicalName == "syed_sourcecontrolqueue")
                    {
                        DeleteSolutionDetail(service, postSourceControlQueue, tracingService);
                        UpdateHTMLWebResource(service, postSourceControlQueue, tracingService);
                    }
                }
            }
        }
    }
}
