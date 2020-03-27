1. in local.testing.json update tenantiid , appid and secret
2. to test locally press f5, in the progrmautility.cs configurekeyvault put last twolines as below
            ConfigurationManager.AppSettings["CRMSourceServiceUrl"] = "https://compasstest.api.crm.dynamics.com/XRMServices/2011/Organization.svc";
            ConfigurationManager.AppSettings["CRMSourceInstanceUrl"] = "https://compasstest.crm.dynamics.com";
3. in the console window it would show the url, copy that url and paste in browser this would trigger the console with follwoing parameters
http://localhost:7071/api/AzureFunction?D365=CRMSourceServiceUrl&D365INST=CRMSourceInstanceUrl&GU=GitUserName&GP=GitPassword&TU=TFSUser&TP=TFSPassword&AR=WEB
4. ensure all the values are there in the configured keyvault