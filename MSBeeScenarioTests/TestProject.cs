// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Extras.FX1_1.ScenarioTests
{
    class TestProject
    {
        private static readonly ResourceManager strings = new ResourceManager("Microsoft.Build.Extras.FX1_1.ScenarioTests.Strings", Assembly.GetExecutingAssembly());

        // The process object for MSBuild.exe
        Process proc;

        // Common configuration variables
        string parameters, buildConfiguration;

        // Needed paths
        string testPath, solutionPath;

        // Output and Error logs
        string logOutput, logError;

        // MSBuild Exit Code
        int exitCode;

        public TestProject()
        {
            proc = new Process();
        }

        #region Properties

        /// <summary>
        /// This MSBuild console output.
        /// </summary>
        public string LogOutput
        {
            get
            {
                return logOutput;
            }
        }

        /// <summary>
        /// The MSBuild console error output.
        /// </summary>
        public string LogError
        {
            get
            {
                return logError;
            }
        }

        /// <summary>
        /// The path to the test project's solution file.
        /// </summary>
        public string SolutionPath
        {
            get
            {
                return solutionPath;
            }
            set
            {
                solutionPath = value;
            }
        }

        /// <summary>
        /// The path to the top level directory where scenario test projects are located.
        /// </summary>
        public string TestPath
        {
            get
            {
                return testPath;
            }
            set
            {
                testPath = value;
            }
        }

        /// <summary>
        /// Parameters that go to MSBuild.exe for all scenario tests.
        /// </summary>
        public string Parameters
        {
            set
            {
                parameters = value;
            }
        }

        /// <summary>
        /// MSBuild's exit code after a build attempt.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public int ExitCode
        {
            get
            {
                return exitCode;
            }
        }

        /// <summary>
        /// The build configuration.
        /// </summary>
        public string BuildConfiguration
        {
            set
            {
                buildConfiguration = value;
            }
            get
            {
                return buildConfiguration;
            }
        }
 
        #endregion

        /// <summary>
        /// Obtain a path to MSBuild.exe so test projects can be built.
        /// </summary>
        private static string GetPathToMSBuild()
        {
            string MSBuild = "MSBuild.exe";
            string msbuildPath = ToolLocationHelper.GetPathToDotNetFrameworkFile(MSBuild, TargetDotNetFrameworkVersion.VersionLatest);

            if (String.IsNullOrEmpty(msbuildPath))
            {
                Console.WriteLine(strings.GetString("NETFrameworkFileWasNotFound", CultureInfo.CurrentUICulture),
                    MSBuild, ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.VersionLatest),
                    ToolLocationHelper.GetDotNetFrameworkRootRegistryKey(TargetDotNetFrameworkVersion.VersionLatest));
            }
  
            return msbuildPath;
        }

        /// <summary>
        /// Invokes PrepareProcess and ExecuteProcess to build the test solution.
        /// </summary>
        public void Build(string testSolutionDir)
        {
            PrepareProcess(testSolutionDir);
            ExecuteProcess();
        }

        /// <summary>
        /// Build the command line for MSBuild.exe, which contains the path to the solutions file and
        /// the necessary parameters. Also, set certain Process.StartInfo fields.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private void PrepareProcess(string testSolutionDir)
        {
            string testsDirectory = Path.Combine(testSolutionDir, testPath);

            // Surround the path with quotations in case there is a space in the path.
            StringBuilder commandLine = new StringBuilder(String.Concat("\"", testsDirectory, solutionPath, "\""));

            // Add the command line parameters.
            commandLine.Append(String.Concat(" ", parameters, " /p:Configuration=", buildConfiguration));

            proc.StartInfo = new ProcessStartInfo(GetPathToMSBuild(), commandLine.ToString());
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardOutput = true;
        }

        /// <summary>
        /// Invokes the Start method on the Process object and sets the error and output streams.
        /// </summary>
        /// <remarks>
        /// From others' experiences, it seems that WaitForExit isn't always reliable so once it returns, 
        /// have the thread sleep in 50 ms intervals until proc.HasExited is true.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private void ExecuteProcess()
        {
            proc.Start();

            // Do not perform synchronous reads on both streams.
            StreamReader standardErrorStream = proc.StandardError;
            logOutput = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();
            while (!proc.HasExited)
            {
                System.Threading.Thread.Sleep(50);
            }

            // Write contents of streams to the console.
            logError = standardErrorStream.ReadLine();

            // Store the exit code.
            exitCode = proc.ExitCode;

            // Close the process
            proc.Close();
        }
    }
}
