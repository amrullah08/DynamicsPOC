//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Main entry point of the program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// main method
        /// </summary>
        /// <param name="args">args for the method</param>
        private static void Main(string[] args)
        {
            string mode = string.Empty;
            if (args == null || args.Length == 0)
            {
                mode = Constants.ArgumentScheduled;
            }
            else
            {
                mode = args[0];
            }

            string solutionUniqueName = null; // args[0];
            string committerName = "Syed Amrullah";
            string committerEmail = "syamrull@microsoft.com";
            string authorEmail = "TestSolutionCommitterService@microsoft.com";

            ConfigurationSettings configurationSettings = Singleton.CrmConstantsInstance;
            EntityCollection configurationSettingsList = configurationSettings.GetConfigurationSettings();
            configurationSettings.SetCrmProperties(configurationSettingsList);
            configurationSettings = Singleton.RepositoryConfigurationConstantsInstance;
            Singleton.RepositoryConfigurationConstantsInstance.ResetLocalDirectory();
            configurationSettings.SetRepositoryConfigurationProperties(configurationSettingsList);

            Singleton.RepositoryHelperInstance.TryUpdateToRepository(solutionUniqueName, committerName, committerEmail, authorEmail, mode);
        }
    }
}
