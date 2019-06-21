using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CrmSolution;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                log.Info("C# HTTP trigger function processed a request.");

                string solutionUniqueName = null; // args[0];
                string committerName = "Syed Amrullah";
                string committerEmail = "syamrull@microsoft.com";
                string authorEmail = "TestSolutionCommitterService@microsoft.com";

                RepositoryHelper.TryUpdateToRepository(solutionUniqueName, committerName, committerEmail, authorEmail);
                return req.CreateResponse(HttpStatusCode.OK);
            }

            catch (System.Exception e)
            {
                log.Info(e.Message);
                log.Info(e.StackTrace);
                string abc = e.Message;
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}
