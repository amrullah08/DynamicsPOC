using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using CrmSolutionLibrary.AzureDevopsAPIs.Schemas;
using Newtonsoft.Json.Linq;

namespace CrmSolutionLibrary.AzureDevopsAPIs.RestClient
{
    public class CreateCommit
    {
        /// <summary>
        /// Commit API Call
        /// </summary>
        /// <param name="commitInputDetails"></param>
        /// <returns></returns>
        public static async Task<string> Commit(string patToken, CommitObject commitInputDetails, string AzureDevopsBaseURL)
        {
            string responseContent = string.Empty;

            try
            {
                //use the httpclient
                using (var client = new HttpClient())
                {
                    HTTPClientHelper clientHelper = new HTTPClientHelper(AzureDevopsBaseURL);
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

                    //check to see if we have a successful response
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        responseContent = await httpResponse.Content.ReadAsStringAsync();
                    }
                }

                JObject json = responseContent != string.Empty ? JObject.Parse(responseContent) : null;
                return json?["commits"][0]["url"].ToString();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        /// <summary>
        /// Commit API Call
        /// </summary>
        /// <param name="fileToBeUploaded"></param>
        /// <param name="filename"> This should be a filename with extension LikeCRMDemoSolution.Zip</param>
        /// <param name="sourceBranchName"></param>
        /// <param name="oldObjectId"></param>
        /// <param name="comments"></param>
        /// <param name="destinationLocation"></param>
        /// <returns></returns>
        //public static CommitObject FillCommitDetails(string fileToBeUploaded,string filename,string sourceBranchName,string oldObjectId,string comments,string destinationLocation)
        //{           
        //    var refUpdate = new Refupdate();
        //    refUpdate.name = sourceBranchName; // "refs/heads/users/shamkh/uwpfolderstructure";
        //    refUpdate.oldObjectId = oldObjectId; // "0fc2e8e38eb732c2e8063ef416a5573cb308fcb3";

        //    var commit = new Commit();
        //    commit.comment = comments;
        //    commit.changes = new Change[] { new Change {changeType = "add", item = new Item {  path =  @"/" + destinationLocation + @"/" + filename} ,
        //        newContent = new Newcontent { content = fileToBeUploaded , contentType = "base64Encoded"} } };

        //    var payload = new CommitObject { refUpdates = new Refupdate[] { refUpdate }, commits = new Commit[] { commit } };

        //    return payload;
        //}

        /// <summary>
        /// FillCommitDetails
        /// </summary>
        /// <param name="changeRequest"></param>
        /// <returns></returns>
        public static CommitObject FillCommitDetails(ChangeRequest changeRequest)
        {
            try
            {
                var refUpdate = new Refupdate();
                refUpdate.name = changeRequest.SourceBranchName; 
                refUpdate.oldObjectId = changeRequest.Lastcomitid;

                var commit = new Commit();
                commit.comment = changeRequest.Comments;
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
