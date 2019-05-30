﻿//-----------------------------------------------------------------------
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

    /// <summary>
    /// constants file for repository configurations
    /// </summary>
    internal class RepositoryConfigurationConstants
    {
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
        /// Gets repository release directory
        /// </summary>
        public static string SolutionFolder
        {
            get
            {
                return Path.Combine(LocalDirectory, ConfigurationManager.AppSettings["RepositorySolutionFolder"]);
            }
        }


        /// <summary>
        /// Gets repository script directory
        /// </summary>
        public static string JsDirectory
        {
            get
            {
                return Path.Combine(LocalDirectory, ConfigurationManager.AppSettings["RepositoryJsDirectory"]);
            }
        }

        /// <summary>
        /// Gets repository html directory
        /// </summary>
        public static string HtmlDirectory
        {
            get
            {
                return Path.Combine(LocalDirectory, ConfigurationManager.AppSettings["RepositoryHtmlDirectory"]);
            }
        }

        /// <summary>
        /// Gets repository Images directory
        /// </summary>
        public static string ImagesDirectory
        {
            get
            {
                return Path.Combine(LocalDirectory, ConfigurationManager.AppSettings["RepositoryImagesDirectory"]);
            }
        }

        /// <summary>
        /// Gets repository url
        /// </summary>
        public static string RepositoryUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["RepositoryUrl"];
            }
        }

        /// <summary>
        /// Gets repository url
        /// </summary>
        public static string CloneRepositoryAlways
        {
            get
            {
                return ConfigurationManager.AppSettings["CloneRepositoryAlways"];
            }
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
        }

        /// <summary>
        /// Gets repository remote name
        /// </summary>
        public static string RepositoryRemoteName
        {
            get
            {
                return ConfigurationManager.AppSettings["RemoteName"];
            }
        }

        /// <summary>
        /// Gets Repository branch name
        /// </summary>
        public static string BranchName
        {
            get
            {
                return ConfigurationManager.AppSettings["BranchName"];
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
