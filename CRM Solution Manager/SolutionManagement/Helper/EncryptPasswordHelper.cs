//-----------------------------------------------------------------------
// <copyright file="EncryptPasswordHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Jaiyanthi</author>
//-----------------------------------------------------------------------

namespace SolutionManagement
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Xrm.Sdk;
    using SolutionConstants;

    /// <summary>
    /// Class that contains operations for Encrypt Password
    /// </summary>
    public class EncryptPasswordHelper : DevOpsBusinessBase, IPluginHelper
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="EncryptPasswordHelper" /> class.
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
            string encrypted = string.Empty;
            Entity deploymentinstance = null;
            if (CrmContext.InputParameters != null)
            {
                if (CrmContext.Depth < 1)
                {
                    return;
                }

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

                            string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                            byte[] clearBytes = Encoding.Unicode.GetBytes(deploymentinstance.Attributes["syed_password"].ToString());

                            using (Aes encryptor = Aes.Create())
                            {
                                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                                encryptor.Key = pdb.GetBytes(32);
                                encryptor.IV = pdb.GetBytes(16);
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                                    {
                                        cs.Write(clearBytes, 0, clearBytes.Length);
                                        cs.Close();
                                    }
                                    encrypted = Convert.ToBase64String(ms.ToArray());
                                }
                            }
                            deploymentinstance.Attributes["syed_password"] = encrypted;
                        }
                    }
                }
            }
        }
    }
}
