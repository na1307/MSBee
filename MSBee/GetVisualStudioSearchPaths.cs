// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Shared;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

[assembly: SecurityPermission(SecurityAction.RequestMinimum)]
namespace Microsoft.Build.Extras.FX1_1
{
    /// <summary>
    /// Implements the "GetVisualStudioSearchPaths" task, which returns search paths for referenced assemblies
    /// </summary>
    public class GetVisualStudioSearchPaths : GetLocalMachineRegistryValues
    {

        /// <summary>
        /// Registry key to search for reference locations. This key is installed by Visual Studio 2003.
        /// </summary>
        /// <remarks>This value should not be changed</remarks>
        public const string VisualStudioSearchPathKey = @"Software\Microsoft\VisualStudio\7.1\AssemblyFolders";

        public GetVisualStudioSearchPaths()
            : base(VisualStudioSearchPathKey)
        {
            TaskResources = new ResourceManager("Microsoft.Build.Extras.FX1_1.Strings", Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Override base Execute method to add functionality to search subkeys under the root key as well.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        /// <remarks>
        /// We still call base.Execute() method because it is used to retrieve values from the top level.
        /// Remaining of the method only retrieves values from the sub keys.
        /// </remarks>
        /// <see cref="GetSearchPathBase.Execute"/>
        public override bool Execute()
        {
            if (base.Execute()) 
            {
                try
                {
                    RegistryKey key = base.RootKey;
                    if (key != null)
                    {
                        // Now, process the next level of subkeys; we already know that SubKeyCount is greater than 0.
                        string[] subkeys = key.GetSubKeyNames();
                        foreach (string subkey in subkeys)
                        {
                            base.AddValuesToRegistryValuesList(key.OpenSubKey(subkey));
                        }
                    }
                    return !Log.HasLoggedErrors;
                }
                // If an exception was thrown, log the exception and return failure.
                catch (SecurityException ex)
                {
                    Log.LogErrorFromException(ex, true);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
}