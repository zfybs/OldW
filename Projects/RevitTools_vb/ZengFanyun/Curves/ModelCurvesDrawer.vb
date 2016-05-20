Imports Autodesk.Revit.DB
Imports Autodesk.Revit.DB.Events
Imports Autodesk.Revit.UI
Imports rvtTools_ez
Imports std_ez

Namespace rvtTools_ez

    ''' <summary>
    ''' 在UI界面中按指定的要求绘制模型线，并将这些模型线保存在对应的列表中。
    ''' 在绘制完成后，必须手动执行 Dispose 方法，以清空数据，以及取消事件的关联。
    ''' </summary>
    Public Class ModelCurvesDrawer

        ''' <summary>
        ''' 在模型线绘制完成时，触发此事件。
        ''' </summary>
        ''' <param name="AddedCurves">添加的模型线</param>
        ''' <param name="FinishedExternally">画笔是否是由外部程序强制关闭的。如果是外部对象通过调用Cancel方法来取消绘制的，则其值为 True。</param>
        ''' <param name="Succeeded">AddedCurves集合中的曲线集合是否满足指定的连续性条件</param>
        Public Event DrawingCompleted(ByVal AddedCurves As List(Of ElementId), FinishedExternally As Boolean， Succeeded As Boolean)

#Region "   ---   Types"

        Public Enum CurvesState

            ''' <summary> 当前的曲线不满足检查要求，应该退出或者撤消绘制 </summary>
            Invalid = 0

            ''' <summary> 当前的曲线还未满足检查要求，但是还可以继续绘制。
            ''' 比如要求绘制一个封闭的曲线链，则在未封闭的过程中，只要其是连续的，就还可以继续绘制。 </summary>
            Validating

            ''' <summary> 当前的曲线已经满足了指定的要求，但此时并不一定要退出。 </summary>
            Validated

        End Enum

#End Region

#Region "   ---   Properties"

        Private F_CheckMode As CurveCheckMode
        ''' <summary>
        ''' 所绘制的曲线要符合何种连续性条件
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CheckMode As CurveCheckMode
            Get
                Return F_CheckMode
            End Get
        End Property

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

        ''' <summary> 模型线绘制器是否正在使用中。
        ''' 注意：每次只能有一个实例正在绘制曲线。</summary>
        Public Shared Property IsBeenUsed As Boolean
#End Region

#Region "   ---   Fields"

        Private rvtUiApp As UIApplication
        Private rvtApp As Autodesk.Revit.ApplicationServices.Application
        Private doc As Document

        ' Public Property AddedModelCurves As List(Of ModelCurve)
        ''' <summary>
        ''' 已经绘制的所有模型线
        ''' </summary>
        ''' <returns></returns>
        Private AddedModelCurvesId As List(Of ElementId)

#End Region

#Region "   ---   实例的构造与回收"

        ''' <summary>
        ''' 构造函数
        ''' </summary>
        ''' <param name="uiApp">进行模型线绘制的Revit程序</param>
        ''' <param name="CheckMode">所绘制的曲线要符合何种连续性条件</param>
        ''' <param name="CheckInTime">是否在每一步绘制时都检测所绘制的曲线是否符合指定的要求，如果为False，则在绘制操作退出后进行统一检测。</param>
        ''' <param name="BaseCurves">
        ''' 在新绘制之前，先指定一组基准曲线集合，而新绘制的曲线将与基准曲线一起来进行连续性条件的检测。
        ''' </param>
        Public Sub New(ByVal uiApp As UIApplication,
                       ByVal CheckMode As CurveCheckMode,
                       ByVal CheckInTime As Boolean,
                       Optional ByVal BaseCurves As List(Of ElementId) = Nothing)

            ' 变量初始化
            rvtUiApp = uiApp
            rvtApp = uiApp.Application
            Me.F_CheckMode = CheckMode
            Me.F_CheckInTime = CheckInTime

            ' 处理已经添加好的基准曲线集合
            Me.AddedModelCurvesId = BaseCurves
            If Me.AddedModelCurvesId Is Nothing Then
                Me.AddedModelCurvesId = New List(Of ElementId)
            End If
            '
            AddHandler rvtApp.DocumentChanged, AddressOf app_DocumentChanged
        End Sub

        ' 绘图与判断处理
        ''' <summary>
        ''' 在UI界面中绘制模型线。此方法为异步操作，程序并不会等待 PostDraw 方法执行完成才继续向下执行。
        ''' </summary>
        Public Sub PostDraw()
            If ModelCurvesDrawer.IsBeenUsed Then
                Throw New InvalidOperationException("有其他的对象正在绘制模型线，请等待其绘制完成后再次启动。")
            Else

                ActiveDraw()
                ModelCurvesDrawer.IsBeenUsed = True
            End If
        End Sub

        ''' <summary>
        ''' 取消模型线的绘制
        ''' </summary>
        Public Sub Cancel()
            ' 最后再检测一次
            Dim blnContinueDraw As Boolean
            Dim cs As CurvesState = ValidateCurves(blnContinueDraw)
            Call RefreshUI(cs, blnContinueDraw)

            If cs = CurvesState.Validated Then
                Me.Finish(True, True)
            Else
                ' 不管是 Validating 还是 Invalid，说明在此刻都没有成功
                Me.Finish(True, Succeeded:=False)
            End If
        End Sub

        ''' <summary>
        ''' 绘制完成，并关闭绘制模式
        ''' </summary>
        ''' <param name="FinishedExternally">画笔是否是由外部程序强制关闭的。如果是外部对象通过调用Cancel方法来取消绘制的，则其值为 True。</param>
        ''' <param name="Succeeded">AddedModelCurves 集合中的曲线集合是否满足指定的连续性条件</param>
        Private Sub Finish(ByVal FinishedExternally As Boolean, Succeeded As Boolean)

            ' 注意以下几步操作的先后顺序
            '
            ModelCurvesDrawer.IsBeenUsed = False
            RaiseEvent DrawingCompleted(AddedModelCurvesId, FinishedExternally, Succeeded)

            ' 数据清空
            AddedModelCurvesId = New List(Of ElementId)
            DeactiveDraw()
        End Sub

        Public Sub Dispose()
            ' 注意以下几步操作的先后顺序
            '
            RemoveHandler rvtApp.DocumentChanged, AddressOf app_DocumentChanged
            ModelCurvesDrawer.IsBeenUsed = False

            ' 变量清空
            rvtUiApp = Nothing
            rvtApp = Nothing
            AddedModelCurvesId = Nothing
            '
            DeactiveDraw()
        End Sub

#End Region

        ''' <summary>
        ''' DocumentChanged事件，并针对不同的绘制情况而进行不同的处理
        ''' </summary>
        ''' <param name="sender">Application对象</param>
        ''' <param name="e"></param>
        Private Sub app_DocumentChanged(sender As Object, e As DocumentChangedEventArgs)

            If e.Operation = UndoOperation.TransactionCommitted OrElse
                e.Operation = UndoOperation.TransactionUndone OrElse
                e.Operation = UndoOperation.TransactionRedone Then

                doc = e.GetDocument
                Dim blnContinueDraw As Boolean ' 在检查连续性后是否要继续绘制
                Try

                    ' 先考察添加的对象：如果添加了新对象，则要么是 DrawNewLines ，要么是 DrawOtherObjects
                    Dim addedCount As Integer = 0
                    Dim addedElement As Element
                    For Each eid As ElementId In e.GetAddedElementIds
                        addedElement = doc.GetElement(eid)
                        If (TypeOf addedElement Is ModelCurve) Then
                            AddedModelCurvesId.Add(eid)
                            addedCount += 1
                        End If
                    Next

                    If addedCount > 0 Then ' 说明绘制了新的模型线
                        ' 检测当前集合中的曲线是否符合指定的连续性要求
                        If Me.CheckInTime Then
                            Dim cs As CurvesState = ValidateCurves(blnContinueDraw)
                            Call RefreshUI(cs, blnContinueDraw)
                        Else ' 说明不进行实时检测，而直接继续绘制

                        End If
                        '
                        Exit Sub
                    End If

                    '
                    ' 再考察删除对象的情况
                    Dim deleted As List(Of ElementId) = e.GetDeletedElementIds
                    If deleted.Count > 0 Then

                        ' 先将被删除的曲线从曲线链集合中剔除掉
                        Dim id_Chain As Integer ' 曲线链中的元素下标
                        Dim id_deleted As Integer  ' 删除的模型线集合中的元素下标
                        For id_Chain = AddedModelCurvesId.Count - 1 To 0 Step -1  ' 曲线链中的元素下标
                            '
                            id_deleted = deleted.IndexOf(AddedModelCurvesId.Item(id_Chain))  ' 找到对应的项
                            '
                            If id_deleted >= 0 Then
                                deleted.RemoveAt(id_deleted)
                                AddedModelCurvesId.RemoveAt(id_Chain)
                            End If
                        Next

                        ' 检测剔除后的集合中的曲线是否符合指定的连续性要求
                        If Me.CheckInTime Then
                            If Me.CheckInTime Then
                                Dim cs As CurvesState = ValidateCurves(blnContinueDraw)
                                Call RefreshUI(cs, blnContinueDraw)
                            Else ' 说明不进行实时检测，而直接继续绘制

                            End If
                        Else ' 说明不进行实时检测，而直接继续绘制

                        End If
                        '
                        Exit Sub
                    End If

                    ' 再考察修改对象的情况（因为在添加对象或者删除对象时，都有可能伴随有修改对象）：在没有添加新对象，只作了修改的情况下，要么是对
                    Dim modifiedCount As Integer = 0
                    Dim modifiedCountElement As Element
                    For Each eid As ElementId In e.GetModifiedElementIds
                        modifiedCountElement = doc.GetElement(eid)
                        If (TypeOf modifiedCountElement Is ModelCurve) Then
                            modifiedCount += 1
                        End If
                    Next
                    If modifiedCount > 0 Then

                        ' 检测剔除后的集合中的曲线是否符合指定的连续性要求
                        If Me.CheckInTime Then
                            Dim cs As CurvesState = ValidateCurves(blnContinueDraw)
                            Call RefreshUI(cs, blnContinueDraw)

                        Else ' 说明不进行实时检测，而直接继续绘制

                        End If
                        '
                        Exit Sub
                    End If

                Catch ex As Exception
                    MessageBox.Show("在绘制模型线及连续性判断时出问题啦~~~" & vbCrLf &
                                           ex.Message & ex.GetType.FullName & vbCrLf &
                                           ex.StackTrace)
                    ' 结束绘制
                    Me.Finish(False, False)
                End Try
            End If

        End Sub

#Region "   ---   曲线绘制的激活、取消"

        ' 绘图操作的启动与终止
        ''' <summary>
        ''' 启动绘图操作
        ''' </summary>
        Private Sub ActiveDraw()
            rvtUiApp.PostCommand(RevitCommandId.LookupPostableCommandId(PostableCommand.ModelLine))
        End Sub

        ''' <summary>
        ''' 绘图结束后的操作。注意，此操作必须要放在Messagebox.Show（或者是其他通过ESC键就可以对窗口进行某些操作的情况，
        ''' 比如关闭窗口等）之后。如果放在Messagebox.Show之前，则会模拟通过按下ESC键而将模态窗口关闭的操作，则模态窗口就只
        ''' 会闪现一下，或者根本就看不见。
        ''' </summary>
        Private Sub DeactiveDraw()
            ' 在Revit UI界面中退出绘制，即按下ESCAPE键
            WindowsUtil.keybd_event(27, 0, 0, 0)  ' 按下 ESCAPE键
            WindowsUtil.keybd_event(27, 0, &H2, 0)  ' 按键弹起

            ' 再按一次
            WindowsUtil.keybd_event(27, 0, 0, 0)
            WindowsUtil.keybd_event(27, 0, &H2, 0)
        End Sub

#End Region

#Region " 扩展区：曲线连续性要求的检测 以及对应的界面响应"

        ''' <summary>
        ''' 检测当前集合中的曲线是否符合指定的连续性要求
        ''' </summary>
        ''' <param name="continueDraw">在检查连续性后是否要继续绘制</param>
        ''' <returns></returns>
        Private Function ValidateCurves(ByRef continueDraw As Boolean) As CurvesState
            Dim cs As CurvesState = CurvesState.Invalid
            continueDraw = False
            Dim curves As New List(Of Curve)

            '将ElementId转换为对应的 Curve 对象
            For Each id In AddedModelCurvesId
                curves.Add(DirectCast(doc.GetElement(id), ModelCurve).GeometryCurve)
            Next

            ' 根据不同的模式进行不同的检测
            Select Case Me.F_CheckMode
                Case CurveCheckMode.Connected  ' 一条连续曲线链
                    If CurvesFormator.GetContiguousCurvesFromCurves(curves) IsNot Nothing Then
                        cs = CurvesState.Validated
                        continueDraw = True
                    Else  ' 说明根本不连续
                        cs = CurvesState.Invalid
                        continueDraw = False
                    End If

                Case CurveCheckMode.Closed
                    Dim CurveChain As List(Of Curve)

                    CurveChain = CurvesFormator.GetContiguousCurvesFromCurves(curves)
                    If CurveChain Is Nothing Then   ' 说明根本就不连续
                        cs = CurvesState.Invalid
                        continueDraw = False

                    Else  ' 说明起码是连续的
                        If CurveChain.First.GetEndPoint(0).DistanceTo(CurveChain.Last.GetEndPoint(1)) < GeoHelper.VertexTolerance Then
                            ' 说明整个连续曲线是首尾相接，即是闭合的。此时就不需要再继续绘制下去了
                            cs = CurvesState.Validated
                            continueDraw = False
                        Else
                            ' 说明整个曲线是连续的，但是还没有闭合。此时就可以继续绘制下去
                            cs = CurvesState.Validating
                            continueDraw = True
                        End If
                    End If

                Case CurveCheckMode.HorizontalPlan Or CurveCheckMode.Closed
                    If CurvesFormator.IsInOnePlan(curves, New XYZ(0, 0, 1)) Then

                    End If
                    Return False
                Case CurveCheckMode.Seperated
                    ' 不用检测，直接符合
                    cs = CurvesState.Validated
                    continueDraw = True
            End Select
            Return cs
        End Function

        ''' <summary>
        ''' 根据当前曲线的连续性状态，以及是否可以继续绘制，来作出相应的UI更新
        ''' </summary>
        ''' <param name="cs"></param>
        ''' <param name="ContinueDraw"></param>
        Private Sub RefreshUI(ByVal cs As CurvesState, ByVal ContinueDraw As Boolean)

            Select Case cs
                Case CurvesState.Validated
                    If ContinueDraw Then  ' 说明是绘制连续线时满足条件
                        ' 继续绘制即可
                    Else  ' 说明是绘制封闭线时终于封闭成功了
                        ' 此时直接绘制绘制就可以了，而不用考虑撤消的问题
                        Me.Finish(False, True)
                        Exit Sub
                    End If

                Case CurvesState.Validating
                    If ContinueDraw Then  ' 说明是绘制封闭线时还未封闭，但是所绘制的曲线都是连续的
                        ' 继续绘制即可
                    Else  ' 暂时没有考虑到何时会出现此种情况
                        ' 不需要任何实现方法
                    End If
                    Exit Sub
                Case CurvesState.Invalid
                    If InquireUndo() Then
                        rvtTools.Undo()
                    Else
                        ' 结束绘制
                        Me.Finish(False, False)
                    End If
                    Exit Sub
            End Select
        End Sub

#End Region

        ''' <summary> 询问用户是否要撤消操作 </summary>
        Private Function InquireUndo() As Boolean
            Dim res As DialogResult = MessageBox.Show("当前操作使得绘制的模型线不满足要求，是否要撤消此操作？", "提示", MessageBoxButtons.YesNo,
                                     MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1)
            If res = DialogResult.Yes Then
                Return True
            Else
                Return False
            End If
        End Function

    End Class
End Namespace