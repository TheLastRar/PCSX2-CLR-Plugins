﻿Namespace USB
    Class USB_Port
        Dim opaque As OHCI.OHCI_State
        Dim index As Integer
        Dim _next As USB_Port
        Public dev As USB_Device

        Sub New(par_opaque As OHCI.OHCI_State, par_index As Integer)
            opaque = par_opaque
            index = par_index
        End Sub

        Public Sub attach(dev As USB_Device) 'ohci_attatch
            Dim s As OHCI.OHCI_State = Me.opaque
            Dim old_state As UInt32 = s.rhport(index).ctrl

            If (Not IsNothing(dev)) Then
                If (Not IsNothing(s.rhport(index).port.dev)) Then
                    Me.attach(Nothing)
                    'usb_attatch(p1,n)
                    'Void usb_attach(USBPort * port, USBDevice * dev)
                    '{
                    '   port->attach(port, dev);
                    '}
                End If
                '/* set connect status */
                s.rhport(index).ctrl = s.rhport(index).ctrl Or (OHCI.OHCI_PORT_CCS Or OHCI.OHCI_PORT_CSC)

                '/* update speed */
                If dev.Speed = USB_SPEED_LOW Then
                    s.rhport(index).ctrl = s.rhport(index).ctrl Or OHCI.OHCI_PORT_LSDA
                Else
                    s.rhport(index).ctrl = s.rhport(index).ctrl And Not OHCI.OHCI_PORT_LSDA
                End If
                s.rhport(index).port.dev = dev
                '//* send the attach message */
                dev.handle_packet(USB_MSG_ATTACH, 0, 0, Nothing, 0)
                s.USBLog.LogWriteLine("usb-ohci: Attached port " & index)
            Else
                '/* set connect status */
                If (s.rhport(index).ctrl And OHCI.OHCI_PORT_CCS) <> 0 Then
                    s.rhport(index).ctrl = s.rhport(index).ctrl And Not OHCI.OHCI_PORT_CCS
                    s.rhport(index).ctrl = s.rhport(index).ctrl Or OHCI.OHCI_PORT_CSC
                End If
                '/* disable port */
                If (s.rhport(index).ctrl And OHCI.OHCI_PORT_PES) <> 0 Then
                    s.rhport(index).ctrl = s.rhport(index).ctrl And Not OHCI.OHCI_PORT_PES
                    s.rhport(index).ctrl = s.rhport(index).ctrl Or OHCI.OHCI_PORT_PESC
                End If
                dev = s.rhport(index).port.dev
                If (Not IsNothing(dev)) Then
                    '/* send the detach message */
                    dev.handle_packet(USB_MSG_DETACH, 0, 0, Nothing, 0)
                End If
                s.rhport(index).port.dev = Nothing
                s.USBLog.LogWriteLine("usb-ohci: Detached port " & index)
            End If
            If (old_state <> s.rhport(index).ctrl) Then
                s.set_interrupt(OHCI.OHCI_INTR_RHSC)
            End If
            '}
        End Sub

        Public Sub Freeze(ByRef FreezeData As CLR_FreezeData, index As Integer, save As Boolean)
            Dim isDev As Boolean = Not IsNothing(dev)
            FreezeData.SetBoolValue("OHCI.P" & index & ".dev", isDev, save)

            If isDev <> Not IsNothing(dev) Then
                opaque.USBLog.ErrorWriteLine("Error: SaveState device restore failed")
                opaque.USBLog.ErrorWriteLine("Device attachment state miss-match")
                attach(dev)
            Else
                If isDev Then
                    Try
                        dev.Freeze(FreezeData, index, save)
                    Catch err As Exception
                        opaque.USBLog.ErrorWriteLine("Error: SaveState device restore failed")
                        opaque.USBLog.ErrorWriteLine("Error encounted " & err.ToString())
                        dev.handle_reset()
                        attach(dev)
                    End Try
                End If
            End If
        End Sub
    End Class
End Namespace
