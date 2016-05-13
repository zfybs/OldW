Imports System
Imports System.Collections.Generic
Imports Autodesk.Revit
Imports Autodesk.Revit.DB
Imports System.Math
Imports Autodesk.Revit.UI.Selection
Imports Autodesk.Revit.UI
Imports rvtTools_ez
Imports std_ez
Imports Autodesk.Revit.DB.Events

Namespace rvtTools_ez
    ''' <summary>
    ''' 通过各种方法来构造出封闭的曲线
    ''' </summary>
    Public Class CurveLoopConstructor


#Region "   ---   在界面中选择出封闭的曲线"
        ' 几何创建
        ''' <summary> 从模型中获取要创建开挖土体的边界线 </summary>
        ''' <param name="CreateMultipli">在一个实体中是否可以有多个分隔的轮廓面</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function SelectLoopedCurveArray(ByVal uiDoc As UIDocument, ByVal CreateMultipli As Boolean) As CurveArrArray
            Dim Profiles As New CurveArrArray  ' 每一次创建开挖土体时，在NewExtrusion方法中，要创建的实体的轮廓
            Dim blnStop As Boolean
            Do
                blnStop = True
                Dim cvLoop As CurveLoop = GetLoopCurve(uiDoc)
                ' 检验并添加
                If cvLoop.Count > 0 Then
                    Dim cvArr As New CurveArray
                    For Each c As Curve In cvLoop
                        cvArr.Append(c)
                    Next
                    Profiles.Append(cvArr)

                    ' 是否要继续添加
                    If CreateMultipli Then
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
        Private Shared Function GetLoopCurve(ByVal uiDoc As UIDocument) As CurveLoop
            Dim Doc As Document = uiDoc.Document
            '
            Dim Boundaries As List(Of Reference) = uiDoc.Selection.PickObjects(ObjectType.Element, New CurveSelectionFilter, "选择一组封闭的模型线。")
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
                    Dim cs As List(Of Curve) = CurvesFormator.GetContiguousCurvesFromSelectedCurveElements(Doc, Boundaries)
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
                    cvLoop = GetLoopCurve(uiDoc)
                Else
                    cvLoop = New CurveLoop
                    Return cvLoop
                End If
            End Try
            Return cvLoop
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
#End Region


    End Class

End Namespace