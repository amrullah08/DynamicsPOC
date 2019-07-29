//-----------------------------------------------------------------------
// <copyright file="SolutionManager.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace MsCrmTools.SolutionComponentsMover.AppCode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CrmSolution;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Metadata;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Class merges components of solution to specified solution
    /// </summary>
    internal class SolutionManager
    {
        /// <summary>
        /// organization service
        /// </summary>
        private readonly IOrganizationService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionManager" /> class.
        /// </summary>
        /// <param name="service">organization service</param>
        public SolutionManager(IOrganizationService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Method returns solution components
        /// </summary>
        /// <param name="solutionUniqueName">solution unique name</param>
        /// <param name="service">organization service</param>
        /// <returns>returns solution compo</returns>
        public IEnumerable<EntityMetadata> GetSolutionEntities(string solutionUniqueName, IOrganizationService service)
        {
            // get solution components for solution unique name
            QueryExpression componentsQuery = new QueryExpression
            {
                EntityName = "solutioncomponent",
                ColumnSet = new ColumnSet("objectid", "solutioncomponent", "solution", "solutionid", "solutionid", "syed_sourcensolutions"),
                Criteria = new FilterExpression(),
            };

            componentsQuery.Criteria.AddCondition(new ConditionExpression("componenttype", ConditionOperator.Equal, 1));
            EntityCollection componentsResult = service.RetrieveMultiple(componentsQuery);

            // Get all entities
            RetrieveAllEntitiesRequest allEntitiesrequest = new RetrieveAllEntitiesRequest()
            {
                EntityFilters = EntityFilters.Entity | Microsoft.Xrm.Sdk.Metadata.EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };

            RetrieveAllEntitiesResponse allEntitiesresponse = null;
            try
            {
                allEntitiesresponse = (RetrieveAllEntitiesResponse)service.Execute(allEntitiesrequest);
            }
            catch (Exception ex)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + "Error for solution " + solutionUniqueName + " " + ex.Message);
                Console.WriteLine("Error for solution " + solutionUniqueName + " " + ex.Message);
            }

            // Join entities Id and solution Components Id
            return allEntitiesresponse.EntityMetadata.Join(componentsResult.Entities.Select(x => x.Attributes["objectid"]), x => x.MetadataId, y => y, (x, y) => x);
        }

        /// <summary>
        /// Method copies components
        /// </summary>
        /// <param name="solutionFileInfo">solution info file</param>
        public void CopyComponents(SolutionFileInfo solutionFileInfo)
        {
            solutionFileInfo.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemMergingStatus;
            solutionFileInfo.Update();
            var solutions = this.RetrieveSolutions();
            CopySettings copySettings = this.GetCopySettings();
            foreach (var solution in solutions)
            {
                if (solution["uniquename"].ToString().ToLower().Equals(solutionFileInfo.SolutionUniqueName.ToString().ToLower()))
                {
                    copySettings.TargetSolutions.Add(solution);
                }
                else if (solutionFileInfo.SolutionsToBeMerged.Count > 0)
                {
                    if (solutionFileInfo.SolutionsToBeMerged.Any(cc => cc.ToString().ToLower().Equals(solution["uniquename"].ToString().ToLower())))
                    {
                        copySettings.SourceSolutions.Add(solution);
                    }
                }
            }

            Console.WriteLine("Copying components into Master Solution.");
            Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<br><br><table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'><tr><th style='background-color: #B8DBFD;border: 1px solid #ccc'>Copying components into Master Solution</th></tr>");

            var components = this.CopyComponents(copySettings);
            Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</table><br><br>");

            var componentsMaster = this.RetrieveComponentsFromSolutions(copySettings.TargetSolutions.Select(T => T.Id).ToList(), copySettings.ComponentsTypes);
            var differentComponents = (from cm in componentsMaster where !components.Any(list => list.GetAttributeValue<Guid>("objectid") == cm.GetAttributeValue<Guid>("objectid")) select cm).ToList();

            Console.WriteLine("Displaying different(additional) components after merging");
            Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<br><br><table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'><tr><th style='background-color: #B8DBFD;border: 1px solid #ccc'>Displaying different(additional) components after merging</th></tr>");

            if (differentComponents != null)
            {
                foreach (var target in copySettings.TargetSolutions)
                {
                    foreach (var componentdetails in differentComponents)
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<tr>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:LightCyan;border: 1px solid #ccc'>");

                        this.GetComponentDetails(copySettings, target, componentdetails, componentdetails.GetAttributeValue<OptionSetValue>("componenttype").Value, componentdetails.GetAttributeValue<Guid>("objectid"), "componenttype", null);
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</tr>");
                    }
                }
            }
            else
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<tr>");
                Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:LightCyan;border: 1px solid #ccc'>");
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" No different(additional components found after Merging)");
                Console.WriteLine("No different(additional components found after Merging)");
                Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</tr>");
            }

            Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</table><br><br>");
            solutionFileInfo.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemMergingSuccessfulStatus;
            solutionFileInfo.Update();
        }

        /// <summary>
        /// Method retrieves target components
        /// </summary>
        /// <param name="serviceProxy">service proxy</param>
        /// <param name="retrieveResponse">retrieve response</param>
        /// <param name="type">type</param>
        private void QueryTargetComponents(OrganizationServiceProxy serviceProxy, RetrieveResponse retrieveResponse, string type)
        {
            var qe = new QueryExpression(type)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                                {
                                    new ConditionExpression("name", ConditionOperator.Equal, retrieveResponse.Entity.Attributes["name"].ToString()),
                                }
                }
            };
            EntityCollection solutionComponents = serviceProxy.RetrieveMultiple(qe);
            Entity solutionCom = solutionComponents.Entities[0];
        }

        /// <summary>
        /// To Get list of components in Solutions
        /// </summary>
        /// <param name="settings">settings details</param>
        /// <param name="target">target details</param>
        /// <param name="component">component details</param>
        /// <param name="componentType">component Type</param>
        /// <param name="componentId">component Id</param>
        /// <param name="componentDetails">component Details</param>
        public void GetComponentDetails(CopySettings settings, Entity target, Entity component, int componentType, Guid componentId, string componentDetails, OrganizationServiceProxy targetService)
        {
            Entity sourceSolution = null;

            if (settings != null)
            {
                sourceSolution = settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id);
            }

            switch (componentType)
            {
                case Constants.Entity:
                    var entityReq = new RetrieveEntityRequest();
                    entityReq.MetadataId = componentId;
                    var retrievedEntity = (RetrieveEntityResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(entityReq);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedEntity.EntityMetadata.LogicalName, component.FormattedValues[componentDetails], component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        var targetEntityReq = new RetrieveEntityRequest();
                        targetEntityReq.LogicalName = retrievedEntity.EntityMetadata.LogicalName;
                        var targetRetrievedEntity = (RetrieveEntityResponse)targetService.Execute(targetEntityReq);
                    }
                    break;

                case Constants.WebResources:
                    var webresource = new RetrieveRequest();
                    webresource.Target = new EntityReference("webresource", componentId);
                    webresource.ColumnSet = new ColumnSet(true);
                    var retrievedWebresource = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(webresource);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedWebresource.Entity.Contains("name") ? retrievedWebresource.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedWebresource, "webresource");
                    }
                    break;

                case Constants.Attribute:
                    var attributeReq = new RetrieveAttributeRequest();
                    attributeReq.MetadataId = componentId;
                    var retrievedAttribute = (RetrieveAttributeResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(attributeReq);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedAttribute.AttributeMetadata.LogicalName, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        var targetAttributeReq = new RetrieveAttributeRequest();
                        targetAttributeReq.EntityLogicalName = retrievedAttribute.AttributeMetadata.EntityLogicalName;
                        targetAttributeReq.LogicalName = retrievedAttribute.AttributeMetadata.LogicalName;
                        var targetRetrievedAttribute = (RetrieveAttributeResponse)targetService.Execute(targetAttributeReq);
                    }
                    break;

                case Constants.Relationship:
                    var relationshipReq = new RetrieveRelationshipRequest();
                    relationshipReq.MetadataId = componentId;
                    var retrievedrelationshipReq = (RetrieveRelationshipResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(relationshipReq);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedrelationshipReq.RelationshipMetadata.SchemaName, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        var targetRelationshipReq = new RetrieveRelationshipRequest();
                        targetRelationshipReq.Name = retrievedrelationshipReq.RelationshipMetadata.SchemaName;
                        var targetRetrievedrelationshipReq = (RetrieveRelationshipResponse)targetService.Execute(targetRelationshipReq);
                    }
                    break;

                case Constants.DisplayString:
                    var displayStringRequest = new RetrieveRequest();
                    displayStringRequest.Target = new EntityReference("displaystring", componentId);
                    displayStringRequest.ColumnSet = new ColumnSet(true);
                    var retrievedDisplayString = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(displayStringRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedDisplayString.Entity.Contains("name") ? retrievedDisplayString.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedDisplayString, "displaystring");
                    }
                    break;

                case Constants.SavedQuery:
                    var savedQueryRequest = new RetrieveRequest();
                    savedQueryRequest.Target = new EntityReference("savedquery", componentId);
                    savedQueryRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSavedQuery = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(savedQueryRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedSavedQuery.Entity.Contains("name") ? retrievedSavedQuery.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedSavedQuery, "savedquery");
                    }
                    break;

                case Constants.SavedQueryVisualization:
                    var savedQueryVisualizationRequest = new RetrieveRequest();
                    savedQueryVisualizationRequest.Target = new EntityReference("savedqueryvisualization", componentId);
                    savedQueryVisualizationRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSavedQueryVisualization = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(savedQueryVisualizationRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedSavedQueryVisualization.Entity.Contains("name") ? retrievedSavedQueryVisualization.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedSavedQueryVisualization, "savedqueryvisualization");
                    }
                    break;

                case Constants.SystemForm:
                    var systemFormRequest = new RetrieveRequest();
                    systemFormRequest.Target = new EntityReference("systemform", componentId);
                    systemFormRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSystemForm = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(systemFormRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedSystemForm.Entity.Contains("name") ? retrievedSystemForm.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedSystemForm, "systemform");
                    }
                    break;

                case Constants.HierarchyRule:
                    var hierarchyRuleRequest = new RetrieveRequest();
                    hierarchyRuleRequest.Target = new EntityReference("hierarchyrule", componentId);
                    hierarchyRuleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedHierarchyRule = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(hierarchyRuleRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedHierarchyRule.Entity.Contains("name") ? retrievedHierarchyRule.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedHierarchyRule, "hierarchyrule");
                    }
                    break;

                case Constants.SiteMap:
                    var siteMapRequest = new RetrieveRequest();
                    siteMapRequest.Target = new EntityReference("sitemap", componentId);
                    siteMapRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSiteMap = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(siteMapRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedSiteMap.Entity.Contains("name") ? retrievedSiteMap.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedSiteMap, "sitemap");
                    }
                    break;

                case Constants.PluginAssembly:
                    var pluginAssemblyRequest = new RetrieveRequest();
                    pluginAssemblyRequest.Target = new EntityReference("pluginassembly", componentId);
                    pluginAssemblyRequest.ColumnSet = new ColumnSet(true);
                    var retrievedPluginAssembly = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(pluginAssemblyRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedPluginAssembly.Entity.Contains("name") ? retrievedPluginAssembly.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedPluginAssembly, "pluginassembly");
                    }
                    break;

                case Constants.PluginType:
                    var pluginTypeRequest = new RetrieveRequest();
                    pluginTypeRequest.Target = new EntityReference("plugintype", componentId);
                    pluginTypeRequest.ColumnSet = new ColumnSet(true);
                    var retrievedPluginTypeRequest = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(pluginTypeRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedPluginTypeRequest.Entity.Contains("name") ? retrievedPluginTypeRequest.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedPluginTypeRequest, "plugintype");
                    }
                    break;

                case Constants.SDKMessageProcessingStep:
                    var sdkMessageProcessingStepRequest = new RetrieveRequest();
                    sdkMessageProcessingStepRequest.Target = new EntityReference("sdkmessageprocessingstep", componentId);
                    sdkMessageProcessingStepRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSDKMessageProcessingStep = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(sdkMessageProcessingStepRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedSDKMessageProcessingStep.Entity.Contains("name") ? retrievedSDKMessageProcessingStep.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedSDKMessageProcessingStep, "sdkmessageprocessingstep");
                    }
                    break;

                case Constants.ServiceEndpoint:
                    var serviceEndpointRequest = new RetrieveRequest();
                    serviceEndpointRequest.Target = new EntityReference("serviceendpoint", componentId);
                    serviceEndpointRequest.ColumnSet = new ColumnSet(true);
                    var retrievedServiceEndpoint = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(serviceEndpointRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedServiceEndpoint.Entity.Contains("name") ? retrievedServiceEndpoint.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedServiceEndpoint, "serviceendpoint");
                    }
                    break;

                case Constants.Report:
                    var reportRequest = new RetrieveRequest();
                    reportRequest.Target = new EntityReference("report", componentId);
                    reportRequest.ColumnSet = new ColumnSet(true);
                    var retrievedReport = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(reportRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedReport.Entity.Contains("name") ? retrievedReport.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedReport, "report");
                    }
                    break;

                case Constants.Role:
                    var roleRequest = new RetrieveRequest();
                    roleRequest.Target = new EntityReference("role", componentId);
                    roleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedRole = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(roleRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedRole.Entity.Contains("name") ? retrievedRole.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedRole, "role");
                    }
                    break;

                case Constants.FieldSecurityProfile:
                    var fieldSecurityProfileRequest = new RetrieveRequest();
                    fieldSecurityProfileRequest.Target = new EntityReference("fieldsecurityprofile", componentId);
                    fieldSecurityProfileRequest.ColumnSet = new ColumnSet(true);
                    var retrievedFieldSecurityProfile = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(fieldSecurityProfileRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedFieldSecurityProfile.Entity.Contains("name") ? retrievedFieldSecurityProfile.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedFieldSecurityProfile, "fieldsecurityprofile");
                    }
                    break;

                case Constants.ConnectionRole:
                    var connectionRoleRequest = new RetrieveRequest();
                    connectionRoleRequest.Target = new EntityReference("connectionrole", componentId);
                    connectionRoleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedConnectionRole = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(connectionRoleRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedConnectionRole.Entity.Contains("name") ? retrievedConnectionRole.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedConnectionRole, "connectionrole");
                    }
                    break;

                case Constants.Workflow:
                    var workflowRequest = new RetrieveRequest();
                    workflowRequest.Target = new EntityReference("workflow", componentId);
                    workflowRequest.ColumnSet = new ColumnSet(true);
                    var retrievedWorkflow = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(workflowRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedWorkflow.Entity.Contains("name") ? retrievedWorkflow.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedWorkflow, "workflow");
                    }
                    break;

                case Constants.KBArticleTemplate:
                    var articleTemplateRequest = new RetrieveRequest();
                    articleTemplateRequest.Target = new EntityReference("kbarticletemplate", componentId);
                    articleTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedKBArticleTemplate = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(articleTemplateRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedKBArticleTemplate.Entity.Contains("name") ? retrievedKBArticleTemplate.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedKBArticleTemplate, "kbarticletemplate");
                    }
                    break;

                case Constants.MailMergeTemplate:
                    var mailMergeTemplateRequest = new RetrieveRequest();
                    mailMergeTemplateRequest.Target = new EntityReference("mailmergetemplate", componentId);
                    mailMergeTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedMailMergeTemplate = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(mailMergeTemplateRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedMailMergeTemplate.Entity.Contains("name") ? retrievedMailMergeTemplate.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedMailMergeTemplate, "mailmergetemplate");
                    }
                    break;

                case Constants.ContractTemplate:
                    var contractTemplateRequest = new RetrieveRequest();
                    contractTemplateRequest.Target = new EntityReference("contracttemplate", componentId);
                    contractTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedContractTemplate = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(contractTemplateRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedContractTemplate.Entity.Contains("name") ? retrievedContractTemplate.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedContractTemplate, "contracttemplate");
                    }
                    break;

                case Constants.EmailTemplate:
                    var emailTemplateRequest = new RetrieveRequest();
                    emailTemplateRequest.Target = new EntityReference("template", componentId);
                    emailTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedEmailTemplate = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(emailTemplateRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedEmailTemplate.Entity.Contains("name") ? retrievedEmailTemplate.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedEmailTemplate, "template");
                    }
                    break;

                case Constants.SLA:
                    var slaRequest = new RetrieveRequest();
                    slaRequest.Target = new EntityReference("sla", componentId);
                    slaRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSLA = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(slaRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedSLA.Entity.Contains("name") ? retrievedSLA.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedSLA, "sla");
                    }
                    break;

                case Constants.ConvertRule:
                    var convertRuleRequest = new RetrieveRequest();
                    convertRuleRequest.Target = new EntityReference("convertrule", componentId);
                    convertRuleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedConvertRule = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(convertRuleRequest);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedConvertRule.Entity.Contains("name") ? retrievedConvertRule.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedConvertRule, "convertrule");
                    }
                    break;

                case Constants.SDKMessageProcessingStepImage:
                    var sdkmessageprocessingstepimage = new RetrieveRequest();
                    sdkmessageprocessingstepimage.Target = new EntityReference("sdkmessageprocessingstepimage", componentId);
                    sdkmessageprocessingstepimage.ColumnSet = new ColumnSet(true);
                    var retrievedSdkmessageprocessingstepimage = (RetrieveResponse)Singleton.CrmConstantsInstance.ServiceProxy.Execute(sdkmessageprocessingstepimage);
                    if (targetService == null)
                    {
                        this.PrintLog(retrievedSdkmessageprocessingstepimage.Entity.Contains("name") ? retrievedSdkmessageprocessingstepimage.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    }
                    else
                    {
                        this.QueryTargetComponents(targetService, retrievedSdkmessageprocessingstepimage, "sdkmessageprocessingstepimage");
                    }
                    break;

                default:
                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine("Unable to copy component type: " + component.FormattedValues[componentDetails] + " and objectID: " + componentId.ToString());
                    Console.WriteLine("Unable to copy component type: " + component.FormattedValues[componentDetails] + " and objectID: " + componentId.ToString());
                    break;
            }
        }

        /// <summary>
        /// copy settings
        /// </summary>
        /// <returns>returns copy settings object</returns>
        private CopySettings GetCopySettings()
        {
            CopySettings copySettings = new CopySettings() { ComponentsTypes = new List<int>(), SourceSolutions = new List<Entity>(), TargetSolutions = new List<Entity>() };
            copySettings.ComponentsTypes.AddRange(Singleton.ConstantsInstance.ComponentTypes);
            return copySettings;
        }

        /// <summary>
        /// method retrieves all solutions that are part of retrieve solution
        /// </summary>
        /// <returns>returns enumerable for solutions</returns>
        private IEnumerable<Entity> RetrieveSolutions()
        {
            var qe = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(new[]
                                        {
                                            "publisherid", "installedon", "version",
                                            "uniquename", "friendlyname", "description",
                                            "ismanaged"
                                        }),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("isvisible", ConditionOperator.Equal, true),
                        new ConditionExpression("uniquename", ConditionOperator.NotEqual, "Default")
                    }
                }
            };

            return this.service.RetrieveMultiple(qe).Entities;
        }

        /// <summary>
        /// method copies components with the specified settings
        /// </summary>
        /// <param name="settings">copy settings</param>
        /// <returns>list of components</returns>
        private List<Entity> CopyComponents(CopySettings settings)
        {
            var components = this.RetrieveComponentsFromSolutions(settings.SourceSolutions.Select(s => s.Id).ToList(), settings.ComponentsTypes);

            if (settings.TargetSolutions.Count > 0)
            {
                foreach (var target in settings.TargetSolutions)
                {
                    foreach (var component in components)
                    {
                        var request = new AddSolutionComponentRequest
                        {
                            AddRequiredComponents = false,
                            ComponentId = component.GetAttributeValue<Guid>("objectid"),
                            ComponentType = component.GetAttributeValue<OptionSetValue>("componenttype").Value,
                            SolutionUniqueName = target.GetAttributeValue<string>("uniquename"),
                        };
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<tr>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:powderblue;border: 1px solid #ccc'>");
                        this.GetComponentDetails(settings, target, component, component.GetAttributeValue<OptionSetValue>("componenttype").Value, component.GetAttributeValue<Guid>("objectid"), "componenttype", null);
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                        Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</tr>");
                        request.DoNotIncludeSubcomponents =
        component.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value == 1 ||
        component.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value == 2;

                        this.service.Execute(request);
                    }
                }
            }
            else
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<tr>");
                Singleton.SolutionFileInfoInstance.WebJobsLog.Append("<td style='width:100px;background-color:powderblue;border: 1px solid #ccc'>");
                Singleton.SolutionFileInfoInstance.WebJobsLog.Append("No Components to Display");
                Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</td>");
                Singleton.SolutionFileInfoInstance.WebJobsLog.Append("</tr>");
            }

            return components;
        }

        /// <summary>
        /// method for updating log file to Dynamic Source control record
        /// </summary>
        /// <param name="componentName">component Name</param>
        /// <param name="componentType">component Type</param>
        /// <param name="componentId">component Id</param>
        /// <param name="sourceSolution">source Solution</param>
        /// <param name="targetSolution">target Solution</param>
        private void PrintLog(string componentName, string componentType, Guid componentId, string sourceSolution, string targetSolution)
        {
            Console.WriteLine("Component Name: " + componentName);
            Console.WriteLine("Component Type: " + componentType);
            if (componentId != Guid.Empty)
            {
                Console.WriteLine("Component Id: " + componentId);
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Component Id: " + componentId + "<br>");
            }

            if (!string.IsNullOrEmpty(sourceSolution))
            {
                Console.WriteLine("Source Solution: " + sourceSolution);
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Source Solution: " + sourceSolution + "<br>");
            }

            if (!string.IsNullOrEmpty(targetSolution))
            {
                Console.WriteLine("Target Solution: " + targetSolution);
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Target Solution: " + targetSolution + "<br>");
            }

            Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Component Type: " + componentType + "<br>");
            Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Component Name: " + componentName + "<br>");
        }

        /// <summary>
        /// method retrieves components from the solution
        /// </summary>
        /// <param name="solutionsIds">list of solution ids</param>
        /// <param name="componentsTypes">list of component types</param>
        /// <returns>returns entity components</returns>
        private List<Entity> RetrieveComponentsFromSolutions(List<Guid> solutionsIds, List<int> componentsTypes)
        {
            var qe = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("solutionid", ConditionOperator.In, solutionsIds.ToArray()),

                        // comment below for all components
                        new ConditionExpression("componenttype", ConditionOperator.In, componentsTypes.ToArray())
                    }
                }
            };

            return this.service.RetrieveMultiple(qe).Entities.ToList();
        }
    }
}
