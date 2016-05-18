Imports Autodesk.Revit.ApplicationServices
Imports Autodesk.Revit.DB.Events

Namespace OldW.Commands

    <Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>
    Public Class cmd_Test : Implements IExternalCommand
        Private WithEvents uiApp As UIApplication
        Private WithEvents App As Application

        Public Function Execute(commandData As ExternalCommandData, ByRef message As String, elements As ElementSet) As Result Implements IExternalCommand.Execute
            App = commandData.Application.Application
        End Function

        Private Sub App_DocumentChanged(sender As Object, e As DocumentChangedEventArgs) Handles App.DocumentChanged
            Dim c As ModelCurve = DirectCast(e.GetDocument.GetElement(e.GetAddedElementIds.First), ModelCurve)
            Dim cc As Curve = c.GeometryCurve
            cc.MakeBound(0, 1)

            MessageBox.Show(cc.GetEndPoint(0).ToString)

            MessageBox.Show(cc.GetType.Name & vbCrLf & "Found")

            RemoveHandler App.DocumentChanged, AddressOf Me.App_DocumentChanged
        End Sub
    End Class
End Namespace
