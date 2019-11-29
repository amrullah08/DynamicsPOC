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
    public class GetRepositoryDetails
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
            List<string> refNames= json["value"].Select(x => x["name"].ToString()).ToList();
            return refNames;
        }


        public static async Task<string> GetItemDetails(string patToken, string ItemRootPath,string ItemFullPath,string AzureDevopsBaseURL)
        {
            string responseContent = string.Empty;

            //use the httpclient
            using (var client = new HttpClient())
            {
                HTTPClientHelper clientHelper = new HTTPClientHelper(AzureDevopsBaseURL);
                string baseURL = clientHelper.GetAzureDevopsItemURL(ItemRootPath.Replace("refs/", ""));
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

            //string FilePath = @"/Test/Release/TemplateSolution";
            string result = "add";
            for (int i = 0; i < json["value"].Count(); i++)
            {
                if (json["value"][i]["path"].ToString().ToLower() == ItemFullPath.ToLower())
                {
                    //Console.WriteLine(json["value"][i]["path"].ToString().ToLower());
                    //result = json["value"][i]["path"].ToString();
                    result = "edit";
                }
             }

            return result;

        }
    }
}
