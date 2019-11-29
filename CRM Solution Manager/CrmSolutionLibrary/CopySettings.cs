//-----------------------------------------------------------------------
// <copyright file="CopySettings.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace MsCrmTools.SolutionComponentsMover.AppCode
{
    using System.Collections.Generic;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// copy settings for components
    /// </summary>
    internal class CopySettings
    {
        /// <summary>
        /// Gets or sets component types
        /// </summary>
        public List<int> ComponentsTypes { get; set; }

        /// <summary>
        /// Gets or sets source solutions or solutions to be merged
        /// </summary>
        public List<Entity> SourceSolutions { get; set; }

        /// <summary>
        /// Gets or sets Solutions to which components needs to be copied
        /// </summary>
        public List<Entity> TargetSolutions { get; set; }
    }
}