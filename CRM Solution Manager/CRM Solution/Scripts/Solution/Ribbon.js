if (typeof (SYED) === "undefined") {
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

                if (selectedId.length > 0) {
                    selectedId = selectedId[0].replace("{", "").replace("}", "").toUpperCase();

                    Xrm.WebApi.online.retrieveRecord("solution", selectedId, "?$select=ismanaged").then(
                        function success(result) {
                            var ismanaged = result["ismanaged"];
                            if (ismanaged) {
                                Xrm.Utility.alertDialog("Please select any Unmanaged Solution to process");
                            }
                            else {
                                SYED.Solution.Ribbon.CallAction(selectedId, mode,
                                    function (results) {
                                        Xrm.Utility.alertDialog(mode + "- is in progress,for more details please refer Dynamics Source Control records.");

                                    },
                                    function (ex) {
                                        console.log("Error at SYED.Solution.Ribbon.Execute function: " + ex.message + "|" + "Stack: " + ex.stack);
                                        throw ex;
                                    }
                                );
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

        CreateDynamicsControl: function (selectedId, mode, successCall, errorCall) {
            try {

                var entity = {};
                entity.syed_name = "SOL-" + new Date().toLocaleString();
                entity.syed_checkin = true;
                if (mode == "Release") {
                    entity.syed_includeinrelease = true;
                }
                else {
                    entity.syed_includeinrelease = false;
                }
                entity.syed_overwritesolutionstxt = 433710000;
                entity.syed_comment = "Check In -" + new Date().toLocaleString();
                entity.syed_checkinbysolution = true;
                entity.syed_checkinbysolutionid = selectedId;

                Xrm.WebApi.online.createRecord("syed_sourcecontrolqueue", entity).then(
                    function success(sourceControlResults) {
                        var sourceControl = sourceControlResults.id;
                        successCall(sourceControl);
                    },
                    function (error) {
                        Xrm.Utility.alertDialog(error.message);
                        errorCall(error.message);
                    });
            }
            catch (ex) {
                console.log("Error at SYED.Solution.Ribbon.CreateMasterSolution function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        },

        CallAction: function (selectedId, mode, successCall, errorCall) {
            try {
                var parameters = {};
                parameters.SolutionId = selectedId;
                parameters.CheckIn = mode;

                var syed_CreateDynamicsSourceControlBySolutionRequest = {
                    SolutionId: parameters.SolutionId,
                    CheckIn: parameters.CheckIn,

                    getMetadata: function () {
                        return {
                            boundParameter: null,
                            parameterTypes: {
                                "SolutionId": {
                                    "typeName": "Edm.String",
                                    "structuralProperty": 1
                                },
                                "CheckIn": {
                                    "typeName": "Edm.String",
                                    "structuralProperty": 1
                                }
                            },
                            operationType: 0,
                            operationName: "syed_CreateDynamicsSourceControlBySolution"
                        };
                    }
                };

                Xrm.WebApi.online.execute(syed_CreateDynamicsSourceControlBySolutionRequest).then(
                    function success(result) {
                        if (result.ok) {
                            successCall(result);
                        }
                    },
                    function (error) {
                        Xrm.Utility.alertDialog(error.message);
                        errorCall(error);
                    }
                );
            }
            catch (ex) {
                console.log("Error at SYED.Solution.Ribbon.CallAction function: " + ex.message + "|" + "Stack: " + ex.stack);
                throw ex;
            }
        }
    }