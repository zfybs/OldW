Imports Autodesk.Revit.DB
Imports Autodesk.Revit.DB.Events
Imports Autodesk.Revit.UI
Imports rvtTools_ez
Imports std_ez

Namespace rvtTools_ez
    Public Class CurvesFormator

        ''' <summary>
        ''' 从选择的Curve Elements中，获得连续排列的多段曲线（不一定要封闭）。作为测试代码保存，但是不进行实际调用
        ''' </summary>
        ''' <param name="doc">曲线所在文档</param>
        ''' <param name="SelectedCurves">多条曲线元素所对应的Reference，可以通过Selection.PickObjects返回。
        ''' 注意，SelectedCurves中每一条曲线都必须是有界的（IsBound），否则，其GetEndPoint会报错。</param>
        ''' <returns>如果输入的曲线可以形成连续的多段线，则返回重新排序后的多段线集合；
        ''' 如果输入的曲线不能形成连续的多段线，则返回Nothing！</returns>
        Public Shared Function GetContiguousCurvesFromSelectedCurveElements_Preserved(ByVal doc As Document, ByVal SelectedCurves As IList(Of Reference)) As IList(Of Curve)
            Dim curves As New List(Of Curve)

            ' Build a list of curves from the curve elements
            For Each reference As Reference In SelectedCurves
                Dim curveElement As CurveElement = TryCast(doc.GetElement(reference), CurveElement)
                curves.Add(curveElement.GeometryCurve.Clone())
            Next reference

            Dim endPoint As XYZ  ' 每一条线的终点，用来与剩下的线段的起点或者终点进行比较
            Dim blnHasCont As Boolean  ' 此终点是否有对应的点与之对应，如果没有，则说明所有的线段中都不能形成连续的多段线

            ' Walk through each curve (after the first) to match up the curves in order
            For ThisCurveId As Integer = 0 To curves.Count - 2
                Dim ThisCurve As Curve = curves(ThisCurveId)
                blnHasCont = False
                endPoint = ThisCurve.GetEndPoint(1) ' 第i条线的终点
                Dim tmpCurve As Curve = curves(ThisCurveId + 1)  ' 当有其余的曲线放置在当ThisCurveId + 1位置时，要将当前状态下的ThisCurveId + 1位置的曲线与对应的曲线对调。

                ' 从剩下的曲线中找出起点与上面的终点重合的曲线 。 find curve with start point = end point
                For NextCurveId As Integer = ThisCurveId + 1 To curves.Count - 1

                    ' Is there a match end->start, if so this is the next curve
                    If GeoHelper.IsAlmostEqualTo(curves(NextCurveId).GetEndPoint(0), endPoint, GeoHelper.VertexTolerance) Then
                        blnHasCont = True
                        ' 向上对换
                        curves(ThisCurveId + 1) = curves(NextCurveId)
                        curves(NextCurveId) = tmpCurve
                        Continue For

                        ' Is there a match end->end, if so, reverse the next curve
                    ElseIf GeoHelper.IsAlmostEqualTo(curves(NextCurveId).GetEndPoint(1), endPoint, GeoHelper.VertexTolerance) Then
                        blnHasCont = True
                        ' 向上对换
                        curves(ThisCurveId + 1) = curves(NextCurveId).CreateReversed()
                        If NextCurveId <> ThisCurveId + 1 Then
                            ' 如果 NextCurveId = ThisCurveId + 1 ，说明 ThisCurveId + 1 就是接下来的那条线，只不过方向反了。
                            ' 这样就不可以将原来的那条线放回去，而只需要执行上面的反转操作就可以了。
                            curves(NextCurveId) = tmpCurve
                        End If

                        Continue For
                    End If

                Next NextCurveId
                If Not blnHasCont Then  ' 说明不可能形成连续的多段线了
                    Return Nothing
                End If
            Next ThisCurveId

            Return curves
        End Function

        ''' <summary>
        ''' 从选择的Curve Elements中，获得连续排列的多段曲线（不一定要封闭）。
        ''' </summary>
        ''' <param name="doc">曲线所在文档</param>
        ''' <param name="SelectedCurves">多条曲线元素所对应的Reference，可以通过Selection.PickObjects返回。
        ''' 注意，SelectedCurves中每一条曲线都必须是有界的（IsBound），否则，其GetEndPoint会报错。</param>
        ''' <returns>如果输入的曲线可以形成连续的多段线，则返回重新排序后的多段线集合；
        ''' 如果输入的曲线不能形成连续的多段线，则返回Nothing！</returns>
        Public Shared Function GetContiguousCurvesFromSelectedCurveElements(ByVal doc As Document, ByVal SelectedCurves As IList(Of Reference)) As IList(Of Curve)
            Dim curves As New List(Of Curve)

            ' Build a list of curves from the curve elements
            For Each reference As Reference In SelectedCurves
                Dim curveElement As CurveElement = TryCast(doc.GetElement(reference), CurveElement)
                curves.Add(curveElement.GeometryCurve.Clone())
            Next reference
            '
            curves = GetContiguousCurvesFromCurves(curves)
            Return curves
        End Function

        ''' <summary>
        ''' 从一组模型线中，获得连续排列的多段曲线（不一定要封闭）。
        ''' </summary>
        ''' <param name="ModelCurves">进行转换的模型线集合。
        ''' 注意，模型线集合中每一条曲线都必须是有界的（IsBound），否则，其GetEndPoint会报错。</param>
        ''' <returns>如果输入的曲线可以形成连续的多段线，则返回重新排序后的多段线集合；
        ''' 如果输入的曲线不能形成连续的多段线，则返回Nothing！</returns>
        Public Shared Function GetContiguousCurvesFromModelCurves(ByVal ModelCurves As IList(Of ModelCurve)) As IList(Of Curve)
            Dim curves As New List(Of Curve)

            ' Build a list of curves from the curve elements
            For Each MC As ModelCurve In ModelCurves
                curves.Add(MC.GeometryCurve.Clone())
            Next MC
            '
            curves = GetContiguousCurvesFromCurves(curves)
            Return curves
        End Function

        ''' <summary>
        ''' 从指定的Curve集合中中，获得连续排列的多段曲线（不一定要封闭）。如果不连续，则返回Nothing。
        ''' </summary>
        ''' <param name="curves">多条曲线元素所对应的集合
        ''' 注意，curves 集合中每一条曲线都必须是有界的（IsBound），否则，其 GetEndPoint 会报错。</param>
        ''' <returns>如果输入的曲线可以形成连续的多段线，则返回重新排序后的多段线集合；
        ''' 如果输入的曲线不能形成连续的多段线，则返回Nothing！</returns>
        Public Shared Function GetContiguousCurvesFromCurves(ByVal curves As IList(Of Curve)) As IList(Of Curve)

            Dim endPoint As XYZ  ' 每一条线的终点，用来与剩下的线段的起点或者终点进行比较
            Dim blnHasCont As Boolean  ' 此终点是否有对应的点与之对应，如果没有，则说明所有的线段中都不能形成连续的多段线

            ' Walk through each curve (after the first) to match up the curves in order
            For ThisCurveId As Integer = 0 To curves.Count - 2
                Dim ThisCurve As Curve = curves(ThisCurveId)
                blnHasCont = False
                endPoint = ThisCurve.GetEndPoint(1) ' 第i条线的终点
                Dim tmpCurve As Curve = curves(ThisCurveId + 1)  ' 当有其余的曲线放置在当ThisCurveId + 1位置时，要将当前状态下的ThisCurveId + 1位置的曲线与对应的曲线对调。

                ' 从剩下的曲线中找出起点与上面的终点重合的曲线 。 find curve with start point = end point
                For NextCurveId As Integer = ThisCurveId + 1 To curves.Count - 1

                    ' Is there a match end->start, if so this is the next curve
                    If GeoHelper.IsAlmostEqualTo(curves(NextCurveId).GetEndPoint(0), endPoint, GeoHelper.VertexTolerance) Then
                        blnHasCont = True
                        ' 向上对换
                        curves(ThisCurveId + 1) = curves(NextCurveId)
                        curves(NextCurveId) = tmpCurve
                        Continue For

                        ' Is there a match end->end, if so, reverse the next curve
                    ElseIf GeoHelper.IsAlmostEqualTo(curves(NextCurveId).GetEndPoint(1), endPoint, GeoHelper.VertexTolerance) Then
                        blnHasCont = True
                        ' 向上对换
                        curves(ThisCurveId + 1) = curves(NextCurveId).CreateReversed()
                        If NextCurveId <> ThisCurveId + 1 Then
                            ' 如果 NextCurveId = ThisCurveId + 1 ，说明 ThisCurveId + 1 就是接下来的那条线，只不过方向反了。
                            ' 这样就不可以将原来的那条线放回去，而只需要执行上面的反转操作就可以了。
                            curves(NextCurveId) = tmpCurve
                        End If

                        Continue For
                    End If

                Next NextCurveId
                If Not blnHasCont Then  ' 说明不可能形成连续的多段线了
                    Return Nothing
                End If
            Next ThisCurveId

            Return curves
        End Function

        ''' <summary>
        ''' 曲线集合在一个平面上
        ''' </summary>
        ''' <param name="curves"></param>
        ''' <returns></returns>
        Public Shared Function IsInPlan(ByVal curves As ICollection(Of ModelCurve))
            Dim c As Curve = curves.First.GeometryCurve


        End Function
    End Class
End Namespace