using CrmSolutionLibrary.AzureDevopsAPIs.Schemas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CrmSolutionLibrary.AzureDevopsAPIs.RestClient
{
    public class PullRequestCreation
    {
        //public static async Task<string> PRCreation(string patToken,PullRequestObject payload )
        //{
        //    string responseContent = string.Empty;

        //    //use the httpclient
        //    using (var client = new HttpClient())
        //    {
        //        HTTPClientHelper clientHelper = new HTTPClientHelper();
        //        client.BaseAddress = new Uri(clientHelper.GetAzureDevopsPullRequestURL());
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", patToken);

               
        //        // Serialize our concrete class into a JSON String
        //        var stringPayload = JsonConvert.SerializeObject(payload);

        //        // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
        //        var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


        //        //connect to the REST endpoint            
        //        HttpResponseMessage httpResponse = client.PostAsync("", httpContent).Result;

        //        //check to see if we have a successful response
        //        if (httpResponse.IsSuccessStatusCode)
        //        {
        //            responseContent = await httpResponse.Content.ReadAsStringAsync();
        //        }
        //    }

        //    JObject json = responseContent != string.Empty ? JObject.Parse(responseContent) : null;
        //    return json?["url"].ToString();
        //}

        //public static PullRequestObject FillPRDetails(string sourceBranchName, string prTitle, string prDecription)
        //{
        //    // Prepare Request

        //    var payload = new PullRequestObject
        //    {
        //        sourceRefName = sourceBranchName,
        //        targetRefName = "refs/" + ConfigurationManager.AppSettings["MergeBranch"],
        //        title = prTitle,
        //        description = prDecription,
        //        reviewers = new Reviewer[] {}
        //    };

        //    return payload;
        //}
    }
}
