Namespace Global.PSE.CLR_PSE_Callbacks

    Public Delegate Sub Close() 'Logs+config
    'Logs
    Public Delegate Function Log_Open(logname As String) As Boolean
    Public Delegate Sub Log_Write(fmt As String)
    Public Delegate Sub Log_SetValue(par As Boolean)

    'USBCallback
    Public Delegate Sub CLR_CyclesCallback(cycles As Integer)

    'Config
    Public Delegate Function Config_Open(filename As String, writeAccess As Boolean) As Boolean
    Public Delegate Function Config_ReadInt(item As String, DefualtValue As Integer) As Integer
    Public Delegate Sub Config_WriteInt(item As String, Value As Integer)
End Namespace
