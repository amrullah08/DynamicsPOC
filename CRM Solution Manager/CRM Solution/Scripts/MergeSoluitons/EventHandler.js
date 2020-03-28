if (typeof SYED === 'undefined') {
    SYED = { __namespace: true };
  }
  if (typeof SYED.MergeSolution === 'undefined') {
    SYED.MergeSolution = { __namespace: true };
  }
  
  ('use strict');
  SYED.MergeSolution.EventHandler = {
    SetCRMSolutionValues: function(executionContext) {
      try {
        const formContext = executionContext.getFormContext();
        let sourceControlValues = formContext.getAttribute('syed_crmsolutions').getValue();
        if (sourceControlValues != null || sourceControlValues != undefined) {
          sourceControlValues = sourceControlValues[0].id
            .replace('{', '')
            .replace('}', '')
            .toUpperCase();
  
          Xrm.WebApi.online
            .retrieveRecord(
              'syed_mastersolutions',
              sourceControlValues,
              '?$select=syed_friendlyname,syed_installedon,syed_ismanaged,syed_listofsolutions,syed_mastersolutionsid,syed_name,syed_order,syed_publisher,syed_solutionid,syed_solutioninstalledon,syed_version,syed_versionnumber',
            )
            .then(
              function success(result) {
                formContext.getAttribute('syed_syed_friendlyname').setValue(result.syed_friendlyname);
                formContext.getAttribute('syed_ismanaged').setValue(result.syed_ismanaged);
                formContext.getAttribute('syed_name').setValue(result.syed_name);
                formContext.getAttribute('syed_publisher').setValue(result.syed_publisher);
                formContext.getAttribute('syed_solutioninstalledon').setValue(new Date(result.syed_solutioninstalledon));
                formContext.getAttribute('syed_uniquename').setValue(result.syed_listofsolutions);
                formContext.getAttribute('syed_version').setValue(result.syed_version);
  
                formContext.getAttribute('syed_order').setValue(0);
              },
              function(error) {
                Xrm.Utility.alertDialog(error.message);
              },
            );
        } else {
          if (!Xrm.Internal.isUci()) {
            formContext.getAttribute('syed_syed_friendlyname').setValue('');
            formContext.getAttribute('syed_ismanaged').setValue(false);
            formContext.getAttribute('syed_name').setValue('');
            formContext.getAttribute('syed_publisher').setValue('');
            formContext.getAttribute('syed_solutioninstalledon').setValue('');
            formContext.getAttribute('syed_uniquename').setValue('');
            formContext.getAttribute('syed_version').setValue('');
            formContext.getAttribute('syed_order').setValue(0);
          }
        }
      } catch (ex) {
        console.log(
          'Error at SYED.MergeSolution.EventHandler.SetCRMSolutionValues function: ' +
            ex.message +
            '|' +
            'Stack: ' +
            ex.stack,
        );
        throw ex;
      }
    },
  
    Back: function(executionContext) {
      try {
        if (Xrm.Internal.isUci()) formContext = executionContext;
        else formContext = executionContext.getFormContext();
  
        const sourceControl = formContext.getAttribute('syed_mastersolution').getValue();
        if (sourceControl != null || sourceControl != undefined) {
          const entityFormOptions = {};
          entityFormOptions.entityName = sourceControl[0].entityType;
          entityFormOptions.entityId = sourceControl[0].id;
  
          Xrm.Navigation.openForm(entityFormOptions);
        }
      } catch (ex) {
        console.log('Error at SYED.MergeSolution.EventHandler.Back function: ' + ex.message + '|' + 'Stack: ' + ex.stack);
        throw ex;
      }
    },
  };
  