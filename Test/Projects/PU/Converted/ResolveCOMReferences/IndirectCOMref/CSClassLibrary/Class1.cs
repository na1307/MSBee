// Copyright (C) Microsoft Corporation. All rights reserved.

using System;

namespace CSClassLibrary
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class Class1
    {
        public Class1()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public void foo()
        {
            Console.WriteLine("Hi from CS ClassLibrary. Now with no further delay...");
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
        }
    }
}
