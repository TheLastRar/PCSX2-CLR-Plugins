Imports System.Runtime.InteropServices

Public Class ManagedMessageLoop
    Public Delegate Function WndProcDelegate(ByVal hwnd As IntPtr, ByVal msg As UInteger, ByVal wParam As UIntPtr, ByVal lParam As IntPtr) As IntPtr
    Public WndProcHook As WndProcDelegate

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        WndProcHook = New WndProcDelegate(AddressOf DoNothing)
    End Sub

    Protected Function DoNothing(ByVal hwnd As IntPtr, ByVal msg As UInteger, ByVal wParam As UIntPtr, ByVal lParam As IntPtr) As IntPtr
        Return IntPtr.Zero
    End Function

    <StructLayout(LayoutKind.Explicit)> _
    Private Structure UnionInt32
        <FieldOffset(0)> _
        Public IntValue As Int32
        <FieldOffset(0)> _
        Public UIntValue As UInt32
    End Structure
    <StructLayout(LayoutKind.Explicit)> _
    Private Structure UnionIntPtr
        <FieldOffset(0)> _
        Public IntValue As IntPtr
        <FieldOffset(0)> _
        Public UIntValue As UIntPtr
    End Structure

    Protected Overrides Sub WndProc(ByRef m As Windows.Forms.Message)
        Dim uMsg As UnionInt32
        uMsg.IntValue = m.Msg
        Dim uWParm As UnionIntPtr
        uWParm.IntValue = m.WParam
        WndProcHook(m.HWnd, uMsg.UIntValue, uWParm.UIntValue, m.LParam)
        MyBase.WndProc(m)
    End Sub
End Class
