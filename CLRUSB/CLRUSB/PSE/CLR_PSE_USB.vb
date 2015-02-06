Namespace Global.PSE
    Public MustInherit Class CLR_PSE_USB
        Inherits CLR_PSE_Base

        'This varies from plugin to plugin, 
        'though it's unlikely for this to be used for those pluings
        Public MustOverride Function USBopen(hWnd As IntPtr) As Int32

        Public MustOverride Function USBread8(addr As UInt32) As Byte
        Public MustOverride Function USBread16(addr As UInt32) As UInt16
        Public MustOverride Function USBread32(addr As UInt32) As UInt32
        Public MustOverride Sub USBwrite8(addr As UInt32, value As Byte)
        Public MustOverride Sub USBwrite16(addr As UInt32, value As UInt16)
        Public MustOverride Sub USBwrite32(addr As UInt32, value As UInt32)
        Public MustOverride Sub USBirqCallback(callback As CLR_PSE_Callbacks.CLR_CyclesCallback)
        Public MustOverride Function _USBirqHandler() As Int32
        Public MustOverride Sub USBsetRam(mem As IO.UnmanagedMemoryStream)


        Public MustOverride Sub USBasync(cycles As UInteger)
    End Class
End Namespace