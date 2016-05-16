Imports Autodesk.Revit.DB
Imports Autodesk.Revit.DB.Events
Imports Autodesk.Revit.UI
Imports rvtTools_ez
Imports std_ez

Namespace rvtTools_ez

    ''' <summary>
    ''' 判断曲线集合是否满足指定的格式，或者将集合中的曲线进行标准化，比如形成连续曲线，投影到同一个平面中等。
    ''' </summary>
    Public Class CurvesFormator

#Region " 曲线共平面"

        ''' <summary>
        ''' 曲线集合在一个平面上
        ''' </summary>
        ''' <param name="curves"></param>
        ''' <returns></returns>
        Public Shared Function IsInPlan(ByVal curves As ICollection(Of ModelCurve))
            Dim c As Curve = curves.First.GeometryCurve


        End Function

#End Region

    End Class

End Namespace