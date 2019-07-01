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
    using System.ComponentModel;
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
                Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Error for solution " + solutionUniqueName + " " + ex.Message);
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
            Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Copying components into Master Solution...");
            var components = this.CopyComponents(copySettings);
            var componentsMaster = this.RetrieveComponentsFromSolutions(copySettings.TargetSolutions.Select(T => T.Id).ToList(), copySettings.ComponentsTypes);
            var differentComponents = (from cm in componentsMaster where !components.Any(list => list.GetAttributeValue<Guid>("objectid") == cm.GetAttributeValue<Guid>("objectid")) select cm).ToList();
            if (differentComponents != null)
            {
                Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Displaying different(additional) components after merging");
                foreach (var target in copySettings.TargetSolutions)
                {
                    foreach (var componentdetails in differentComponents)
                    {
                        GetComponentDetails(copySettings, target, componentdetails, componentdetails.GetAttributeValue<OptionSetValue>("componenttype").Value, false);
                    }
                }
            }
            else
            {
                Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("No different(additional components found after Merging)");
            }

            //GetComponentDetails(copySettings,)
            //var differComponents = componentsMaster.Select(list => list.GetAttributeValue<Guid>("objectid") != components[0].GetAttributeValue<Guid>("objectid"));
            solutionFileInfo.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemMergingSuccessfulStatus;
            solutionFileInfo.Update();
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

                    GetComponentDetails(settings, target, component, component.GetAttributeValue<OptionSetValue>("componenttype").Value, true);

                    request.DoNotIncludeSubcomponents =
                        component.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value == 1 ||
                        component.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value == 2;

                    this.service.Execute(request);
                }
            }
            return components;
        }

        private void GetComponentDetails(CopySettings settings, Entity target, Entity component, int ComponentType, bool isSourceSolutionAvailable)
        {
            switch (ComponentType)
            {
                case Constants.Entity:
                    var entityReq = new RetrieveEntityRequest();
                    entityReq.MetadataId = component.GetAttributeValue<Guid>("objectid");
                    var retrievedEntity = (RetrieveEntityResponse)service.Execute(entityReq);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + retrievedEntity.EntityMetadata.LogicalName + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.WebResources:
                    var webresource = new RetrieveRequest();
                    webresource.Target = new EntityReference("webresource", component.GetAttributeValue<Guid>("objectid"));
                    webresource.ColumnSet = new ColumnSet(true);
                    var retrievedWebresource = (RetrieveResponse)service.Execute(webresource);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedWebresource.Entity.Contains("name") ? retrievedWebresource.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.Attribute:
                    var attributeReq = new RetrieveAttributeRequest();
                    attributeReq.MetadataId = component.GetAttributeValue<Guid>("objectid");
                    var retrievedAttribute = (RetrieveAttributeResponse)service.Execute(attributeReq);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + retrievedAttribute.AttributeMetadata.LogicalName + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.Relationship:
                    var relationshipReq = new RetrieveRelationshipRequest();
                    relationshipReq.MetadataId = component.GetAttributeValue<Guid>("objectid");
                    var retrievedrelationshipReq = (RetrieveRelationshipResponse)service.Execute(relationshipReq);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + retrievedrelationshipReq.RelationshipMetadata.SchemaName + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.DisplayString:
                    var displayStringRequest = new RetrieveRequest();
                    displayStringRequest.Target = new EntityReference("displaystring", component.GetAttributeValue<Guid>("objectid"));
                    displayStringRequest.ColumnSet = new ColumnSet(true);
                    var retrievedDisplayString = (RetrieveResponse)service.Execute(displayStringRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedDisplayString.Entity.Contains("name") ? retrievedDisplayString.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.SavedQuery:
                    var savedQueryRequest = new RetrieveRequest();
                    savedQueryRequest.Target = new EntityReference("savedquery", component.GetAttributeValue<Guid>("objectid"));
                    savedQueryRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSavedQuery = (RetrieveResponse)service.Execute(savedQueryRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedSavedQuery.Entity.Contains("name") ? retrievedSavedQuery.Entity.Attributes["name"] : string.Empty));
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.SavedQueryVisualization:
                    var savedQueryVisualizationRequest = new RetrieveRequest();
                    savedQueryVisualizationRequest.Target = new EntityReference("savedqueryvisualization", component.GetAttributeValue<Guid>("objectid"));
                    savedQueryVisualizationRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSavedQueryVisualization = (RetrieveResponse)service.Execute(savedQueryVisualizationRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedSavedQueryVisualization.Entity.Contains("name") ? retrievedSavedQueryVisualization.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.SystemForm:
                    var systemFormRequest = new RetrieveRequest();
                    systemFormRequest.Target = new EntityReference("systemform", component.GetAttributeValue<Guid>("objectid"));
                    systemFormRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSystemForm = (RetrieveResponse)service.Execute(systemFormRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedSystemForm.Entity.Contains("name") ? retrievedSystemForm.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.HierarchyRule:
                    var hierarchyRuleRequest = new RetrieveRequest();
                    hierarchyRuleRequest.Target = new EntityReference("hierarchyrule", component.GetAttributeValue<Guid>("objectid"));
                    hierarchyRuleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedHierarchyRule = (RetrieveResponse)service.Execute(hierarchyRuleRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedHierarchyRule.Entity.Contains("name") ? retrievedHierarchyRule.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.SiteMap:
                    var siteMapRequest = new RetrieveRequest();
                    siteMapRequest.Target = new EntityReference("sitemap", component.GetAttributeValue<Guid>("objectid"));
                    siteMapRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSiteMap = (RetrieveResponse)service.Execute(siteMapRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedSiteMap.Entity.Contains("name") ? retrievedSiteMap.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.PluginAssembly:
                    var pluginAssemblyRequest = new RetrieveRequest();
                    pluginAssemblyRequest.Target = new EntityReference("pluginassembly", component.GetAttributeValue<Guid>("objectid"));
                    pluginAssemblyRequest.ColumnSet = new ColumnSet(true);
                    var retrievedPluginAssembly = (RetrieveResponse)service.Execute(pluginAssemblyRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedPluginAssembly.Entity.Contains("name") ? retrievedPluginAssembly.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.SDKMessageProcessingStep:
                    var sdkMessageProcessingStepRequest = new RetrieveRequest();
                    sdkMessageProcessingStepRequest.Target = new EntityReference("sdkmessageprocessingstep", component.GetAttributeValue<Guid>("objectid"));
                    sdkMessageProcessingStepRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSDKMessageProcessingStep = (RetrieveResponse)service.Execute(sdkMessageProcessingStepRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedSDKMessageProcessingStep.Entity.Contains("name") ? retrievedSDKMessageProcessingStep.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.ServiceEndpoint:
                    var serviceEndpointRequest = new RetrieveRequest();
                    serviceEndpointRequest.Target = new EntityReference("serviceendpoint", component.GetAttributeValue<Guid>("objectid"));
                    serviceEndpointRequest.ColumnSet = new ColumnSet(true);
                    var retrievedServiceEndpoint = (RetrieveResponse)service.Execute(serviceEndpointRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedServiceEndpoint.Entity.Contains("name") ? retrievedServiceEndpoint.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.Report:
                    var reportRequest = new RetrieveRequest();
                    reportRequest.Target = new EntityReference("report", component.GetAttributeValue<Guid>("objectid"));
                    reportRequest.ColumnSet = new ColumnSet(true);
                    var retrievedReport = (RetrieveResponse)service.Execute(reportRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedReport.Entity.Contains("name") ? retrievedReport.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.Role:
                    var roleRequest = new RetrieveRequest();
                    roleRequest.Target = new EntityReference("role", component.GetAttributeValue<Guid>("objectid"));
                    roleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedRole = (RetrieveResponse)service.Execute(roleRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedRole.Entity.Contains("name") ? retrievedRole.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.FieldSecurityProfile:
                    var fieldSecurityProfileRequest = new RetrieveRequest();
                    fieldSecurityProfileRequest.Target = new EntityReference("fieldsecurityprofile", component.GetAttributeValue<Guid>("objectid"));
                    fieldSecurityProfileRequest.ColumnSet = new ColumnSet(true);
                    var retrievedFieldSecurityProfile = (RetrieveResponse)service.Execute(fieldSecurityProfileRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedFieldSecurityProfile.Entity.Contains("name") ? retrievedFieldSecurityProfile.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.ConnectionRole:
                    var connectionRoleRequest = new RetrieveRequest();
                    connectionRoleRequest.Target = new EntityReference("connectionrole", component.GetAttributeValue<Guid>("objectid"));
                    connectionRoleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedConnectionRole = (RetrieveResponse)service.Execute(connectionRoleRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedConnectionRole.Entity.Contains("name") ? retrievedConnectionRole.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.Workflow:
                    var workflowRequest = new RetrieveRequest();
                    workflowRequest.Target = new EntityReference("workflow", component.GetAttributeValue<Guid>("objectid"));
                    workflowRequest.ColumnSet = new ColumnSet(true);
                    var retrievedWorkflow = (RetrieveResponse)service.Execute(workflowRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedWorkflow.Entity.Contains("name") ? retrievedWorkflow.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.KBArticleTemplate:
                    var kbArticleTemplateRequest = new RetrieveRequest();
                    kbArticleTemplateRequest.Target = new EntityReference("kbarticletemplate", component.GetAttributeValue<Guid>("objectid"));
                    kbArticleTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedKBArticleTemplate = (RetrieveResponse)service.Execute(kbArticleTemplateRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedKBArticleTemplate.Entity.Contains("name") ? retrievedKBArticleTemplate.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.MailMergeTemplate:
                    var mailMergeTemplateRequest = new RetrieveRequest();
                    mailMergeTemplateRequest.Target = new EntityReference("mailmergetemplate", component.GetAttributeValue<Guid>("objectid"));
                    mailMergeTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedMailMergeTemplate = (RetrieveResponse)service.Execute(mailMergeTemplateRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedMailMergeTemplate.Entity.Contains("name") ? retrievedMailMergeTemplate.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.ContractTemplate:
                    var contractTemplateRequest = new RetrieveRequest();
                    contractTemplateRequest.Target = new EntityReference("contracttemplate", component.GetAttributeValue<Guid>("objectid"));
                    contractTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedContractTemplate = (RetrieveResponse)service.Execute(contractTemplateRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedContractTemplate.Entity.Contains("name") ? retrievedContractTemplate.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.EmailTemplate:
                    var emailTemplateRequest = new RetrieveRequest();
                    emailTemplateRequest.Target = new EntityReference("template", component.GetAttributeValue<Guid>("objectid"));
                    emailTemplateRequest.ColumnSet = new ColumnSet(true);
                    var retrievedEmailTemplate = (RetrieveResponse)service.Execute(emailTemplateRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedEmailTemplate.Entity.Contains("name") ? retrievedEmailTemplate.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.SLA:
                    var slaRequest = new RetrieveRequest();
                    slaRequest.Target = new EntityReference("sla", component.GetAttributeValue<Guid>("objectid"));
                    slaRequest.ColumnSet = new ColumnSet(true);
                    var retrievedSLA = (RetrieveResponse)service.Execute(slaRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedSLA.Entity.Contains("name") ? retrievedSLA.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                case Constants.ConvertRule:
                    var convertRuleRequest = new RetrieveRequest();
                    convertRuleRequest.Target = new EntityReference("convertrule", component.GetAttributeValue<Guid>("objectid"));
                    convertRuleRequest.ColumnSet = new ColumnSet(true);
                    var retrievedConvertRule = (RetrieveResponse)service.Execute(convertRuleRequest);
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Name: " + (retrievedConvertRule.Entity.Contains("name") ? retrievedConvertRule.Entity.Attributes["name"] : string.Empty) + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Type: " + component.FormattedValues["componenttype"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Component Id: " + component.Id + "<br>");
                    if (isSourceSolutionAvailable)
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Source Solution: " + settings.SourceSolutions.Find(item => item.Id == component.GetAttributeValue<EntityReference>("solutionid").Id).Attributes["friendlyname"] + "<br>");
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Master Solution: " + target.Attributes["friendlyname"] + "<br>");
                    break;

                default:
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine("Unable to copy component type: " + component.FormattedValues["componenttype"] + " and objectID: " + component.Attributes["objectid"].ToString() + "<br>");
                    break;
            }
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