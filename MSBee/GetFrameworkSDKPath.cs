// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Shared;
using Microsoft.Build.Utilities;

[assembly: SecurityPermission(SecurityAction.RequestMinimum)]
namespace Microsoft.Build.Extras.FX1_1
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased")]
    public class GetFrameworkSDKPath : Task
    {
        public GetFrameworkSDKPath()
        {
            // Create a resource manager.
            TaskResources = new ResourceManager("Microsoft.Build.Extras.FX1_1.Strings", Assembly.GetExecutingAssembly());
        }

        // Path to the .NET Framework SDK assemblies.
        private string path;

        #region Properties

        /// <summary>
        /// Contains the path to the .NET Framework 1.1 SDK assemblies.
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
        /// Obtains the path to the .NET Framework 1.1 SDK assemblies.
        /// </summary>
        /// <returns>True if the path is found; false otherwise.</returns>
        public override bool Execute()
        {
            path = ToolLocationHelper.GetPathToDotNetFrameworkSdk(TargetDotNetFrameworkVersion.Version11);

            if (String.IsNullOrEmpty(path))
            {
                Log.LogErrorFromResources("NETFrameworkSDKNotFound");
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