// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.Text;

namespace CSharpConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
#if FX1_1
            Console.WriteLine("FX1_1 is defined!");
#else
#error FX1_1 is not defined.
#endif
        }
    }
}
