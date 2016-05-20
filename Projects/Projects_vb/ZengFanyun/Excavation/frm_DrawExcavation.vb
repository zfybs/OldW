Imports Forms = System.Windows.Forms
Imports OldW.Soil
Imports std_ez
Imports System.ComponentModel
Imports rvtTools_ez
Imports System.Threading

Namespace OldW.Excavation

    ''' <summary>
    ''' 无模态窗口的模板
    ''' 此窗口可以直接通过Form.Show来进行调用
    ''' </summary>
    ''' <remarks></remarks>
    Public Class frm_DrawExcavation
        Implements IExternalEventHandler

#Region "   ---   Declarations"


#Region "   ---   Fields"

        ''' <summary>用来触发外部事件（通过其Raise方法） </summary>
        ''' <remarks>ExEvent属性是必须有的，它用来执行Raise方法以触发事件。</remarks>
        Private ExEvent As ExternalEvent

        ''' <summary> Execute方法所要执行的需求 </summary>
        ''' <remarks>在Form中要执行某一个操作时，先将对应的操作需求信息赋值为一个RequestId枚举值，然后再执行ExternalEvent.Raise()方法。
        ''' 然后Revit会在会在下个闲置时间（idling time cycle）到来时调用IExternalEventHandler.Excute方法，在这个Execute方法中，
        ''' 再通过RequestId来提取对应的操作需求，</remarks>
        Private RequestPara As RequestParameter

        Private Document As Document

        Public ExcavDoc As ExcavationDoc

        ''' <summary> 要绘制的模型的深度，单位为m </summary>
        Private Depth As Double

        ''' <summary> 开挖土体开挖完成的日期 </summary>
        Private CompletedDate As Nullable(Of Date)

        ''' <summary> 开挖土体开始开挖的日期 </summary>
        Private StartedDate As Nullable(Of Date)

        ''' <summary>
        ''' 为开挖土体或者模型墙体预设的名称
        ''' </summary>
        Private DesiredName As String

#End Region

#End Region

#Region "   ---   构造函数与窗口的打开关闭"
        Public Sub New(ByVal ExcavDoc As ExcavationDoc)
            ' This call is required by the designer.
            InitializeComponent()
            ' Add any initialization after the InitializeComponent() call.
            '' ----------------------

            ' Me.TopMost = True
            Me.StartPosition = FormStartPosition.CenterScreen

            ' 参数绑定
            LabelCompletedDate.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", False, DataSourceUpdateMode.OnPropertyChanged)
            btn__DateCalendar.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", False, DataSourceUpdateMode.OnPropertyChanged)
            TextBox_StartedDate.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", False, DataSourceUpdateMode.OnPropertyChanged)
            TextBox_CompletedDate.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", False, DataSourceUpdateMode.OnPropertyChanged)
            TextBox_SoilName.DataBindings.Add("Enabled", RadioBtn_ExcavSoil, "Checked", False, DataSourceUpdateMode.OnPropertyChanged)
            LabelSides.DataBindings.Add("Enabled", RadioBtn_Polygon, "Checked", False, DataSourceUpdateMode.OnPropertyChanged)
            ComboBox_sides.DataBindings.Add("Enabled", RadioBtn_Polygon, "Checked", False, DataSourceUpdateMode.OnPropertyChanged)
            btn_DrawCurves.DataBindings.Add("Enabled", RadioBtn_Draw, "Checked", False, DataSourceUpdateMode.OnPropertyChanged)
            drawnCurveArrArr = Nothing
            '
            Me.ExcavDoc = ExcavDoc
            Me.Document = ExcavDoc.Document

            '' ------ 将所有的初始化工作完成后，执行外部事件的绑定 ----------------
            ' 新建一个外部事件实例
            Me.ExEvent = ExternalEvent.Create(Me)
        End Sub

        Private Sub frm_DrawExcavation_Closed(sender As Object, e As EventArgs) Handles Me.Closed
            ' 保存的实例需要进行释放
            If ModelCurvesDrawer.IsBeenUsed AndAlso Me.ClosedCurveDrawer IsNot Nothing Then
                RemoveHandler ClosedCurveDrawer.DrawingCompleted, AddressOf ClosedCurveDrawer_DrawingCompleted
                ' ClosedCurveDrawer.cancel()
                ClosedCurveDrawer = Nothing
            End If
            '
            Me.ExEvent.Dispose()
            Me.ExEvent = Nothing
        End Sub

        Public Function GetName() As String Implements IExternalEventHandler.GetName
            Return "绘制基坑的模型土体与开挖土体。"
        End Function

        Protected Overrides Sub OnClosing(e As CancelEventArgs)
            MyBase.OnClosing(e)
        End Sub
#End Region

#Region "   ---   界面效果与事件响应"

        ''' <summary> 在Revit执行相关操作时，禁用窗口中的控件 </summary>
        Private Sub DozeOff()
            Me.BtnModeling.Enabled = False
        End Sub

        ''' <summary> 在外部事件RequestHandler中的Execute方法执行完成后，用来激活窗口中的控件 </summary>
        Private Sub WarmUp()
            For Each c As Forms.Control In Me.Controls
                c.Enabled = True
            Next
        End Sub

        Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox_Depth.TextChanged, TextBox_SoilName.TextChanged
            If Not Double.TryParse(TextBox_Depth.Text, Depth) Then
                TextBox_Depth.Text = ""
            End If
        End Sub

#End Region


#Region "   ---   执行操作 ExternalEvent.Raise 与 IExternalEventHandler.Execute"

        ' 绘制模型线
        Private Sub btn_DrawCurves_Click(sender As Object, e As EventArgs) Handles btn_DrawCurves.Click
            Me.RequestPara = New RequestParameter(Request.DrawCurves, e, sender)
            Me.ExEvent.Raise()
            Me.DozeOff()
        End Sub
        '删除模型线
        Private Sub Btn_ClearCurves_Click(sender As Object, e As EventArgs) Handles Btn_ClearCurves.Click
            Me.RequestPara = New RequestParameter(Request.DeleteCurves, e, sender)
            Me.ExEvent.Raise()
            Me.DozeOff()
        End Sub
        ' 建模
        Private Sub BtnModeling_Click(sender As Object, e As EventArgs) Handles BtnModeling.Click

            Dim blnDraw As Boolean = CheckUI()
            If blnDraw Then
                Me.RequestPara = New RequestParameter(Request.StartModeling, e, sender)
                '
                Me.ExEvent.Raise()
                Me.DozeOff()
            End If
            '
        End Sub

        ''' <summary>
        ''' 对窗口中的数据进行检测，并判断是否可以进行绘制
        ''' </summary>
        Private Function CheckUI() As Boolean
            Dim blnDraw As Boolean = True
            ' 提取开挖深度
            If Me.Depth = 0 Then
                MessageBox.Show("深度值不能为0。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If

            Dim strDate As String

            ' 提取开始开挖的时间

            strDate = Me.TextBox_StartedDate.Text
            If Not String.IsNullOrEmpty(strDate) Then
                If Utils.String2Date(strDate, Me.StartedDate) Then  ' 说明不能直接转化为日期

                Else
                    MessageBox.Show("请输入正确格式的开挖完成日期。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return False
                End If
            End If

            strDate = Me.TextBox_CompletedDate.Text
            If Not String.IsNullOrEmpty(strDate) Then
                If Utils.String2Date(strDate, Me.CompletedDate) Then  ' 说明不能直接转化为日期
                    DesiredName = Me.CompletedDate.Value.ToShortDateString
                Else
                    MessageBox.Show("请输入正确格式的开挖完成日期。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return False
                End If
            End If

            ' 是否有指定开挖土体的名称
            If Not String.IsNullOrEmpty(Me.TextBox_SoilName.Text) Then
                DesiredName = Me.TextBox_SoilName.Text
            End If
            Return blnDraw
        End Function

        ''为每一项操作执行具体的实现
        ''' <summary>
        ''' 在执行ExternalEvent.Raise()方法之前，请先将操作需求信息赋值给其RequestHandler对象的RequestId属性。
        ''' 当ExternalEvent.Raise后，Revit会在下个闲置时间（idling time cycle）到来时调用IExternalEventHandler.Execute方法的实现。
        ''' </summary>
        ''' <param name="app">此属性由Revit自动提供，其值不是Nothing，而是一个真实的UIApplication对象</param>
        ''' <remarks>由于在通过外部程序所引发的操作中，如果出现异常，Revit并不会给出任何提示或者报错，
        ''' 而是直接退出函数。所以要将整个操作放在一个Try代码块中，以处理可能出现的任何报错。</remarks>
        Public Sub Execute(uiApp As UIApplication) Implements IExternalEventHandler.Execute

            Try  ' 由于在通过外部程序所引发的操作中，如果出现异常，Revit并不会给出任何提示或者报错，而是直接退出函数。所以这里要将整个操作放在一个Try代码块中，以处理可能出现的任何报错。
                Dim uiDoc As UIDocument = New UIDocument(Document)


                ' 开始执行具体的操作
                Select Case RequestPara.Id  ' 判断具体要干什么

                    Case Request.DrawCurves
                        ' -------------------------------------------------------------------------------------------------------------------------

                        ' 先确定模型土体或者开挖土体的轮廓曲线
                        drawnCurveArrArr = Nothing

                        ' 绘制轮廓
                        Me.ClosedCurveDrawer = New ClosedCurvesDrawer(uiApp, True, Nothing)
                        Me.ClosedCurveDrawer.PostDraw()

                        ' 由于 PostDraw 是异步操作，所以这里不能直接获得绘制好的模型线
                        ' 而是要在 Drawer_DrawingCompleted 事件中来构造土体的轮廓线
                        Exit Sub

                    Case Request.DeleteCurves
                        If drawnCurveArrArr IsNot Nothing Then
                            ' 删除模型线
                            Using t As New Transaction(Document, "删除绘制好的模型线。")
                                Dim elemIds As New List(Of ElementId)
                                t.Start()
                                Try
                                    For Each ca As List(Of ElementId) In DrawnCurveIds
                                        For Each cid As ElementId In ca
                                            elemIds.Add(cid)
                                        Next
                                    Next
                                    Document.Delete(elemIds)
                                    t.Commit()
                                Catch ex As Exception
                                    t.RollBack()
                                End Try
                            End Using

                            ' 清空数据
                            drawnCurveArrArr = Nothing
                        End If
                    Case Request.StartModeling  ' 通过 多边形的族样板 来直接放置土体模型 
                        ' 考虑不同的建模方式
                        ' -------------------------------------------------------------------------------------------------------------------------
                        If Me.RadioBtn_Draw.Checked = True Then
                            If Me.drawnCurveArrArr IsNot Nothing Then

                                ' 根据选择好的轮廓线来进行土体的建模
                                Call DrawSoilFromCurve(drawnCurveArrArr)
                            Else
                                MessageBox.Show("请先绘制好要进行建模的封闭轮廓。")
                            End If
                            ' -------------------------------------------------------------------------------------------------------------------------
                        ElseIf Me.RadioBtn_PickShape.Checked = True Then

                            ' 先确定模型土体或者开挖土体的轮廓曲线
                            Dim CurveArrArr As CurveArrArray = Nothing

                            ' 选择轮廓
                            Dim cs As New ClosedCurveSelector(uiDoc, True)
                            CurveArrArr = cs.SendSelect()

                            ' 根据选择好的轮廓线来进行土体的建模
                            Call DrawSoilFromCurve(CurveArrArr)

                            ' -------------------------------------------------------------------------------------------------------------------------
                        ElseIf Me.RadioBtn_Polygon.Checked = True Then


                        End If

                End Select
            Catch ex As Exception
                MessageBox.Show("出错" & vbCrLf & ex.Message & vbCrLf & ex.TargetSite.Name & vbCrLf & ex.StackTrace,
                                "外部事件执行出错", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                ' 刷新Form，将Form中的Controls的Enable属性设置为True
                Me.WarmUp()
            End Try
        End Sub

        ''' <summary>
        ''' 根据绘制或者选择出来的土体轮廓模型线来进行模型土体或者开挖土体的建模
        ''' </summary>
        ''' <param name="CurveArrArr"></param>
        Private Sub DrawSoilFromCurve(ByVal CurveArrArr As CurveArrArray)

            Dim soil As Soil_Model

            If Me.RadioBtn_ExcavSoil.Checked Then   ' 绘制开挖土体

                soil = Me.ExcavDoc.FindSoilModel()
                Dim ex As Soil_Excav = Me.ExcavDoc.CreateExcavationSoil(soil, Me.Depth, CurveArrArr, DesiredName)

                ' 设置开挖开始或者完成的时间
                If (Me.StartedDate IsNot Nothing) OrElse (Me.CompletedDate IsNot Nothing) Then
                    Using t As New Transaction(Document, "设置开挖开始或者完成的时间")
                        t.Start()
                        If (Me.StartedDate IsNot Nothing) Then
                            ex.SetExcavatedDate(t, True, StartedDate.Value)
                        End If
                        If (Me.CompletedDate IsNot Nothing) Then
                            ex.SetExcavatedDate(t, False, CompletedDate.Value)
                        End If
                        t.Commit()
                    End Using
                End If

                ' 将开挖土体在模型土体中隐藏起来
                soil.RemoveSoil(ex)

            Else    ' 绘制模型土体
                ' 获得用来创建实体的模型线
                soil = Me.ExcavDoc.CreateModelSoil(Me.Depth, CurveArrArr)

            End If

        End Sub

#End Region

#Region "   ---   绘制土体轮廓 "

        ''' <summary>
        ''' 用来绘制封闭的模型线
        ''' </summary>
        Private WithEvents ClosedCurveDrawer As ClosedCurvesDrawer

        ' 绘制好的模型线
        Private DrawnCurveIds As List(Of List(Of ElementId))
        Private F_drawnCurveArrArr As CurveArrArray
        ''' <summary>
        ''' 通过界面绘制出来的模型线
        ''' </summary>
        Private Property drawnCurveArrArr As CurveArrArray
            Get
                Return F_drawnCurveArrArr
            End Get
            Set(value As CurveArrArray)
                If value Is Nothing Then
                    Me.CheckBox_DrawSucceeded.Checked = False
                    Btn_ClearCurves.Enabled = False
                Else
                    Me.CheckBox_DrawSucceeded.Checked = True
                    Btn_ClearCurves.Enabled = True
                End If
                F_drawnCurveArrArr = value
            End Set
        End Property

        Private Sub ClosedCurveDrawer_DrawingCompleted(AddedCurves As List(Of List(Of ElementId)), FinishedExternally As Boolean,
                                                       Succeeded As Boolean) Handles ClosedCurveDrawer.DrawingCompleted
            If Succeeded Then
                ' 构造土体轮廓
                drawnCurveArrArr = New CurveArrArray
                Dim c As ModelCurve
                '
                For Each cs As List(Of ElementId) In AddedCurves
                    Dim CurveArr As New CurveArray
                    For Each cid As ElementId In cs
                        c = DirectCast(Me.Document.GetElement(cid), ModelCurve)
                        CurveArr.Append(c.GeometryCurve)
                    Next
                    drawnCurveArrArr.Append(CurveArr)
                Next
                DrawnCurveIds = AddedCurves
            Else
                drawnCurveArrArr = Nothing
                '
                Me.WarmUp()
            End If
        End Sub

#End Region

    End Class
End Namespace