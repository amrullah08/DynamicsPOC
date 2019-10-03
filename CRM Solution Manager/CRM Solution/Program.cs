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
    using Microsoft.Azure.KeyVault;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System.Threading.Tasks;

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
                KeyVaultClient kvc = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken));

                foreach (string item in args)
                {
                    if (item.StartsWith(Constants.ArgumentDU, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["DynamicsUserName"] = DoVault(item.Replace(Constants.ArgumentDU, ""), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentD365, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["OrgServiceUrl"] = DoVault(item.Replace(Constants.ArgumentD365, ""), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentDP, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["DynamicsPassword"] = DoVault(item.Replace(Constants.ArgumentDP, ""), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentGU, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["GitUserName"] = DoVault(item.Replace(Constants.ArgumentGU, ""), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentGP, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["GitPassword"] = DoVault(item.Replace(Constants.ArgumentGP, ""), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentTU, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["TFSUserName"] = DoVault(item.Replace(Constants.ArgumentTU, ""), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentTP, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["TFSPassword"] = DoVault(item.Replace(Constants.ArgumentTP, ""), kvc);
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
        public static string DoVault(string Name, KeyVaultClient kvc)
        {
            string secret = Task.Run(() => kvc.GetSecretAsync(ConfigurationManager.AppSettings["BASESECRETURI"], Name)).ConfigureAwait(false).GetAwaiter().GetResult().Value;
            return secret;
        }

        public static async Task<string> GetToken(string authority, string resource, string scope)
        {
            AuthenticationResult result = null;
            try
            {
                string CLIENTSECRET = ConfigurationManager.AppSettings["CLIENTSECRET"];
                string CLIENTID = ConfigurationManager.AppSettings["CLIENTIDKEY"];

                var authContext = new AuthenticationContext(authority);
                ClientCredential clientCred = new ClientCredential(CLIENTID, CLIENTSECRET);
                result = await authContext.AcquireTokenAsync(resource, clientCred);

                if (result == null)
                    throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            catch (Exception ex)
            {

                Console.WriteLine("tracing" + ex.Message);
            }

            return result.AccessToken;
        }
    }
}
