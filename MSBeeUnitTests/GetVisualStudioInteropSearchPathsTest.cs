// Copyright (C) Microsoft Corporation. All rights reserved.

using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Collections.Generic;
using Microsoft.Build.Extras;
using Microsoft.Build.Extras.FX1_1;
using Microsoft.Win32;

[assembly: SecurityPermission(SecurityAction.RequestMinimum)]
[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum, ViewAndModify = "HKEY_LOCAL_MACHINE")]
namespace Microsoft.Build.Extras.FX1_1.UnitTests
{
    /// <summary>
    /// This is a test class for Microsoft.Build.Extras.FX1_1.GetVisualStudioPIASearchPaths and is intended
    /// to contain all Microsoft.Build.Extras.FX1_1.GetVisualStudioPIASearchPaths Unit Tests
    /// </summary>
    /// <remarks>
    /// Some of these unit tests use the MockEngine class. An explanation of the MockEngine class is in MockEngine.cs.
    /// </remarks>
    [TestFixture]
    public class GetVisualStudioInteropSearchPathsTest
    {
        /// <summary>
        /// Target registry key that task should retrieve values from
        /// </summary>
        private string targetKey = GetVisualStudioInteropSearchPaths.VisualStudioInteropSearchPathKey;

        /// <summary>
        /// This parameter should be set to true if a subkey for target key is created during the test
        /// </summary>
        private bool keyCreated;

        // Required for testing error messages generated from the MSBee.Strings resources.
        private static readonly ResourceManager MSBeeStrings = new ResourceManager("Microsoft.Build.Extras.FX1_1.Strings", Assembly.Load("MSBee"));
        private static readonly ResourceManager failureStrings = new ResourceManager("Microsoft.Build.Extras.FX1_1.UnitTests.TestFailureStrings", Assembly.GetExecutingAssembly());

        [TestFixtureSetUp]
        public void TestInitialize()
        {
            try
            {
                Registry.LocalMachine.OpenSubKey(targetKey);
            }
            catch (SecurityException)
            {
                Assert.Ignore(failureStrings.GetString("GetSearchPathIgnoredDueToPermissions", CultureInfo.CurrentUICulture));
            }
        }

        [TestFixtureTearDown]
        public void TestCleanup()
        {
            if (keyCreated)
            {
               Registry.LocalMachine.DeleteSubKey(targetKey);
            }
        }

        /// <summary>
        /// Checks if the registry key is present on the system, if not it is created.
        /// This method also retrieves all values in the registry key.
        /// </summary>
        /// <returns>List of values stored under the registry key (only toplevel)</returns>
        private List<string> CheckAndCreateRegistryKey()
        {
            List<string> values = new List<string>();
            RegistryKey key = Registry.LocalMachine.OpenSubKey(this.targetKey);
            if (key == null)
            {
                // Create the key if it is not present, key will be removed at the end of the test
                key = Registry.LocalMachine.CreateSubKey(this.targetKey);
                keyCreated = true;
                // String.Empty will cause the default value to be set
                key.SetValue(String.Empty, Environment.SystemDirectory);
            }
            foreach (string name in key.GetValueNames())
            {
                values.Add(key.GetValue(name).ToString());
            }
            return values;    
        }

        /// <summary>
        /// Test for Execute(); ensures that the task returns true when key exists in the system. Tries to creates the key
        /// if it is not present.
        /// Test is ignored if user does not have permission to access the key. 
        ///</summary>
        [Test]
        public void ExecuteWhenKeyPresent()
        {
            List<string> expectedValues = null;
            try
            {
                expectedValues = CheckAndCreateRegistryKey();
            }
            catch (SecurityException)
            {
                Assert.Ignore(failureStrings.GetString("GetSearchPathIgnoredDueToPermissions", CultureInfo.CurrentUICulture));
            }

            // Making sure the registry key exists where expected.
            MockEngine mEngine = new MockEngine(true);
            GetVisualStudioInteropSearchPaths target = new GetVisualStudioInteropSearchPaths();
            target.BuildEngine = mEngine;

            bool actual = target.Execute();
            int numValues = target.Values.Length;

            Assert.IsTrue(actual, failureStrings.GetString("TaskFailed", CultureInfo.CurrentUICulture), "GetVisualStudioPIASearchPaths");
            Assert.IsTrue(numValues == expectedValues.Count, failureStrings.GetString("RegistryKeyMissingValues", CultureInfo.CurrentUICulture), numValues);

            // Check to see if expected value is in the array returned from the task. Number of values can be 0 if the key was already
            // present on the system without a default value.
            if (numValues > 0)
            {
                foreach (string path in target.Values)
                {
                    if (path.Equals(expectedValues[0]))
                    {
                        return;
                    }
                }
                Assert.Fail(failureStrings.GetString("ExpectedRegistryValuesNotPresent", CultureInfo.CurrentUICulture), expectedValues[0]);
            }
        }

        /// <summary>
        /// Test for Execute(); ensures that the task returns true when key does not exists in the system but it returns no paths.
        /// Test is ignored if key is present or user does not have permission to access the key.
        ///</summary>
        [Test]
        public void ExecuteIfKeyNotPresent()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(this.targetKey);
                if (key != null)
                {
                    Assert.Ignore(failureStrings.GetString("GetSearchPathIgnoredKeyPresent", CultureInfo.CurrentUICulture));
                }
            }
            catch (SecurityException)
            {
                Assert.Ignore(failureStrings.GetString("GetSearchPathIgnoredDueToPermissions", CultureInfo.CurrentUICulture));
            }
            // Making sure the registry key exists where expected.
            MockEngine mEngine = new MockEngine(true);
            GetVisualStudioInteropSearchPaths target = new GetVisualStudioInteropSearchPaths();
            target.BuildEngine = mEngine;

            bool actual = target.Execute();
            int numValues = target.Values.Length;

            Assert.IsTrue(actual, failureStrings.GetString("TaskFailed", CultureInfo.CurrentUICulture), "GetVisualStudioPIASearchPaths");
            Assert.IsTrue(numValues == 0, failureStrings.GetString("RegistryKeyMissingValues", CultureInfo.CurrentUICulture), numValues);
            Assert.IsTrue(mEngine.LogContains(
                String.Format(CultureInfo.CurrentUICulture, MSBeeStrings.GetString("RegistryKeyNotFound"), 
                Path.Combine(Registry.LocalMachine.ToString(), this.targetKey))));
        }

         

    }
}