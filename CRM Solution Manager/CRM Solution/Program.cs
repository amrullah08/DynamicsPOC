//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    using Microsoft.Xrm.Sdk;
    using System;
    using System.Configuration;

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

            if (args != null && args.Length != 0)
            {
                foreach (string item in args)
                {
                    if (item.StartsWith(Constants.ArgumentDU, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["DynamicsUserName"] = item.Replace(Constants.ArgumentDU, "");
                    }
                    else if (item.StartsWith(Constants.ArgumentD365, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["OrgServiceUrl"] = item.Replace(Constants.ArgumentD365, "");
                    }
                    else if (item.StartsWith(Constants.ArgumentDP, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["DynamicsPassword"] = item.Replace(Constants.ArgumentDP, "");
                    }
                    else if (item.StartsWith(Constants.ArgumentGU, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["GitUserName"] = item.Replace(Constants.ArgumentGU, "");
                    }
                    else if (item.StartsWith(Constants.ArgumentGP, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["GitPassword"] = item.Replace(Constants.ArgumentGP, "");
                    }
                    else if (item.StartsWith(Constants.ArgumentTU, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["TFSUserName"] = item.Replace(Constants.ArgumentTU, "");
                    }
                    else if (item.StartsWith(Constants.ArgumentTP, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["TFSPassword"] = item.Replace(Constants.ArgumentTP, "");
                    }
                    else if (item.StartsWith(Constants.ArgumentAR, StringComparison.InvariantCulture))
                    {
                        mode = item.Replace(Constants.ArgumentAR, "");
                        Console.WriteLine(mode);
                    }
                }
            }
            else
            {
                Console.WriteLine("No Arguments");
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
