using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmSolutionLibrary.AzureDevopsAPIs.Schemas
{
    public class ChangeRequest
    {
        public List<RequestDetails> RequestDetails { get; set; }
        public string Comments { get; set; }

        public string SourceBranchName { get; set; }

        public string  Lastcomitid { get; set; }
        

    }
    public class RequestDetails {

        public string FileContent { get; set; }

        public string FileName { get; set; }

        public string FileDestinationPath { get; set; }

        public string ChangeType { get; set; }

        public string ContentType { get; set; }
    }
}
