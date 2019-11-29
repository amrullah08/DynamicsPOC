using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmSolutionLibrary.AzureDevopsAPIs.Schemas
{
    
    public class PullRequestObject
    {
        public string sourceRefName { get; set; }
        public string targetRefName { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Reviewer[] reviewers { get; set; }
    }

    public class Reviewer
    {
        public string id { get; set; }
        public string directoryAlias { get; set; }
    }
}
