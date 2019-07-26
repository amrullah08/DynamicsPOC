//-----------------------------------------------------------------------
// <copyright file="AzureFunction.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace MyFunction
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;   
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Main entry point of the program
    /// </summary>
    public static class AzureFunction
    {
        /// <summary>
        /// bin path
        /// </summary>
        private static string binPath;

        /// <summary>
        /// org url
        /// </summary>
        private static string orgUrl;

        /// <summary>
        /// client id
        /// </summary>
        private static string clientId;

        /// <summary>
        /// tenant id
        /// </summary>
        private static string tenandId;

        /// <summary>
        /// client secret
        /// </summary>
        private static string clientSecret;

        /// <summary>
        /// Gets remote/local bin path
        /// </summary>
        public static string BinPath
        {
            get
            {
                return binPath;
            }
        }

        /// <summary>
        /// Gets org url
        /// </summary>
        public static string OrgUrl
        {
            get
            {
                return orgUrl;
            }
        }

        /// <summary>
        /// Gets client id
        /// </summary>
        public static string ClientId
        {
            get
            {
                return clientId;
            }
        }

        /// <summary>
        /// Gets tenant id
        /// </summary>
        public static string TenandId
        {
            get
            {
                return tenandId;
            }
        }

        /// <summary>
        /// Gets client secret
        /// </summary>
        public static string ClientSecret
        {
            get
            {
                return clientSecret;
            }
        }

        /// <summary>
        /// Method runs when http request raised for azure functions
        /// </summary>
        /// <param name="httpReq">http request</param>
        /// <param name="traceLog">trace log</param>
        /// <param name="executionContext">execution context</param>
        /// <returns>returns http response message</returns>
        [FunctionName("Function1")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage httpReq, TraceWriter traceLog, ExecutionContext executionContext)
        {
            binPath = executionContext?.FunctionAppDirectory + @"\bin\";
            traceLog.Info("C# HTTP trigger function processed a request.");

            var queryStrings = httpReq.GetQueryNameValuePairs();
            orgUrl = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, "orgUrl", true) == 0).Value;
            clientId = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, "clientId", true) == 0).Value;
            tenandId = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, "tenandId", true) == 0).Value;
            clientSecret = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, "clientSecret", true) == 0).Value;

            string solutionUniqueName = null; // args[0];
            string committerName = "Syed Amrullah";
            string committerEmail = "syamrull@microsoft.com";
            string authorEmail = "TestSolutionCommitterService@microsoft.com";

            if (string.IsNullOrEmpty(orgUrl) && string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(tenandId) && string.IsNullOrEmpty(clientSecret))
            {
                return null;
            }
            else
            {
                ConfigurationSettings configurationSettings = Singleton.CrmConstantsInstance;
                EntityCollection configurationSettingsList = configurationSettings.GetConfigurationSettings(orgUrl, clientId, clientSecret, tenandId);
                configurationSettings.SetCrmProperties(configurationSettingsList);
                configurationSettings = Singleton.RepositoryConfigurationConstantsInstance;
                Singleton.RepositoryConfigurationConstantsInstance.ResetLocalDirectory();
                configurationSettings.SetRepositoryConfigurationProperties(configurationSettingsList);

                Singleton.RepositoryHelperInstance.TryUpdateToRepository(solutionUniqueName, committerName, committerEmail, authorEmail);

                return configurationSettingsList.Entities == null
                    ? httpReq.CreateResponse(HttpStatusCode.BadRequest, "Bad Request")
                    : httpReq.CreateResponse(HttpStatusCode.OK, "Request executed successfully");
            }
        }
    }
}
