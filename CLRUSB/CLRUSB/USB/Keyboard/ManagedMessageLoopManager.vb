Namespace USB.Keyboard
    Class ManagedMessageLoopManager
        Private frmBuild As ManagedMessageLoop
        Sub New(WndProc As RAW_Keyboard.SubClassProcDelegate)
            Dim th As System.Threading.Thread = New Threading.Thread(AddressOf Me.MessageThread)
            th.SetApartmentState(System.Threading.ApartmentState.STA)
            th.Start(WndProc)
        End Sub

        Private Sub MessageThread(obj As Object)
            Dim WndProc As RAW_Keyboard.SubClassProcDelegate = CType(obj, RAW_Keyboard.SubClassProcDelegate)
            frmBuild = New ManagedMessageLoop
            frmBuild.WndProcHook = WndProc
            Windows.Forms.Application.Run(frmBuild)
        End Sub

        Public Sub Close()
            frmBuild.BeginInvoke(New Action(Sub() frmBuild.Close()))
        End Sub

    End Class
End Namespace
