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
                else if (solutionFileInfo.SolutionsToBeMerged.Any(cc => cc.ToString().ToLower().Equals(solution["uniquename"].ToString().ToLower())))
                {
                    copySettings.SourceSolutions.Add(solution);
                }
            }

            Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Copying components into Master Solution.");
            Console.WriteLine("Copying components into Master Solution.");
            var components = this.CopyComponents(copySettings);
            var componentsMaster = this.RetrieveComponentsFromSolutions(copySettings.TargetSolutions.Select(T => T.Id).ToList(), copySettings.ComponentsTypes);
            var differentComponents = (from cm in componentsMaster where !components.Any(list => list.GetAttributeValue<Guid>("objectid") == cm.GetAttributeValue<Guid>("objectid")) select cm).ToList();
            if (differentComponents != null)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Displaying different(additional) components after merging");
                Console.WriteLine("Displaying different(additional) components after merging");
                foreach (var target in copySettings.TargetSolutions)
                {
                    foreach (var componentdetails in differentComponents)
                    {
                        this.GetComponentDetails(copySettings, target, componentdetails, componentdetails.GetAttributeValue<OptionSetValue>("componenttype").Value, componentdetails.GetAttributeValue<Guid>("objectid"), "componenttype");
                    }
                }
            }
            else
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" No different(additional components found after Merging)");
                Console.WriteLine("No different(additional components found after Merging)");
            }

            solutionFileInfo.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemMergingSuccessfulStatus;
            solutionFileInfo.Update();
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
        public void GetComponentDetails(CopySettings settings, Entity target, Entity component, int componentType, Guid componentId, string componentDetails)
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
                    var retrievedEntity = (RetrieveEntityResponse)this.service.Execute(entityReq);
                    this.PrintLog(retrievedEntity.EntityMetadata.LogicalName, component.FormattedValues[componentDetails], component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.WebResources:
                    var webresource = new RetrieveRequest();
                    webresource.Target = new EntityReference("webresource", componentId);
                    webresource.ColumnSet = new ColumnSet(true);
                    var retrievedWebresource = (RetrieveResponse)this.service.Execute(webresource);
                    this.PrintLog(retrievedWebresource.Entity.Contains("name") ? retrievedWebresource.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.Attribute:
                    var attributeReq = new RetrieveAttributeRequest();
                    attributeReq.MetadataId = componentId;
                    var retrievedAttribute = (RetrieveAttributeResponse)this.service.Execute(attributeReq);
                    this.PrintLog(retrievedAttribute.AttributeMetadata.LogicalName, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.Relationship:
                    var relationshipReq = new RetrieveRelationshipRequest();
                    relationshipReq.MetadataId = componentId;
                    var retrievedrelationshipReq = (RetrieveRelationshipResponse)this.service.Execute(relationshipReq);
                    this.PrintLog(retrievedrelationshipReq.RelationshipMetadata.SchemaName, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.DisplayString:
                    var displayStringRequest = new RetrieveRequest();
                    displayStringRequest.Target = new EntityReference("displaystring", componentId);
                    displayStringRequest.ColumnSet = new ColumnSet(true);
                    var retrievedDisplayString = (RetrieveResponse)this.service.Execute(displayStringRequest);
                    this.PrintLog(retrievedDisplayString.Entity.Contains("name") ? retrievedDisplayString.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.SavedQuery:
                    var savedQueryRequest = new RetrieveRequest();
                    savedQueryRequest.Target = new EntityReference("savedquery", componentId);
                    savedQueryRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSavedQuery = (RetrieveResponse)this.service.Execute(savedQueryRequest);
                    this.PrintLog(retrievedSavedQuery.Entity.Contains("name") ? retrievedSavedQuery.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.SavedQueryVisualization:
                    var savedQueryVisualizationRequest = new RetrieveRequest();
                    savedQueryVisualizationRequest.Target = new EntityReference("savedqueryvisualization", componentId);
                    savedQueryVisualizationRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSavedQueryVisualization = (RetrieveResponse)this.service.Execute(savedQueryVisualizationRequest);
                    this.PrintLog(retrievedSavedQueryVisualization.Entity.Contains("name") ? retrievedSavedQueryVisualization.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.SystemForm:
                    var systemFormRequest = new RetrieveRequest();
                    systemFormRequest.Target = new EntityReference("systemform", componentId);
                    systemFormRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSystemForm = (RetrieveResponse)this.service.Execute(systemFormRequest);
                    this.PrintLog(retrievedSystemForm.Entity.Contains("name") ? retrievedSystemForm.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.HierarchyRule:
                    var hierarchyRuleRequest = new RetrieveRequest();
                    hierarchyRuleRequest.Target = new EntityReference("hierarchyrule", componentId);
                    hierarchyRuleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedHierarchyRule = (RetrieveResponse)this.service.Execute(hierarchyRuleRequest);
                    this.PrintLog(retrievedHierarchyRule.Entity.Contains("name") ? retrievedHierarchyRule.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.SiteMap:
                    var siteMapRequest = new RetrieveRequest();
                    siteMapRequest.Target = new EntityReference("sitemap", componentId);
                    siteMapRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSiteMap = (RetrieveResponse)this.service.Execute(siteMapRequest);
                    this.PrintLog(retrievedSiteMap.Entity.Contains("name") ? retrievedSiteMap.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.PluginAssembly:
                    var pluginAssemblyRequest = new RetrieveRequest();
                    pluginAssemblyRequest.Target = new EntityReference("pluginassembly", componentId);
                    pluginAssemblyRequest.ColumnSet = new ColumnSet(true);
                    var retrievedPluginAssembly = (RetrieveResponse)this.service.Execute(pluginAssemblyRequest);
                    this.PrintLog(retrievedPluginAssembly.Entity.Contains("name") ? retrievedPluginAssembly.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.SDKMessageProcessingStep:
                    var sdkMessageProcessingStepRequest = new RetrieveRequest();
                    sdkMessageProcessingStepRequest.Target = new EntityReference("sdkmessageprocessingstep", componentId);
                    sdkMessageProcessingStepRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSDKMessageProcessingStep = (RetrieveResponse)this.service.Execute(sdkMessageProcessingStepRequest);
                    this.PrintLog(retrievedSDKMessageProcessingStep.Entity.Contains("name") ? retrievedSDKMessageProcessingStep.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.ServiceEndpoint:
                    var serviceEndpointRequest = new RetrieveRequest();
                    serviceEndpointRequest.Target = new EntityReference("serviceendpoint", componentId);
                    serviceEndpointRequest.ColumnSet = new ColumnSet(true);
                    var retrievedServiceEndpoint = (RetrieveResponse)this.service.Execute(serviceEndpointRequest);
                    this.PrintLog(retrievedServiceEndpoint.Entity.Contains("name") ? retrievedServiceEndpoint.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.Report:
                    var reportRequest = new RetrieveRequest();
                    reportRequest.Target = new EntityReference("report", componentId);
                    reportRequest.ColumnSet = new ColumnSet(true);
                    var retrievedReport = (RetrieveResponse)this.service.Execute(reportRequest);
                    this.PrintLog(retrievedReport.Entity.Contains("name") ? retrievedReport.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.Role:
                    var roleRequest = new RetrieveRequest();
                    roleRequest.Target = new EntityReference("role", componentId);
                    roleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedRole = (RetrieveResponse)this.service.Execute(roleRequest);
                    this.PrintLog(retrievedRole.Entity.Contains("name") ? retrievedRole.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.FieldSecurityProfile:
                    var fieldSecurityProfileRequest = new RetrieveRequest();
                    fieldSecurityProfileRequest.Target = new EntityReference("fieldsecurityprofile", componentId);
                    fieldSecurityProfileRequest.ColumnSet = new ColumnSet(true);
                    var retrievedFieldSecurityProfile = (RetrieveResponse)this.service.Execute(fieldSecurityProfileRequest);
                    this.PrintLog(retrievedFieldSecurityProfile.Entity.Contains("name") ? retrievedFieldSecurityProfile.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.ConnectionRole:
                    var connectionRoleRequest = new RetrieveRequest();
                    connectionRoleRequest.Target = new EntityReference("connectionrole", componentId);
                    connectionRoleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedConnectionRole = (RetrieveResponse)this.service.Execute(connectionRoleRequest);
                    this.PrintLog(retrievedConnectionRole.Entity.Contains("name") ? retrievedConnectionRole.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.Workflow:
                    var workflowRequest = new RetrieveRequest();
                    workflowRequest.Target = new EntityReference("workflow", componentId);
                    workflowRequest.ColumnSet = new ColumnSet(true);
                    var retrievedWorkflow = (RetrieveResponse)this.service.Execute(workflowRequest);
                    this.PrintLog(retrievedWorkflow.Entity.Contains("name") ? retrievedWorkflow.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.KBArticleTemplate:
                    var articleTemplateRequest = new RetrieveRequest();
                    articleTemplateRequest.Target = new EntityReference("kbarticletemplate", componentId);
                    articleTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedKBArticleTemplate = (RetrieveResponse)this.service.Execute(articleTemplateRequest);
                    this.PrintLog(retrievedKBArticleTemplate.Entity.Contains("name") ? retrievedKBArticleTemplate.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.MailMergeTemplate:
                    var mailMergeTemplateRequest = new RetrieveRequest();
                    mailMergeTemplateRequest.Target = new EntityReference("mailmergetemplate", componentId);
                    mailMergeTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedMailMergeTemplate = (RetrieveResponse)this.service.Execute(mailMergeTemplateRequest);
                    this.PrintLog(retrievedMailMergeTemplate.Entity.Contains("name") ? retrievedMailMergeTemplate.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.ContractTemplate:
                    var contractTemplateRequest = new RetrieveRequest();
                    contractTemplateRequest.Target = new EntityReference("contracttemplate", componentId);
                    contractTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedContractTemplate = (RetrieveResponse)this.service.Execute(contractTemplateRequest);
                    this.PrintLog(retrievedContractTemplate.Entity.Contains("name") ? retrievedContractTemplate.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.EmailTemplate:
                    var emailTemplateRequest = new RetrieveRequest();
                    emailTemplateRequest.Target = new EntityReference("template", componentId);
                    emailTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedEmailTemplate = (RetrieveResponse)this.service.Execute(emailTemplateRequest);
                    this.PrintLog(retrievedEmailTemplate.Entity.Contains("name") ? retrievedEmailTemplate.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.SLA:
                    var slaRequest = new RetrieveRequest();
                    slaRequest.Target = new EntityReference("sla", componentId);
                    slaRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSLA = (RetrieveResponse)this.service.Execute(slaRequest);
                    this.PrintLog(retrievedSLA.Entity.Contains("name") ? retrievedSLA.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
                    break;

                case Constants.ConvertRule:
                    var convertRuleRequest = new RetrieveRequest();
                    convertRuleRequest.Target = new EntityReference("convertrule", componentId);
                    convertRuleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedConvertRule = (RetrieveResponse)this.service.Execute(convertRuleRequest);
                    this.PrintLog(retrievedConvertRule.Entity.Contains("name") ? retrievedConvertRule.Entity.Attributes["name"].ToString() : string.Empty, component.FormattedValues[componentDetails].ToString(), component.Id, sourceSolution?.Attributes["friendlyname"].ToString() ?? string.Empty, target?.Attributes["friendlyname"].ToString());
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

                    this.GetComponentDetails(settings, target, component, component.GetAttributeValue<OptionSetValue>("componenttype").Value, component.GetAttributeValue<Guid>("objectid"), "componenttype");

                    request.DoNotIncludeSubcomponents =
                        component.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value == 1 ||
                        component.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value == 2;

                    this.service.Execute(request);
                }
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
            Console.WriteLine("Component Id: " + componentId);
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

            Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Component Name: " + componentName + "<br>");
            Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Component Type: " + componentType + "<br>");
            Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Component Id: " + componentId + "<br>");
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
