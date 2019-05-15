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

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "subst";
                startInfo.Arguments = SubstDrive + ": '" + LocalDirectory + "'";
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
                LocalDirectory = ConfigurationManager.AppSettings["RepositoryLocalDirectory"];
            }
        }

        /// <summary>
        /// method deletes substituted temporary directory
        /// </summary>
        private static void DeleteSubstTempDirectory()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "subst";
                startInfo.Arguments = SubstDrive + ": '" + LocalDirectory + "'";
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
