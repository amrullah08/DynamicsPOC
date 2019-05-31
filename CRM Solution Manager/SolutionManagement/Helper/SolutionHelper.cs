﻿//-----------------------------------------------------------------------
// <copyright file="SolutionHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Jaiyanthi</author>
//-----------------------------------------------------------------------

namespace SolutionManagement
{
    using System;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;
    using SolutionConstants;

    /// <summary>
    /// Class that contains retrieve functions from CRM
    /// </summary>
    public class SolutionHelper
    {
        /// <summary>
        /// To retrieve CRM solutions.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        /// <returns>returns CRM solutions as entity collection</returns>
        public static EntityCollection FetchCrmSolutions(IOrganizationService service, ITracingService tracingService)
        {
            try
            {
                string fetchSolutions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='solution'>
                        <all-attributes />
                          <filter type='and'>                        
                          <condition value='true' attribute='isvisible' operator='eq' />
                           </filter>
                      </entity>
                    </fetch>";
                EntityCollection solutionlist = service.RetrieveMultiple(new FetchExpression(fetchSolutions));
                return solutionlist;
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.ToString());
                throw new InvalidPluginExecutionException(ex.Message.ToString(), ex);
            }
        }

        /// <summary>
        /// To retrieve Master solutions entity by CRM Solution ID.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solutionId">CRM Solution GUID</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        /// <returns>returns Master solutions as entity collection</returns>
        public static EntityCollection RetrieveMasterSolutionById(IOrganizationService service, Guid solutionId, ITracingService tracingService)
        {
            try
            {
                string fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='syed_mastersolutions'>
                                        <attribute name='syed_mastersolutionsid' />
                                        <attribute name='syed_name' />
                                        <attribute name='syed_solutionid' />
                                        <order attribute='syed_name' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='syed_solutionid' operator='eq' value='" + solutionId + @"' />
                                       </filter>
                                      </entity>
                                   </fetch>";

                EntityCollection masterSolution = service.RetrieveMultiple(new FetchExpression(fetchXML));
                return masterSolution;
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.ToString());
                throw new InvalidPluginExecutionException(ex.Message.ToString(), ex);
            }
        }

        /// <summary>
        /// To retrieve Master solutions entity.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        /// <returns>returns Master solutions as entity collection</returns>
        public static EntityCollection RetrieveMasterSolutions(IOrganizationService service, ITracingService tracingService)
        {
            try
            {
                string fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                        <entity name='syed_mastersolutions'>
                                        <attribute name='syed_mastersolutionsid' />
                                        <attribute name='syed_name' />
                                        <attribute name='createdon' /> 
                                        <attribute name='syed_solutionid' />
                                        <order attribute='syed_name' descending='false' />
                                        </entity>
                                    </fetch>";

                EntityCollection masterSolution = service.RetrieveMultiple(new FetchExpression(fetchXML));
                return masterSolution;
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.ToString());
                throw new InvalidPluginExecutionException(ex.Message.ToString(), ex);
            }
        }

        /// <summary>
        /// To retrieve CRM solutions entity ID.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="solutionId">CRM Solution GUID</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        /// <returns> returns CRM solutions as entity collection </returns>
        public static EntityCollection RetrieveSolutionById(IOrganizationService service, Guid solutionId, ITracingService tracingService)
        {
            try
            {
                string fetchSolutions = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='solution'>
                        <all-attributes />
                          <filter type='and'>                        
                          <condition value='true' attribute='isvisible' operator='eq' />
                          <condition attribute='solutionid' operator='eq' value='" + solutionId + @"' />
                           </filter>
                      </entity>
                    </fetch>";
                EntityCollection solutionlist = service.RetrieveMultiple(new FetchExpression(fetchSolutions));
                return solutionlist;
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.ToString());
                throw new InvalidPluginExecutionException(ex.Message.ToString(), ex);
            }
        }

        /// <summary>
        /// Method retrieves the list of Solution Details to be merged.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="sourceControlId">Dynamic Source Control GUID</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        /// <returns>returns solutions detail as entity collection</returns>
        public static EntityCollection RetrieveSolutionDetailsToBeMergedByListOfSolutionId(IOrganizationService service, Guid sourceControlId, ITracingService tracingService)
        {
            try
            {
                string fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='syed_mergesolutions'>
                                    <attribute name='syed_mergesolutionsid' />
                                    <attribute name='syed_name' />
                                    <attribute name='createdon' />
                                    <attribute name='syed_order' />
                                    <attribute name='syed_listofsolution' />
                                    <attribute name='syed_uniquename' />
                                    <order attribute='syed_order' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='syed_listofsolution' operator='eq' uitype='syed_sourcecontrolqueue' value='" + sourceControlId + @"' />
                                    </filter>
                                  </entity>
                                </fetch>";
                EntityCollection associatedRecordList = service.RetrieveMultiple(new FetchExpression(fetchXML));
                return associatedRecordList;
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.ToString());
                throw new InvalidPluginExecutionException(ex.Message.ToString(), ex);
            }
        }

        /// <summary>
        /// Method retrieves Master Solutions.
        /// </summary>
        /// <param name="service">Organization service</param>
        /// <param name="sourceControlId">Dynamic Source Control GUID</param>
        /// <param name="tracingService">Tracing Service to trace error</param>
        /// <returns>returns Solution Details as entity collection</returns>
        public static EntityCollection RetrieveMasterSolutionDetailsByListOfSolutionId(IOrganizationService service, Guid sourceControlId, ITracingService tracingService)
        {
            try
            {
                string fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='syed_solutiondetail'>
                                        <attribute name='syed_solutiondetailid' />
                                        <attribute name='syed_name' />
                                        <attribute name='createdon' />
                                        <attribute name='syed_order' />
                                        <attribute name='syed_solutionid' />
                                        <attribute name='syed_ismaster' />
                                        <attribute name='syed_listofsolutions' />
                                        <order attribute='syed_order' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='syed_listofsolutionid' operator='eq'  uitype='syed_sourcecontrolqueue'  value='" + sourceControlId + @"' />
                                        </filter>
                                      </entity>
                                    </fetch>";
                EntityCollection associatedRecordList = service.RetrieveMultiple(new FetchExpression(fetchXML));
                return associatedRecordList;
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.ToString());
                throw new InvalidPluginExecutionException(ex.Message.ToString(), ex);
            }
        }
    }
}
