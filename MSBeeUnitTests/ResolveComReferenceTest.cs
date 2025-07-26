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
using Microsoft.Build.Extras.FX1_1;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

[assembly: SecurityPermission(SecurityAction.RequestMinimum)]
namespace Microsoft.Build.Extras.FX1_1.UnitTests
{
    /// <summary>
    /// This is a test class for Microsoft.Build.Extras.FX1_1.ResolveComReference and is intended
    /// to contain all Microsoft.Build.Extras.FX1_1.ResolveComReference Unit Tests
    /// </summary>
    [TestFixture]
    public class ResolveComReferenceTest
    {
        private static readonly ResourceManager MSBeeStrings = new ResourceManager("Microsoft.Build.Extras.FX1_1.Strings", Assembly.Load("MSBee"));
        private static readonly ResourceManager failureStrings = new ResourceManager("Microsoft.Build.Extras.FX1_1.UnitTests.TestFailureStrings", Assembly.GetExecutingAssembly());

        // The delimeter used to seperate metadata in TaskItem strings.
        private const string metadataDelimiter = "|~!&";


        /// <summary>
        /// Test for ReadOutputFile; confirms it fails if an unexpected switch is provided in the output file. 
        /// Also checks that the proper message appears in the log.
        /// </summary>
        [Test]
        public void ReadOutputFileWithBadFile1()
        {
            MockEngine mEngine = new MockEngine(true);
            ResolveComReferenceAccessor accessor = new ResolveComReferenceAccessor();
            accessor.BuildEngine = mEngine;

            // Read a bad output file.
            string filePath = Path.Combine(GetTestProjectDirectory(), Path.Combine("UnitTestFiles", "BadOutputFile1.txt"));
            accessor.ReadOutputFile(filePath);

            // Confirm the failureMessage was written to the log.
            string unexpectedSwitch = "resolvedFle";
            string failureMessage = String.Format(CultureInfo.CurrentUICulture, MSBeeStrings.GetString("OutputFileHasUnexpectedSwitch", CultureInfo.CurrentUICulture), unexpectedSwitch);
            Assert.IsTrue(mEngine.LogContains(failureMessage));
        }

        /// <summary>
        /// Test for ReadOutputFile; confirms it works for a sample output file.
        /// </summary>
        [Test]
        public void ReadOutputFileWithRealFile()
        {
            MockEngine mEngine = new MockEngine(true);
            ResolveComReferenceAccessor accessor = new ResolveComReferenceAccessor();
            accessor.BuildEngine = mEngine;

            string filePath = Path.Combine(GetTestProjectDirectory(), Path.Combine("UnitTestFiles", "GoodOutputFile.txt"));

            Console.WriteLine("filePath = " + filePath);

            // Read a good output file.
            accessor.ReadOutputFile(filePath);

            // Confirm that the generated TaskItem's metadata matches the contents of the output file.
            foreach (TaskItem resolvedFile in accessor.ResolvedFiles)
            {
                Console.WriteLine("resolvedFile.ItemSpec = " + resolvedFile.ItemSpec);
                switch (resolvedFile.ItemSpec)
                {
                    case "obj\\FX1_1\\Release\\Interop.SHDocVw.dll":
                        StringAssert.AreEqualIgnoringCase(resolvedFile.GetMetadata("Guid"), "{EAB22AC0-30C1-11CF-A7EB-0000C05BAE0B}");
                        StringAssert.AreEqualIgnoringCase(resolvedFile.GetMetadata("FusionName"), "Interop.SHDocVw, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null");
                        StringAssert.AreEqualIgnoringCase(resolvedFile.GetMetadata("WrapperTool"), "tlbimp");
                        break;

                    case "obj\\FX1_1\\Release\\AxInterop.SHDocVw.dll":
                        StringAssert.AreEqualIgnoringCase(resolvedFile.GetMetadata("Guid"), "{EAB22AC0-30C1-11CF-A7EB-0000C05BAE0B}");
                        StringAssert.AreEqualIgnoringCase(resolvedFile.GetMetadata("FusionName"), "AxInterop.SHDocVw, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null");
                        StringAssert.AreEqualIgnoringCase(resolvedFile.GetMetadata("WrapperTool"), "aximp");
                        break;

                    default:
                        Assert.Fail(failureStrings.GetString("UnexpectedResolvedFileEntry", CultureInfo.CurrentUICulture), resolvedFile.ItemSpec);
                        break;
                }
            }
        }
        
        /// <summary>
        /// Test for ConvertItemMetadataToString; confirms it works for sample data.
        /// </summary>
        [Test]
        public void ConvertItemMetadataToStringWithGoodValues()
        {
            TaskItem ti;
            string[] metadata = { "PM Names",
                                    "Name1", "Josh",
                                    "Name2", "Sara",
                                    "Name3", "Joe" };

            Assert.IsTrue(metadata.Length % 2 == 1, failureStrings.GetString("MetadataLengthIsEven", CultureInfo.CurrentUICulture));
                
            // Build the TaskItem that will be converted to a string.
            ti = new TaskItem(metadata[0]);
            for (int i = 1; i < metadata.Length; i += 2)
            {
                ti.SetMetadata(metadata[i], metadata[i + 1]);
            }

            // Get the actual string that represents the TaskItem.
            string actual = ResolveComReferenceAccessor.ConvertItemMetadataToString(ti);

            // For each metadata name, check that it's name/value pair is contained within the TaskItem string.
            for (int j = 1; j < metadata.Length; j += 2)
            {
                string metadataPair = metadata[j] + metadataDelimiter + metadata[j + 1];
                Assert.IsTrue(actual.Contains(metadataPair), failureStrings.GetString("TaskItemStringIsMissing", CultureInfo.CurrentUICulture), 
                    metadataPair);
            }
        }

        /// <summary>
        /// Test for ConvertItemMetadataToString; confirms that the produced string does not contain null metadata.
        /// </summary>
        [Test]
        public void ConvertItemMetadataToStringWithNullValues()
        {
            // Build the TaskItem that will be converted to a string.
            TaskItem ti = new TaskItem("NullNames");
            ti.SetMetadata("NullName", "");
            ti.SetMetadata("RealName", "Llun");

            // Get the actual string that represents the TaskItem.
            string actual = ResolveComReferenceAccessor.ConvertItemMetadataToString(ti);

            // Confirm the non-empty value is present in the string.
            string metadataPair = "RealName" + metadataDelimiter + ti.GetMetadata("RealName");
            Assert.IsTrue(actual.Contains(metadataPair), failureStrings.GetString("TaskItemStringIsMissing", CultureInfo.CurrentUICulture), 
                metadataPair);

            // Confirm the empty value is not present.
            metadataPair = "NullName" + metadataDelimiter + ti.GetMetadata("NullName");
            Assert.IsFalse(actual.Contains(metadataPair), failureStrings.GetString("TaskItemStringContainsEmptyValue", CultureInfo.CurrentUICulture), 
                metadataPair);
        }

        /// <summary>
        /// Test for ConvertItemMetadataToString; confirms a null TaskItem will return an empty string.
        /// </summary>
        [Test]
        public void ConvertNullItemToString()
        {
            // Get the actual string that represents the TaskItem.
            string expected = "";
            string actual = ResolveComReferenceAccessor.ConvertItemMetadataToString(null);
            StringAssert.AreEqualIgnoringCase(expected, actual, failureStrings.GetString("ConvertMetadataToStringReturnedNonEmptyString", CultureInfo.CurrentUICulture));
        }

        /// <summary>
        /// Test for ConvertItemMetadataToString; confirms that the produced string is null if there is a missing metadata value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2241:ProvideCorrectArgumentsToFormattingMethods")]
        [Test]
        public void ConvertStringsToItemMetadataWithBadArray()
        {
            MockEngine mEngine = new MockEngine(true);
            ResolveComReferenceAccessor accessor = new ResolveComReferenceAccessor();
            accessor.BuildEngine = mEngine;

            // ConvertStringsToItemMetadata expects the string array to start with the TaskItem's name
            // and then contain pairs of strings (a metadata name and a value).
            string[] values = new string[] { "Name", "FirstMetadataName" };

            // Confirm ConvertStringsToItemMetadata returns null when part of a metadata pair is missing.
            Assert.IsNull(accessor.ConvertStringsToItemMetadata(values), failureStrings.GetString("SetItemMetadataReturnedNonNull", CultureInfo.CurrentUICulture));

            // Confirm the failureMessage was written to the log.
            string failureMessage = String.Format(CultureInfo.CurrentUICulture, MSBeeStrings.GetString("IncorrectNumberOfMetadata", CultureInfo.CurrentUICulture), values.Length);
            Assert.IsTrue(mEngine.LogContains(failureMessage));
        }

        /// <summary>
        /// Test for ConvertStringsToItemMetadata; confirms it fails if a null array is provided as metadata.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2241:ProvideCorrectArgumentsToFormattingMethods")]
        [Test]
        public void ConvertStringsToItemMetadataWithNullArray()
        {
            MockEngine mEngine = new MockEngine(true);
            ResolveComReferenceAccessor accessor = new ResolveComReferenceAccessor();
            accessor.BuildEngine = mEngine;

            // Pass a null array to ConvertStringsToItemMetadata; confirm the method returns null.
            Assert.IsNull(accessor.ConvertStringsToItemMetadata(null), failureStrings.GetString("SetItemMetadataReturnedNonNull", CultureInfo.CurrentUICulture));

            // Confirm the failureMessage was written to the log.
            string failureMessage = String.Format(CultureInfo.CurrentUICulture, MSBeeStrings.GetString("NullParameter", CultureInfo.CurrentUICulture), "values");
            Assert.IsTrue(mEngine.LogContains(failureMessage));
        }

        /// <summary>
        /// Test for ConvertStringsToItemMetadata; confirms it succeeds for sample metadata.
        /// </summary>
        [Test]
        public void ConvertStringsToItemMetadata()
        {
            MockEngine mEngine = new MockEngine(true);
            ResolveComReferenceAccessor accessor = new ResolveComReferenceAccessor();
            accessor.BuildEngine = mEngine;

            // Create a string[] of metadata for a TaskItem
            string[] metadata = { "PM Names",
                                    "Name1", "Josh",
                                    "Name2", "Sara",
                                    "Name3", "Joe" };

            // Produce a TaskItem that should contain the above metadata
            TaskItem actual = accessor.ConvertStringsToItemMetadata(metadata);

            // Confirm the ItemSpec is correct.
            Assert.AreEqual(metadata[0], actual.ItemSpec, 
                failureStrings.GetString("ItemSpecValuesArentEqual", CultureInfo.CurrentUICulture),
                metadata[0], actual.ItemSpec);

            // Confirm the array's metadata values match the values in the produced TaskItem
            for (int i = 1; i < metadata.Length; i += 2)
            {
                Assert.AreEqual(metadata[i + 1], actual.GetMetadata(metadata[i]),
                    failureStrings.GetString("MetadataValuesArentEqual", CultureInfo.CurrentUICulture),
                    metadata[i], metadata[i + 1], actual.GetMetadata(metadata[i]));
            }
        }

        /// <summary>
        /// Derives an absolute path to the project directory.
        /// </summary>
        /// <returns>A path to the project directory.</returns>
        /// <remarks>
        /// Originally, I relied on the EnvDTE assembly to obtain the absolute path to the solution file
        /// using System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.8.0"). 
        /// Of course, this won't work when attempting to build this project from the command line
        /// when the IDE isn't running. 
        /// For NUnit, the current directory will be where the test DLL resides. Thus, we can simply
        /// go up two directories to be in the test project directory. This method assumes the 
        /// relative directory hierarchies will not change.
        /// </remarks>
        private static string GetTestProjectDirectory()
        {
            return new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        }
    }
}