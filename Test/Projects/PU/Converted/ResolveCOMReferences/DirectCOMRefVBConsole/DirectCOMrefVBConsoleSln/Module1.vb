' Copyright (C) Microsoft Corporation. All rights reserved.

Imports System
Imports System.Threading

Module Module1

    Sub Main()
        Dim filename As String = Environment.GetEnvironmentVariable("SystemRoot") & "\clock.avi"
        Dim graphManager As QuartzTypeLib.FilgraphManager = New QuartzTypeLib.FilgraphManager

        ' QueryInterface for the IMediaControl interface:
        Dim mc As QuartzTypeLib.IMediaControl = CType(graphManager, QuartzTypeLib.IMediaControl)

        ' Call some methods on a COM interface 
        ' Pass in file to RenderFile method on COM object. 
        mc.RenderFile(filename)

        ' Show file. 
        mc.Run()
        Thread.Sleep(1000)
    End Sub

End Module
