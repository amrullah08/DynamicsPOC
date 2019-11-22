using System;
using System.Configuration;

namespace CrmSolutionLibrary
{
    public class HTTPClientHelper
    {
        private string httpURL = "";
        public HTTPClientHelper(string AzureDevopsBaseURL)
        {
            this.httpURL = AzureDevopsBaseURL;
            //this.httpURL = ConfigurationManager.AppSettings["AzureDevopsBaseURL"];
            //this.httpURL = this.httpURL.Replace("{organization}", ConfigurationManager.AppSettings["AzureDevopsOrganization"]);
            //this.httpURL = this.httpURL.Replace("{project}", ConfigurationManager.AppSettings["AzureDevopsProject"]);
            //this.httpURL = this.httpURL.Replace("{repositoryName}", ConfigurationManager.AppSettings["AzureDevopsRepositoryName"]);
        }
        public string GetAzureDevopsPushURL()
        {
            this.httpURL = this.httpURL.Replace("{action}", "pushes");

            return this.httpURL;
        }
        public string GetAzureDevopsPullRequestURL()
        {
            this.httpURL = this.httpURL.Replace("{action}", "pullrequests");

            return this.httpURL;
        }
        public string GetAzureDevopsRefsURL()
        {
            this.httpURL = this.httpURL.Replace("{action}", "refs");

            return this.httpURL;
        }
        public string GetAzureDevopsLastCommitURL(string filterSourceBranchName)
        {
            this.httpURL = this.httpURL.Replace("{action}", "refs");
            this.httpURL = this.httpURL.Replace("api-version=5.0", "filter=" + filterSourceBranchName + "&api-version=5.0");

            return this.httpURL;
        }
        public string GetAzureDevopsItemURL(string filterItemName)
        {
            this.httpURL = this.httpURL.Replace("{action}", "items");
            
            //this.httpURL = this.httpURL.Replace("api-version=5.0", "scopePath=" + filterItemName + "&api-version=5.0");
            this.httpURL = this.httpURL.Replace("api-version=5.0", "scopePath=" + filterItemName+ "&recursionLevel=Full&includeContentMetadata=true&api-version=5.0");

            return this.httpURL;
        }

    }
}
