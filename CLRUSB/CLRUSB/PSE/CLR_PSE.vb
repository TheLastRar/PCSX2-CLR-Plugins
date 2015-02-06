Imports CLRUSB

Namespace Global.PSE
    Public Class CLR_PSE
        'Multi-in-one is not supported
        Private Enum CLR_Type As Integer
            GS = 1
            PAD = 2
            SPU2 = 4
            CDVD = 8
            USB = 16
            FW = 32
            DEV9 = 64
        End Enum
        Public Const revision As Byte = 0 'major
        Public Const build As Byte = 6 'minor

        Private Const libraryName As String = "CLR USB Keyboard"
        Public Shared Function PS2EgetLibName() As String
            Return libraryName
        End Function

        Public Shared Function PS2EgetLibType() As Integer
            Return CLR_Type.USB
        End Function

        Public Shared Function NewUSB() As CLR_PSE_USB
            Return New CLR_USB
        End Function
        Public Shared Function NewDEV9() As CLR_PSE_DEV9
            Return Nothing
        End Function
        Public Shared Function NewLog() As CLR_PSE_PluginLog
            Return New CLR_PSE_PluginLog
        End Function
        Public Shared Function NewConfig() As CLR_PSE_Config
            Return New Config.CLR_Config
        End Function
    End Class

    Public MustInherit Class CLR_PSE_Base
        Public PluginLog As CLR_PSE_PluginLog
        Protected PluginConf As CLR_PSE_Config

        Public MustOverride Function Init(wrapperRev As Byte, wrapperBuid As Byte) As Int32
        Public MustOverride Sub Shutdown()

        Public MustOverride Sub Close()

        Public MustOverride Sub SetSettingsDir(dir As String)
        Public MustOverride Sub SetLogDir(dir As String)

        Public MustOverride Function FreezeSave() As Byte()
        Public MustOverride Function FreezeLoad(data As Byte()) As Int32
        Public MustOverride Function FreezeSize() As Int32

        Public MustOverride Function Test() As Int32

        Public Sub SetConfig(pPluginConf As CLR_PSE_Config)
            PluginConf = pPluginConf
            PluginConf.SetBase(Me)
        End Sub
    End Class

    Public MustInherit Class CLR_PSE_Config
        Protected Base As CLR_PSE_Base

        Public IniFolderPath As String = "inis"
        'PluginConf calls
        Public Open As CLR_PSE_Callbacks.Config_Open
        Public Close As CLR_PSE_Callbacks.Close
        Public WriteInt As CLR_PSE_Callbacks.Config_WriteInt
        Public ReadInt As CLR_PSE_Callbacks.Config_ReadInt

        Public Sub SetBase(common As CLR_PSE_Base)
            Base = common
        End Sub

        Public MustOverride Sub About()
        Public MustOverride Sub Configure()

        Public MustOverride Sub LoadConfig()
        Protected MustOverride Sub SaveConfig()
    End Class
End Namespace
