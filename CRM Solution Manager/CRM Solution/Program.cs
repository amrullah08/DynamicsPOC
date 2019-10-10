//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    using System;
    using System.Configuration;
    using System.Threading.Tasks;
    using Microsoft.Azure.KeyVault;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Main entry point of the program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Get Azure Secret 
        /// </summary>
        /// <param name="name">Azure Name</param>
        /// <param name="kvc">Key Vault Client</param>
        /// <returns> returns secret</returns>
        public static string DoVault(string name, KeyVaultClient kvc)
        {
            string secret = Task.Run(() => kvc.GetSecretAsync(ConfigurationManager.AppSettings["BASESECRETURI"], name)).ConfigureAwait(false).GetAwaiter().GetResult().Value;
            return secret;
        }

        /// <summary>
        /// Get Azure Token 
        /// </summary>
        /// <param name="authority">Azure Authority</param>
        /// <param name="resource">Azure Resource</param>
        /// <param name="scope">Azure scope</param>
        /// <returns> returns access token</returns>
        public static async Task<string> GetToken(string authority, string resource, string scope)
        {
            AuthenticationResult result = null;
            try
            {
                string clientsecret = ConfigurationManager.AppSettings["ClientApplicationSecret"];
                string clientid = ConfigurationManager.AppSettings["SolutionCheckerAppClientId"];

                var authContext = new AuthenticationContext(authority);
                ClientCredential clientCred = new ClientCredential(clientid, clientsecret);
                result = await authContext.AcquireTokenAsync(resource, clientCred).ConfigureAwait(true);

                if (result == null)
                {
                    throw new InvalidOperationException("Failed to obtain the JWT token");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("tracing" + ex.Message);
            }

            return result.AccessToken;
        }

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
                        ConfigurationManager.AppSettings["CRMSourceUserName"] = DoVault(item.Replace(Constants.ArgumentDU, string.Empty), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentD365, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["CRMSourceInstanceUrl"] = DoVault(item.Replace(Constants.ArgumentD365, string.Empty), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentDP, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["CRMSourcePassword"] = DoVault(item.Replace(Constants.ArgumentDP, string.Empty), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentGU, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["GitUserName"] = DoVault(item.Replace(Constants.ArgumentGU, string.Empty), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentGP, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["GitPassword"] = DoVault(item.Replace(Constants.ArgumentGP, string.Empty), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentTU, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["TFSUser"] = DoVault(item.Replace(Constants.ArgumentTU, string.Empty), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentTP, StringComparison.InvariantCulture))
                    {
                        ConfigurationManager.AppSettings["TFSPassword"] = DoVault(item.Replace(Constants.ArgumentTP, string.Empty), kvc);
                    }
                    else if (item.StartsWith(Constants.ArgumentAR, StringComparison.InvariantCulture))
                    {
                        mode = item.Replace(Constants.ArgumentAR, string.Empty);
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
