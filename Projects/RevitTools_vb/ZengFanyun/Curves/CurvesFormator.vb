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
        ''' 曲线集合在同一个平面上，而不限定是哪一个法向的平面
        ''' </summary>
        ''' <param name="curves"></param>
        ''' <returns></returns>
        Public Shared Function IsInOnePlan(ByVal curves As ICollection(Of Curve)) As Boolean
            Dim bln As Boolean = True
            If curves.Count < 2 Then
                bln = True
            Else
                ' 先找到一个基准平面法向
                Dim v1 As XYZ ' 通过两个方向向量确定一个平面
                Dim v2 As XYZ
                Dim norm As XYZ = Nothing ' 两个方向向量所确定的平面的法向
                '
                Dim CurveIndex As Integer
                '
                Dim c As Curve ' 进行搜索的曲线
                Dim start As Single = 0 ' 从曲线的起始处还是中间开始截取那半条曲线
                c = curves(0)
                v1 = (c.GetEndPoint(start) - c.GetEndPoint(start + 0.5)) ' 第一条曲线的前半段

                ' 以半条曲线作为递进，来搜索出基准平面
                For CurveIndex = 0.5 To curves.Count - 1 Step 0.5
                    If (CurveIndex / 0.5) Mod 2 = 0 Then
                        start = 0
                    Else
                        start = 0.5
                    End If
                    CurveIndex = Math.Floor(CurveIndex)
                    '
                    c = curves(CurveIndex)
                    If Not c.IsBound Then c.MakeBound(0, 1)
                    v2 = (c.GetEndPoint(start) - c.GetEndPoint(start + 0.5))
                    If v1.AngleTo(v2) < GeoHelper.AngleTolerance Then
                        Continue For
                    Else
                        norm = v1.CrossProduct(v2)
                        Exit For
                    End If
                Next
                ' 找到了基准法向后，再对后面的曲线进行比较
                If norm Is Nothing Then
                    bln = True ' 说明所有的曲线都是共线的，那自然也就是共面的
                Else
                    For i As Integer = CurveIndex + 1 To curves.Count - 1
                        c = curves(i)
                        If Not InPlan(c, norm) Then
                            bln = False
                            Exit For
                        End If
                    Next
                End If
            End If
            Return bln
        End Function

        ''' <summary>
        ''' 曲线集合中的所有曲线是否在指定法向的平面上
        ''' </summary>
        ''' <param name="curves"></param>
        ''' <param name="planNormal">指定平面的法向向量</param>
        ''' <returns></returns>
        Public Shared Function IsInOnePlan(ByVal curves As ICollection(Of Curve), planNormal As XYZ)
            Dim bln As Boolean = True
            For Each c As Curve In curves
                If Not InPlan(c, planNormal) Then
                    Return False
                End If
            Next
            Return bln
        End Function

        ''' <summary>
        ''' 指定的曲线是否位于指定的平面内。
        ''' </summary>
        ''' <param name="c"></param>
        ''' <param name="planNormal"></param>
        ''' <returns>空间三维曲线可能并不会在任何一个平面内，此时其自然是返回False。</returns>
        Private Shared Function InPlan(ByVal c As Curve, planNormal As XYZ) As Boolean
            Dim vec1 As XYZ
            Dim vec2 As XYZ
            If Not c.IsBound Then
                c.MakeBound(0, 1)
            End If
            '
            If TypeOf c Is Line Then
                vec1 = c.GetEndPoint(0) - c.GetEndPoint(1)
                If Math.Abs(vec1.AngleTo(planNormal) - 0.5 * Math.PI) < GeoHelper.AngleTolerance Then
                    Return True
                Else
                    Return False
                End If
            ElseIf （TypeOf c Is Arc） OrElse (TypeOf c Is Ellipse) OrElse (TypeOf c Is NurbSpline) Then
                vec1 = c.GetEndPoint(0) - c.GetEndPoint(0.5)
                vec2 = c.GetEndPoint(0.5) - c.GetEndPoint(1)
                If (Math.Abs(vec1.AngleTo(planNormal) - 0.5 * Math.PI) < GeoHelper.AngleTolerance) AndAlso
                        (Math.Abs(vec2.AngleTo(planNormal) - 0.5 * Math.PI) < GeoHelper.AngleTolerance) Then
                    Return True
                Else
                    Return False
                End If
            ElseIf (TypeOf c Is CylindricalHelix) Then  ' 圆柱螺旋线
                Return False
            End If
            '
            Return False
        End Function

#End Region

    End Class

End Namespace