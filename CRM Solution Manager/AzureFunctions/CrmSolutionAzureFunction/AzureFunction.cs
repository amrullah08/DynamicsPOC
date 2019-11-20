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
            log.Info("C# HTTP trigger function processed a request.");
              

            IEnumerable<KeyValuePair<string, string>> querystruings = req.GetQueryNameValuePairs();
            List<string> allValues = (from qs in querystruings select qs.Value).Distinct().ToList();
            //List<string> allKeys = (from qs in querystruings select qs.Key).Distinct().ToList();

            string[] args = allValues.ToArray();

            if (ProgramUtility.UpdateRepository(args))
            {
                
            return req.CreateResponse(HttpStatusCode.OK,"Success");
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Error");
            }

            //// parse query parameter
            //string name = req.GetQueryNameValuePairs()
            //    .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
            //    .Value;

            //if (name == null)
            //{
            //    // Get request body
            //    dynamic data = await req.Content.ReadAsAsync<object>();
            //    name = data?.name;
            //}

            //return name == null
            //    ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
            //    : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
        }
    }
}
