' Copyright (C) Microsoft Corporation. All rights reserved.

Module Module1

    Sub Main()
#If FX1_1 Then
        Console.WriteLine("FX1_1 is defined!")
#Else
        'Will produce a build error when FX1_1 is undefined.
        Console.Fake()
#End If
    End Sub

End Module
