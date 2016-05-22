Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports std_ez

Namespace rvtTools_ez
    ''' <summary>
    ''' 在Revit界面中选择出多个封闭的模型线
    ''' </summary>
    Public Class ClosedCurveSelector
        Inherits ModelCurveSelector

        ''' <summary>
        ''' 是否要分次选择多个封闭的模型曲线链
        ''' </summary>
        Private MultipleClosed As Boolean

        ''' <summary>
        ''' 构造函数
        ''' </summary>
        ''' <param name="uiDoc">进行模型线选择的那个文档</param>
        ''' <param name="Multiple"> 是否要分次选择多个封闭的模型曲线链</param>
        Public Sub New(ByVal uiDoc As UIDocument, ByVal Multiple As Boolean)
            MyBase.New(uiDoc)
            Me.MultipleClosed = Multiple
        End Sub

        ''' <summary>
        ''' 开启同步操作：在Revit UI 界面中选择封闭的模型曲线链
        ''' </summary>
        ''' <returns></returns>
        Public Function SendSelect() As CurveArrArray
            Dim Profiles As New CurveArrArray  ' 每一次创建开挖土体时，在NewExtrusion方法中，要创建的实体的轮廓
            Dim blnStop As Boolean
            Do
                blnStop = True
                Dim cvLoop As CurveLoop = GetLoopedCurve()
                ' 检验并添加
                If cvLoop.Count > 0 Then
                    Dim cvArr As New CurveArray
                    For Each c As Curve In cvLoop
                        cvArr.Append(c)
                    Next
                    Profiles.Append(cvArr)

                    ' 是否要继续添加
                    If Me.MultipleClosed Then
                        Dim res As DialogResult = MessageBox.Show("曲线添加成功，是否还要继续添加？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        If res = DialogResult.Yes Then
                            blnStop = False
                        End If
                    End If
                End If
            Loop Until blnStop
            Return Profiles
        End Function

        ''' <summary>
        ''' 获取一组连续封闭的模型线
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetLoopedCurve() As CurveLoop
            Dim Doc As Document = Me.uiDoc.Document
            '
            Dim Boundaries As List(Of Reference) = MyBase.SelectModelCurve()

            Dim cvLoop As New CurveLoop
            Try
                If Boundaries.Count = 1 Then  ' 要么是封闭的圆或圆弧，要么就不封闭
                    Dim c As Curve = DirectCast(Doc.GetElement(Boundaries.Item(0)), ModelCurve).GeometryCurve
                    If ((TypeOf c Is Arc) OrElse (TypeOf c Is Ellipse)) AndAlso (Not c.IsBound) Then
                        cvLoop.Append(c)
                    Else
                        Throw New InvalidOperationException("选择的一条圆弧线或者椭圆线并不封闭。")
                    End If
                Else
                    ' 对于选择了多条曲线的情况
                    Dim cs As List(Of Curve) = GetContiguousCurvesFromSelectedCurveElements(Doc, Boundaries)
                    If cs IsNot Nothing Then
                        For Each c As Curve In cs
                            cvLoop.Append(c)
                        Next
                    Else
                        ' 显示出选择的每一条线的两个端点
                        Dim nn As Integer = Boundaries.Count
                        Dim c As Curve
                        Dim cvs(0 To nn - 1) As String
                        For i = 0 To nn - 1
                            c = TryCast(Doc.GetElement(Boundaries(i)), CurveElement).GeometryCurve
                            cvs(i) = c.GetEndPoint(0).ToString & c.GetEndPoint(1).ToString
                        Next
                        Utils.ShowEnumerable(cvs)

                        Throw New InvalidOperationException("所选择的曲线不连续。")
                    End If

                    If cvLoop.IsOpen Then
                        Throw New InvalidOperationException("所选择的曲线不能形成封闭的曲线。")
                    ElseIf Not cvLoop.HasPlane Then
                        Throw New InvalidOperationException("所选择的曲线不在同一个平面上。")
                    Else
                        Return cvLoop
                    End If
                End If
            Catch ex As Exception
                Dim res As DialogResult = MessageBox.Show(ex.Message & " 点击是以重新选择，点击否以退出绘制。" & vbCrLf & "当前选择的曲线条数为：" & Boundaries.Count & "条。" &
                                                      vbCrLf & ex.StackTrace, "Warnning", MessageBoxButtons.OKCancel)
                If res = DialogResult.OK Then
                    cvLoop = GetLoopedCurve()
                Else
                    cvLoop = New CurveLoop
                    Return cvLoop
                End If
            End Try
            Return cvLoop
        End Function

        '''' <summary>
        '''' 检测当前集合中的曲线是否符合指定的连续性要求
        '''' </summary>
        '''' <param name="curvesIn"> 用户选择的用于检测的曲线集合 </param>
        '''' <param name="curvesOut"> 进行检测与重新排列后的新的曲线集合 </param>
        '''' <returns></returns>
        'Private Function ValidateCurves(ByVal curvesIn As List(Of Curve), ByRef curvesOut As List(Of Curve)) As CurveCheckState
        '    curvesOut = Nothing
        '    Dim cs As CurveCheckState = CurveCheckState.Invalid_Exit

        '    ' 根据不同的模式进行不同的检测
        '    Select Case Me.CheckMode
        '        Case CurveCheckMode.Connected  ' 一条连续曲线链
        '            curvesOut = CurvesFormator.GetContiguousCurvesFromCurves(curvesIn)
        '            If curvesOut IsNot Nothing Then
        '                cs = CurveCheckState.Valid_Continue
        '            Else  ' 说明根本不连续
        '                cs = CurveCheckState.Invalid_InquireForUndo
        '            End If

        '        Case CurveCheckMode.Closed

        '            curvesOut = CurvesFormator.GetContiguousCurvesFromCurves(curvesIn)
        '            If curvesOut Is Nothing Then   ' 说明根本就不连续
        '                cs = CurveCheckState.Invalid_InquireForUndo
        '            Else  ' 说明起码是连续的
        '                If curvesOut.First.GetEndPoint(0).DistanceTo(curvesOut.Last.GetEndPoint(1)) < GeoHelper.VertexTolerance Then
        '                    ' 说明整个连续曲线是首尾相接，即是闭合的。此时就不需要再继续选择了
        '                    cs = CurveCheckState.Valid_Exit
        '                Else
        '                    ' 说明整个曲线是连续的，但是还没有闭合。此时应该重新选择
        '                    cs = CurveCheckState.Invalid_InquireForUndo
        '                End If
        '            End If

        '        Case CurveCheckMode.MultiClosed

        '            curvesOut = CurvesFormator.GetContiguousCurvesFromCurves(curvesIn)
        '            If curvesOut Is Nothing Then   ' 说明根本就不连续
        '                cs = CurveCheckState.Invalid_InquireForUndo
        '            Else  ' 说明起码是连续的
        '                If curvesOut.First.GetEndPoint(0).DistanceTo(curvesOut.Last.GetEndPoint(1)) < GeoHelper.VertexTolerance Then
        '                    ' 说明整个连续曲线是首尾相接，即是闭合的。此时应该询问用户是否要继续绘制新的闭合曲线
        '                    cs = CurveCheckState.Valid_InquireForContinue
        '                Else
        '                    ' 说明整个曲线是连续的，但是还没有闭合。此时应该重新选择
        '                    cs = CurveCheckState.Invalid_InquireForUndo
        '                End If
        '            End If


        '        Case CurveCheckMode.HorizontalPlan Or CurveCheckMode.Closed
        '            If CurvesFormator.IsInOnePlan(curvesIn, New XYZ(0, 0, 1)) Then

        '            End If
        '            Return False
        '        Case CurveCheckMode.Seperated
        '            ' 不用检测，直接符合
        '            cs = CurveCheckState.Valid_InquireForContinue
        '    End Select
        '    Return cs
        'End Function

    End Class
End Namespace