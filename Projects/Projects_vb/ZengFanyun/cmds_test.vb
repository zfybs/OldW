Imports System
Imports Autodesk.Revit.UI
Imports Autodesk.Revit.DB
Imports OldW.Instrumentation
Imports OldW.Soil
Imports rvtTools_ez.ExtensionMethods
Imports OldW.DataManager
Imports OldW.Excavation
Imports Autodesk.Revit.DB.Events
Imports System.Windows.Input

Namespace OldW.Commands

    ''' <summary> 在UI界面中绘制模型线 </summary>
    <Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>
    Public Class cmd_DrawModelLine : Implements IExternalCommand

        Private lstIds As New List(Of ElementId)

        Public Function Execute(commandData As ExternalCommandData, ByRef message As String, elements As ElementSet) As Result Implements IExternalCommand.Execute



            Dim dat As New DllActivator.DllActivator_Projects_vb
            dat.ActivateReferences()

            Dim uiapp = commandData.Application

            Dim doc As Document = uiapp.ActiveUIDocument.Document

            Dim frm As New rvtTools_ez.Test.ModelessForm(doc)
            frm.Show(Nothing)


            Return Result.Succeeded
        End Function

    End Class
End Namespace
