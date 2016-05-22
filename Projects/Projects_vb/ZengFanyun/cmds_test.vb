Imports Autodesk.Revit.ApplicationServices
Imports Autodesk.Revit.DB.Events

Namespace OldW.Commands

    <Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>
    Public Class cmd_Test : Implements IExternalCommand
        Private WithEvents uiApp As UIApplication
        Private uiDoc As UIDocument
        Private WithEvents App As Application

        Public Function Execute(commandData As ExternalCommandData, ByRef message As String, elements As ElementSet) As Result Implements IExternalCommand.Execute
            App = commandData.Application.Application
            uiDoc = commandData.Application.ActiveUIDocument

            Dim a = uiDoc.Selection.PickObjects(objectType:=Selection.ObjectType.Element)
            MessageBox.Show(a.Count)
            Return Result.Cancelled
        End Function


    End Class
End Namespace
