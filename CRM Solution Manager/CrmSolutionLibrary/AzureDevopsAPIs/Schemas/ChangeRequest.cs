using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmSolutionLibrary.AzureDevopsAPIs.Schemas
{
    public class ChangeRequest
    {
        /// <summary>
        /// RequestDetails
        /// </summary>
        public List<RequestDetails> RequestDetails { get; set; }
        /// <summary>
        /// Comments
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// SourceBranchName
        /// </summary>
        public string SourceBranchName { get; set; }
        /// <summary>
        /// Lastcomitid
        /// </summary>
        public string Lastcomitid { get; set; }
        /// <summary>
        /// CommitDate
        /// </summary>
        public string CommitDate{ get; set; }
        /// <summary>
        /// AutherEmail
        /// </summary>
        public string AutherEmail { get; set; }
        /// <summary>
        /// AutherName
        /// </summary>
        public string AutherName { get; set; }



    }
    public class RequestDetails {
       
        public RequestDetails(string FileContent, string FileName, string FileDestinationPath, string ChangeType, string ContentType)
        {
            this.FileContent = FileContent;
            this.FileName = FileName;
            this.FileDestinationPath = FileDestinationPath;
            this.ChangeType = ChangeType;
            this.ContentType = ContentType;
        }
        /// <summary>
        /// FileContent
        /// </summary>
        public string FileContent { get; set; }
        /// <summary>
        /// FileName
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// FileDestinationPath
        /// </summary>
        public string FileDestinationPath { get; set; }
        /// <summary>
        /// ChangeType
        /// </summary>
        public string ChangeType { get; set; }
        /// <summary>
        /// ContentType
        /// </summary>
        public string ContentType { get; set; }
    }
}
