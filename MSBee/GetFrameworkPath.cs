// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Security.Permissions;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Shared;
using Microsoft.Build.Utilities;

[assembly: SecurityPermission(SecurityAction.RequestMinimum)]
namespace Microsoft.Build.Extras.FX1_1
{
    public class GetFrameworkPath : Task
    {
        public GetFrameworkPath()
        {
            // Create a resource manager.
            TaskResources = new ResourceManager("Microsoft.Build.Extras.FX1_1.Strings", Assembly.GetExecutingAssembly());
        }

        // Path to the .NET Framework assemblies.
        private string path;

        #region Properties

        /// <summary>
        /// Contains the path to the .NET Framework 1.1 assemblies.
        /// </summary>
        [Output]
        public string Path
        {
            get
            {
                return path;
            }

            set
            {
                path = value;
            }
        }

        #endregion

        /// <summary>
        /// Obtains the path to the .NET Framework 1.1 assemblies.
        /// </summary>
        /// <returns>True if the path is found; false otherwise.</returns>
        public override bool Execute()
        {
            path = ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version11);

            if (String.IsNullOrEmpty(path))
            {
                Log.LogErrorFromResources("NETFrameworkNotFound");
                path = null;
            }
            else
            {
                // If the path doesn't end with a directory separator, add one.
                if (!path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()) &&
                    !path.EndsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
                {
                    path += System.IO.Path.DirectorySeparatorChar;
                }
            }

            return !Log.HasLoggedErrors;
        }
    }
}
