Imports System
Imports Autodesk.Revit.UI
Imports Autodesk.Revit.DB
Imports OldW.Instrumentation
Imports OldW.Soil
Imports rvtTools_ez.ExtensionMethods
Imports OldW.DataManager
Imports OldW.Excavation
Imports Autodesk.Revit.DB.Events

Namespace OldW.Commands
    <Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>
    Public Class cmd_Excavation : Implements IExternalCommand

        Private Shared Frm As frm_DrawExcavation

        Public Function Execute(commandData As ExternalCommandData, ByRef message As String, elements As ElementSet) As Result Implements IExternalCommand.Execute

            Dim dat As New DllActivator.DllActivator_Projects_vb
            dat.ActivateReferences()

            Dim uiApp As UIApplication = commandData.Application
            Dim doc As Document = uiApp.ActiveUIDocument.Document

            '
            Dim WApp As OldWApplication = OldWApplication.Create(uiApp.Application)
            Dim WDoc As OldWDocument = OldWDocument.SearchOrCreate(WApp, doc)
            '
            Dim ExcavDoc As New ExcavationDoc(WDoc)

            If Frm Is Nothing OrElse Frm.IsDisposed Then
                Frm = New frm_DrawExcavation(ExcavDoc)
            End If
            Frm.Show(Nothing)

            '
            Return Result.Succeeded
        End Function

    End Class

    ''' <summary> 提取模型中的开挖土体信息 </summary>
    <Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>
    Public Class cmd_ExcavationInfo : Implements IExternalCommand

        Public Function Execute(commandData As ExternalCommandData, ByRef message As String, elements As ElementSet) As Result Implements IExternalCommand.Execute

            Dim dat As New DllActivator.DllActivator_Projects_vb
            dat.ActivateReferences()

            Dim uiApp As UIApplication = commandData.Application
            Dim doc As Document = uiApp.ActiveUIDocument.Document
            '

            Dim WApp As OldWApplication = OldWApplication.Create(uiApp.Application)
            Dim WDoc As OldWDocument = OldWDocument.SearchOrCreate(WApp, doc)
            '
            Dim ExcavDoc As New ExcavationDoc(WDoc)

            Dim frm As New frm_ExcavationInfo(ExcavDoc)
            frm.Show(Nothing)


            Return Result.Succeeded
            '

        End Function

    End Class


End Namespace