' Copyright (C) Microsoft Corporation. All rights reserved.

Imports System
Imports System.Threading
Module Module1

    Sub Main()
        Dim a As CSClassLibrary.Class1 = New CSClassLibrary.Class1
        a.foo()
        Dim b As VBClassLibrary.Class1 = New VBClassLibrary.Class1
        b.foo()
        Thread.Sleep(2000)
    End Sub

End Module
