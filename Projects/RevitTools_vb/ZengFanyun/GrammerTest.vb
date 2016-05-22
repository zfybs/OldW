Public Class GrammerTest

    Private Event event1(ByVal UserName As String)
    Private Event event2 As Action(Of String)
    Private Event event3 As EventHandler(Of String)

    ' Declare the delegate (if using non-generic pattern). 
    Public Delegate Sub SampleEventHandler()

    ' Declare the event. 
    Public Event event4 As SampleEventHandler

    Public Sub New()
        RaiseEvent event1("")
        RaiseEvent event2("")
        RaiseEvent event3(Me, "")
        RaiseEvent event4()
    End Sub

End Class