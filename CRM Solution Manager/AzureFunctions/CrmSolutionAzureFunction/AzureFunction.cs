using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using CrmSolutionLibrary;
//using System.Net;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Primitives;
//using Newtonsoft.Json;
using System.Collections.Generic;

namespace CrmSolutionAzureFunction
{
    public static class AzureFunction
    {
        [FunctionName("AzureFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Azure function processing a request.");
            IEnumerable<KeyValuePair<string, string>> querystruings = req.GetQueryNameValuePairs();
            List<string> allValues = (from qs in querystruings select qs.Value).Distinct().ToList();
            string[] args = allValues.ToArray();
            if (ProgramUtility.UpdateRepository(args))
            {
                return req.CreateResponse(HttpStatusCode.OK, "Success");
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Error");
            }
        }
    }
}
