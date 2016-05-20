Imports Autodesk.Revit.DB
Imports Autodesk.Revit.DB.Events
Imports Autodesk.Revit.UI
Imports rvtTools_ez
Imports std_ez

Namespace rvtTools_ez
    Public Class ClosedCurvesDrawer

#Region "   ---   Events"
        ''' <summary>
        ''' 在模型线绘制完成时，触发此事件。
        ''' </summary>
        ''' <param name="AddedCurves">添加的模型线</param>
        ''' <param name="FinishedExternally">画笔是否是由外部程序强制关闭的。如果是外部对象通过调用Cancel方法来取消绘制的，则其值为 True。</param>
        ''' <param name="Succeeded">AddedCurves集合中的曲线集合是否满足指定的连续性条件</param>
        Public Event DrawingCompleted(ByVal AddedCurves As List(Of List(Of ElementId)), FinishedExternally As Boolean， Succeeded As Boolean)

#End Region

#Region "   ---   Properties"

        Private F_CheckInTime As Boolean
        ''' <summary>
        ''' 是否在每一步绘制时都检测所绘制的曲线是否符合指定的要求，如果为False，则在绘制操作退出后进行统一检测。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CheckInTime As Boolean
            Get
                Return F_CheckInTime
            End Get
        End Property

#End Region

#Region "   ---   Fields"

        Private uiApp As UIApplication
        Private rvtApp As Autodesk.Revit.ApplicationServices.Application
        Private doc As Document

        ''' <summary>
        ''' 用来绘制封闭的模型线
        ''' </summary>
        Private WithEvents ClosedCurveDrawer As ModelCurvesDrawer

        ''' <summary>
        ''' 已经绘制的所有模型线
        ''' </summary>
        Private AddedModelCurvesId As List(Of List(Of ElementId))

#End Region
        ''' <summary>
        ''' 构造函数
        ''' </summary>
        ''' <param name="uiApp">进行模型线绘制的Revit程序</param>
        ''' <param name="CheckInTime">是否在每一步绘制时都检测所绘制的曲线是否符合指定的要求，如果为False，则在绘制操作退出后进行统一检测。</param>
        ''' <param name="BaseCurves">
        ''' 在新绘制之前，先指定一组基准曲线集合，而新绘制的曲线将与基准曲线一起来进行连续性条件的检测。
        ''' </param>
        Public Sub New(ByVal uiApp As UIApplication,
                       ByVal CheckInTime As Boolean,
                       Optional ByVal BaseCurves As List(Of ElementId) = Nothing)
            Me.uiApp = uiApp
            Me.F_CheckInTime = CheckInTime
            AddedModelCurvesId = New List(Of List(Of ElementId))
        End Sub

        Public Sub PostDraw()
            ' 绘制轮廓
            Me.ClosedCurveDrawer = New ModelCurvesDrawer(Me.uiApp, CurveCheckMode.Closed, Me.CheckInTime)
            AddHandler ClosedCurveDrawer.DrawingCompleted, AddressOf Drawer_DrawingCompleted
            Me.ClosedCurveDrawer.PostDraw()

        End Sub

        Public Sub cancel()
            Me.ClosedCurveDrawer.Cancel()
        End Sub

        Private Sub Drawer_DrawingCompleted(AddedCurves As List(Of ElementId), FinishedExternally As Boolean, Succeeded As Boolean)
            If Succeeded Then

                ' 将结果添加到集合中
                AddedModelCurvesId.Add(AddedCurves)

                ' 询问是否还要添加
                Dim res As DialogResult = MessageBox.Show("封闭曲线绘制成功，是否还要继续绘制另一组封闭曲线？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If res = DialogResult.Yes Then
                    ' Can not subscribe to an event during execution of that event. revit.exception.InvalidOperationException
                    Me.ClosedCurveDrawer.PostDraw()
                Else
                    ' 取消与这个绘制器的关联
                    RemoveHandler ClosedCurveDrawer.DrawingCompleted, AddressOf Drawer_DrawingCompleted
                    Me.ClosedCurveDrawer.Dispose()
                    Me.ClosedCurveDrawer = Nothing
                    '
                    RaiseEvent DrawingCompleted(AddedModelCurvesId, FinishedExternally, True)
                End If
            Else
                ' 取消与这个绘制器的关联
                RemoveHandler ClosedCurveDrawer.DrawingCompleted, AddressOf Drawer_DrawingCompleted
                Me.ClosedCurveDrawer.Dispose()
                Me.ClosedCurveDrawer = Nothing

                RaiseEvent DrawingCompleted(AddedModelCurvesId, FinishedExternally, False)
            End If

        End Sub

    End Class
End Namespace