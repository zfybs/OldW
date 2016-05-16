Imports Autodesk.Revit.DB
Imports Autodesk.Revit.DB.Events
Imports Autodesk.Revit.UI
Imports rvtTools_ez
Imports std_ez

Namespace rvtTools_ez

    ''' <summary>
    ''' 在UI界面中按指定的要求绘制模型线，并将这些模型线保存在对应的列表中。
    ''' 在绘制的过程中，可以指定所绘制的曲线集合符合指定的连续性条件，也可以直接绘制分散的模型线。
    ''' 注意：每次只能有一个实例正在绘制曲线。
    ''' </summary>
    Public Class ModelCurveDrawer

        ''' <summary>
        ''' 在模型线绘制完成时，触发此事件。
        ''' </summary>
        ''' <param name="AddedCurves">添加的模型线</param>
        ''' <param name="FinishedInternally">画笔是否是由内部程序自己关闭的。如果是外部对象通过调用Cancel方法来取消绘制的，则其值为False。</param>
        ''' <param name="Succeeded">AddedCurves集合中的曲线集合是否满足指定的连续性条件</param>
        Public Event DrawingCompleted(ByVal AddedCurves As List(Of ModelCurve), FinishedInternally As Boolean， Succeeded As Boolean)

#Region "   ---   Types"

        ''' <summary>
        ''' 画线时的检查模式
        ''' </summary>
        Public Enum CurveCheckMode

            ''' <summary>
            ''' 所绘制的模型线并不要求相连，也就是不进行任何检测。
            ''' </summary>
            Seperated = 1

            ''' <summary>
            ''' 集合中的线条在整体上是连续的，但是线条之间的顺序可能是混乱的。
            ''' </summary>
            Connected = 2

            ''' <summary>
            ''' 集合中的曲线在同一个平面上，但是并不一定是连续的
            ''' </summary>
            InPlan = 4

            '''' <summary>
            '''' 当前绘制的线条与上一条曲线是相连的，但是这两条曲线不一定是首尾相接，而也有可能是“首-首”或者是“尾-尾”相连。
            '''' </summary>
            'Contiguous = 2

        End Enum

#End Region

#Region "   ---   Properties"

        ''' <summary>
        ''' 已经绘制的所有模型线
        ''' </summary>
        ''' <returns></returns>
        Public Property AddedModelCurves As List(Of ModelCurve)

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

        ''' <summary>
        ''' 集合中最多绘制多少条曲线
        ''' </summary>
        Private F_MaxCurves As Integer
        ''' <summary>
        ''' 集合中最多绘制多少条曲线，如果不指定此属性的值，可默认可以绘制Interger.MaxValue
        ''' </summary>
        Public ReadOnly Property MaxCurves As Integer
            Get
                Return F_MaxCurves
            End Get
        End Property

        ''' <summary> 模型线绘制器是否正在使用中。 </summary>
        Public Shared Property IsBeenUsed As Boolean
#End Region

#Region "   ---   Fields"

        Private rvtUiApp As UIApplication
        Private rvtApp As Autodesk.Revit.ApplicationServices.Application

        ''' <summary> 上一条曲线 </summary>
        Private lastCurve As ModelCurve
        ''' <summary> 刚刚新添加的曲线 </summary>
        Private NewlyAddedCurve As ModelCurve

        ''' <summary>
        ''' 画笔是否是由内部程序自己关闭的。如果是外部对象通过调用Cancel方法来取消绘制的，则其值为False。
        ''' </summary>
        Private FinishedInternally As Boolean
        ''' <summary>
        ''' AddedModelCurves 集合中的曲线集合是否满足指定的连续性条件
        ''' </summary>
        Private Succeeded As Boolean

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
        ''' 当指定连续性条件为新绘制的曲线必须与上一条曲线相连时，基准曲线集合中的最后一条将作为“上一次绘制的曲线”。
        ''' </param>
        ''' <param name="Max">集合中最多绘制多少条曲线，如果不指定此属性的值，可默认可以绘制Interger.MaxValue</param>
        Public Sub New(ByVal uiApp As UIApplication,
                       ByVal CheckMode As CurveCheckMode,
                       ByVal CheckInTime As Boolean,
                       Optional ByVal BaseCurves As List(Of ModelCurve) = Nothing,
                       Optional Max As Integer = Integer.MaxValue)

            ' 变量初始化
            rvtUiApp = uiApp
            rvtApp = uiApp.Application
            Me.F_CheckMode = CheckMode
            Me.F_CheckInTime = CheckInTime
            Me.F_MaxCurves = Max
            Me.FinishedInternally = True

            ' 处理已经添加好的基准曲线集合
            Me.AddedModelCurves = BaseCurves
            If Me.AddedModelCurves Is Nothing Then
                Me.AddedModelCurves = New List(Of ModelCurve)
            End If
            If Me.AddedModelCurves.Count > 0 Then
                Me.lastCurve = Me.AddedModelCurves.First
            End If
        End Sub

        ''' <summary>
        ''' 绘制完成或者从外部取消曲线绘制
        ''' </summary>
        Private Sub Dispose()

            RemoveHandler rvtApp.DocumentChanged, AddressOf app_DocumentChanged

            ModelCurveDrawer.IsBeenUsed = False

            ' 变量清空
            rvtUiApp = Nothing
            rvtApp = Nothing
            AddedModelCurves = Nothing
        End Sub

#End Region

        ' 绘图与判断处理
        ''' <summary>
        ''' 在UI界面中绘制模型线。此方法为异步操作，程序并不会等待SendDraw方法执行完成才继续向下执行。
        ''' </summary>
        Public Sub SendDraw()
            If ModelCurveDrawer.IsBeenUsed Then
                Throw New InvalidOperationException("有其他的对象正在绘制模型线，请等待其绘制完成后再次启动。")
            Else
                AddHandler rvtApp.DocumentChanged, AddressOf app_DocumentChanged
                ActiveDraw()
                ModelCurveDrawer.IsBeenUsed = True
            End If
        End Sub

        ''' <summary>
        ''' DocumentChanged事件，并针对不同的绘制情况而进行不同的处理
        ''' </summary>
        ''' <param name="sender">Application对象</param>
        ''' <param name="e"></param>
        Private Sub app_DocumentChanged(sender As Object, e As DocumentChangedEventArgs)
            MessageBox.Show(AddedModelCurves.Count)
            If e.Operation = UndoOperation.TransactionCommitted OrElse
                e.Operation = UndoOperation.TransactionUndone OrElse
                e.Operation = UndoOperation.TransactionRedone Then

                Dim doc As Document = e.GetDocument

                Try

                    ' 先考察添加的对象：如果添加了新对象，则要么是 DrawNewLines ，要么是 DrawOtherObjects
                    Dim addedCount As Integer = 0
                    Dim addedElement As Element
                    For Each eid As ElementId In e.GetAddedElementIds
                        addedElement = doc.GetElement(eid)
                        If (TypeOf addedElement Is ModelCurve) Then
                            AddedModelCurves.Add(DirectCast(addedElement, ModelCurve))
                            addedCount += 1
                        End If
                    Next
                    If addedCount > 0 Then ' 说明绘制了新的模型线
                        ' 检测当前集合中的曲线是否符合指定的连续性要求
                        If Me.CheckInTime Then
                            Dim curveValidated As Boolean = ValidateCurves()
                            If curveValidated Then  ' 说明在新添加了这一条曲线后，集合中的曲线还是符合指定的连续性要求
                                Me.Succeeded = True

                            Else  ' 说明在新添加了这一条曲线后，集合中的曲线不符合指定的连续性要求了，但是在添加之前的曲线集合还是符合的。
                                Me.Succeeded = False
                                If InquireUndo() Then

                                    rvtTools.Undo()
                                    Exit Sub
                                Else
                                    ' 绘制结束
                                    Me.Finish(True, False)
                                    Exit Sub
                                End If
                            End If
                        Else ' 说明不进行实时检测，而直接继续绘制
                            Me.Succeeded = True
                        End If
                        '
                        Exit Sub
                    End If

                    '
                    ' 再考察删除对象的情况
                    Dim deleted As List(Of ElementId) = e.GetDeletedElementIds
                    If deleted.Count > 0 Then

                        ' 先将被删除的曲线从曲线链集合中剔除掉
                        Dim c As ModelCurve
                        Dim id_Chain As Integer ' 曲线链中的元素下标
                        Dim id_deleted As Integer  ' 删除的模型线集合中的元素下标
                        For id_Chain = AddedModelCurves.Count - 1 To 0 Step -1  ' 曲线链中的元素下标
                            c = AddedModelCurves.Item(id_Chain)
                            id_deleted = deleted.IndexOf(c.Id)  ' 找到对应的项
                            If id_deleted >= 0 Then
                                deleted.RemoveAt(id_deleted)
                                AddedModelCurves.RemoveAt(id_Chain)
                            End If
                        Next

                        ' 检测剔除后的集合中的曲线是否符合指定的连续性要求
                        If Me.CheckInTime Then
                            Dim curveValidated As Boolean = ValidateCurves()
                            If curveValidated Then  ' 说明在新添加了这一条曲线后，集合中的曲线还是符合指定的连续性要求
                                Me.Succeeded = True
                            Else  ' 说明在删除了某些曲线后，集合中的曲线不符合指定的连续性要求了
                                Me.Succeeded = False
                                If InquireUndo() Then

                                    rvtTools.Undo()
                                    Exit Sub
                                Else
                                    ' 绘制结束
                                    Me.Finish(True, False)
                                    Exit Sub
                                End If
                            End If
                        Else ' 说明不进行实时检测，而直接继续绘制
                            Me.Succeeded = True
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

                        ' 检测修改后的集合中的曲线是否符合指定的连续性要求
                        If Me.CheckInTime Then
                            Dim curveValidated As Boolean = ValidateCurves()
                            If curveValidated Then  ' 说明在新添加了这一条曲线后，集合中的曲线还是符合指定的连续性要求
                                Me.Succeeded = True

                            Else  ' 说明在删除了某些曲线后，集合中的曲线不符合指定的连续性要求了
                                Me.Succeeded = False
                                If InquireUndo() Then

                                    rvtTools.Undo()
                                    Exit Sub
                                Else
                                    ' 绘制结束
                                    Me.Finish(True, False)
                                    Exit Sub
                                End If
                            End If
                        Else ' 说明不进行实时检测，而直接继续绘制
                            Me.Succeeded = True

                        End If
                        '
                        Exit Sub
                    End If

                Catch ex As Exception
                    MessageBox.Show("在绘制模型线及连续性判断时出问题啦~~~" & vbCrLf &
                                           ex.Message & ex.GetType.FullName & vbCrLf &
                                           ex.StackTrace)
                    '绘制结束
                    Me.Finish(True, False)
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

        ''' <summary>
        ''' 取消模型线的绘制
        ''' </summary>
        Public Sub Cancel()
            Me.FinishedInternally = False
            ' 如果是实时检测，则这里不用再检测了，否则，必须进行最终的检测
            If Not F_CheckInTime Then
                Succeeded = ValidateCurves()
            End If
            Me.Finish(FinishedInternally, Succeeded)
        End Sub

        ''' <summary>
        ''' 绘制完成，并关闭绘制模式
        ''' </summary>
        ''' <param name="FinishedInternally">画笔是否是由内部程序自己关闭的。如果是外部对象通过调用Cancel方法来取消绘制的，则其值为False。</param>
        ''' <param name="Succeeded">AddedModelCurves 集合中的曲线集合是否满足指定的连续性条件</param>
        Private Sub Finish(ByVal FinishedInternally As Boolean, Succeeded As Boolean)
            RaiseEvent DrawingCompleted(AddedModelCurves, FinishedInternally, Succeeded)
            Me.Dispose()
            '
            DeactiveDraw()
        End Sub

#End Region

        ' 曲线连续性要求的检测
        ''' <summary>
        ''' 检测当前集合中的曲线是否符合指定的连续性要求
        ''' </summary>
        ''' <returns></returns>
        Private Function ValidateCurves() As Boolean
            Dim blnValidated As Boolean = False

            ' 根据不同的模式进行不同的检测
            Select Case Me.F_CheckMode
                Case CurveCheckMode.Connected
                    If CurvesFormator.GetContiguousCurvesFromModelCurves(Me.AddedModelCurves) IsNot Nothing Then
                        blnValidated = True
                    End If
                Case CurveCheckMode.InPlan
                    Return False
                Case CurveCheckMode.Seperated
                    ' 不用检测，直接符合
                    blnValidated = True
            End Select
            Return blnValidated
        End Function

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