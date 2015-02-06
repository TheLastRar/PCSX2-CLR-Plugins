Namespace Global.PSE
    Public MustInherit Class CLR_PSE_DEV9
        Inherits CLR_PSE_Base

        'This varies from plugin to plugin, 
        'though it's unlikely for this to be used for those pluings
        Public MustOverride Function DEV9open(hWnd As IntPtr) As Int32

        Public MustOverride Function DEV9read8(addr As UInt32) As Byte
        Public MustOverride Function DEV9read16(addr As UInt32) As UInt16
        Public MustOverride Function DEV9read32(addr As UInt32) As UInt32
        Public MustOverride Sub DEV9write8(addr As UInt32, value As Byte)
        Public MustOverride Sub DEV9write16(addr As UInt32, value As UInt16)
        Public MustOverride Sub DEV9write32(addr As UInt32, value As UInt32)
        Public MustOverride Sub DEV9readDMA8Mem(addr As IO.UnmanagedMemoryStream, size As Integer)
        Public MustOverride Sub DEV9writeDMA8Mem(addr As IO.UnmanagedMemoryStream, size As Integer)
        Public MustOverride Sub DEV9irqCallback(callback As CLR_PSE_Callbacks.CLR_CyclesCallback)
        Public MustOverride Function _DEV9irqHandler() As Int32

    End Class
End Namespace