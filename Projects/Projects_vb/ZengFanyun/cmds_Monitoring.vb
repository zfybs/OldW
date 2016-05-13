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
    Public Class cmd_DataEdit : Implements IExternalCommand

        Public Function Execute(commandData As ExternalCommandData, ByRef message As String, elements As ElementSet) As Result Implements IExternalCommand.Execute
            Dim uiApp As UIApplication = commandData.Application
            Dim doc As Document = uiApp.ActiveUIDocument.Document
            Dim eleIds As ICollection(Of ElementId) = uiApp.ActiveUIDocument.Selection.GetElementIds

            Dim frm As New ElementDataManager(eleIds, doc)
            frm.ShowDialog()
            Return Result.Succeeded
        End Function

    End Class

End Namespace
