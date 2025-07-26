// Copyright (C) Microsoft Corporation. All rights reserved.

using NUnit.Framework;
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

[assembly: CLSCompliant(true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum)]
namespace Microsoft.Build.Extras.FX1_1.ScenarioTests
{
    public class ScenarioTest
    {
        private static readonly ResourceManager strings = new ResourceManager("Microsoft.Build.Extras.FX1_1.ScenarioTests.Strings", Assembly.GetExecutingAssembly());

        /// <summary>
        /// Instanciates a new TestProject object and loads and reads the configuration file.
        /// </summary>
        public ScenarioTest()
        {
            configFile = new XmlDocument();

            // This assumes the project directory will always be a subdirectory of the solution directory.
            solutionDir = GetMSBeeSolutionDirectory();
            projectDir = Path.Combine(solutionDir, "MSBeeScenarioTests");

            LoadConfigFile();
            StoreCommonConfig();
        }

        // A TestProject object, which actually creates and runs the MSBuild process to perform the build.
        private TestProject testProject;

        // The scenario test's name, project path, and parameters.
        private string testName, testCommonPath, testCommonParams;

        // This project's solution and project directory.
        private string solutionDir, projectDir;


        // Configuration file variables.
        private XmlDocument configFile;
        private XmlElement configFileRoot;

        // Constant that is returned from functions to indicate no XML data was obtained.
        private const int NoXMLData = Int32.MaxValue;

        #region Verification Data

        /// <summary>
        /// While we check for entire the version number entered by the user (e.g. v2.0.50727 or v1.1) 
        /// we validate the config file data using only the shorter format.
        /// </summary>
        private Regex shortV1dot1 = new Regex("v1\\.1");
        private Regex shortV2dotN = new Regex("v2\\.0");

        // If you want to try testing with a minor release above 2.0, you can substitute this 
        // regular expression for the one above. Obviously, we can not know that MSBee will work
        // without tweeking to work with future releases of .NET Framework but you would have
        // to change this regular expression to allow using the FrameworkVersion with v2.1
        // private Regex shortV2dotN = new Regex("v2\\.\\d");
        private string frameworkVersion;

        /// <summary>
        /// Log output containing the compilers that were executed
        /// </summary>
        private MatchCollection vbMatches;
        private MatchCollection vcMatches;

        #endregion

        /// <summary>
        /// This MSBuild console output.
        /// </summary>
        public string LogOutput
        {
            get
            {
                return testProject.LogOutput;
            }
        }

        /// <summary>
        /// This MSBuild error output.
        /// </summary>
        public string LogError
        {
            get
            {
                return testProject.LogError;
            }
        }

        /// <summary>
        /// Calls the TestProject build function to build the test project after validating input parameters.
        /// </summary>
        /// <param name="name">The name of the test, usually matching the name of the test project.</param>
        /// <param name="configuration">The configuration of the build, typically debug or release.</param>
        public void RunTest(string name, string configuration)
        {
            Assert.IsFalse(String.IsNullOrEmpty(name), strings.GetString("MissingProjectName"));
            Assert.IsFalse(String.IsNullOrEmpty(configuration), strings.GetString("MissingProjectConfiguration"));

            // Create a test project object.
            testProject = new TestProject();
            testProject.TestPath = testCommonPath;
            testProject.Parameters = testCommonParams;

            // Confirm the specified test is present in the config file.
            XmlNode testNameNode = configFileRoot.SelectSingleNode("//" + name);
            Assert.IsNotNull(testNameNode, strings.GetString("RequiredElementIsMissing", CultureInfo.CurrentUICulture), name);
            testName = name;

            // Confirm the configuration name provided by InvokeScenarioTests matches a build configuration name in the config file.
            XmlNode buildConfigNode = configFileRoot.SelectSingleNode("//" + testName + "/TestValues/Configuration[@Name='" + configuration + "']");
            Assert.IsNotNull(buildConfigNode, strings.GetString("RequiredElementIsMissing", CultureInfo.CurrentUICulture), "Configuration Name=" + configuration);
            testProject.BuildConfiguration = configuration;

            // SolutionPath is a required value that should be set in the configuration file; if it's not present, fail the test.
            XmlNode solutionPathNode = configFileRoot.SelectSingleNode("//" + testName + "/SolutionPath");
            Assert.IsNotNull(solutionPathNode, strings.GetString("RequiredElementIsMissing", CultureInfo.CurrentUICulture),
                        "SolutionPath");
            testProject.SolutionPath = solutionPathNode.InnerText;

            // Check for values tied to the testParameters element.
            XmlNode testParameters = configFileRoot.SelectSingleNode("//" + testName + "/OverrideCommonParameters");
            if (testParameters != null)
            {
                testProject.Parameters = testParameters.InnerText;
            }

            // Build the test project.
            testProject.Build(solutionDir);
        }

        /// <summary>
        /// Load the XML configuration file.
        /// </summary>
        private void LoadConfigFile()
        {
            const string ScenarioTestsConfigFile = "ScenarioTestsConfig.xml";

            // Load the config file into a XmlDocument object and obtain a root element.
            configFile.Load(Path.Combine(projectDir, ScenarioTestsConfigFile));
            configFileRoot = configFile.DocumentElement;
        }

        /// <summary>
        /// Process the XML configuration file's common parameters.
        /// </summary>
        private void StoreCommonConfig()
        {
            // These parameters are required; tests should fail if these values haven't been provided.
            XmlNode testPathNode = configFileRoot.SelectSingleNode("//Common/TestPath");
            Assert.IsNotNull(testPathNode, strings.GetString("RequiredElementIsMissing", CultureInfo.CurrentUICulture), "TestPath");
            testCommonPath = testPathNode.InnerText;

            XmlNode projParamsNode = configFileRoot.SelectSingleNode("//Common/ProjectParameters");
            Assert.IsNotNull(projParamsNode, strings.GetString("RequiredElementIsMissing", CultureInfo.CurrentUICulture), "ProjectParameters");
            testCommonParams = projParamsNode.InnerText;
        }

        /// <summary>
        /// Return the expected test project exit code, which is set in the configuration file.
        /// </summary>
        /// <returns>The expected exit code from MSBuild.</returns>
        private int GetExpectedExitCode()
        {
            string exitCode = "";

            XmlNode exitCodeNode = configFileRoot.SelectSingleNode("//" + testName + "/TestValues/Configuration[@Name='" + testProject.BuildConfiguration + "']/ExitCode");
            if (exitCodeNode != null)
            {
                exitCode = exitCodeNode.InnerText;
            }
            return String.IsNullOrEmpty(exitCode) ? NoXMLData : Int32.Parse(exitCode, System.Globalization.CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Writes the build output to the console and compares the actual exit code to expected exit code.
        /// </summary>
        public void ConfirmExpectedBuildResult()
        {
            Console.WriteLine();
            Console.WriteLine("-----------------------" + testProject.BuildConfiguration.ToUpperInvariant() + " CONFIGURATION -----------------------");
            Console.WriteLine();
            Console.WriteLine(testProject.LogOutput);
            Console.WriteLine(testProject.LogError);
            int expectedExitCode = GetExpectedExitCode();

            if (expectedExitCode != NoXMLData)
            {
                int actualExitCode = testProject.ExitCode;

                Assert.AreEqual(expectedExitCode, actualExitCode,
                    strings.GetString("ExitCodesDoNotMatch", CultureInfo.CurrentUICulture),
                        expectedExitCode, actualExitCode);

                WriteTestPassed("ConfirmExpectedBuildResult");
            }
        }

        /// <summary>
        /// Reads the configuration file for the expected number of build errors.
        /// </summary>
        /// <returns>The expected number of build errors for the project.</returns>
        private int GetExpectedNumberOfErrors()
        {
            string numErrors = "";
            
            XmlNode numErrorsNode = configFileRoot.SelectSingleNode("//" + testName + "/TestValues/Configuration[@Name='" + testProject.BuildConfiguration + "']/NumberOfErrors");
            if (numErrorsNode != null)
            {
                numErrors = numErrorsNode.InnerText;
            }
            return String.IsNullOrEmpty(numErrors) ? NoXMLData : Int32.Parse(numErrors, System.Globalization.CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Uses a regular expression on the MSBuild output to obtain the actual number of build errors.
        /// </summary>
        /// <returns>The actual number of build errors for the project.</returns>
        private int GetActualNumberOfErrors()
        {
            Regex errorExpression = new Regex("(\\d+)\\sError\\(s\\)");
            Match errorCount = errorExpression.Match(testProject.LogOutput);
            if (errorCount.Success)
            {
                // Only one value should have been captured for the number of errors.
                if (errorCount.Groups[1].Captures.Count == 1)
                {
                    string numErrors = errorCount.Groups[1].ToString();
                    return Int32.Parse(numErrors, System.Globalization.CultureInfo.CurrentCulture);
                }
            }

            // Return -1 if the match fails.
            return -1;
        }

        /// <summary>
        /// HasCorrectNumberOfErrors confirms that the expected number of errors were generated
        /// when building. 
        /// </summary>
        /// <remarks>
        /// Typically, the 'correct' number of errors would be 0. But, you could have a negative test
        /// where the build is expected to fail. In this case, the correct number of errors would not be 0.
        /// </remarks>
        public void HasCorrectNumberOfErrors()
        {
            int expectedErrors = GetExpectedNumberOfErrors();
            if (expectedErrors != NoXMLData)
            {
                int actualErrors = GetActualNumberOfErrors();

                // Confirm that actualErrors received a non-negative value and thus the function didn't fail.
                Assert.IsTrue(actualErrors >= 0, strings.GetString("FunctionFailed", CultureInfo.CurrentUICulture),
                    "GetActualNumberOfErrors");

                Assert.AreEqual(expectedErrors, actualErrors,
                    strings.GetString("NumberOfErrorsDoNotMatch", CultureInfo.CurrentUICulture),
                        expectedErrors, actualErrors);

                WriteTestPassed("HasCorrectNumberOfErrors");
            }
        }

        /// <summary>
        /// Reads the configuration file for the expected number of build warnings.
        /// </summary>
        /// <returns>The expected number of build warnings for the project.</returns>
        private int GetExpectedNumberOfWarnings()
        {
            string numWarnings = "";

            XmlNode numWarningsNode = configFileRoot.SelectSingleNode("//" + testName + "/TestValues/Configuration[@Name='" + testProject.BuildConfiguration + "']/NumberOfWarnings");
            if (numWarningsNode != null)
            {
                numWarnings = numWarningsNode.InnerText;
            }
            return String.IsNullOrEmpty(numWarnings) ? NoXMLData : Int32.Parse(numWarnings, System.Globalization.CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Uses a regular expression on the MSBuild output to obtain the actual number of build warnings.
        /// </summary>
        /// <returns>The actual number of build warnings for the project.</returns>
        private int GetActualNumberOfWarnings()
        {
            Regex warningExpression = new Regex("(\\d+)\\sWarning\\(s\\)");
            Match warningCount = warningExpression.Match(testProject.LogOutput);
            if (warningCount.Success)
            {
                // Only one value should have been captured for the number of warnings.
                if (warningCount.Groups[1].Captures.Count == 1)
                {
                    string numWarnings = warningCount.Groups[1].ToString();
                    return Int32.Parse(numWarnings, System.Globalization.CultureInfo.CurrentCulture);
                }
            }

            // Return -1 if the match fails.
            return -1;
        }


        /// <summary>
        /// HasCorrectNumberOfWarnings confirms that the expected number of warnings were generated
        /// when building. 
        /// </summary>
        public void HasCorrectNumberOfWarnings()
        {
            int expectedWarnings = GetExpectedNumberOfWarnings();
            if (expectedWarnings != NoXMLData)
            {
                int actualWarnings = GetActualNumberOfWarnings();

                // Confirm that actualWarnings received a non-negative value and thus the function didn't fail.
                Assert.IsTrue(actualWarnings >= 0, strings.GetString("FunctionFailed", CultureInfo.CurrentUICulture),
                    "GetActualNumberOfWarnings");

                Assert.AreEqual(expectedWarnings, actualWarnings,
                    strings.GetString("NumberOfWarningsDoNotMatch", CultureInfo.CurrentUICulture),
                        expectedWarnings, actualWarnings);

                WriteTestPassed("HasCorrectNumberOfWarnings");
            }
        }

        /// <summary>
        /// Reads the list of files that are expected to be produced by the build attempt.
        /// For each file, its existence is confirmed and its last write time is checked to
        /// confirm the file was created by the current build attempt and not a previous one.
        /// </summary>
        public void ConfirmExpectedFilesExist()
        {
            string testProjectsPath = Path.Combine(solutionDir, testProject.TestPath);
            string pathToSolutionFile = Path.Combine(testProjectsPath, testProject.SolutionPath);
            string testSolutionAbsPath = Path.GetDirectoryName(pathToSolutionFile);

            XmlNodeList fileNameList = 
                configFileRoot.SelectNodes("//" + testName + "/TestValues/Configuration[@Name='" + testProject.BuildConfiguration + "']/GeneratedFiles/File/@Name");
            
            foreach (XmlNode file in fileNameList)
            {
                // Confirm file exists.
                string path = Path.Combine(testSolutionAbsPath, file.InnerText);
                Assert.IsTrue(File.Exists(path), strings.GetString("FileDoesNotExist", CultureInfo.CurrentUICulture),
                    path);

                // The value of Ticks represents the number of 100-nanosecond intervals that have elapsed 
                // since 12:00:00 midnight, January 1, 0001.
                long currentTimeInTicks = DateTime.Now.Ticks;
                DateTime lastWriteTime = File.GetLastWriteTime(path);
                long fileCreationTimeInTicks = lastWriteTime.Ticks;

                // If a file's last write time is not within the provided time interval of the currentTime, 
                // we assume that it wasn't created or copied with this build run so fail here.
                // The default is the config file is 60 seconds, which is admittedly arbitrary 
                // but seems reasonable given that these are small projects so builds should be fast.
                // Note: 1 second is 10^7 ticks.
                const long OneSecondOfTicks = 10000000;

                XmlNode generateFileNode = configFileRoot.SelectSingleNode("//" + testName + "/TestValues/Configuration[@Name='" + testProject.BuildConfiguration + "']/GeneratedFiles/@TimeInterval");
                Assert.IsNotNull(generateFileNode, strings.GetString("RequiredAttributeIsMissing", CultureInfo.CurrentUICulture), "TimeInterval", "GeneratedFiles");
                string timeInterval = generateFileNode.InnerText;

                int timeIntervalInSeconds = Int32.Parse(timeInterval, System.Globalization.CultureInfo.CurrentCulture);
                long timeIntervalInTicks = timeIntervalInSeconds * OneSecondOfTicks;

                // Confirm file was built during this test run. 
                Assert.IsTrue((currentTimeInTicks - fileCreationTimeInTicks) < timeIntervalInTicks,
                    strings.GetString("FileWasNotProducedByThisBuild", CultureInfo.CurrentUICulture),
                    path, timeIntervalInSeconds, lastWriteTime);
            }

            if (fileNameList.Count > 0)
            {
                WriteTestPassed("ConfirmExpectedFilesExist");
            }
        }

        /// <summary>
        /// Reads the list of files that are expected to not be produced by the build attempt.
        /// For each file, its non-existence is confirmed.
        /// </summary>
        public void ConfirmUnexpectedFilesDontExist()
        {
            string testProjectsPath = Path.Combine(solutionDir, testProject.TestPath);
            string pathToSolutionFile = Path.Combine(testProjectsPath, testProject.SolutionPath);
            string testSolutionAbsPath = Path.GetDirectoryName(pathToSolutionFile);

            XmlNodeList fileNameList =
                configFileRoot.SelectNodes("//" + testName + "/TestValues/Configuration[@Name='" + testProject.BuildConfiguration + "']/NotGeneratedFiles/File/@Name");
            
            foreach (XmlNode file in fileNameList)
            {
                string path = Path.Combine(testSolutionAbsPath, file.InnerText);
                Assert.IsTrue(!File.Exists(path), strings.GetString("FileShouldNotExist", CultureInfo.CurrentUICulture), 
                    path);
            }

            if (fileNameList.Count > 0)
            {
                WriteTestPassed("ConfirmUnexpectedFilesDontExist");
            }
        }

        /// <summary>
        /// Prints the function passed message to the console.
        /// </summary>
        /// <param name="functionName">Name of the function that passed.</param>
        private static void WriteTestPassed(string functionName)
        {
            Console.WriteLine(strings.GetString("FunctionPassed", CultureInfo.CurrentUICulture), functionName);
        }

        #region Initialization for Log based verification

        /// <summary>
        /// Goes through the console log output and captures various strings
        /// that we will need for later verifications.
        /// </summary>
        public void InitializeLogParsing()
        {
            GetCompilerCommandLines();
        }

        /// <summary>
        /// Searches the output log for the target built by MSBee
        /// </summary>
        private MatchCollection GetTargetsBuilt()
        {
            Regex targetsString = new Regex("Target Clean:|Target Rebuild:|Target Build:");
            MatchCollection targetMatches = targetsString.Matches(testProject.LogOutput);
            return targetMatches;
        }

        /// <summary>
        /// Searches the output log for executions of the VB and C# compilers
        /// </summary>
        private void GetCompilerCommandLines()
        {
            Regex vbCompilerCommandString = new Regex("[a-zA-Z]:\\\\.*\\\\Microsoft.NET\\\\Framework\\\\.*\\\\Vbc\\.exe.*");
            Regex vcCompilerCommandString = new Regex("[a-zA-Z]:\\\\.*\\\\Microsoft.NET\\\\Framework\\\\.*\\\\Csc\\.exe.*");

            vbMatches = vbCompilerCommandString.Matches(testProject.LogOutput);
            vcMatches = vcCompilerCommandString.Matches(testProject.LogOutput);
        }

        /// <summary>
        /// Searches the output log for the configuration built by MSBee
        /// </summary>
        private MatchCollection GetConfigurationLines()
        {
            Regex configurationString = new Regex("Building solution configuration.*");
            MatchCollection configMatches = configurationString.Matches(testProject.LogOutput);
            return configMatches;
        }
        
        /// <summary>
        /// Searches command lines for the command option specified by the Regex and then searches
        /// the instances of the option for the parameter specified by the parameterName string.
        /// </summary>
        /// <param name="commandLineMatches"></param>
        /// <param name="optionRegex"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        private static bool CheckCommandLineOption(MatchCollection commandLineMatches, Regex optionRegex, string parameterName)
        {
            // This method searches a collection of command lines for options that take parameters
            // and verifies that the string defined by the method's parameterName argument was on the commandline
            // in an option specified by the optionRegex argument.
            //
            // For example, if the commandLineMatches MatchCollection collection represents calls to the 
            // VB/C# compiler and the optionRegex were searching for /resource: options, this method would
            // search all instances of the /resource: options for the resource named by the parameterName
            // argument (e.g. Form1.resources)
            bool passed = false;
            foreach (Match match in commandLineMatches)
            {
                // Search each commandline for all instances of the regular expression which is expected
                // to be an option for the command being searched
                MatchCollection optionMatches = optionRegex.Matches(match.Value);
                foreach (Match option in optionMatches)
                {
                    // Search each option matched for the parameter string passed in and
                    // break out of the loop if it's found
                    if (option.Value.Contains(parameterName) == true)
                    {
                        passed = true;
                        break;
                    }
                }
            }
            return passed;
        }

        #endregion

        #region Checking resources

        /// <summary>
        /// Check compile command lines that resources specified by the Resources/Resource 
        /// tags in the config file were passed to the compiler.
        /// </summary>
        public void CheckResources()
        {
            Regex resourceRegex = new Regex("/resource:\\S*");
            // Get expected resources from config file
            XmlNodeList resourceNameList = configFileRoot.SelectNodes("//" + testName + "/TestValues/Resources/Resource/@Name");
            if ((resourceNameList != null) && (resourceNameList.Count > 0))
            {
                // Check that each resource named appears in a compiler command for the solution/project
                foreach (XmlNode resource in resourceNameList)
                {
                    bool passed = CheckCommandLineOption(vbMatches, resourceRegex, resource.InnerText) || CheckCommandLineOption(vcMatches, resourceRegex, resource.InnerText);
                    Assert.IsTrue(passed, strings.GetString("ResourceNotFound", CultureInfo.CurrentUICulture), resource.InnerText);
                }
                WriteTestPassed("CheckResources");
            }
        }

        /// <summary>
        /// Verfiy that the resources specified in the config file were passed to AL.exe
        /// to be incorporated into the resource DLL specified in the config file.
        /// </summary>
        public void CheckLinkedResources()
        {
            // Get list of expected resource dlls
            XmlNodeList resourceDLLList = configFileRoot.SelectNodes("//" + testName + "/TestValues/LinkedResourceDLLs/DLL/@Name");

            // If there are no LinkedResourceDLLs tags in the config file skip this check
            if ((resourceDLLList != null) && (resourceDLLList.Count > 0))
            {
                // Find all the calls to AL.exe in test's log output
                Regex alLineRegex = new Regex("AL\\.exe.*");
                MatchCollection alLines = alLineRegex.Matches(testProject.LogOutput);

                // Process each DLL specified by the config file
                foreach (XmlNode resourceDLL in resourceDLLList)
                {
                    // Get resources that are expected to be linked into the DLL
                    XmlNodeList resourceNameList = configFileRoot.SelectNodes("//" + testName + "/TestValues/LinkedResourceDLLs/DLL[@Name='" + resourceDLL.InnerText + "']/LinkedResource/@Name");
 
                    // Error out if the config file has no LinkedResource tags under the LinkedResourceDLLs
                    Assert.IsTrue(((resourceNameList != null) && (resourceNameList.Count > 0)), strings.GetString("NoLinkedResources", CultureInfo.CurrentUICulture));
                    
                    // This Regex finds the output option in the calls to AL.exe
                    Regex outputOptionRegex = new Regex("/out:\\S*");

                    // This Regex finds the embed option in the calls to AL.exe
                    Regex embedOptionRegex = new Regex("/embed:\\S*");

                    int matchingCount = 0;

                    // Search all the AL lines for one which output to the current DLL name
                    foreach (Match alLine in alLines)
                    {
                        
                        // Fine the outuput option on this line
                        Match outputOptionMatch = outputOptionRegex.Match(alLine.Value);

                        // See whether or not the output option contains the current DLL name
                        if (outputOptionMatch.Value.Contains(resourceDLL.InnerText) == true)
                        {
                           matchingCount++;
                           // Get all the /embed options on this command line
                           MatchCollection embedOptions = embedOptionRegex.Matches(alLine.Value);

                           // Verify that each resource named in the config file appears in an
                           // embed option on the AL command line for the resourceDLL
                           foreach (XmlNode resource in resourceNameList)
                           {
                               bool passed = false;
                               foreach (Match embedMatch in embedOptions)
                               {
                                   if (embedMatch.Value.Contains(resource.InnerText) == true)
                                   {
                                       passed = true;
                                       break;
                                   }
                               }
                               Assert.IsTrue(passed, strings.GetString("LinkedResourceNotFound", CultureInfo.CurrentUICulture),
                                   resource.InnerText, resourceDLL.InnerText);
                           }
                        }
                    }
                    // Error if there were no matches in the log
                    Assert.IsTrue((matchingCount > 0), strings.GetString("ALNotFound", CultureInfo.CurrentUICulture), resourceDLL.InnerText);
                }
                WriteTestPassed("CheckLinkedResources");
            }
        }
        #endregion

        #region Checking Assembly References
        /// <summary>
        /// Verify that assembly references specified in the config file were passed to the compiler.
        /// </summary>
        public void CheckAssemblyReferences()
        {
            Regex referenceRegex = new Regex("/reference:.*\\S");
            // Get expected references from config file
            XmlNodeList referenceNameList = configFileRoot.SelectNodes("//" + testName + "/TestValues/References/Reference/@Name");
            if ((referenceNameList != null) && (referenceNameList.Count > 0))
            {
                // Check that each reference named appears in a compiler command for the solution/project
                foreach (XmlNode reference in referenceNameList)
                {
                    bool passed = CheckCommandLineOption(vbMatches, referenceRegex, reference.InnerText) || CheckCommandLineOption(vcMatches, referenceRegex, reference.InnerText);
                    Assert.IsTrue(passed, strings.GetString("ReferenceNotFound", CultureInfo.CurrentUICulture), reference.InnerText);
                }
                WriteTestPassed("CheckAssemblyReferences");
            }
        }
        #endregion

        #region Checking the .NET Framework version

        /// <summary>
        /// Gets the optional FrameworkVersion XML tag from the config file
        /// </summary>
        /// 
        private void GetExpectedFrameworkVersion()
        {
            frameworkVersion = null;
            XmlNode frameworkVersionNode = configFileRoot.SelectSingleNode("//" + testName + "/TestValues/FrameworkVersion");
            if (frameworkVersionNode != null)
            {
                frameworkVersion = frameworkVersionNode.InnerText;
                if (!(String.IsNullOrEmpty(frameworkVersion)))
                {
                    if ((shortV1dot1.IsMatch(frameworkVersion) == false) &&
                        (shortV2dotN.IsMatch(frameworkVersion) == false))
                    {
                        Assert.Fail(
                            strings.GetString("InvalidFrameworkVersion", CultureInfo.CurrentUICulture),
                            frameworkVersion);
                    }
                }
                else
                {
                    Assert.Fail(
                            strings.GetString("NullFrameworkVersion", CultureInfo.CurrentUICulture),
                            frameworkVersion);
                }
            }
        }

        /// <summary>
        /// Verifies that the expected framework version was used for the build.
        /// </summary>
        public void IsCorrectFrameworkVersion()
        {
            GetExpectedFrameworkVersion();
            // Skip the test if the user didn't define the FrameworkVersion tag
            if (frameworkVersion != null)
            {
                CheckActualFrameworkVersion(vbMatches);
                CheckActualFrameworkVersion(vcMatches);
                WriteTestPassed("IsCorrectFrameWorkVersion");
            }
        }

        /// <summary>
        /// Parses all the executions of one compiler (e.g. VB or C#) and determines whether
        /// all of them were the .NET framework version specified by the user.
        /// </summary>
        /// <param name="matches"></param>
        private void CheckActualFrameworkVersion(MatchCollection matches)
        {        
            foreach (Match match in matches)
            {
                Assert.IsTrue(match.Value.Contains(frameworkVersion),
                            strings.GetString("FrameworkVersionsDoNotMatch", CultureInfo.CurrentUICulture),
                            frameworkVersion, match.Value);
            }
        }
        #endregion

        #region Verifying the configuration that was built

        /// <summary>
        /// Parses console log output to verify that the expected configuration was built.
        /// </summary>
        /// <param name="configName"></param>
        public void VerifyBuildConfig(string configName)
        {
            bool passed = true;
            Regex expectedConfigName = new Regex(configName);
            MatchCollection configMatches = GetConfigurationLines();

            foreach (Match match in configMatches)
            {
                if (expectedConfigName.IsMatch(match.Value) == false)
                {
                    passed = false;
                }
            }

            Assert.IsTrue(passed, strings.GetString("ConfigurationsDoNotMatch", CultureInfo.CurrentUICulture), expectedConfigName);
            WriteTestPassed("VerifyBuildConfig");
        }

        #endregion

        #region Verifying expected target was built

        /// <summary>
        /// Gets the value of the optional ExpectedTarget tag from the config file.
        /// It also restricts the user to entering legitimate values Clean | Build | Rebuild
        /// </summary>
        private string GetExpectedTarget()
        {
            string expectedTarget = null;
            XmlNode targetNode = configFileRoot.SelectSingleNode("//" + testName + "/TestValues/ExpectedTarget");
            
            if (targetNode != null)
            {
                Regex legitValues = new Regex("Clean|Build|Rebuild");
                string userValue = targetNode.InnerText;
                if (legitValues.IsMatch(userValue))
                {
                    if (userValue == "Build")
                    {
                        Assert.Fail(strings.GetString("TargetBuildUnsupported", CultureInfo.CurrentUICulture));
                    }
                    else
                    {
                        expectedTarget = userValue;
                    }
                }
                else
                {
                    Assert.Fail(strings.GetString("InvalidExpectedTarget", CultureInfo.CurrentUICulture), userValue);
                }
            }

            return expectedTarget;
        }

        /// <summary>
        /// When the optinal ExpectedTarget XML tag is specified, this method verifies that
        /// the expected target was built by searching the output log.
        /// </summary>
        public void VerifyExpectedBuildTarget()
        {         
            bool passed = true;
            string expectedTarget = GetExpectedTarget();

            if (expectedTarget != null)
            {
                Regex expectedBuildTarget = new Regex(expectedTarget);
                MatchCollection targetMatches = GetTargetsBuilt();
                foreach (Match match in targetMatches)
                {
                    if (!expectedBuildTarget.IsMatch(match.Value))
                    {
                        passed = false;
                    }
                }
                Assert.IsTrue(passed, strings.GetString("BuildTargetsDoNotMatch", CultureInfo.CurrentUICulture), expectedTarget);
                WriteTestPassed("VerifyExpectedBuildTarget");
            }
        }

        #endregion

        /// <returns>A path to the project directory.</returns>
        /// <remarks>
        /// Originally, I relied on the EnvDTE assembly to obtain the absolute path to the solution file
        /// using System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.8.0"). 
        /// Of course, this won't work when attempting to build this project from the command line
        /// when the IDE isn't running. 
        /// For NUnit, the current directory will be where the test DLL resides. Thus, we can simply
        /// go up three directories to be in the solution directory. This method assumes the 
        /// relative directory hierarchies will not change.
        /// </remarks>
        private static string GetMSBeeSolutionDirectory()
        {
            return new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
        }
    }
}
