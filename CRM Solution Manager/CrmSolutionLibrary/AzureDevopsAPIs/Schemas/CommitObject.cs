using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmSolutionLibrary.AzureDevopsAPIs.Schemas
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~P:CrmSolutionLibrary.AzureDevopsAPIs.Schemas.Newcontent.content")]
    public class CommitObject
    {
        public Refupdate[] refUpdates { get; set; }
        public Commit[] commits { get; set; }
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~P:CrmSolutionLibrary.AzureDevopsAPIs.Schemas.Newcontent.content")]
    public class Refupdate
    {
        public string name { get; set; }
        public string oldObjectId { get; set; }
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~P:CrmSolutionLibrary.AzureDevopsAPIs.Schemas.Newcontent.content")]
    public class Commit
    {
        public string comment { get; set; }
        public Change[] changes { get; set; }
        public Auther auther { get; set; }
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~P:CrmSolutionLibrary.AzureDevopsAPIs.Schemas.Newcontent.content")]
    public class Change
    {
        public string changeType { get; set; }
        public Item item { get; set; }
        public Newcontent newContent { get; set; }
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~P:CrmSolutionLibrary.AzureDevopsAPIs.Schemas.Newcontent.content")]
    public class Item
    {
        public string path { get; set; }
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~P:CrmSolutionLibrary.AzureDevopsAPIs.Schemas.Newcontent.content")]
    public class Newcontent
    {
        public string content { get; set; }
        public string contentType { get; set; }
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~P:CrmSolutionLibrary.AzureDevopsAPIs.Schemas.Newcontent.content")]
    public class Auther
    {
        public string date{ get; set; }
        public string email{ get; set; }
        public string name { get; set; }
    }
}
