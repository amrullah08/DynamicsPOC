//-----------------------------------------------------------------------
// <copyright file="EncryptPasswordHelper.cs" company="Microsoft">
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
    /// Class that contains operations for Encrypt Password
    /// </summary>
    public class EncryptPasswordHelper : DevOpsBusinessBase, IPluginHelper
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="DynamicSourceControlOperationsHelper" /> class.
        /// </summary>
        /// <param name="crmService">Organization service</param>
        /// <param name="crmInitiatingUserService">Initiating User Service</param>
        /// <param name="crmContext">Plugin Execution Context</param>
        /// <param name="crmTracingService">Tracing Service</param>
        public EncryptPasswordHelper(IOrganizationService crmService, IOrganizationService crmInitiatingUserService, IPluginExecutionContext crmContext, ITracingService crmTracingService) : base(crmService, crmInitiatingUserService, crmContext, crmTracingService)
        {
        }

        /// <summary>
        ///  Dynamics Source Control Operations
        /// </summary>
        public void Plugin()
        {
            Entity deploymentinstance = null;
            if (CrmContext.InputParameters != null)
            {
                if (CrmContext.Depth < 1)
                    return;

                if (CrmContext.MessageName.ToLower() == CRMConstant.PluginCreate || CrmContext.MessageName.ToLower() == CRMConstant.PluginUpdate)
                {
                    if (CrmContext.InputParameters.Contains(CRMConstant.PluginTarget) && CrmContext.InputParameters[CRMConstant.PluginTarget] is Entity)
                    {
                        deploymentinstance = (Entity)CrmContext.InputParameters[CRMConstant.PluginTarget];
                    }

                    if (deploymentinstance != null && deploymentinstance.LogicalName == syed_deploymentinstance.EntityLogicalName)
                    {
                        if (deploymentinstance.Attributes.Contains("syed_password") && deploymentinstance.Attributes["syed_password"] != null)
                        {
                            byte[] bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(deploymentinstance.Attributes["syed_password"].ToString());
                            string encrypted = Convert.ToBase64String(bytes);
                            deploymentinstance.Attributes["syed_password"] = encrypted;
                        }
                    }
                }
            }
        }
    }
}
