//-----------------------------------------------------------------------
// <copyright file="VersionManagement.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Jaiyanthi</author>
//-----------------------------------------------------------------------

namespace SolutionManagement
{
    using System;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Class that assist in Version Management
    /// </summary>
    public class VersionManagement : IPlugin
    {
        /// <summary>
        /// Plugin Solution Management.
        /// </summary>
        /// <param name="serviceProvider">serviceProvider to connect CRM</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            var crmContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var crmService = serviceFactory.CreateOrganizationService(crmContext.UserId);
            var crmInitiatingUserService = serviceFactory.CreateOrganizationService(crmContext.InitiatingUserId);
            var crmTracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginHelper solutionHelper = new VersionManagementHelper(crmService, crmInitiatingUserService, crmContext, crmTracingService);
            solutionHelper.Plugin();
        }
    }
}
