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
            string filename = Environment.GetEnvironmentVariable("SystemRoot") + @"\clock.avi";
            QuartzTypeLib.FilgraphManager graphManager =
                new QuartzTypeLib.FilgraphManager();

            // QueryInterface for the IMediaControl interface:
            QuartzTypeLib.IMediaControl mc =
                (QuartzTypeLib.IMediaControl)graphManager;

            // Call some methods on a COM interface 
            // Pass in file to RenderFile method on COM object. 
            mc.RenderFile(filename);

            // Show file. 
            mc.Run();

            Thread.Sleep(2000);
            //
            // TODO: Add code to start application here
            //
        }
    }
}
