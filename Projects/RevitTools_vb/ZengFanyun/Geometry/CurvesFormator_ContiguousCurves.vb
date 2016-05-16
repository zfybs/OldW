Imports Autodesk.Revit.DB
Imports Autodesk.Revit.DB.Events
Imports Autodesk.Revit.UI
Imports rvtTools_ez
Imports std_ez

Namespace rvtTools_ez

    Partial Public Class CurvesFormator

#Region " 曲线连续性 的 开发功能"

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
            curves = GetContiguousCurvesFromCurves1(curves)
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
            curves = GetContiguousCurvesFromCurves1(curves)
            Return curves
        End Function

#End Region

#Region " 曲线连续性 的 实现方法"

        ''' <summary>
        ''' 从指定的Curve集合中中，获得连续排列的多段曲线（不一定要封闭）。如果不连续，则返回Nothing。
        ''' </summary>
        ''' <param name="curves">多条曲线元素所对应的集合
        ''' 注意，curves 集合中每一条曲线都必须是有界的（IsBound），否则，其 GetEndPoint 会报错。</param>
        ''' <returns>如果输入的曲线可以形成连续的多段线，则返回重新排序后的多段线集合；
        ''' 如果输入的曲线不能形成连续的多段线，则返回Nothing！</returns>
        Private Shared Function GetContiguousCurvesFromCurves_OneDirection(ByVal curves As IList(Of Curve)) As IList(Of Curve)

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
                            ' 这样就不可以将反转前的那条线放回去，而只需要执行上面的反转操作就可以了。
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
        ''' 从指定的Curve集合中中，获得连续排列的多段曲线（不一定要封闭）。如果不连续，则返回Nothing。
        ''' </summary>
        ''' <param name="curves">多条曲线元素所对应的集合
        ''' 注意，curves 集合中每一条曲线都必须是有界的（IsBound），否则，其 GetEndPoint 会报错。</param>
        ''' <returns>如果输入的曲线可以形成连续的多段线，则返回重新排序后的多段线集合；
        ''' 如果输入的曲线不能形成连续的多段线，则返回Nothing！</returns>
        Private Shared Function GetContiguousCurvesFromCurves1(ByVal curves As IList(Of Curve)) As IList(Of Curve)
            Dim curvesCount As Integer = curves.Count

            ' 获取所有曲线的端点坐标
            Dim sourcePoints As New List(Of XYZ())
            Dim searchIdLeft As New List(Of Integer) ' 还剩下多少曲线要进行连续性搜索

            ' 以集合中第一条曲线为基准，向左、向右搜索对应的连续曲线
            Dim rightIndex As New List(Of Integer)     ' 向右搜索的连续曲线链的下标值
            Dim rightReverse As New List(Of Boolean)  ' 与 rightIndex 相对应的曲线是否要反转
            Dim leftIndex As New List(Of Integer)     ' 向左搜索的连续曲线链的下标值
            Dim leftReverse As New List(Of Boolean)  ' 与 leftIndex 相对应的曲线是否要反转

            ' 集合数据初始化
            Dim tempCurve As Curve
            If curvesCount >= 1 Then
                For i As Integer = 0 To curvesCount - 1
                    tempCurve = curves.Item(i)
                    sourcePoints.Add({tempCurve.GetEndPoint(0), tempCurve.GetEndPoint(1)})
                    searchIdLeft.Add(i)
                Next
            Else
                Return Nothing
            End If

            ' 开始搜索！！！！
            Dim endPoint As XYZ  ' 每一条线的终点，用来与剩下的线段的起点或者终点进行比较
            Dim blnHasCont As Boolean  ' 此终点是否有对应的点与之对应，如果没有，则说明所有的线段中都不能形成连续的多段线
            Dim tempIndex As Integer ' 在曲线Id值进行对换时用来临时保存Id
            Dim searchedPoint() As XYZ  ' 正在被搜索的那条曲线的两个端点坐标
            ' 先从基准线向右搜索
            endPoint = sourcePoints.Item(0)(1) ' 第1条线的终点
            For BaseId As Integer = 0 To curvesCount - 2
                blnHasCont = False

                ' 从剩下的曲线中找出起点与上面的终点重合的曲线 。 find curve with start point = end point
                For searchIndex As Integer = BaseId + 1 To curvesCount - 1

                    searchedPoint = sourcePoints.Item(searchIdLeft(searchIndex))

                    ' Is there a match end->start, if so this is the next curve
                    If GeoHelper.IsAlmostEqualTo(searchedPoint(0), endPoint, GeoHelper.VertexTolerance) Then
                        blnHasCont = True
                        ' searchIdLeft.Remove(searchIndex)
                        rightIndex.Add(searchIndex)
                        rightReverse.Add(False)
                        endPoint = searchedPoint(1) ' 以下一条线的终点作为基点

                        ' 向上对换
                        tempIndex = searchIdLeft(BaseId + 1)
                        searchIdLeft(BaseId + 1) = searchIdLeft(searchIndex)
                        searchIdLeft(searchIndex) = tempIndex
                        Exit For

                        ' Is there a match end->end, if so, reverse the next curve
                    ElseIf GeoHelper.IsAlmostEqualTo(searchedPoint(1), endPoint, GeoHelper.VertexTolerance) Then
                        blnHasCont = True
                        ' searchIdLeft.Remove(searchIndex)
                        rightIndex.Add(searchIndex)
                        rightReverse.Add(True)
                        endPoint = searchedPoint(0) ' 因为下一条线是反转的，所以应该以以下一条线的起点（而不是终点）作为新的基点

                        ' 向上对换
                        tempIndex = searchIdLeft(BaseId + 1)
                        searchIdLeft(BaseId + 1) = searchIdLeft(searchIndex)
                        searchIdLeft(searchIndex) = tempIndex
                        Exit For
                    End If
                Next searchIndex
                If Not blnHasCont Then  ' 说明不可能形成连续的多段线了
                    Exit For
                End If
            Next BaseId

            '
            If rightIndex.Count < curvesCount - 1 Then
                ' 向左搜索
                endPoint = sourcePoints.Item(0)(0) ' 第1条线的左端点
                For BaseId As Integer = rightIndex.Count To curvesCount - 2
                    blnHasCont = False

                    ' 从剩下的曲线中找出起点与上面的终点重合的曲线 。 find curve with start point = end point
                    For searchIndex As Integer = BaseId + 1 To curvesCount - 1
                        searchedPoint = sourcePoints.Item(searchIdLeft(searchIndex))

                        ' Is there a match end->start, if so this is the next curve
                        If GeoHelper.IsAlmostEqualTo(searchedPoint(0), endPoint, GeoHelper.VertexTolerance) Then
                            blnHasCont = True
                            ' searchIdLeft.Remove(searchIndex)
                            rightIndex.Add(searchIndex)
                            rightReverse.Add(True)
                            endPoint = searchedPoint(1)  ' 因为下一条线是反转的，所以应该以以下一条线的终点（而不是起点）作为新的基点

                            ' 向上对换
                            tempIndex = searchIdLeft(BaseId + 1)
                            searchIdLeft(BaseId + 1) = searchIdLeft(searchIndex)
                            searchIdLeft(searchIndex) = tempIndex
                            Exit For

                            ' Is there a match end->end, if so, reverse the next curve
                        ElseIf GeoHelper.IsAlmostEqualTo(searchedPoint(1), endPoint, GeoHelper.VertexTolerance) Then
                            blnHasCont = True
                            ' searchIdLeft.Remove(searchIndex)
                            rightIndex.Add(searchIndex)
                            rightReverse.Add(False)
                            endPoint = searchedPoint(0) ' 以下一条线的起点作为新的基点

                            ' 向上对换
                            tempIndex = searchIdLeft(BaseId + 1)
                            searchIdLeft(BaseId + 1) = searchIdLeft(searchIndex)
                            searchIdLeft(searchIndex) = tempIndex
                            Exit For
                        End If
                    Next searchIndex
                    If Not blnHasCont Then  ' 说明不可能形成连续的多段线了
                        Exit For
                    End If
                Next BaseId
            End If

            If rightIndex.Count + leftIndex.Count = curvesCount - 1 Then  ' 说明整个曲线集合刚好可以形成一条连续曲线链
                Dim targetCurves As New List(Of Curve)
                ' 左段
                For i As Integer = leftIndex.Count - 1 To 0 Step -1
                    If leftReverse.Item(i) Then ' 是否要反转
                        targetCurves.Add(curves.Item(leftIndex.Item(i)).CreateReversed)
                    Else
                        targetCurves.Add(curves.Item(leftIndex.Item(i)))
                    End If
                Next
                ' 基准线
                targetCurves.Add(curves.Item(0))
                ' 右段
                For i As Integer = 0 To rightIndex.Count - 1
                    If rightReverse.Item(i) Then ' 是否要反转
                        targetCurves.Add(curves.Item(rightIndex.Item(i)).CreateReversed)
                    Else
                        targetCurves.Add(curves.Item(rightIndex.Item(i)))
                    End If
                Next
                '
                Return targetCurves
            Else
                Return Nothing ' 说明整个曲线集合都不能形成连续曲线链
            End If
        End Function

        ''' <summary>
        ''' 从指定的Curve集合中中，获得连续排列的多段曲线（不一定要封闭）。如果不连续，则返回Nothing。
        ''' </summary>
        ''' <param name="curves">多条曲线元素所对应的集合
        ''' 注意，curves 集合中每一条曲线都必须是有界的（IsBound），否则，其 GetEndPoint 会报错。</param>
        ''' <returns>如果输入的曲线可以形成连续的多段线，则返回重新排序后的多段线集合；
        ''' 如果输入的曲线不能形成连续的多段线，则返回Nothing！</returns>
        ''' <remarks>GetContiguousCurvesFromCurves2与函数GetContiguousCurvesFromCurves2的功能完全相同，只是GetContiguousCurvesFromCurves1是
        ''' 通过数值的方法来实现，而GetContiguousCurvesFromCurves2是通过类与逻辑的判断来实现。所以GetContiguousCurvesFromCurves1的执行速度可能会快一点点，
        ''' 而GetContiguousCurvesFromCurves2的扩展性会好得多。</remarks>
        Private Shared Function GetContiguousCurvesFromCurves2(ByVal curves As IList(Of Curve)) As IList(Of Curve)

            Dim CurvesLeft As New Dictionary(Of Integer, Curve)
            Dim cc As ContiguousCurves
            '
            If curves.Count >= 1 Then
                cc = New ContiguousCurves(curves.Item(0))
                For i = 1 To curves.Count - 1
                    CurvesLeft.Add(i, curves.Item(i))
                Next
            Else
                Return Nothing
            End If
            '
            Dim foundedIndex As Nullable(Of Integer) = Nothing
            ' 先向右端延伸搜索
            For i = 0 To CurvesLeft.Count - 1
                foundedIndex = cc.CheckForward(CurvesLeft)
                If foundedIndex IsNot Nothing Then  ' 说明找到了相连的曲线
                    cc.ConnectForward(CurvesLeft.Item(foundedIndex))
                    CurvesLeft.Remove(foundedIndex)

                Else ' 说明剩下的曲线中，没有任何一条曲线能与当前连续链的右端相连了
                    Exit For
                End If
            Next

            ' 再向左端延伸搜索
            For i = 0 To CurvesLeft.Count - 1
                foundedIndex = cc.CheckBackward(CurvesLeft)
                If foundedIndex IsNot Nothing Then  ' 说明找到了相连的曲线
                    cc.ConnectBackward(CurvesLeft.Item(foundedIndex))
                    CurvesLeft.Remove(foundedIndex)

                Else ' 说明剩下的曲线中，没有任何一条曲线能与当前连续链的右端相连了
                    Exit For
                End If
            Next

            ' 
            If cc.Curves.Count <> curves.Count Then
                Return Nothing
            End If
            Return cc.Curves
        End Function

#End Region

        ''' <summary>
        ''' 模拟一段从左向右的连续性曲线链集合，集合中的第一个元素表示最左边的曲线；end0 与 end1 分别代表整个连续曲线段的最左端点与最右端点。
        ''' </summary>
        Public Class ContiguousCurves

            Private CurvesChain As List(Of Curve)
            ''' <summary> 连续性曲线链，此集合中的曲线肯定是首尾相连的。且第一个元素表示最左边的那条曲线。 </summary>
            Public ReadOnly Property Curves As List(Of Curve)
                Get
                    Return CurvesChain
                End Get
            End Property

            ''' <summary> 整个连续性曲线链的最左端点的坐标 </summary>
            Private end0 As XYZ

            ''' <summary> 整个连续性曲线链的最右端点的坐标 </summary>
            Private end1 As XYZ

            ''' <summary>
            ''' 从一条曲线开始构造连续曲线链
            ''' </summary>
            ''' <param name="BaseCurve"></param>
            Public Sub New(ByVal BaseCurve As Curve)
                CurvesChain = New List(Of Curve)
                CurvesChain.Add(BaseCurve)
                Me.end0 = BaseCurve.GetEndPoint(0)
                Me.end1 = BaseCurve.GetEndPoint(1)
            End Sub

#Region "检测连续性"

            ''' <summary>
            ''' 从一组曲线中找到一条与连续链右端点相接的曲线，并且在适当的情况下，对搜索到的曲线进行反转。
            ''' </summary>
            ''' <param name="curves">进行搜索的曲线集合。在此函数中，可能会对连接到的那条曲线进行反转。
            ''' IDictionary中的键值表示每一条Curve的Id值，这个值并不一定是从1开始递增的。
            ''' </param>
            ''' <returns>
            ''' 与连续曲线链的最右端相连的那一条曲线在输入的曲线集合中所对应的Id键值。
            ''' 如果没有找到连接的曲线，则返回Nothing！</returns>
            Public Function CheckForward(ByVal curves As Dictionary(Of Integer, Curve)) As Nullable(Of Integer)
                ' 搜索到的那一条曲线所对应的Id值
                Dim ConnectedCurveIndex As Nullable(Of Integer)
                ConnectedCurveIndex = Nothing  ' 如果没有找到，则返回Nothing

                '
                Dim Ids = curves.Keys.ToArray
                Dim Cvs = curves.Values
                '
                Dim tempId As Integer
                Dim tempCurve As Curve

                ' 从曲线集合中找出起点与上面的终点重合的曲线 。 find curve with start point = end point
                For i As Integer = 0 To Ids.Length - 1
                    tempId = Ids(i)
                    tempCurve = curves.Item(tempId)

                    ' Is there a match end->start, if so this is the next curve
                    If GeoHelper.IsAlmostEqualTo(tempCurve.GetEndPoint(0), Me.end1, GeoHelper.VertexTolerance) Then
                        Return tempId
                        ' Is there a match end->end, if so, reverse the next curve
                    ElseIf GeoHelper.IsAlmostEqualTo(tempCurve.GetEndPoint(1), Me.end1, GeoHelper.VertexTolerance) Then
                        ' 将曲线进行反转
                        curves.Item(tempId) = tempCurve.CreateReversed() ' 将反转后的曲线替换掉原来的曲线
                        Return tempId
                    End If
                Next i
                '
                Return ConnectedCurveIndex
            End Function

            ''' <summary>
            ''' 从一组曲线中找到一条与连续链左端点相接的曲线，并且在适当的情况下，对搜索到的曲线进行反转。
            ''' </summary>
            ''' <param name="curves">进行搜索的曲线集合。在此函数中，可能会对连接到的那条曲线进行反转。
            ''' IDictionary中的键值表示每一条Curve的Id值，这个值并不一定是从1开始递增的。</param>
            ''' <returns>与连续曲线链的最右端相连的那一条曲线在输入的曲线集合中所对应的Id键值。
            ''' 如果没有找到连接的曲线，则返回Nothing！</returns>
            Public Function CheckBackward(ByVal curves As Dictionary(Of Integer, Curve)) As Nullable(Of Integer)
                ' 搜索到的那一条曲线所对应的Id值
                Dim ConnectedCurveIndex As Nullable(Of Integer)
                ConnectedCurveIndex = Nothing  ' 如果没有找到，则返回Nothing
                '
                Dim Ids = curves.Keys.ToArray
                Dim Cvs = curves.Values
                '
                Dim tempId As Integer
                Dim tempCurve As Curve

                ' 从曲线集合中找出起点与上面的终点重合的曲线 。 find curve with start point = end point
                For i As Integer = 0 To Ids.Length - 1
                    tempId = Ids(i)
                    tempCurve = curves.Item(tempId)

                    ' Is there a match end->start, if so this is the next curve
                    If GeoHelper.IsAlmostEqualTo(tempCurve.GetEndPoint(0), Me.end0, GeoHelper.VertexTolerance) Then
                        ' 将曲线进行反转
                        curves.Item(tempId) = tempCurve.CreateReversed()
                        Return tempId

                    ElseIf GeoHelper.IsAlmostEqualTo(tempCurve.GetEndPoint(1), Me.end0, GeoHelper.VertexTolerance) Then
                        Return tempId
                    End If
                Next i
                '
                Return ConnectedCurveIndex
            End Function

#End Region

#Region "连接"

            ''' <summary>
            ''' 将曲线添加到连续曲线链的右端
            ''' </summary>
            ''' <param name="c">请自行确保添加的曲线是可以与当前的连续链首尾相接的。
            ''' 如果不能确保，请通过CheckForward函数进行检测。</param>
            Public Sub ConnectForward(ByVal c As Curve)
                CurvesChain.Add(c)
                end1 = c.GetEndPoint(1)
            End Sub

            ''' <summary>
            ''' 将曲线添加到连续曲线链的左端
            ''' </summary>
            ''' <param name="c">请自行确保添加的曲线是可以与当前的连续链首尾相接的。
            ''' 如果不能确保，请通过CheckBackward函数进行检测。</param>
            Public Sub ConnectBackward(ByVal c As Curve)
                CurvesChain.Insert(0, c)
                end0 = c.GetEndPoint(0)
            End Sub

#End Region
        End Class

    End Class

End Namespace