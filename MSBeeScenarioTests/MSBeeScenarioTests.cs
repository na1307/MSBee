// Copyright (C) Microsoft Corporation. All rights reserved.

using NUnit.Framework;
using System;
using System.Text;
using System.Collections.Generic;

namespace Microsoft.Build.Extras.FX1_1.ScenarioTests
{
    /// <summary>
    /// This class includes test methods for each test project specified in the config file.
    /// Each test method should call InvokeScenarioTests, which builds the test project
    /// and confirms expected behavior based on the values in the config file. There should
    /// be one test method for each test project element in the config file.
    /// </summary>
    [TestFixture]
    public class MSBeeScenarioTests
    {
        ScenarioTest scenarioTest;

        public MSBeeScenarioTests()
        {
            scenarioTest = new ScenarioTest();
        }

        // Tests for Project to Project assembly references in the same solution

        [Test]
        public void SingleLevelCrossProjRefSln()
        {
            InvokeScenarioTests("SingleLevelCrossProjRefSln", "Debug");
            InvokeScenarioTests("SingleLevelCrossProjRefSln", "Release");
        }

        [Test]
        public void CrossProjMultilevelSln()
        {
            InvokeScenarioTests("CrossProjMultiLevelSln", "Debug");
            InvokeScenarioTests("CrossProjMultiLevelSln", "Release");
        }

        // Tests for file references to assemblies that are not part of the solution

        [Test]
        public void SingleLevelFileRefSln()
        {
            InvokeScenarioTests("SingleLevelFileRefSln", "Debug");
            InvokeScenarioTests("SingleLevelFileRefSln", "Release");
        }

        [Test]
        public void MultilevelFileRefSln()
        {
            InvokeScenarioTests("MultiLevelFileRefSln", "Debug");
            InvokeScenarioTests("MultiLevelFileRefSln", "Release");
        }

        // Tests for building with the intended framework version and custom configurations

        [Test]
        public void GenBuildForDotNetV1dot1()
        {
            InvokeScenarioTests("GenBuildForDotNetV1dot1", "Debug");
            InvokeScenarioTests("GenBuildForDotNetV1dot1", "Release");
            InvokeScenarioTests("GenBuildForDotNetV1dot1", "CustomConfig");
            InvokeScenarioTests("GenBuildForDotNetV1dot1", "CustomDebugConfig");
        }

        [Test]
        public void GenBuildForDotNetV2dot0()
        {
            InvokeScenarioTests("GenBuildForDotNetV2dot0", "Debug");
            InvokeScenarioTests("GenBuildForDotNetV2dot0", "Release");
        }

        [Test]
        public void GenBuildForDotNetV2dot0Too()
        {
            InvokeScenarioTests("GenBuildForDotNetV2dot0Too", "Debug");
            InvokeScenarioTests("GenBuildForDotNetV2dot0Too", "Release");
        }

        // Tests for building targets other than the default Rebuild target

        [Test]
        public void GenTestCleanTarget()
        {
            InvokeScenarioTests("GenTestCleanTarget", "Debug");
            InvokeScenarioTests("GenTestCleanTarget", "Release");
        }
  
        // Tests for FX1_1 Constant

        // Suppressing FxCop messages since the underscore in the method name is intentional.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member"), Test]
        public void CSharpAppFX1_1Constant()
        {
            InvokeScenarioTests("CSharpAppFX1_1Constant", "Debug");
            InvokeScenarioTests("CSharpAppFX1_1Constant", "Release");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member"), Test]
        public void VisualBasicAppFX1_1Constant()
        {
            InvokeScenarioTests("VisualBasicAppFX1_1Constant", "Debug");
            InvokeScenarioTests("VisualBasicAppFX1_1Constant", "Release");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member"), Test]
        public void CSharpAppFX1_1ConstantV2()
        {
            InvokeScenarioTests("CSharpAppFX1_1ConstantV2", "Debug");
            InvokeScenarioTests("CSharpAppFX1_1ConstantV2", "Release");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member"), Test]
        public void VisualBasicAppFX1_1ConstantV2()
        {
            InvokeScenarioTests("VisualBasicAppFX1_1ConstantV2", "Debug");
            InvokeScenarioTests("VisualBasicAppFX1_1ConstantV2", "Release");
        }
        
        // Tests for resolving basic COM references

        [Test]
        public void DirectCOMrefCSConsoleSln()
        {
            InvokeScenarioTests("DirectCOMrefCSConsoleSln", "Debug");
            InvokeScenarioTests("DirectCOMrefCSConsoleSln", "Release");
        }

        [Test]
        public void DirectCOMrefVBConsoleSln()
        {
            InvokeScenarioTests("DirectCOMrefVBConsoleSln", "Debug");
            InvokeScenarioTests("DirectCOMrefVBConsoleSln", "Release");
        }

        [Test]
        public void DirectCOMrefCSWinFormsSln()
        {
            InvokeScenarioTests("DirectCOMrefCSWinFormsSln", "Debug");
            InvokeScenarioTests("DirectCOMrefCSWinFormsSln", "Release");
        }

        [Test]
        public void DirectCOMrefVBWinFormsSln()
        {
            InvokeScenarioTests("DirectCOMrefVBWinFormsSln", "Debug");
            InvokeScenarioTests("DirectCOMrefVBWinFormsSln", "Release");
        }

        [Test]
        public void IndirectCOMrefSln()
        {
            InvokeScenarioTests("IndirectCOMrefSln", "Debug");
            InvokeScenarioTests("IndirectCOMrefSln", "Release");
        }

        [Test]
        public void MultipleComRefsSln()
        {
            InvokeScenarioTests("MultipleComRefsSln", "Debug");
            InvokeScenarioTests("MultipleComRefsSln", "Release");
        }

        // Tests verifying Assembly Reference Resolution

        [Test]
        public void CandidateAssemblyFilesSln()
        {
            InvokeScenarioTests("CandidateAssemblyFilesSln", "Debug");
            InvokeScenarioTests("CandidateAssemblyFilesSln", "Release");
        }

        [Test]
        public void HintPathSln()
        {
            InvokeScenarioTests("HintPathSln", "Debug");
            InvokeScenarioTests("HintPathSln", "Release");
        }
   
        [Test]
        public void OutputPathSln()
        {
            InvokeScenarioTests("OutputPathSln", "Debug");
            InvokeScenarioTests("OutputPathSln", "Release");
        }

        [Test]
        public void OutputPathBeforeHintPathFromItemSln()
        {
            InvokeScenarioTests("OutputPathBeforeHintPathFromItemSln", "Debug");
            InvokeScenarioTests("OutputPathBeforeHintPathFromItemSln", "Release");
        }

        [Test]
        public void OutPathBeforeHintPathFailsIn2005()
        {
            InvokeScenarioTests("OutPathBeforeHintPathFailsIn2005", "Debug");
            InvokeScenarioTests("OutPathBeforeHintPathFailsIn2005", "Release");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member"), Test]
        public void BaseFX1_1OutputPathWithSlash()
        {
            InvokeScenarioTests("BaseFX1_1OutputPathWithSlash", "Debug");
            InvokeScenarioTests("BaseFX1_1OutputPathWithSlash", "Release");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member"), Test]
        public void BaseFX1_1OutputPathWithoutSlash()
        {
            InvokeScenarioTests("BaseFX1_1OutputPathWithoutSlash", "Debug");
            InvokeScenarioTests("BaseFX1_1OutputPathWithoutSlash", "Release");
        }

        [Test]
        public void SingleProjFromMultilevelSln()
        {
            InvokeScenarioTests("SingleProjFromMultilevelSln", "Debug");
            InvokeScenarioTests("SingleProjFromMultilevelSln", "Release");
        }

        [Test]
        public void ComReferenceAndProjectReference()
        {
            InvokeScenarioTests("COMReferenceAndProjectReference", "Debug");
            InvokeScenarioTests("COMReferenceAndProjectReference", "Release");
        }

        /// <summary>
        /// Calls each scenarioTest function to confirm expected behavior.
        /// </summary>
        /// <param name="testName">The name of the test in the config file.</param>
        /// <param name="configuration">The configuration type to build.</param>
        private void InvokeScenarioTests(string testName, string configuration)
        {
            scenarioTest.RunTest(testName, configuration);
            scenarioTest.InitializeLogParsing();
            scenarioTest.ConfirmExpectedBuildResult();
            scenarioTest.HasCorrectNumberOfWarnings();
            scenarioTest.HasCorrectNumberOfErrors();
            scenarioTest.IsCorrectFrameworkVersion();
            scenarioTest.VerifyBuildConfig(configuration);
            scenarioTest.VerifyExpectedBuildTarget();
            scenarioTest.ConfirmExpectedFilesExist();
            scenarioTest.ConfirmUnexpectedFilesDontExist();
            scenarioTest.CheckResources();
            scenarioTest.CheckLinkedResources();
            scenarioTest.CheckAssemblyReferences();
        }
    }
}
