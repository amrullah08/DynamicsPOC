//-----------------------------------------------------------------------
// <copyright file="ExecuteOperations.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Jaiyanthi</author>
//-----------------------------------------------------------------------

namespace SolutionManagement
{
    using System;
    using System.Collections;
    using Microsoft.Xrm.Sdk;
    
    /// <summary>
    /// Class that contains execute functions in CRM
    /// </summary>
    public class ExecuteOperations
    {
        /// <summary>
        /// To Create Records in Master Solution
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="masterSolution">Master Solution</param>
        public static void CreateRecords(IOrganizationService service, Entity masterSolution)
        {
            Entity masterSolutionUpdate = new Entity("syed_mastersolutions");
            masterSolutionUpdate["syed_name"] = masterSolution.Attributes["friendlyname"].ToString();
            masterSolutionUpdate["syed_friendlyname"] = masterSolution.Attributes["friendlyname"].ToString();
            masterSolutionUpdate["syed_publisher"] = ((EntityReference)masterSolution.Attributes["publisherid"]).Name;
            masterSolutionUpdate["syed_listofsolutions"] = masterSolution.Attributes["uniquename"].ToString();
            masterSolutionUpdate["syed_solutionid"] = masterSolution.Id.ToString();
            masterSolutionUpdate["syed_solutioninstalledon"] = masterSolution.Attributes["installedon"];
            masterSolutionUpdate["syed_version"] = masterSolution.Attributes["version"].ToString();
            masterSolutionUpdate["syed_ismanaged"] = masterSolution.Attributes["ismanaged"];
            service.Create(masterSolutionUpdate);
        }

        /// <summary>
        /// To Update comma separated List of solutions unique name to Dynamic Source Control Entity.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="sourceControlId">Guid of Dynamic Source Control</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public static void AddSolutionToList(IOrganizationService service, Guid sourceControlId, ITracingService tracingService)
        {
            EntityCollection associatedRecordList = RetrieveSolutions.AddListToSolution(service, sourceControlId, tracingService);
            Entity sourceControlQueue = new Entity("syed_sourcecontrolqueue");
            string uniqueName = string.Empty;
            ArrayList uniqueArray = new ArrayList();

            if (associatedRecordList.Entities.Count > 0)
            {
                foreach (Entity item in associatedRecordList.Entities)
                {
                    if (item.Attributes.Contains("syed_listofsolutions") && item.Attributes["syed_listofsolutions"] != null)
                    {
                        uniqueArray.Add(item.Attributes["syed_listofsolutions"].ToString());
                    }
                }
            }

            uniqueName = string.Join(",", uniqueArray.ToArray());
            sourceControlQueue["syed_sourcecontrolqueueid"] = sourceControlId;
            sourceControlQueue["syed_sourcensolutions"] = uniqueName;
            service.Update(sourceControlQueue);
        }

        /// <summary>
        /// To Update comma separated Master solutions unique name to Dynamic Source Control Entity.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="sourceControlId">Guid of Dynamic Source Control</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        public static void AddSolutionToMaster(IOrganizationService service, Guid sourceControlId, ITracingService tracingService)
        {
            EntityCollection associatedRecordList = RetrieveSolutions.AddListToMaster(service, sourceControlId, tracingService);
            Entity sourceControlQueue = new Entity("syed_sourcecontrolqueue");
            string uniqueName = string.Empty;
            ArrayList uniqueArray = new ArrayList();

            if (associatedRecordList.Entities.Count > 0)
            {
                foreach (Entity item in associatedRecordList.Entities)
                {
                    if (item.Attributes.Contains("syed_listofsolutions") && item.Attributes["syed_listofsolutions"] != null)
                    {
                        uniqueArray.Add(item.Attributes["syed_listofsolutions"].ToString());
                    }
                }
            }

            uniqueName = string.Join(",", uniqueArray.ToArray());
            sourceControlQueue["syed_sourcecontrolqueueid"] = sourceControlId;
            sourceControlQueue["syed_solutionname"] = uniqueName;
            service.Update(sourceControlQueue);
        }
    }
}
