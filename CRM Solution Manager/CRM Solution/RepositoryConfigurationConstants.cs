//-----------------------------------------------------------------------
// <copyright file="RepositoryConfigurationConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// constants file for repository configurations
    /// </summary>
    internal class RepositoryConfigurationConstants : ConfigurationSettings
    {
        private static string solutionFolder;
        private static string jsDirectory;
        private static string htmlDirectory;
        private static string imagesDirectory;
        private static string repositoryUrl;
        private static string cloneRepositoryAlways;
        private static string repositoryRemoteName;
        private static string branchName;
        /// <summary>
        /// Method substitutes drive
        /// </summary>
        public const string SubstDrive = "k";

        /// <summary>
        /// Gets or sets Repository local directory
        /// </summary>
        public static string LocalDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets repository url
        /// </summary>
        public static string GitUserName
        {
            get
            {
                return ConfigurationManager.AppSettings["GitUserName"];
            }
            set { }
        }

        /// <summary>
        /// Gets repository url
        /// </summary>
        public static string GitUserPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["GitPassword"];
            }
            set { }
        }

        /// <summary>
        /// Gets repository release directory containing CRM Solutions
        /// </summary>
        public static string SolutionFolder
        {
            get
            {
                return solutionFolder;
            }
            set { }
        }

        /// <summary>
        /// Gets repository script directory
        /// </summary>
        public static string JsDirectory
        {
            get
            {
                return jsDirectory;
            }
            set { }
        }

        /// <summary>
        /// Gets repository html directory
        /// </summary>
        public static string HtmlDirectory
        {
            get
            {
                return htmlDirectory;
            }
            set { }
        }

        /// <summary>
        /// Gets repository Images directory
        /// </summary>
        public static string ImagesDirectory
        {
            get
            {
                return imagesDirectory;
            }
            set { }
        }

        /// <summary>
        /// Gets repository url
        /// </summary>
        public static string RepositoryUrl
        {
            get
            {
                return repositoryUrl;
            }
            set { }
        }

        /// <summary>
        /// Gets repository url
        /// </summary>
        public static string CloneRepositoryAlways
        {
            get
            {
                return cloneRepositoryAlways;
            }
            set { }
        }

        /// <summary>
        /// Gets repository remote name
        /// </summary>
        public static string RepositoryRemoteName
        {
            get
            {
                return repositoryRemoteName;
            }
            set { }
        }

        /// <summary>
        /// Gets Repository branch name
        /// </summary>
        public static string BranchName
        {
            get
            {
                return branchName;
            }
            set { }
        }

        /// <summary>
        /// Method sets repository configuration constant property values
        /// </summary>
        /// <param name="retrievedConfigurationSettingsList">entity collection</param>
        public override void SetRepositoryConfigurationProperties(EntityCollection retrievedConfigurationSettingsList)
        {
            foreach (Entity setting in retrievedConfigurationSettingsList.Entities)
            {
                string key = setting.GetAttributeValue<string>("syed_name");

                switch (key)
                {
                    case Constants.RepositorySolutionFolder:
                        solutionFolder = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositoryJsDirectory:
                        jsDirectory = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositoryHtmlDirectory:
                        htmlDirectory = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositoryImagesDirectory:
                        imagesDirectory = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositoryUrl:
                        repositoryUrl = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.CloneRepositoryAlways:
                        cloneRepositoryAlways = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RemoteName:
                        repositoryRemoteName = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.BranchName:
                        branchName = setting.GetAttributeValue<string>("syed_value");
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// method resets local directory
        /// </summary>
        public static void ResetLocalDirectory()
        {
            // LocalDirectory = @"\\?\" + Path.GetTempFileName().Replace(".","") + "devopsTmp\\";
            LocalDirectory = Path.GetTempFileName().Replace(".", string.Empty) + "devopsTmp\\";
            CrmSolutionHelper.CreateEmptyFolder(LocalDirectory);
            SubstTempDirectory();
        }

        /// <summary>
        /// method substitutes temporary directory
        /// </summary>
        private static void SubstTempDirectory()
        {
            try
            {
                if (Directory.Exists(SubstDrive + ":\\"))
                {
                    DeleteSubstTempDirectory();
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "subst",
                    Arguments = SubstDrive + ": '" + LocalDirectory + "'"
                };
                Process.Start(startInfo);

                Process process = new Process();
                process.StartInfo.FileName = "subst.exe";
                process.StartInfo.Arguments = " " + SubstDrive + ": \"" + LocalDirectory.Remove(LocalDirectory.Length - 1, 1) + "\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
                string err = process.StandardError.ReadToEnd();
                Console.WriteLine(err);
                process.WaitForExit();

                Directory.CreateDirectory(SubstDrive + ":\\1\\");
                LocalDirectory = SubstDrive + ":\\1\\";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// method deletes substituted temporary directory
        /// </summary>
        private static void DeleteSubstTempDirectory()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "subst",
                    Arguments = SubstDrive + ": '" + LocalDirectory + "'"
                };
                Process.Start(startInfo);

                Process process = new Process();
                process.StartInfo.FileName = "subst.exe";
                process.StartInfo.Arguments = " " + SubstDrive + ": /d";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
                string err = process.StandardError.ReadToEnd();
                Console.WriteLine(err);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
