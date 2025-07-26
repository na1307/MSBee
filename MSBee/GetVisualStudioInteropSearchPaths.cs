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
    /// Implements the "GetVisualStudioPIASearchPaths" task, which returns directories
    /// that contains Primary Interop Assemblies
    /// </summary>
    public class GetVisualStudioInteropSearchPaths : GetLocalMachineRegistryValues
    {

        /// <summary>
        /// Registry key to search for Primary Interop assembly locations under .Net 2.0 sub-hive as
        /// they are shared by .NET 1.1 and .NET 2.0.
        /// </summary>
        /// <remarks>This value should not be changed</remarks>
        public const string VisualStudioInteropSearchPathKey = 
            @"Software\Microsoft\.NetFramework\v2.0.50727\AssemblyFoldersEx\Primary Interop Assemblies";

        public GetVisualStudioInteropSearchPaths()
            : base(VisualStudioInteropSearchPathKey)
        {
            TaskResources = new ResourceManager("Microsoft.Build.Extras.FX1_1.Strings", Assembly.GetExecutingAssembly());
        }
    }
}