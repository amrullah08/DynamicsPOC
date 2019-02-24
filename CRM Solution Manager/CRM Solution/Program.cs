//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace CrmSolution
{
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
            if (args == null || args.Length == 0)
            {
                args = new string[] { "SyedAmrullahMazhar", "Syed Amrullah", "syamrull@microsoft.com" };
            }

            string solutionUniqueName = null; // args[0];
            string committerName = args[1];
            string committerEmail = args[2];
            string authorEmail = "TestSolutionCommitterService@microsoft.com";

            RepositoryHelper.TryUpdateToRepository(solutionUniqueName, committerName, committerEmail, authorEmail);
        }
    }
}
