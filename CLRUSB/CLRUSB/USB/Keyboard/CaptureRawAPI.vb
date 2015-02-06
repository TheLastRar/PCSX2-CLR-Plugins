Public Class CaptureRawAPI

    Private Sub CaptureRawAPI_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Protected Overrides Sub WndProc(ByRef m As Windows.Forms.Message)
        Console.Error.WriteLine("Got Managed WM")
        If m.Msg = 255 Then
            Console.Error.WriteLine("Got WM_Input")
        End If
        MyBase.WndProc(m)
    End Sub
End Class