// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Collections;
using System.Globalization;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Extras.FX1_1.UnitTests
{
    /* Most of the MSBuild Toolkit for .NET 1.1 (MSBee) unit tests use this MockEngine class. 
    MSBuild tasks rely on a build engine to log events and the functions being tested use the expected BuildEngine logging facilities. 
    However, because we're executing these tasks in code, we need to explicitly provide a BuildEngine so the logger can function. 
    Instead of taking a dependency on BuildEngine.dll, we use a MockEngine class which provides static logging functionality. 
    If you forget to set a task's BuildEngine property and then attempt to use its logging functions, 
    an InvalidOperationException is thrown indicating the task attempted to log before it was initialized. */

    public sealed class MockEngine : IBuildEngine
    {
        private int messages, warnings, errors;
        private string log, upperLog;
        private Engine engine;
        private bool logToConsole = false;

        internal MockEngine() : this(false)
        {
        }

        public MockEngine(bool logToConsole)
        {
            this.logToConsole = logToConsole;
            this.engine = new Engine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().EscapedCodeBase).LocalPath));
            this.engine.RegisterLogger(new ConsoleLogger());
        }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (e.File != null && e.File.Length > 0)
            {
                if (logToConsole)
                    Console.Write("{0}({1},{2}): ", e.File, e.LineNumber, e.ColumnNumber);
            }

            if (logToConsole)
                Console.Write("ERROR " + e.Code + ": ");
            log += "ERROR " + e.Code + ": ";
            ++errors;

            if (logToConsole)
                Console.WriteLine(e.Message);
            log += e.Message;
            log += "\n";
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (e.File != null && e.File.Length > 0)
            {
                if (logToConsole)
                    Console.Write("{0}({1},{2}): ", e.File, e.LineNumber, e.ColumnNumber);
            }

            if (logToConsole)
                Console.Write("WARNING " + e.Code + ": ");
            log += "WARNING " + e.Code + ": ";
            ++warnings;

            if (logToConsole)
                Console.WriteLine(e.Message);
            log += e.Message;
            log += "\n";
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (logToConsole)
                Console.WriteLine(e.Message);
            log += e.Message;
            log += "\n";
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (logToConsole)
                Console.WriteLine(e.Message);
            log += e.Message;
            log += "\n";
            ++messages;
        }

        public bool ContinueOnError
        {
            get
            {
                return false;
            }
        }

        public string ProjectFileOfTaskNode
        {
            get
            {
                return String.Empty;
            }
        }

        public int LineNumberOfTaskNode
        {
            get
            {
                return 0;
            }
        }

        public int ColumnNumberOfTaskNode
        {
            get
            {
                return 0;
            }
        }

        /// <remarks>
        /// This method isn't required for the MockEngine to log events from our tasks.
        /// Thus, we simply return false rather than implement it.
        /// </remarks>
        public bool BuildProjectFile
            (
            string projectFileName, 
            string[] targetNames, 
            IDictionary globalProperties, 
            IDictionary targetOutputs
            )
        {
            return false;
        }

        /// <summary>
        /// Case insensitive check that the log file contains the given string.
        /// </summary>
        /// <param name="contains">String to be searched for in the log.</param>
        /// <returns>True if the given string is present in the log; false otherwise.</returns>
        public bool LogContains(string contains)
        {
            if (contains == null)
            {
                throw new ArgumentNullException("contains");
            }

            upperLog = log;
            upperLog = upperLog.ToUpperInvariant();

            return upperLog.Contains(contains.ToUpperInvariant());
        }
    }
}
