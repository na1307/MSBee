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
    /// This is a test class for Microsoft.Build.Extras.FX1_1.GetVisualStudioSearchPaths and is intended
    /// to contain all Microsoft.Build.Extras.FX1_1.GetVisualStudioSearchPaths Unit Tests
    /// </summary>
    /// <remarks>
    /// Some of these unit tests use the MockEngine class. An explanation of the MockEngine class is in MockEngine.cs.
    /// </remarks>
    [TestFixture]
    public class GetVisualStudioSearchPathsTest
    {

        /// <summary>
        /// Target registry key that task should retrieve values from
        /// </summary>
        private string targetKey = GetVisualStudioSearchPaths.VisualStudioSearchPathKey;

        /// <summary>
        /// If Target key does not already have any subkeys, a subkey named below will be created for testing purposes.
        /// </summary>
        private string targetSubKey = @"TestKey";

        /// <summary>
        /// This parameter should be set to true if a subkey for target key is created during the test
        /// </summary>
        private bool targetKeyCreated;

        /// <summary>
        /// This parameter should be set to true if a subkey in the target key is created during the test
        /// </summary>
        private bool targetSubKeyCreated;

        // Required for testing error messages generated from the MSBee.Strings resources.
        private static readonly ResourceManager MSBeeStrings = new ResourceManager("Microsoft.Build.Extras.FX1_1.Strings", Assembly.Load("MSBee"));
        private static readonly ResourceManager failureStrings = new ResourceManager("Microsoft.Build.Extras.FX1_1.UnitTests.TestFailureStrings", Assembly.GetExecutingAssembly());


        /// <summary>
        /// If user does not have permission to access to registry, test fixture will be ignored.
        /// </summary>
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
            if (targetKeyCreated)
            {
                Registry.LocalMachine.DeleteSubKeyTree(this.targetKey);
            }
            else if (targetSubKeyCreated)
            {
                Registry.LocalMachine.OpenSubKey(this.targetKey).DeleteSubKey(this.targetSubKey);
            }
        }

        /// <summary>
        /// Creates the test key structure on the system and generates a list
        /// values stored under those keys.
        /// </summary>
        /// <returns>List of values</returns>
        private List<string> CheckAndCreateTestKeys()
        {
            List<string> values = new List<string>();

            RegistryKey key = Registry.LocalMachine.OpenSubKey(targetKey);
            // If key is not present, create it to be removed later
            if (key == null)
            {
                key = Registry.LocalMachine.CreateSubKey(this.targetKey);
                this.targetKeyCreated = true;
                // String.Empty will cause the default value to be set
                key.SetValue(String.Empty, Environment.SystemDirectory);
            }
            foreach (string name in key.GetValueNames())
            {
                values.Add(key.GetValue(name).ToString());
            }
            string[] subKeyNames = key.GetSubKeyNames();
            // If there are no subkeys, create a new one to be removed later
            if (subKeyNames.Length == 0)
            {
                try
                {
                    RegistryKey subKey = key.CreateSubKey(this.targetSubKey);
                    // String.Empty will cause the default value to be set
                    subKey.SetValue(String.Empty, Environment.SystemDirectory);
                    this.targetSubKeyCreated = true;
                }
                catch (UnauthorizedAccessException)
                {
                    // Do not have permission to create sub key, testing without subkeys.
                }
            }
                foreach (string subKeyName in key.GetSubKeyNames())
                {
                    RegistryKey subKey = key.OpenSubKey(subKeyName);
                    foreach (string name in subKey.GetValueNames())
                    {
                        values.Add(subKey.GetValue(name).ToString());
                    }
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
                expectedValues = CheckAndCreateTestKeys();
            }
            catch (SecurityException)
            {
                Assert.Ignore(failureStrings.GetString("GetSearchPathIgnoredDueToPermissions", CultureInfo.CurrentUICulture));
            }

            MockEngine mEngine = new MockEngine(true);
            GetVisualStudioSearchPaths target = new GetVisualStudioSearchPaths();
            target.BuildEngine = mEngine;

            bool actual = target.Execute();
            int numValues = target.Values.Length;
            Assert.IsTrue(actual, failureStrings.GetString("TaskFailed", CultureInfo.CurrentUICulture), "GetVisualStudioSearchPaths");
            Assert.IsTrue(numValues == expectedValues.Count, failureStrings.GetString("RegistryKeyMissingValues", CultureInfo.CurrentUICulture), numValues);

            /// Check to see if expected value is in the array returned from the task. Number of values can be 0 if the key was already
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
            
            MockEngine mEngine = new MockEngine(true);
            GetVisualStudioSearchPaths target = new GetVisualStudioSearchPaths();
            target.BuildEngine = mEngine;

            bool actual = target.Execute();
            int numValues = target.Values.Length;

            Assert.IsTrue(actual, failureStrings.GetString("TaskFailed", CultureInfo.CurrentUICulture), "GetVisualStudioSearchPaths");
            Assert.IsTrue(numValues == 0, failureStrings.GetString("RegistryKeyMissingValues", CultureInfo.CurrentUICulture), numValues);
            Assert.IsTrue(mEngine.LogContains(
                String.Format(CultureInfo.CurrentUICulture, MSBeeStrings.GetString("RegistryKeyNotFound"),
                Path.Combine(Registry.LocalMachine.ToString(), this.targetKey))));
        }
    }
}