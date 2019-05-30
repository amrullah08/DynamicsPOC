//-----------------------------------------------------------------------
// <copyright file="DynamicSourceControlOperations.cs" company="Microsoft">
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
    public class DynamicSourceControlOperations : IPlugin
    {
        /// <summary>
        /// Plugin for updating HTML WebResource.
        /// </summary>
        /// <param name="serviceProvider">serviceProvider to connect CRM</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            var crmContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var crmService = serviceFactory.CreateOrganizationService(crmContext.UserId);
            var crmInitiatingUserService = serviceFactory.CreateOrganizationService(crmContext.InitiatingUserId);
            var crmTracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginHelper pluginHelper = new DynamicSourceControlOperationsHelper(crmService, crmInitiatingUserService, crmContext, crmTracingService);
            pluginHelper.Plugin();
        }
    }
}
