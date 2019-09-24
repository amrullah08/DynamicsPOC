﻿if (typeof (SYED) === "undefined") {
    SYED = { __namespace: true };
}
if (typeof (SYED.SolutionDetail) === "undefined") {
    SYED.Solution = { __namespace: true };
}

'use strict';
SYED.Solution.Ribbon =
    {
        Execute: function (selectedId, mode) {
            try {
                debugger;
                if (selectedId.length > 0) {
                    selectedId = selectedId[0].replace("{", "").replace("}", "").toUpperCase();

                    Xrm.WebApi.online.retrieveRecord("solution", selectedId, "?$select=ismanaged").then(
                        function success(result) {
                            var ismanaged = result["ismanaged"];
                            if (ismanaged) {
                                Xrm.Utility.alertDialog("Please select any Unmanaged Solution to process");
                            }
                            else {
                                SYED.Solution.Ribbon.CallAction(selectedId, mode);
                            }
                        },
                        function (error) {
                            Xrm.Utility.alertDialog(error.message);
                        }
                    );

                }
                else {
                    Xrm.Utility.alertDialog("Please select any one of Unmanaged Solution to process");
                }

            }
            catch (ex) {
                console.log("Error at SYED.Solution.Ribbon.Execute function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        },

        CallAction: function (selectedId, mode) {
            try {
                debugger;
                var parameters = {};
                parameters.SolutionId = selectedId;
                parameters.CheckIn = mode;

                var req = new XMLHttpRequest();
                req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/syed_CreateDynamicsSourceControlBySolution", false);
                req.setRequestHeader("OData-MaxVersion", "4.0");
                req.setRequestHeader("OData-Version", "4.0");
                req.setRequestHeader("Accept", "application/json");
                req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                req.onreadystatechange = function () {
                    if (this.readyState === 4) {
                        req.onreadystatechange = null;
                        if (this.status === 200) {
                            var results = JSON.parse(this.response);
                            Xrm.Utility.alertDialog(mode + "- is in progress, for more details, Please refer Dynamics Source Control record");
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
        }
    }