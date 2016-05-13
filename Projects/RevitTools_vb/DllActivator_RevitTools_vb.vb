﻿Namespace OldW.DllActivator
    Public Class DllActivator_RevitTools_vb
        Implements IDllActivator
        ''' <summary>
        ''' 激活本DLL所引用的那些DLLs
        ''' </summary>
        Public Sub ActivateReferences() Implements IDllActivator.ActivateReferences
            Dim dat As IDllActivator
            ' 
            dat = New DllActivator_std_vb
            dat.ActivateReferences()
            '
            dat = New DllActivator_OldWGlobal
            dat.ActivateReferences()
            '
            dat = New DllActivator_Projects_cs
            dat.ActivateReferences()
        End Sub
    End Class
End Namespace