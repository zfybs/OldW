Imports Forms = System.Windows.Forms
Imports OldW.Soil
Imports std_ez
Imports System.ComponentModel
Imports rvtTools_ez
Imports System.Threading

Namespace OldW.Excavation

    Partial Class frm_DrawExcavation

#Region "   ---   Types"

        ''' <summary>
        ''' 每一个外部事件调用时所提出的需求，为了在Execute方法中充分获取窗口的需求，
        ''' 所以将调用外部事件的窗口控件以及对应的触发事件参数也传入Execute方法中。
        ''' </summary>
        ''' <remarks></remarks>
        Private Class RequestParameter

            Private F_sender As Object
            ''' <summary> 引发Form事件控件对象 </summary>
            Public ReadOnly Property sender As Object
                Get
                    Return F_sender
                End Get
            End Property

            Private F_e As EventArgs
            ''' <summary> Form中的事件所对应的事件参数 </summary>
            Public ReadOnly Property e As EventArgs
                Get
                    Return F_e
                End Get
            End Property

            Private F_Id As Request
            ''' <summary> 具体的需求 </summary>
            Public ReadOnly Property Id As Request
                Get
                    Return F_Id
                End Get
            End Property

            ''' <summary>
            ''' 定义事件需求与窗口中引发此事件的控件对象及对应的事件参数
            ''' </summary>
            ''' <param name="RequestId">具体的需求</param>
            ''' <param name="e">Form中的事件所对应的事件参数</param>
            ''' <param name="sender">引发Form事件控件对象</param>
            ''' <remarks></remarks>
            Public Sub New(ByVal RequestId As Request, Optional e As EventArgs = Nothing, Optional ByVal sender As Object = Nothing)
                With Me
                    .F_sender = sender
                    .F_e = e
                    .F_Id = RequestId
                End With
            End Sub
        End Class

        ''' <summary>
        ''' ModelessForm的操作需求，用来从窗口向IExternalEventHandler对象传递需求。
        ''' </summary>
        ''' <remarks></remarks>
        Private Enum Request

            ''' <summary>
            ''' 通过在UI界面绘制模型线来作为土体的轮廓
            ''' </summary>
            DrawCurves

            ''' <summary>
            ''' 删除绘制好的模型线并清空曲线集合数据
            ''' </summary>
            DeleteCurves

            ''' <summary>
            ''' 开始建模
            ''' </summary>
            StartModeling
        End Enum

#End Region

    End Class
End Namespace