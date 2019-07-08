using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmSolution
{
    public class DependentSolutionCom
    {
        /// <summary>
        /// Gets or sets ID
        /// </summary>
        public List<EntityCollection> sourceSolutions { get; set; }
        public List<EntityCollection> targetSolutions { get; set; }

    }

}
