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
    public class RepositoryConfigurationConstants : ConfigurationSettings
    {
        /// <summary>
        /// Method substitutes drive
        /// </summary>
        public const string SubstDrive = "k";

        /// <summary>
        /// Solution Folder
        /// </summary>
        private string solutionFolder;

        /// <summary>
        /// JavaScript Directory
        /// </summary>
        private string javaScriptDirectory;

        /// <summary>
        /// Html Directory
        /// </summary>
        private string htmlDirectory;

        /// <summary>
        /// Images Directory
        /// </summary>
        private string imagesDirectory;

        /// <summary>
        /// Repository Url
        /// </summary>
        private string repositoryUrl;

        /// <summary>
        /// Clone Repository Always
        /// </summary>
        private string cloneRepositoryAlways;

        /// <summary>
        /// Repository Remote Name
        /// </summary>
        private string repositoryRemoteName;

        /// <summary>
        /// Branch Name
        /// </summary>
        private string branchName;

        /// <summary>
        /// Gets or sets Repository local directory
        /// </summary>
        public string LocalDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets repository url
        /// </summary>
        public string GitUserName
        {
            get
            {
                return ConfigurationManager.AppSettings["GitUserName"];
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository url
        /// </summary>
        public string GitUserPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["GitPassword"];
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository release directory containing CRM Solutions
        /// </summary>
        public string SolutionFolder
        {
            get
            {
                return this.solutionFolder;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository script directory
        /// </summary>
        public string JsDirectory
        {
            get
            {
                return this.javaScriptDirectory;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository html directory
        /// </summary>
        public string HtmlDirectory
        {
            get
            {
                return this.htmlDirectory;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository Images directory
        /// </summary>
        public string ImagesDirectory
        {
            get
            {
                return this.imagesDirectory;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository url
        /// </summary>
        public string RepositoryUrl
        {
            get
            {
                return this.repositoryUrl;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository url
        /// </summary>
        public string CloneRepositoryAlways
        {
            get
            {
                return this.cloneRepositoryAlways;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository remote name
        /// </summary>
        public string RepositoryRemoteName
        {
            get
            {
                return this.repositoryRemoteName;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets Repository branch name
        /// </summary>
        public string BranchName
        {
            get
            {
                return this.branchName;
            }

            set
            {
            }
        }

        /// <summary>
        /// method resets local directory
        /// </summary>
        public void ResetLocalDirectory()
        {
            // LocalDirectory = @"\\?\" + Path.GetTempFileName().Replace(".","") + "devopsTmp\\";
            this.LocalDirectory = Path.GetTempFileName().Replace(".", string.Empty) + "devopsTmp\\";
            Singleton.CrmSolutionHelperInstance.CreateEmptyFolder(this.LocalDirectory);
            this.SubstTempDirectory();
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
                        this.solutionFolder = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositoryJsDirectory:
                        this.javaScriptDirectory = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositoryHtmlDirectory:
                        this.htmlDirectory = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositoryImagesDirectory:
                        this.imagesDirectory = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RepositoryUrl:
                        this.repositoryUrl = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.CloneRepositoryAlways:
                        this.cloneRepositoryAlways = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.RemoteName:
                        this.repositoryRemoteName = setting.GetAttributeValue<string>("syed_value");
                        break;
                    case Constants.BranchName:
                        this.branchName = setting.GetAttributeValue<string>("syed_value");
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// method substitutes temporary directory
        /// </summary>
        private void SubstTempDirectory()
        {
            try
            {
                if (Directory.Exists(SubstDrive + ":\\"))
                {
                    this.DeleteSubstTempDirectory();
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "subst",
                    Arguments = SubstDrive + ": '" + this.LocalDirectory + "'"
                };
                Process.Start(startInfo);

                Process process = new Process();
                process.StartInfo.FileName = "subst.exe";
                process.StartInfo.Arguments = " " + SubstDrive + ": \"" + this.LocalDirectory.Remove(this.LocalDirectory.Length - 1, 1) + "\"";
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
                this.LocalDirectory = SubstDrive + ":\\1\\";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// method deletes substituted temporary directory
        /// </summary>
        private void DeleteSubstTempDirectory()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "subst",
                    Arguments = SubstDrive + ": '" + this.LocalDirectory + "'"
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
