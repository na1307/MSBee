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
    /// Implements functionality to get all values underneath a provided registry key
    /// under HKEY_LOCAL_MACHINE hive. 
    /// </summary>
    public class GetLocalMachineRegistryValues : Task
    {

        /// <summary>
        /// Used to initalize root registry key.
        /// </summary>
        /// <param name="registryKey">Registry key to search for directories</param>
        protected GetLocalMachineRegistryValues(string registryKey)
        {
            TaskResources = new ResourceManager("Microsoft.Build.Extras.FX1_1.Strings", Assembly.GetExecutingAssembly());
            this.registryKey = registryKey;
            this.registryValues = new List<string>();
            try
            {
                this.rootKey = this.baseRegistryKey.OpenSubKey(registryKey);
            }
            catch (SecurityException ex)
            {
                Log.LogErrorFromException(ex, true);
            }
        }

        #region Fields

        // The registry key/path.
        private string registryKey;

        // The base registry key; the default is HKEY_LOCAL_MACHINE.
        private RegistryKey baseRegistryKey = Registry.LocalMachine;

        // List of values found underneath the provided registry key.
        private List<string> registryValues;

        /// <summary>
        /// RegistryKey object for the provided registry key name
        /// </summary>
        private RegistryKey rootKey;

        #endregion

        #region Properties

        /// <summary>
        /// The list of values underneath the provided registry key/path.
        /// </summary>
        [Output]
        public string[] Values
        {
            get
            {
                return registryValues.ToArray();
            }
        }

        /// <summary>
        /// Returns RegistryKey object for the registry key provided in the constructor.
        /// </summary>
        /// <remarks>
        /// Used by GetVisualStudioSearchPaths to under subkeys stored in this key as well.
        /// </remarks>
        protected RegistryKey RootKey
        {
            get
            {
                return this.rootKey;
            }
        }
        #endregion

        /// <summary>
        /// Searches underneath the provided registry key/path for all values, adding them to a list.
        /// This method does not search subkeys stored under baseKey. It only stays at the initial level.
        /// </summary>
        /// <remarks>
        /// If any exceptions are thrown, they are caught in Execute().
        /// </remarks>
        /// <param name="baseKey">The key provided by the user.</param>
        protected void AddValuesToRegistryValuesList(RegistryKey baseKey)
        {
            string[] values = baseKey.GetValueNames();
            foreach (string value in values)
            {
                registryValues.Add(baseKey.GetValue(value).ToString());
            }
        }

        /// <summary>
        /// Primary function for this task. 
        /// </summary>
        /// <remarks>
        /// Invokes GetValues() to obtain the values underneath the provided registry key/path. 
        /// Any exceptions thrown from GetValues() are caught here and logged; 
        /// the function then returns false to indicate failure.
        /// </remarks>
        /// <returns>True if successful, false otherwise.</returns>
        public override bool Execute()
        {
            // If an exception was raised in constructor, return immediately.
            if (Log.HasLoggedErrors)
            {
                return true;
            }

            try
            {
                RegistryKey key = this.RootKey;

                if (key != null)
                {
                    AddValuesToRegistryValuesList(key);

                    // Return !Log.HasLoggedErrors so if errors are logged in a method but false isn't returned,
                    // we'll still return false here.
                    return !Log.HasLoggedErrors;
                }
                else
                {
                    string registryKeyPath = this.baseRegistryKey.ToString() + Path.DirectorySeparatorChar + registryKey;
                    Log.LogMessageFromResources(MessageImportance.Normal, "RegistryKeyNotFound", registryKeyPath);
                    return !Log.HasLoggedErrors;
                }
            }
            // If an exception was thrown, log the exception and return failure.
            catch (SecurityException ex)
            {
                Log.LogErrorFromException(ex, true);
                return false;
            }
        }
    }
}