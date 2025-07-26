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
using Microsoft.Build.Utilities;
using Microsoft.Build.Extras;
using Microsoft.Build.Extras.FX1_1;

[assembly: CLSCompliant(true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum)]
namespace Microsoft.Build.Extras.FX1_1.UnitTests
{
    /// <summary>
    /// This is a test class for Microsoft.Build.Extras.FX1_1.GenerateResource and is intended
    /// to contain all Microsoft.Build.Extras.FX1_1.GenerateResource Unit Tests
    /// </summary>
    /// <remarks>
    /// Some of these unit tests use the MockEngine class. An explanation of the MockEngine class is in MockEngine.cs.
    /// </remarks>
    [TestFixture]
    public class GenerateResourceTest
    {
        private static readonly ResourceManager MSBeeStrings = new ResourceManager("Microsoft.Build.Extras.FX1_1.Strings", Assembly.Load("MSBee"));
        private static readonly ResourceManager failureStrings = new ResourceManager("Microsoft.Build.Extras.FX1_1.UnitTests.TestFailureStrings", Assembly.GetExecutingAssembly());


        /// <summary>
        /// Test for CreateOutputResourcesNames; ensures that outputResources doesn't change
        /// when the lengths of sources and outputResources are the same.
        /// </summary>
        [Test]
        public void CompareSourcesAndOutputWithSameLength()
        {
            int arrayLength = 2;
            TaskItem[] sources = new TaskItem[arrayLength];
            TaskItem[] outputResources = new TaskItem[sources.Length];
            string[] expectedOutput = new string[sources.Length];

            for (int i = 0; i < arrayLength; i++)
            {
                string name = "res" + i;
                sources[i] = new TaskItem(Path.ChangeExtension(name, ".resx"));
                outputResources[i] = new TaskItem(Path.ChangeExtension(name, ".resources"));
                expectedOutput[i] = outputResources[i].ItemSpec;
            }

            GenerateResourceAccessor accessor = new GenerateResourceAccessor();
            MockEngine mEngine = new MockEngine(true);
            accessor.BuildEngine = mEngine;
            accessor.Sources = sources;
            accessor.OutputResources = outputResources;

            bool actual = accessor.CreateOutputResourcesNames();
            Assert.AreEqual(true, actual, failureStrings.GetString("CreateOutputResourceNamesReturnedFalse", CultureInfo.CurrentUICulture));

            Assert.AreEqual(accessor.OutputResources.Length, accessor.OutputResources.Length,
                failureStrings.GetString("OutputResourcesHasUnexpectedLength", CultureInfo.CurrentUICulture),
                accessor.OutputResources.Length, accessor.OutputResources.Length);

            for (int j = 0; j < arrayLength; j++)
            {
                Assert.IsTrue(expectedOutput[j].Equals(accessor.OutputResources[j].ItemSpec, StringComparison.InvariantCultureIgnoreCase),
                    failureStrings.GetString("WrongResourceNameWasGenerated", CultureInfo.CurrentUICulture),
                    accessor.Sources[j].ItemSpec, expectedOutput[j], accessor.OutputResources[j].ItemSpec);
            }
        }

        /// <summary>
        /// Test for CreateOutputResourcesNames; ensures that outputResources is changed when 
        /// its length is different that sources. Particularly, the resource names in outputResources
        /// should be the same as those in sources.
        /// </summary>
        [Test]
        public void CompareSourcesAndOutputWithDiffLengths()
        {
            int arrayLength = 2;
            TaskItem[] sources = new TaskItem[arrayLength];
            TaskItem[] outputResources = new TaskItem[sources.Length + 1];
            string[] expectedOutput = new string[sources.Length];

            for (int i = 0; i < arrayLength; i++)
            {
                string name = "res" + i;
                sources[i] = new TaskItem(Path.ChangeExtension(name, ".resx"));
                outputResources[i] = new TaskItem(Path.ChangeExtension(name, ".resources"));
                expectedOutput[i] = outputResources[i].ItemSpec;
            }

            GenerateResourceAccessor accessor = new GenerateResourceAccessor();
            MockEngine mEngine = new MockEngine(true);
            accessor.BuildEngine = mEngine;
            accessor.Sources = sources;
            accessor.OutputResources = outputResources;

            bool actual = accessor.CreateOutputResourcesNames();
            Assert.AreEqual(true, actual, failureStrings.GetString("CreateOutputResourceNamesReturnedFalse", CultureInfo.CurrentUICulture));

            Assert.AreEqual(accessor.Sources.Length, accessor.OutputResources.Length,
                failureStrings.GetString("OutputResourcesHasUnexpectedLength", CultureInfo.CurrentUICulture),
                accessor.Sources.Length, accessor.OutputResources.Length);

            for (int j = 0; j < arrayLength; j++)
            {
                Assert.IsTrue(expectedOutput[j].Equals(accessor.OutputResources[j].ItemSpec, StringComparison.InvariantCultureIgnoreCase),
                    failureStrings.GetString("WrongResourceNameWasGenerated", CultureInfo.CurrentUICulture),
                    accessor.Sources[j].ItemSpec, expectedOutput[j], accessor.OutputResources[j].ItemSpec);
            }

            Assert.IsTrue(mEngine.LogContains(MSBeeStrings.GetString("DefaultOutputResources")));
        }

        /// <summary>
        /// Test for CreateOutputResourcesNames; ensures that outputResources is populated
        /// with expected values when outputResources is initialized to null.
        /// </summary>
        [Test]
        public void CreateResourceNamesWithOutputInitializedAsNull()
        {
            int arrayLength = 4;
            TaskItem[] sources = new TaskItem[arrayLength];
            string[] expectedOutput = new string[sources.Length];

            for (int i = 0; i < arrayLength; i++)
            {
                string name = "res" + i;
                sources[i] = new TaskItem(Path.ChangeExtension(name, ".resx"));
                expectedOutput[i] = Path.ChangeExtension(sources[i].ItemSpec, ".resources");
            }

            GenerateResourceAccessor accessor = new GenerateResourceAccessor(); 
            MockEngine mEngine = new MockEngine(true);
            accessor.BuildEngine = mEngine;
            accessor.Sources = sources;

            bool actual = accessor.CreateOutputResourcesNames();
            Assert.AreEqual(true, actual, failureStrings.GetString("CreateOutputResourceNamesReturnedFalse", CultureInfo.CurrentUICulture));

            Assert.AreEqual(accessor.Sources.Length, accessor.OutputResources.Length,
                failureStrings.GetString("OutputResourcesHasUnexpectedLength", CultureInfo.CurrentUICulture),
                accessor.Sources.Length, accessor.OutputResources.Length);

            for (int j = 0; j < arrayLength; j++)
            {
                Assert.IsTrue(expectedOutput[j].Equals(accessor.OutputResources[j].ItemSpec, StringComparison.InvariantCultureIgnoreCase),
                    failureStrings.GetString("WrongResourceNameWasGenerated", CultureInfo.CurrentUICulture),
                    accessor.Sources[j].ItemSpec, expectedOutput[j], accessor.OutputResources[j].ItemSpec);
            }
        }

        /// <summary>
        /// Test for ToolName(); ensures that "resgen.exe" is returned.
        /// </summary>
        [Test]
        public void ToolNameTest()
        {
            string val = "Resgen.exe";

            GenerateResourceAccessor accessor = new GenerateResourceAccessor();

            Assert.IsTrue(val.Equals(accessor.ToolName, StringComparison.InvariantCultureIgnoreCase),
                failureStrings.GetString("ToolNameIsIncorrect", CultureInfo.CurrentUICulture),
                accessor.ToolName, val);
        }

        /// <summary>
        /// Test for CopyReferences; ensures it returns true when no references are provided.
        /// </summary>
        [Test]
        public void CopyReferencesWithNoReferences()
        {
            GenerateResourceAccessor accessor = new GenerateResourceAccessor();
            MockEngine mEngine = new MockEngine(true);
            accessor.BuildEngine = mEngine;

            Assert.IsTrue(accessor.CopyReferences(), failureStrings.GetString("CopyReferencesReturnedFalseWithNoRefs", CultureInfo.CurrentUICulture));
        }

        /// <summary>
        /// Test for DeleteTempResGenPath() and CreateUniqueTempDirectory(); ensures DeleteTempResGenPath() 
        /// returns true when the temp subdirectory was created but nothing was copied into it.
        /// </summary>
        [Test]
        public void CopyAndDeleteWithNoReferences()
        {
            GenerateResourceAccessor accessor = new GenerateResourceAccessor();
            MockEngine mEngine = new MockEngine(true);
            accessor.BuildEngine = mEngine;

            // Confirm that CreateUniqueTempDirectory() returns true.
            Console.WriteLine("Creating temp directory.");
            accessor.tempResGenPath = accessor.CreateUniqueTempDirectory();
            Assert.IsTrue(!String.IsNullOrEmpty(accessor.tempResGenPath), failureStrings.GetString("CreateUniqueTempDirectoryFailed", CultureInfo.CurrentUICulture));

            // Confirm the created temp directory exists.
            Assert.IsTrue(Directory.Exists(accessor.tempResGenPath), failureStrings.GetString("TempDirectoryNotCreated", CultureInfo.CurrentUICulture));

            // Confirm that DeleteTempResGenPath() returned true.
            Console.WriteLine("Deleting ResGen temp directory.");
            Assert.IsTrue(accessor.DeleteTempResGenPath(), failureStrings.GetString("DeleteTempResGenPathFailed", CultureInfo.CurrentUICulture));

            // Confirm that the temp ResGen directory was deleted.
            Console.WriteLine("Confirming that " + accessor.tempResGenPath + " was deleted.");
            Assert.IsTrue(!Directory.Exists(accessor.tempResGenPath),
                failureStrings.GetString("TempResGenDirectoryStillExists", CultureInfo.CurrentUICulture),
                accessor.tempResGenPath);
        }

        /// <summary>
        /// Test for CopyReferences(), CopyResgen(), and DeleteTempResGenPath().
        /// </summary>
        /// <remarks>
        /// Since these copy and delete functions are tied together, they're included
        /// in a single test. Copy framework references and ResGen.exe to the 
        /// temp subdirectory and ensure they're present in that directory. Then 
        /// delete the subdirectory and ensure it's been deleted.
        /// </remarks>
        [Test]
        public void CopyAndDeleteWithReferences()
        {
            string pathToFramework = ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version11);

            GenerateResourceAccessor accessor = new GenerateResourceAccessor();
            MockEngine mEngine = new MockEngine(true);
            accessor.BuildEngine = mEngine;


            // CREATE TEMP DIRECTORY

            // Confirm that CreateUniqueTempDirectory() returns true.
            Console.WriteLine("Creating temp directory.");
            accessor.tempResGenPath = accessor.CreateUniqueTempDirectory();
            Assert.IsTrue(!String.IsNullOrEmpty(accessor.tempResGenPath), failureStrings.GetString("CreateUniqueTempDirectoryFailed", CultureInfo.CurrentUICulture));

            // Confirm the created temp directory exists.
            Assert.IsTrue(Directory.Exists(accessor.tempResGenPath), failureStrings.GetString("TempDirectoryNotCreated", CultureInfo.CurrentUICulture));


            // CREATE REFERENCES LIST

            // To test with additional references, add additional strings in the references array.
            string[] references = { "System.dll", "System.Xml.dll" };
            int numReferences = references.Length;

            TaskItem[] currentReferences = new TaskItem[numReferences];
            TaskItem[] copiedReferences = new TaskItem[numReferences];

            for (int a = 0; a < numReferences; a++)
            {
                currentReferences[a] = new TaskItem(Path.Combine(pathToFramework, references[a]));
                copiedReferences[a] = new TaskItem(Path.Combine(accessor.tempResGenPath, references[a]));
            }

            accessor.References = currentReferences;


            // COPY REFERENCES

            // Confirm that CopyReferences() returned true.
            Console.WriteLine("Copying references.");
            Assert.IsTrue(accessor.CopyReferences(), failureStrings.GetString("CopyReferencesFailed", CultureInfo.CurrentUICulture));

            // Confirm the files were copied.
            for (int i = 0; i < copiedReferences.Length; i++)
            {
                Console.WriteLine("Confirming that " + copiedReferences[i] + " exists.");
                Assert.IsTrue(File.Exists(copiedReferences[i].ItemSpec),
                    failureStrings.GetString("FileNotCopied", CultureInfo.CurrentUICulture),
                    currentReferences[i].ItemSpec, copiedReferences[i].ItemSpec);
            }


            // COPY RESGEN

            // Confirm that CopyResGen() returned true.
            Console.WriteLine("Copying Resgen.exe.");
            Assert.IsTrue(accessor.CopyResGen(), failureStrings.GetString("CopyResGenFailed", CultureInfo.CurrentUICulture));

            // Confirm that Resgen.exe was copied.
            Assert.IsTrue(File.Exists(Path.Combine(accessor.tempResGenPath, accessor.ToolName)),
                failureStrings.GetString("FileNotCopied", CultureInfo.CurrentUICulture),
                accessor.ToolName, accessor.tempResGenPath);


            // DELETE RESGEN TEMP DIRECTORY

            // Confirm that DeleteTempResGenPath() returned true.
            Console.WriteLine("Deleting ResGen temp directory.");
            Assert.IsTrue(accessor.DeleteTempResGenPath(), failureStrings.GetString("DeleteTempResGenPathFailed", CultureInfo.CurrentUICulture));

            // Confirm that the temp ResGen directory was deleted.
            Console.WriteLine("Confirming that " + accessor.tempResGenPath + " was deleted.");
            Assert.IsTrue(!Directory.Exists(accessor.tempResGenPath),
                failureStrings.GetString("TempResGenDirectoryStillExists", CultureInfo.CurrentUICulture),
                accessor.tempResGenPath);
        }

        /// <summary>
        /// Test for CopyReferences(), CopyResgen(), and DeleteTempResGenPath() using Read-Only references.
        /// </summary>
        /// <remarks>
        /// This is the same test as CopyAndDeleteWithReferences() except read-only references are included.
        /// This test checks for a bug where read-only files were being copied to the temp directory, 
        /// which stopped the temp directory from being deleted.
        /// </remarks>
        [Test]
        public void CopyAndDeleteWithReadOnlyReferences()
        {
            GenerateResourceAccessor accessor = new GenerateResourceAccessor();
            MockEngine mEngine = new MockEngine(true);
            accessor.BuildEngine = mEngine;


            // CREATE TEMP DIRECTORY

            // Confirm that CreateUniqueTempDirectory() returns true.
            Console.WriteLine("Creating temp directory.");
            accessor.tempResGenPath = accessor.CreateUniqueTempDirectory();
            Assert.IsTrue(!String.IsNullOrEmpty(accessor.tempResGenPath), failureStrings.GetString("CreateUniqueTempDirectoryFailed", CultureInfo.CurrentUICulture));

            // Confirm the created temp directory exists.
            Assert.IsTrue(Directory.Exists(accessor.tempResGenPath), failureStrings.GetString("TempDirectoryNotCreated", CultureInfo.CurrentUICulture));


            // CREATE REFERENCES LIST

            // We need a read-only reference for this test. Instead of taking a random DLL from somewhere,
            // we're going to leverage SimpleDLL.dll which is present in the UnitTestFiles directory.
            string reference = Path.Combine(GetTestProjectDirectory(), Path.Combine("UnitTestFiles", "SimpleDLL.dll"));
            int numReferences = 1;
            
            // Ensure the DLL is read-only by setting it to read-only.
            File.SetAttributes(reference, FileAttributes.ReadOnly);

            // Confirm the file has been set to read-only.
            Assert.IsTrue(new FileInfo(reference).IsReadOnly,
                failureStrings.GetString("NoReadOnlyReferences", CultureInfo.CurrentUICulture));
            
            TaskItem[] currentReferences = new TaskItem[numReferences];
            currentReferences[0] = new TaskItem(reference);

            TaskItem[] copiedReferences = new TaskItem[numReferences];
            copiedReferences[0] = new TaskItem(Path.Combine(accessor.tempResGenPath, Path.GetFileName(reference)));

            accessor.References = currentReferences;


            // COPY REFERENCES

            // Confirm that CopyReferences() returned true.
            Console.WriteLine("Copying references.");
            Assert.IsTrue(accessor.CopyReferences(), failureStrings.GetString("CopyReferencesFailed", CultureInfo.CurrentUICulture));

            // Confirm the files were copied.
            for (int i = 0; i < copiedReferences.Length; i++)
            {
                Console.WriteLine("Confirming that " + copiedReferences[i] + " exists.");
                Assert.IsTrue(File.Exists(copiedReferences[i].ItemSpec),
                    failureStrings.GetString("FileNotCopied", CultureInfo.CurrentUICulture),
                    currentReferences[i].ItemSpec, copiedReferences[i].ItemSpec);
            }


            // COPY RESGEN

            // Confirm that CopyResGen() returned true.
            Console.WriteLine("Copying Resgen.exe.");
            Assert.IsTrue(accessor.CopyResGen(), failureStrings.GetString("CopyResGenFailed", CultureInfo.CurrentUICulture));

            // Confirm that Resgen.exe was copied.
            Assert.IsTrue(File.Exists(Path.Combine(accessor.tempResGenPath, accessor.ToolName)),
                failureStrings.GetString("FileNotCopied", CultureInfo.CurrentUICulture),
                accessor.ToolName, accessor.tempResGenPath);


            // DELETE RESGEN TEMP DIRECTORY

            // Confirm that DeleteTempResGenPath() returned true.
            Console.WriteLine("Deleting ResGen temp directory.");
            Assert.IsTrue(accessor.DeleteTempResGenPath(), failureStrings.GetString("DeleteTempResGenPathFailed", CultureInfo.CurrentUICulture));

            // Confirm that the temp ResGen directory was deleted.
            Console.WriteLine("Confirming that " + accessor.tempResGenPath + " was deleted.");
            Assert.IsTrue(!Directory.Exists(accessor.tempResGenPath),
                failureStrings.GetString("TempResGenDirectoryStillExists", CultureInfo.CurrentUICulture),
                accessor.tempResGenPath);
        }

        /// <summary>
        /// Confirms the GenerateResource task works in a simple scenario.
        /// </summary>
        /// <remarks>
        /// This test confirms that Execute returns true and that the .resources
        /// file is where it's expected.
        /// 
        /// Assumptions for this test are:
        /// The relative structure of the MSBee projects hasn't been changed (no directories or files were relocated).
        /// The TestResults directory hierarchy remains consistent.
        /// </remarks>
        [Test]
        public void GenerateResourceFiles()
        {
            // Generate TaskItem lists of resx files and resources files.

            // Initialize the lists.
            int arrayLength = 1;
            TaskItem[] sources = new TaskItem[arrayLength];
            TaskItem[] outputResources = new TaskItem[arrayLength];

            string[] resxFilesPaths = { Path.Combine(GetTestProjectDirectory(), "TestFailureStrings.resx") };
            string[] resourcesFilesPaths = { Path.Combine(GetTestProjectDirectory(), "TestFailureStrings.resources") };

            // Add the resx and resources file names to the appropriate TaskItem arrays.
            for (int i = 0; i < arrayLength; i++)
            {
                sources[i] = new TaskItem(resxFilesPaths[i]);
                outputResources[i] = new TaskItem(resourcesFilesPaths[i]);
            }

            GenerateResourceAccessor accessor = new GenerateResourceAccessor();
            MockEngine mEngine = new MockEngine(true);
            accessor.BuildEngine = mEngine;

            // Confirm that all test resx files exist.
            for (int j = 0; j < arrayLength; j++)
            {
                string fileName = sources[j].ItemSpec;
                Console.WriteLine("Confirming that " + fileName + " exists.");
                Assert.IsTrue(File.Exists(fileName), failureStrings.GetString("FileDoesntExist", CultureInfo.CurrentUICulture),
                    fileName);
            }

            // Assign the array of resx files to the task's Sources array.
            accessor.Sources = sources;

            // Assign the array of expected .resources files to the task's OutputResources array.
            accessor.OutputResources = outputResources;

            // Execute the task and confirm it returns true.
            Assert.IsTrue(accessor.Execute(), failureStrings.GetString("GenerateResourceReturnedFalse", CultureInfo.CurrentUICulture));

            // Confirm the expected .resources files were generated.
            for (int k = 0; k < arrayLength; k++)
            {
                string outputFileName = outputResources[k].ItemSpec;
                Console.WriteLine("Confirming that " + outputFileName + " exists.");
                Assert.IsTrue(File.Exists(outputFileName), failureStrings.GetString("FileShouldExist", CultureInfo.CurrentUICulture),
                    outputFileName);
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