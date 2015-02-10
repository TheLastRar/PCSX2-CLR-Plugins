Public Class ManagedMessageLoopManager
    Private frmBuild As ManagedMessageLoop
    Sub New(WndProc As ManagedMessageLoop.WndProcDelegate)
        Dim th As System.Threading.Thread = New Threading.Thread(AddressOf Me.MessageThread)
        th.SetApartmentState(System.Threading.ApartmentState.STA)
        th.Start(WndProc)
    End Sub

    Private Sub MessageThread(obj As Object)
        Dim WndProc As ManagedMessageLoop.WndProcDelegate = CType(obj, ManagedMessageLoop.WndProcDelegate)
        frmBuild = New ManagedMessageLoop
        frmBuild.WndProcHook = WndProc
        Windows.Forms.Application.Run(frmBuild)
    End Sub

    Public Sub Close()
        frmBuild.BeginInvoke(New Action(Sub() frmBuild.Close()))
    End Sub

End Class
