Imports CLRUSB.USB
Imports CLRUSB.USB.Keyboard
Imports PSE

Class CLR_USB
    Inherits CLR_PSE_USB

    Private LogFolderPath As String = "logs"

    '2195220e0524e7c289229b36ee1ef2da8dedaf01

    Private Const SaveStateV As Int32 = 2

    Dim qemu_ohci As OHCI.OHCI_State = Nothing
    Dim usb_device1 As USB_Device = Nothing
    Dim usb_device2 As USB_Device = Nothing

    Private Sub Reset()
        If (Not IsNothing(qemu_ohci)) Then
            qemu_ohci.Reset()
            remaining = 0
        End If
    End Sub
    Private Sub DestroyDevices()
        '//FIXME something throws an null ptr exception?
        'Destroy in reverse order
        If (Not IsNothing(qemu_ohci) AndAlso Not IsNothing(qemu_ohci.rhport(PLAYER_TWO_PORT).port.dev)) Then
            qemu_ohci.rhport(PLAYER_TWO_PORT).port.dev.handle_destroy()
            qemu_ohci.rhport(PLAYER_TWO_PORT).port.dev = Nothing
        ElseIf (Not IsNothing(usb_device2)) Then
            usb_device2.handle_destroy()
        End If

        If (Not IsNothing(qemu_ohci) AndAlso Not IsNothing(qemu_ohci.rhport(PLAYER_ONE_PORT).port.dev)) Then
            qemu_ohci.rhport(PLAYER_ONE_PORT).port.dev.handle_destroy()
            qemu_ohci.rhport(PLAYER_ONE_PORT).port.dev = Nothing
        ElseIf (Not IsNothing(usb_device1)) Then '//maybe redundant
            usb_device1.handle_destroy()
        End If

        usb_device1 = Nothing
        usb_device2 = Nothing
    End Sub

    Public Sub CreateDevices()
        If IsNothing(qemu_ohci) Then
            Return
        End If
        DestroyDevices()
        'switch for p0 and p1, which will recreate the devices
        Dim pConfig As Config.CLR_Config = DirectCast(PluginConf, Config.CLR_Config)
        Select Case pConfig.Port1SelectedDevice
            Case Config.SelectedDevice.Keyboard
                usb_device1 = New USB_Keyboard(PluginLog, DirectCast(pConfig.Port1Options, Config.ConfigDataKeyboard))
            Case Else
                'already null
        End Select

        Select Case pConfig.Port2SelectedDevice
            Case Config.SelectedDevice.Keyboard
                usb_device2 = New USB_Keyboard(PluginLog, DirectCast(pConfig.Port2Options, Config.ConfigDataKeyboard))
            Case Else
                'already null
        End Select

        qemu_ohci.rhport(PLAYER_ONE_PORT).port.attach(usb_device1)
        qemu_ohci.rhport(PLAYER_TWO_PORT).port.attach(usb_device2)
    End Sub

    Private Sub LogInit()
        If LogFolderPath.EndsWith(IO.Path.DirectorySeparatorChar) Then
            PluginLog.Open(LogFolderPath + "USB_CLR.log")
        Else
            PluginLog.Open(LogFolderPath + IO.Path.DirectorySeparatorChar + "USB_CLR.log")
        End If
    End Sub

    Public Overrides Function Init(wrapperRev As Byte, wrapperBuild As Byte) As Int32
        PluginConf.LoadConfig()
        LogInit()
        PluginLog.LogWriteLine("CLR Wrapper version " & wrapperRev & "." & wrapperBuild)

        PluginLog.LogWriteLine(CLR_PSE.PS2EgetLibName() & " plugin version " & CLR_PSE.revision & "." & CLR_PSE.build)
        PluginLog.LogWriteLine("Initializing " & CLR_PSE.PS2EgetLibName())

        'Initialize here.
        qemu_ohci = New OHCI.OHCI_State(&H1F801600, 2, PluginLog)
        CreateDevices()
        Return 0
    End Function
    Public Overrides Sub Shutdown()
        'Yes, we close things in the Shutdown routine, and
        'don't do anything in the close routine.
        DestroyDevices()
        PluginLog.Close()
        'The UnmanagedMemoryStream 
        'currently will do NOTHING to free this memory
        If Not IsNothing(qemu_ohci.Ram) Then
            qemu_ohci.Ram.Close()
            qemu_ohci.Ram.Dispose()
        End If
    End Sub
    Public Overrides Function USBopen(hWnd As IntPtr) As Int32
        PluginLog.LogWriteLine("Opening " & CLR_PSE.PS2EgetLibName())
        '// Take care of anything else we need on opening, other then initialization.

        If (Not IsNothing(usb_device1)) Then
            usb_device1.open(hWnd)
        End If
        If (Not IsNothing(usb_device2)) Then
            usb_device2.open(hWnd)
        End If
        Return 0
    End Function
    Public Overrides Sub Close()
        PluginLog.LogWriteLine("Closing " & CLR_PSE.PS2EgetLibName())
        'Close in reverse Order
        If (Not IsNothing(usb_device2)) Then
            usb_device2.close()
        End If

        If (Not IsNothing(usb_device1)) Then
            usb_device1.close()
        End If
    End Sub
    Public Overrides Function USBread8(addr As UInt32) As Byte
        PluginLog.LogWriteLine("(USB) Invalid 8 bit read at address " & addr.ToString("X"))
        Return 0
    End Function
    Public Overrides Function USBread16(addr As UInt32) As UInt16
        PluginLog.LogWriteLine("(USB) Invalid 16 bit read at address " & addr.ToString("X"))
        Return 0
    End Function
    Public Overrides Function USBread32(addr As UInt32) As UInt32
        PluginLog.LogWrite("USB:R32: 32 bit read  at address :0x" & addr.ToString("X8") & ": got  :")
        Dim returnval As UInt32 = qemu_ohci.mem_read(addr)
        PluginLog.LogWriteLine("0x" & returnval.ToString("X8"))
        Return returnval
    End Function

    Public Overrides Sub USBwrite8(addr As UInt32, value As Byte)
        PluginLog.LogWriteLine("(USB) Invalid 8 bit write at address " & addr.ToString("X"))
    End Sub
    Public Overrides Sub USBwrite16(addr As UInt32, value As UInt16)
        PluginLog.LogWriteLine("(USB) Invalid 16 bit write at address " & addr.ToString("X"))
    End Sub
    Public Overrides Sub USBwrite32(addr As UInt32, value As UInt32)
        PluginLog.LogWriteLine("USB:W32: 32 bit write at address :0x" & addr.ToString("X8") & ": with :0x" & value.ToString("X8"))
        qemu_ohci.mem_write(addr, value)
    End Sub
    Public Overrides Sub USBirqCallback(callback As CLR_PSE_Callbacks.CLR_CyclesCallback)
        qemu_ohci.USBirq = callback
    End Sub
    Public Overrides Function _USBirqHandler() As Int32 'void
        '// This is our USB irq handler, so if an interrupt gets triggered,
        '// deal with it here.
        If FULL_DEBUG Then
            PluginLog.LogWriteLine("USB:IRQ: irq Called")
        End If
        Return 1
    End Function
    Public Overrides Sub USBsetRam(mem As IO.UnmanagedMemoryStream)
        qemu_ohci.Ram = mem 'IOPram
        PluginLog.LogWriteLine("USB:*SR: Setting ram")
        Reset()
    End Sub
    Public Overrides Sub SetSettingsDir(dir As String)
        PluginConf.IniFolderPath = dir
    End Sub
    Public Overrides Sub SetLogDir(dir As String)
        LogFolderPath = dir
        PluginLog.Close()
        LogInit()
    End Sub

    Const PaddedSize As Integer = 10 * 1000 '~10kb a little more then 2* what we use for 1 port
    Public Overrides Function FreezeSave() As Byte()
        PluginLog.ErrorWriteLine("SavingState")
        Dim sd As New CLR_FreezeData()
        sd.SetInt32Value("Version", SaveStateV, True)
        sd.SetInt64Value("OHCI.remaining", remaining, True)
        qemu_ohci.Freeze(sd, True)

        Dim sdBytes As Byte() = sd.ToBytes()

        PluginLog.ErrorWriteLine("Pre Padding Size = " & sdBytes.Length & " bytes")

        Dim PaddedData(PaddedSize - 1) As Byte
        memcpy(PaddedData, 0, sdBytes, 0, sdBytes.Length)
        Return PaddedData
    End Function

    Public Overrides Function FreezeLoad(data As Byte()) As Int32
        PluginLog.ErrorWriteLine("LoadingState")
        PluginLog.ErrorWriteLine("Size = " & data.Length & " bytes")
        Try
            Dim sd As New CLR_FreezeData()
            sd.FromBytes(data)
            Dim ssV As Int32 = 0
            sd.SetInt32Value("Version", ssV, False)
            If ssV <> SaveStateV Then
                PluginLog.ErrorWriteLine("Warning, SaveState verison does not match")
            End If
            sd.SetInt64Value("OHCI.remaining", remaining, False)
            qemu_ohci.Freeze(sd, False)
            Return 1
        Catch err As Exception
            PluginLog.ErrorWriteLine("Load Failed: " & err.Message & err.StackTrace)
            Return -1
        End Try
    End Function
    Public Overrides Function FreezeSize() As Int32
        PluginLog.ErrorWriteLine("SizingState")
        'Dim data As Byte() = FreezeSave()
        'PluginLog.ErrorWriteLine("Size = " & data.Length & " bytes")
        'Return data.Length
        Return PaddedSize
    End Function

    Dim remaining As Int64 = 0
    Public Overrides Sub USBasync(cycles As UInteger)
        If FULL_DEBUG Then
            PluginLog.LogWriteLine("USB:ASC: Async :" & cycles)
        End If
        remaining += cycles
        'Overflow check (like that will ever happen)
        If Int64.MaxValue - remaining < qemu_ohci.clocks Then
            qemu_ohci.clocks = 0
        Else
            qemu_ohci.clocks += remaining
        End If
        'end overflow check

        If (qemu_ohci.eof_timer > 0) Then
            While (remaining >= qemu_ohci.eof_timer) And (qemu_ohci.eof_timer > 0)
                remaining -= Convert.ToInt64(qemu_ohci.eof_timer)
                qemu_ohci.eof_timer = 0
                qemu_ohci.frame_boundary()
            End While
            If ((remaining > 0) AndAlso qemu_ohci.eof_timer > 0) Then
                Dim m As Int64 = CLng(qemu_ohci.eof_timer)
                If (remaining < m) Then
                    m = remaining
                End If
                qemu_ohci.eof_timer = CULng(qemu_ohci.eof_timer - m)
                remaining -= m
            End If
        Else
            remaining = 0 ' I assume this won't break anything
        End If
    End Sub

    Public Overrides Function Test() As Int32
        '// 0 if the plugin works, non-0 if it doesn't
        Return 0
    End Function
End Class
