// Copyright (C) Microsoft Corporation. All rights reserved.

using System;

namespace bar
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
			SimpleDLL.Class1 class1 = new SimpleDLL.Class1();
			class1.ShowMessageBox();

			//
			// TODO: Add code to start application here
			//
		}
	}
}
