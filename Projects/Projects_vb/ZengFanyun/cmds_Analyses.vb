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
    Public Class cmd_Analyze : Implements IExternalCommand

        Public Function Execute(commandData As ExternalCommandData, ByRef message As String, elements As ElementSet) As Result Implements IExternalCommand.Execute
            Dim uiApp As UIApplication = commandData.Application
            Dim WApp As OldWApplication = OldWApplication.Create(uiApp.Application)
            Dim WDoc As OldWDocument = OldWDocument.SearchOrCreate(WApp, uiApp.ActiveUIDocument.Document)



            Dim doc As Document = uiApp.ActiveUIDocument.Document
            '
            Dim inclineEle As Element = doc.GetElement(New ElementId(460115))
            Dim Incline As New Instrum_Incline(inclineEle)
            '
            Dim eleEarht As FamilyInstance = doc.GetElement(New ElementId(460116))
            Dim exca As New ExcavationDoc(WDoc)

            Dim soil As Soil_Model = exca.FindSoilModel()
            Incline.FindAdjacentEarthElevation(soil.Soil)

            Return Result.Succeeded
        End Function

    End Class


    ''' <summary> 查看指定日期的开挖工况 </summary>
    <Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>
    Public Class cmd_ViewStage : Implements IExternalCommand

        Public Function Execute(commandData As ExternalCommandData, ByRef message As String, elements As ElementSet) As Result Implements IExternalCommand.Execute
            Dim uiApp As UIApplication = commandData.Application
            Dim doc As Document = uiApp.ActiveUIDocument.Document
            '

            Dim WApp As OldWApplication = OldWApplication.Create(uiApp.Application)
            Dim WDoc As OldWDocument = OldWDocument.SearchOrCreate(WApp, doc)
            '
            Dim f As New ConstructionReview
            f.Show(Nothing)

            Return Result.Succeeded
            '

        End Function

    End Class
End Namespace
