if (typeof (SYED) === "undefined") {
    SYED = { __namespace: true };
}
if (typeof (SYED.SourceControlQueue) === "undefined") {
    SYED.SourceControlQueue = { __namespace: true };
}

'use strict';
SYED.SourceControlQueue.EventHandler =
    {
        ShowHideSolutionDetailsSections: function (executionContext) {
            try {
                var formContext = executionContext.getFormContext();
                var HTMLSection = formContext.ui.tabs.get("Reference");
                SYED.SourceControlQueue.EventHandler.LockFields(executionContext);

                var formType = formContext.ui.getFormType();
                if (formType != "1") {
                    SYED.SourceControlQueue.EventHandler.ShowHideHTML(formContext, HTMLSection);
                    formContext.ui.tabs.get("MergeSolutions").setVisible(true);
                    formContext.ui.tabs.get("General").setVisible(true);
                    formContext.ui.tabs.get("Deployment_Details").sections.get("InstanceDetails").setVisible(true);
                }
                else {
                    formContext.ui.tabs.get("MergeSolutions").setVisible(false);
                    formContext.ui.tabs.get("Reference").setVisible(false);
                    formContext.ui.tabs.get("General").setVisible(false);
                    formContext.ui.tabs.get("Deployment_Details").sections.get("InstanceDetails").setVisible(false);
                    formContext.getAttribute("syed_name").setValue("SOL-" + new Date().toLocaleString());
                }

            }
            catch (ex) {
                console.log("Error at SYED.SourceControlQueue.EventHandler.ShowHideSolutionDetailsSections function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        },

        ShowHideHTML: function (formContext, HTMLSection) {
            try {
                var commitID = formContext.getAttribute("syed_commitids").getValue();
                if (commitID !== "" && commitID !== null) {
                    HTMLSection.setVisible(true);
                } else {
                    HTMLSection.setVisible(false);
                }

            }
            catch (ex) {
                console.log("Error at SYED.SourceControlQueue.EventHandler.RefreshPage function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        },

        RefreshPage: function (executionContext) {
            try {
                var formContext = null;

                //if (Xrm.Internal.isUci())
                //    formContext = executionContext;
                //else
                formContext = executionContext.getFormContext();

                var entityFormOptions = {};
                entityFormOptions["entityName"] = formContext.data.entity.getEntityName();
                entityFormOptions["entityId"] = formContext.data.entity.getId();

                Xrm.Navigation.openForm(entityFormOptions);
            }
            catch (ex) {
                console.log("Error at SYED.SourceControlQueue.EventHandler.RefreshPage function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        },

        SavePage: function (executionContext) {
            try {
                var formContext = null;

                if (Xrm.Internal.isUci())
                    formContext = executionContext;
                else
                    formContext = executionContext.getFormContext();

                formContext.data.save().then(
                    function () {

                        SYED.SourceControlQueue.EventHandler.LockFields(executionContext);
                        SYED.SourceControlQueue.EventHandler.RefreshPage(executionContext);
                    },
                    function (error) {
                        Xrm.Navigation.openAlertDialog(error.message);
                    });

            }
            catch (ex) {
                console.log("Error at SYED.SourceControlQueue.EventHandler.SavePage function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        },

        OnRowSelected: function (executionContext) {
            try {
                executionContext.getFormContext().getData().getEntity().attributes.forEach(function (attr) {
                    if (attr.getName() !== "syed_order" && attr.getName() !== "syed_newversion" && attr.getName() !== "syed_exportas") {
                        attr.controls.forEach(function (c) {
                            c.setDisabled(true);
                        })
                    }
                });
            }
            catch (ex) {
                console.log("Error at SYED.SourceControlQueue.EventHandler.OnRowSelected function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        },

        CheckDeploymentInstance: function (executionContext, successCall, errorCall) {
            try {
                var formContext = null;

                formContext = executionContext.getFormContext();

                var sourceControlQueueId = formContext.data.entity.getId();
                Xrm.WebApi.online.retrieveMultipleRecords("syed_deploymentinstance", "?$select=syed_instanceurl&$filter=_syed_dynamicssourcecontrol_value eq " + sourceControlQueueId + "").then(
                    function success(results) {
                        successCall(results);
                    },
                    function (error) {
                        Xrm.Utility.alertDialog(error.message);
                        errorCall(error);
                    }
                );

            }
            catch (ex) {
                console.log("Error at SYED.SourceControlQueue.EventHandler.CheckDeploymentInstance function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        },

        CallAction: function (executionContext, selectedId) {
            try {
                debugger;

                var formContext = null;
                if (Xrm.Internal.isUci())
                    formContext = executionContext;
                else
                    formContext = executionContext.getFormContext();

                selectedId = selectedId.replace("{", "").replace("}", "").toUpperCase();
                var parameters = {};
                parameters.SourceControlId = selectedId;

                var req = new XMLHttpRequest();
                req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/syed_CloneAPatch", false);
                req.setRequestHeader("OData-MaxVersion", "4.0");
                req.setRequestHeader("OData-Version", "4.0");
                req.setRequestHeader("Accept", "application/json");
                req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                req.onreadystatechange = function () {
                    if (this.readyState === 4) {
                        req.onreadystatechange = null;
                        if (this.status === 200) {
                            var results = JSON.parse(this.response);
                            formContext.getAttribute("syed_status").setValue("Queued");
                            SYED.SourceControlQueue.EventHandler.SavePage(executionContext);
                        } else {
                            Xrm.Utility.alertDialog(this.statusText);
                        }
                    }
                };
                req.send(JSON.stringify(parameters));
            }
            catch (ex) {
                console.log("Error at SYED.Solution.Ribbon.CallAction function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        },


        ShowHideMergeButton: function (executionContext) {
            try {
                var formContext = null;

                if (Xrm.Internal.isUci())
                    formContext = executionContext;
                else
                    formContext = executionContext.getFormContext();

                formContext.ui.clearFormNotification("SUBMIT");
                var sourceControlQueueId = formContext.data.entity.getId();
                var formType = formContext.ui.getFormType();

                if (formType != "1") {

                    Xrm.WebApi.online.retrieveMultipleRecords("syed_solutiondetail", "?$select=_syed_crmsolutionsid_value,syed_solutionoptions&$filter=_syed_listofsolutionid_value eq " + sourceControlQueueId + "").then(
                        function success(results) {
                            if (results.entities.length > 0) {
                                for (var i = 0; i < results.entities.length; i++) {
                                    var syed_solutionoptions = results.entities[i]["syed_solutionoptions"];
                                    if (syed_solutionoptions == "433710001" || syed_solutionoptions == "433710002") {
                                        SYED.SourceControlQueue.EventHandler.CallAction(executionContext, sourceControlQueueId.toLocaleString());
                                    }
                                    else {
                                        formContext.getAttribute("syed_status").setValue("Queued");
                                        SYED.SourceControlQueue.EventHandler.SavePage(executionContext);
                                    }
                                }
                            }
                            else {
                                formContext.ui.setFormNotification('To Submit, Please select Master Solution', 'ERROR', 'SUBMIT');
                            }
                        },
                        function (error) {
                            Xrm.Utility.alertDialog(error.message);
                        }
                    );
                }
            }
            catch (ex) {
                console.log("Error at SYED.SourceControlQueue.EventHandler.ShowHideMergeButton function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        },

        DispalySubmitButton: function (executionContext) {
            try {
                var formContext = null;

                if (Xrm.Internal.isUci())
                    formContext = executionContext;
                else
                    formContext = executionContext.getFormContext();

                var QueueStatus = formContext.getAttribute("syed_status").getValue();
                if (QueueStatus != "" && QueueStatus != null && QueueStatus != "Draft") {
                    return false;
                }
                else {
                    return true;
                }
            } catch (ex) {
                console.log("Error at SYED.SourceControlQueue.EventHandler.DispalySubmitButton function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        },

        LockFields: function (executionContext) {
            try {
                var formContext = null;
                formContext = executionContext.getFormContext();

                var QueueStatus = formContext.getAttribute("syed_status").getValue();
                if (QueueStatus != "" && QueueStatus != null && QueueStatus != "Draft") {
                    var section = Xrm.Page.ui.tabs.get("Deployment_Details").sections.get("Deployment_Details_Section");
                    var controls = section.controls.get();
                    var controlsLenght = controls.length;
                    for (var i = 0; i < controlsLenght; i++) {
                        controls[i].setDisabled(true);
                    }
                }
            } catch (ex) {
                console.log("Error at SYED.SourceControlQueue.EventHandler.LockFields function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        }
    }