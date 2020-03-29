using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using CrmSolutionLibrary.AzureDevopsAPIs.Schemas;
using Newtonsoft.Json.Linq;

namespace CrmSolutionLibrary.AzureDevopsAPIs.RestClient
{
    public static class CreateCommit
    {
        /// <summary>
        /// Commit API Call
        /// </summary>
        /// <param name="commitInputDetails"></param>
        /// <returns></returns>
        public static async Task<string> Commit(string patToken, CommitObject commitInputDetails, Uri AzureDevopsBaseURL)
        {
            string responseContent = string.Empty;

            try
            {
                //use the httpclient
                using (var client = new HttpClient())
                {
                    if (AzureDevopsBaseURL != null)
                    {
                        HTTPClientHelper clientHelper = new HTTPClientHelper(AzureDevopsBaseURL.ToString());
                        client.BaseAddress = new Uri(clientHelper.GetAzureDevopsPushURL());
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", patToken);

                        // Serialize our concrete class into a JSON String
                        var stringPayload = JsonConvert.SerializeObject(commitInputDetails);

                        // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
                        var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


                        //connect to the REST endpoint            
                        HttpResponseMessage httpResponse = client.PostAsync("", httpContent).Result;
                        httpContent.Dispose();
                        //check to see if we have a successful response
                        //if (httpResponse.IsSuccessStatusCode)
                        {
                            responseContent = await httpResponse.Content.ReadAsStringAsync();
                        }
                    }
                }

                return responseContent;
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// FillCommitDetails
        /// </summary>
        /// <param name="changeRequest"></param>
        /// <returns></returns>
        public static CommitObject FillCommitDetails(ChangeRequest changeRequest)
        {
            try
            {
                var refUpdate = new Refupdate
                {
                    name = changeRequest.SourceBranchName,
                    oldObjectId = changeRequest.Lastcomitid
                };

                var commit = new Commit
                {
                    comment = changeRequest.Comments
                };
                Auther auther = new Auther
                {
                    date = changeRequest.CommitDate,
                    name = changeRequest.AutherName
                };
                commit.auther = auther;
                Change[] changes = new Change[changeRequest.RequestDetails.Count()];
                int i = 0;
                foreach (var item in changeRequest.RequestDetails)
                {
                    changes[i] = new Change
                    {
                        changeType = item.ChangeType,//"add",
                        item = new Item { path = @"/" + item.FileDestinationPath + @"/" + item.FileName },
                        newContent = new Newcontent { content = item.FileContent, contentType = item.ContentType }
                    };
                    i = i + 1;

                }
                commit.changes = changes;

                var payload = new CommitObject { refUpdates = new Refupdate[] { refUpdate }, commits = new Commit[] { commit } };

                return payload;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
