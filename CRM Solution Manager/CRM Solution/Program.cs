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
    using CrmSolutionLibrary;

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
            //if (ProgramUtility.UpdateRepository(args))
            //{
            //    //log success message
            //}
            //else
            //{
            //    //log Error message
            //}



            bool result=ProgramUtility.UpdateRepository(args);
            Console.WriteLine("job completed");
            Console.ReadLine();

        }
    }
}
