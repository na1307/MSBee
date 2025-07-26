// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Threading;

namespace CSConsoleApplication
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    class Class1
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            VBClassLibrary.Class1 a = new VBClassLibrary.Class1();
            a.foo();
            CSClassLibrary.Class1 b = new CSClassLibrary.Class1();
            b.foo();
            Thread.Sleep(2000);
            //
            // TODO: Add code to start application here
            //
        }
    }
}
