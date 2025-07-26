// Copyright (C) Microsoft Corporation. All rights reserved.
// ------------------------------------------------------------------------------
// This file contains wrapper classes for the MSBee classes that are being tested.
// Originally, we were using MSTest for MSBee unit tests; MSTest conveinently created
// accessors for private fields and methods which were used in our unit tests.
// Later, we switched to NUnit since MSTest is not available for free. Since NUnit
// doesn't generate accessors automatically, we've created these wrapper classes.
// The classes are marked as internal since they should only be accessed within this assembly. 
//------------------------------------------------------------------------------

using NUnit.Framework;
using System;
using System.Globalization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Extras.FX1_1.UnitTests
{
    internal class GenerateResourceAccessor : GenerateResource
    {
        internal GenerateResourceAccessor() : base()
        {
        }
        
        internal new string tempResGenPath
        {
            get {
                return base.tempResGenPath;
            }
            set {
                base.tempResGenPath = value;
            }
        }

        internal new string ToolName
        {
            get {
                return base.ToolName;
            }
        }

        internal new bool CreateOutputResourcesNames()
        {
            return base.CreateOutputResourcesNames();
        }

        internal new string CreateUniqueTempDirectory()
        {
            return base.CreateUniqueTempDirectory();
        }

        internal new bool CopyReferences()
        {
            return base.CopyReferences();
        }

        internal new bool CopyResGen()
        {
            return base.CopyResGen();
        }

        internal new bool DeleteTempResGenPath()
        {
            return base.DeleteTempResGenPath();
        }
    }

    internal class ResolveComReferenceAccessor : ResolveComReference
    {
        internal ResolveComReferenceAccessor() : base()
        {
        }
        
        internal new void ReadOutputFile(string filePath) 
        {
            base.ReadOutputFile(filePath);
        }
        
        internal new static string ConvertItemMetadataToString(ITaskItem ti) 
        {
            return ResolveComReference.ConvertItemMetadataToString(ti);
        }
        
        internal new TaskItem ConvertStringsToItemMetadata(string[] values) 
        {
            return base.ConvertStringsToItemMetadata(values);
        }
    }
}