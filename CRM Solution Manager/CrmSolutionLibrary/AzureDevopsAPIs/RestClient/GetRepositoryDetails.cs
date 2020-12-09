using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CrmSolutionLibrary.AzureDevopsAPIs.RestClient
{
    public static class GetRepositoryDetails
    {
        public static async Task<string> GetLastCommitDetails(string patToken, string sourceBranchName,string AzureDevopsBaseURL)
        {
            string responseContent = string.Empty;

            //use the httpclient
            using (var client = new HttpClient())
            {
                HTTPClientHelper clientHelper = new HTTPClientHelper(AzureDevopsBaseURL);
                string baseURL = clientHelper.GetAzureDevopsLastCommitURL(sourceBranchName.Replace("refs/", ""));
                client.BaseAddress = new Uri(baseURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", patToken);

                //connect to the REST endpoint            
                HttpResponseMessage httpResponse = client.GetAsync("").Result;

                //check to see if we have a successful response
                if (httpResponse.IsSuccessStatusCode)
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync();
                }
               
            }

            JObject json = responseContent != string.Empty ? JObject.Parse(responseContent) : null;
            return  json?["value"][0]["objectId"].ToString();
        }

        public static async Task<List<string>> GetBranches(string patToken, string AzureDevopsBaseURL)
        {
            string responseContent = string.Empty;
            //use the httpclient
            using (var client = new HttpClient())
            {
                HTTPClientHelper clientHelper = new HTTPClientHelper(AzureDevopsBaseURL);
                string baseURL = clientHelper.GetAzureDevopsRefsURL();
                client.BaseAddress = new Uri(baseURL);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", patToken);

                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                //    Convert.ToBase64String(
                //        System.Text.ASCIIEncoding.ASCII.GetBytes(
                //            string.Format("{0}:{1}", "", patToken))));

                //connect to the REST endpoint            
                HttpResponseMessage httpResponse = client.GetAsync("").Result;

                //check to see if we have a successful response
                if (httpResponse.IsSuccessStatusCode)
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync();
                }
                else
                {
                    const string message = "Probable Github pat token error";
                    throw new UnauthorizedAccessException(message);
                }
            }

            JObject json = responseContent != string.Empty ? JObject.Parse(responseContent) : null;
            List<string> refNames= json["value"].Select(x => x["name"].ToString()).ToList();
            return refNames;
        }


        public static async Task<string> GetItemDetails(string patToken, string ItemRootPath,string ItemFullPath,string AzureDevopsBaseURL, string branchname)
        {
            string responseContent = string.Empty;

            //use the httpclient
            using (var client = new HttpClient())
            {
                HTTPClientHelper clientHelper = new HTTPClientHelper(AzureDevopsBaseURL);
                string baseURL = clientHelper.GetAzureDevopsItemURL(ItemFullPath.Replace("refs/", ""), branchname.Replace("refs/heads/", ""));
                client.BaseAddress = new Uri(baseURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", patToken);

                //connect to the REST endpoint            
                HttpResponseMessage httpResponse = client.GetAsync("").Result;

                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync();
                }

            }

            JObject json = responseContent != string.Empty ? JObject.Parse(responseContent) : null;

            //string FilePath = @"/Test/Release/TemplateSolution";
            return Convert.ToInt32(json["count"]) > 0 ? "edit" : "add";
        }
    }
}
