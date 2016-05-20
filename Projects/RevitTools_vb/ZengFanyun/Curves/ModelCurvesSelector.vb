Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI.Selection
Imports Autodesk.Revit.UI
Imports std_ez

Namespace rvtTools_ez
    ''' <summary>
    ''' 通过各种方法来构造出封闭的曲线
    ''' </summary>
    Public Class ModelCurveSelector

#Region "   ---   Properties"



#End Region

#Region "   ---   Fields"

        Protected uiDoc As UIDocument

        ''' <summary>
        ''' 模型线选择器中已经选择了的曲线
        ''' </summary>
        Private SelectedCurves As List(Of Curve)

        ''' <summary>
        ''' 模型线选择器中已经选择了的曲线的Id值
        ''' </summary>
        Private SelectedCurvesId As List(Of ElementId)


#End Region

        ''' <summary>
        ''' 构造函数
        ''' </summary>
        ''' <param name="uiDoc"> 进行模型线选择的那个文档 </param>
        Public Sub New(ByVal uiDoc As UIDocument)
            With Me
                .uiDoc = uiDoc
            End With
        End Sub

#Region "   ---   在界面中选择出封闭的曲线"

        ''' <summary>
        ''' 选择模型中的模型线
        ''' </summary>
        ''' <returns></returns>
        Protected Function SelectModelCurve() As List(Of Reference)
            Dim Boundaries As List(Of Reference) = uiDoc.Selection.PickObjects(ObjectType.Element, New CurveSelectionFilter, "选择一组封闭的模型线。")
            Return Boundaries
        End Function

        ''' <summary>
        ''' 曲线选择过滤器
        ''' </summary>
        ''' <remarks></remarks>
        Private Class CurveSelectionFilter
            Implements ISelectionFilter
            Public Function AllowElement(element As Element) As Boolean Implements ISelectionFilter.AllowElement
                Dim bln As Boolean = False
                If (TypeOf element Is ModelCurve) Then
                    Return True
                End If
                Return bln
            End Function

            Public Function AllowReference(refer As Reference, point As XYZ) As Boolean Implements ISelectionFilter.AllowReference
                Return False
            End Function
        End Class

        ''' <summary>
        ''' 从选择的Curve Elements中，获得连续排列的多段曲线（不一定要封闭）。
        ''' </summary>
        ''' <param name="doc">曲线所在文档</param>
        ''' <param name="SelectedCurves">多条曲线元素所对应的Reference，可以通过Selection.PickObjects返回。
        ''' 注意，SelectedCurves中每一条曲线都必须是有界的（IsBound），否则，其GetEndPoint会报错。</param>
        ''' <returns>如果输入的曲线可以形成连续的多段线，则返回重新排序后的多段线集合；
        ''' 如果输入的曲线不能形成连续的多段线，则返回Nothing！</returns>
        Protected Function GetContiguousCurvesFromSelectedCurveElements(ByVal doc As Document, ByVal SelectedCurves As IList(Of Reference)) As IList(Of Curve)
            Dim curves As New List(Of Curve)

            ' Build a list of curves from the curve elements
            For Each reference As Reference In SelectedCurves
                Dim curveElement As CurveElement = TryCast(doc.GetElement(reference), CurveElement)
                curves.Add(curveElement.GeometryCurve.Clone())
            Next reference
            '
            curves = CurvesFormator.GetContiguousCurvesFromCurves(curves)
            Return curves
        End Function

#End Region

#Region " 扩展区：曲线连续性要求的检测 以及对应的界面响应"

#End Region

    End Class

End Namespace