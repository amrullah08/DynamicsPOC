//-----------------------------------------------------------------------
// <copyright file="DevOpsBusinessBase.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Jaiyanthi</author>
//-----------------------------------------------------------------------

using Microsoft.Xrm.Sdk;

/// <summary>
/// Base class
/// </summary>
public class DevOpsBusinessBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DevOpsBusinessBase" /> class.
    /// </summary>
    /// <param name="crmService">CRM Service</param>
    /// <param name="crmInitiatingUserService">CRM Initiating User Service</param>
    /// <param name="crmContext">CRM Context</param>
    /// <param name="crmTracingService">CRM Tracing Service</param>
    public DevOpsBusinessBase(IOrganizationService crmService, IOrganizationService crmInitiatingUserService, IPluginExecutionContext crmContext, ITracingService crmTracingService)
    {
        this.CrmService = crmService;
        this.CrmInitiatingUserService = crmInitiatingUserService;
        this.CrmContext = crmContext;
        this.CrmTracingService = crmTracingService;
    }

    /// <summary>
    /// Gets or sets CRM Service
    /// </summary>
    protected IOrganizationService CrmService { get; set; }

    /// <summary>
    /// Gets or sets IOrganization Service
    /// </summary>
    protected IOrganizationService CrmInitiatingUserService { get; set; }

    /// <summary>
    /// Gets or sets IPlugin Execution Context
    /// </summary>
    protected IPluginExecutionContext CrmContext { get; set; }

    /// <summary>
    /// Gets or sets ITelemetry Helper
    /// </summary>
    protected ITracingService CrmTracingService { get; set; }
}