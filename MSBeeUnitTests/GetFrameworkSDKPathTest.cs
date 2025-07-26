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
using Microsoft.Build.Extras.FX1_1;
using Microsoft.Build.Utilities;

[assembly: SecurityPermission(SecurityAction.RequestMinimum)]
namespace Microsoft.Build.Extras.FX1_1.UnitTests
{
    /// <summary>
    /// This is a test class for Microsoft.Build.Extras.FX1_1.GetFrameworkSDKPath and is intended
    /// to contain all Microsoft.Build.Extras.FX1_1.GetFrameworkSDKPath Unit Tests
    /// </summary>
    [TestFixture]
    public class GetFrameworkSdkPathTest
    {
        private static readonly ResourceManager failureStrings = new ResourceManager("Microsoft.Build.Extras.FX1_1.UnitTests.TestFailureStrings", Assembly.GetExecutingAssembly());


        /// <summary>
        /// Tests the GetFrameworkSdkPath task by comparing the value it returns to the expected framework SDK path.
        /// </summary>
        [Test]
        public void ObtainFrameworkSdkPath()
        {
            GetFrameworkSDKPath target = new GetFrameworkSDKPath();
            MockEngine mEngine = new MockEngine(true);
            target.BuildEngine = mEngine;

            string expected = ToolLocationHelper.GetPathToDotNetFrameworkSdk(TargetDotNetFrameworkVersion.Version11);
            if (!expected.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                expected += Path.DirectorySeparatorChar;
            }

            // Confirm the task succeeded.
            bool success = target.Execute();
            Assert.IsTrue(success, failureStrings.GetString("TaskFailed", CultureInfo.CurrentUICulture), "GetFrameworkSDKPath");

            // Make sure the path ends with a separator character.
            Assert.IsTrue(target.Path.EndsWith(Path.DirectorySeparatorChar.ToString()),
                failureStrings.GetString("PathDoesntEndWithSeparator", CultureInfo.CurrentUICulture),
                target.Path);

            // Compare the expected path to the actual path.
            string actual = target.Path;
            Assert.AreEqual(expected, actual,
                failureStrings.GetString("WrongFrameworkSDKPath", CultureInfo.CurrentUICulture),
                expected, actual);
        }
    }

}
