// Copyright (C) Microsoft Corporation. All rights reserved.

using System;

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
			CSClassLibrary.Class1 c = new CSClassLibrary.Class1();
			VBClassLibrary.Class1 b = new VBClassLibrary.Class1();
			
			//
			// TODO: Add code to start application here
			//
		}
	}
}
