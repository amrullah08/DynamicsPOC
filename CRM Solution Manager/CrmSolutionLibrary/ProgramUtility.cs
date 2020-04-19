using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmSolutionLibrary
{
    using System;
    using System.Configuration;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Azure.KeyVault;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Xrm.Sdk;
    public static class ProgramUtility
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
        public static bool Execute(string[] args)
        {

            string mode = string.Empty;
            try
            {
                mode = GetModeAndConfigureSettings(args, mode);

                string solutionUniqueName = null; // args[0];
                //string committerName = "Syed Amrullah";
                //string committerEmail = "syamrull@microsoft.com";
                //string authorEmail = "TestSolutionCommitterService@microsoft.com";
                ConfigurationSettings configurationSettings = Singleton.CrmConstantsInstance;
                EntityCollection configurationSettingsList = configurationSettings.GetConfigurationSettings();
                configurationSettings.SetCrmProperties(configurationSettingsList);
                configurationSettings = Singleton.RepositoryConfigurationConstantsInstance;
                Singleton.RepositoryConfigurationConstantsInstance.ResetLocalDirectory();
                configurationSettings.SetRepositoryConfigurationProperties(configurationSettingsList);
                //Singleton.RepositoryHelperInstance.TryUpdateToRepository(solutionUniqueName, committerName, committerEmail, authorEmail, mode);
                Singleton.RepositoryHelperInstance.InitiateRequest(solutionUniqueName, mode);
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }

        private static string GetModeAndConfigureSettings(string[] args, string mode)
        {
            if (args != null && args.Length != 0)
            {
                KeyVaultClient kvc = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken));

                foreach (string item in args)
                {
                    if (item == "WEB" || item == "Scheduled")
                    {
                        mode = item;
                    }
                    else
                    {
                        ConfigurationManager.AppSettings[item] = DoVault(item, kvc);
                    }

                }
            }
            else
            {
                Console.WriteLine("No Arguments");
            }

            // ConfigurationManager.AppSettings["CRMSourceServiceUrl"] = "https://compasstest.api.crm.dynamics.com/XRMServices/2011/Organization.svc";
            // ConfigurationManager.AppSettings["CRMSourceInstanceUrl"] = "https://compasstest.crm.dynamics.com";
            return mode;
        }
    }
}
