Imports System.IO
Namespace Global.PSE
    Public Class CLR_PSE_PluginLog
        Public Open As CLR_PSE_Callbacks.Log_Open
        Public Close As CLR_PSE_Callbacks.Close
        Public LogWrite As CLR_PSE_Callbacks.Log_Write
        Public LogWriteLine As CLR_PSE_Callbacks.Log_Write

        Public SetWriteToFile As CLR_PSE_Callbacks.Log_SetValue
        'Public SetWriteToConsole As CLR_Callbacks.Log_SetValue

        Public Sub ErrorWrite(str As String)
            Console.Error.Write(str) 'log to stderr
            Me.LogWrite(str)
        End Sub
        Public Sub ErrorWriteLine(str As String)
            Console.Error.WriteLine(str) 'log to stderr
            Me.LogWriteLine(str)
        End Sub

    End Class
End Namespace
