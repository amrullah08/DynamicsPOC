//-----------------------------------------------------------------------
// <copyright file="RepositoryConfigurationConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolutionLibrary
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
        /// solution text file 
        /// </summary>
        private string solutionText;

        /// <summary>
        /// Time Trigger text file 
        /// </summary>
        private string timeTriggerPath;

        /// <summary>
        /// solution text file 
        /// </summary>
        private string solutionCheckerPath;

        /// <summary>
        /// solution text file 
        /// </summary>
        private string solutionTextRelease;

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
        /// Gets or sets repository url
        /// </summary>
        public string TFSUserName
        {
            get
            {
                return ConfigurationManager.AppSettings["TFSUser"];
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository url
        /// </summary>
        public string TFSPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["TFSPassword"];
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
        /// Gets or sets repository solution text file
        /// </summary>
        public string SolutionText
        {
            get
            {
                return this.solutionText;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets solution checker path file
        /// </summary>
        public string SolutionCheckerPath
        {
            get
            {
                return this.solutionCheckerPath;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets solution checker path file
        /// </summary>
        public string TimeTriggerPath
        {
            get
            {
                return this.timeTriggerPath;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets repository solution text file for Release
        /// </summary>
        public string SolutionTextRelease
        {
            get
            {
                return this.solutionTextRelease;
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
                        if (!string.IsNullOrEmpty(setting.GetAttributeValue<string>("syed_value")))
                        {
                            this.solutionFolder = Path.Combine(this.LocalDirectory, setting.GetAttributeValue<string>("syed_value"));
                        }

                        break;
                    case Constants.SolutionTextPath:
                        if (!string.IsNullOrEmpty(setting.GetAttributeValue<string>("syed_value")))
                        {
                            this.solutionText = Path.Combine(this.LocalDirectory, setting.GetAttributeValue<string>("syed_value"));
                        }

                        break;

                    case Constants.SolutionCheckerPath:
                        if (!string.IsNullOrEmpty(setting.GetAttributeValue<string>("syed_value")))
                        {
                            this.solutionCheckerPath = Path.Combine(this.LocalDirectory, setting.GetAttributeValue<string>("syed_value"));
                        }

                        break;

                    case Constants.TimeTriggerPath:
                        if (!string.IsNullOrEmpty(setting.GetAttributeValue<string>("syed_value")))
                        {
                            this.timeTriggerPath = Path.Combine(this.LocalDirectory, setting.GetAttributeValue<string>("syed_value"));
                        }

                        break;

                    case Constants.SolutionTextPathForRelease:
                        if (!string.IsNullOrEmpty(setting.GetAttributeValue<string>("syed_value")))
                        {
                            this.solutionTextRelease = Path.Combine(this.LocalDirectory, setting.GetAttributeValue<string>("syed_value"));
                        }

                        break;
                    case Constants.RepositoryJsDirectory:
                        if (!string.IsNullOrEmpty(setting.GetAttributeValue<string>("syed_value")))
                        {
                            this.javaScriptDirectory = Path.Combine(this.LocalDirectory, setting.GetAttributeValue<string>("syed_value"));
                        }

                        break;
                    case Constants.RepositoryHtmlDirectory:
                        if (!string.IsNullOrEmpty(setting.GetAttributeValue<string>("syed_value")))
                        {
                            this.htmlDirectory = Path.Combine(this.LocalDirectory, setting.GetAttributeValue<string>("syed_value"));
                        }

                        break;
                    case Constants.RepositoryImagesDirectory:
                        if (!string.IsNullOrEmpty(setting.GetAttributeValue<string>("syed_value")))
                        {
                            this.imagesDirectory = Path.Combine(this.LocalDirectory, setting.GetAttributeValue<string>("syed_value"));
                        }

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
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + output + "<br>");
                Console.WriteLine(output);
                string err = process.StandardError.ReadToEnd();
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + err + "<br>");
                Console.WriteLine(err);
                process.WaitForExit();

                Directory.CreateDirectory(SubstDrive + ":\\1\\");
                this.LocalDirectory = SubstDrive + ":\\1\\";
            }
            catch (Exception ex)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                Console.WriteLine(ex.Message);
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
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + output + "<br>");
                Console.WriteLine(output);
                string err = process.StandardError.ReadToEnd();
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + err + "<br>");
                Console.WriteLine(err);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(ex.Message + "<br>");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
