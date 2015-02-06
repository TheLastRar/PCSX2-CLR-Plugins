using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CLRDEV9.PacketReader
{
    class ARPPacket : EthernetPayload
    {
        public override ushort Length
        {
            get
            {
                return (UInt16)(8 + (2 * HardwareAddressLength) + (2 * ProtocolAddressLength));
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }
        public override byte[] GetBytes
        {
            get {
                byte[] ret = new byte[Length];
                Utils.memcpy(ref ret, 0, BitConverter.GetBytes(NO_Htype), 0, 2);
                Utils.memcpy(ref ret, 2, BitConverter.GetBytes(Protocol), 0, 2);
                ret[4] = HardwareAddressLength;
                ret[5] = ProtocolAddressLength;
                Utils.memcpy(ref ret, 6, BitConverter.GetBytes(NO_Op), 0, 2);

                int offset = 8;
                Utils.memcpy(ref ret, offset, SenderHardwareAddress, 0, SenderHardwareAddress.Length);
                offset += HardwareAddressLength;
                Utils.memcpy(ref ret, offset, SenderProtocolAddress, 0, SenderProtocolAddress.Length);
                offset += ProtocolAddressLength;

                Utils.memcpy(ref ret, offset, TargetHardwareAddress, 0, TargetHardwareAddress.Length);
                offset += HardwareAddressLength;
                Utils.memcpy(ref ret, offset, TargetProtocolAddress, 0, TargetProtocolAddress.Length);
                offset += ProtocolAddressLength;
                return ret;
            }
        }
        Int16 NO_Htype;
        public UInt16 HardWareType
        {
            get
            {
                return (UInt16)IPAddress.NetworkToHostOrder(NO_Htype);
            }
            set
            {
                NO_Htype = IPAddress.HostToNetworkOrder((Int16)value);
            }
        }
        public UInt16 Protocol;
        public byte HardwareAddressLength = 6;
        public byte ProtocolAddressLength = 4;
        Int16 NO_Op;
        public UInt16 OP
        {
            get
            {
                return (UInt16)IPAddress.NetworkToHostOrder(NO_Htype);
            }
            set
            {
                NO_Htype = IPAddress.HostToNetworkOrder((Int16)value);
            }
        }
        public byte[] SenderHardwareAddress;
        public byte[] SenderProtocolAddress;
        public byte[] TargetHardwareAddress;
        public byte[] TargetProtocolAddress;

        public ARPPacket()
        {
            HardWareType = 1;
        }
        public ARPPacket(EthernetFrame Ef)
        {
            int pktoffset = Ef.HeaderLength;
            NO_Htype = BitConverter.ToInt16(Ef.RawPacket.buffer, pktoffset);
            Protocol = BitConverter.ToUInt16(Ef.RawPacket.buffer, pktoffset + 2);
            
            HardwareAddressLength = Ef.RawPacket.buffer[pktoffset + 4];
            ProtocolAddressLength = Ef.RawPacket.buffer[pktoffset + 5];
            NO_Op = BitConverter.ToInt16(Ef.RawPacket.buffer, pktoffset + 6);
            //Console.Error.WriteLine("OP" + OP);
            pktoffset += 8;

            SenderHardwareAddress = new byte[HardwareAddressLength];
            Utils.memcpy(ref SenderHardwareAddress, 0,Ef.RawPacket.buffer, pktoffset,HardwareAddressLength);
            pktoffset += HardwareAddressLength;
            //Console.WriteLine("sender MAC :" + SenderHardwareAddress[0] + ":" + SenderHardwareAddress[1] + ":" + SenderHardwareAddress[2] + ":" + SenderHardwareAddress[3] + ":" + SenderHardwareAddress[4] + ":" + SenderHardwareAddress[5]);

            SenderProtocolAddress = new byte[ProtocolAddressLength];
            Utils.memcpy(ref SenderProtocolAddress, 0, Ef.RawPacket.buffer, pktoffset, ProtocolAddressLength);
            pktoffset += ProtocolAddressLength;
            //Console.WriteLine("sender IP :" + SenderProtocolAddress[0] + "." + SenderProtocolAddress[1] + "." + SenderProtocolAddress[2] + "." + SenderProtocolAddress[3]);
            

            TargetHardwareAddress = new byte[HardwareAddressLength];
            Utils.memcpy(ref TargetHardwareAddress, 0, Ef.RawPacket.buffer, pktoffset, HardwareAddressLength);
            pktoffset += HardwareAddressLength;
            //Console.WriteLine("target MAC :" + TargetHardwareAddress[0] + ":" + TargetHardwareAddress[1] + ":" + TargetHardwareAddress[2] + ":" + TargetHardwareAddress[3] + ":" + TargetHardwareAddress[4] + ":" + TargetHardwareAddress[5]);

            TargetProtocolAddress = new byte[ProtocolAddressLength];
            Utils.memcpy(ref TargetProtocolAddress, 0, Ef.RawPacket.buffer, pktoffset, ProtocolAddressLength);
            pktoffset += ProtocolAddressLength;
            //Console.WriteLine("target IP :" + TargetProtocolAddress[0] + "." + TargetProtocolAddress[1] + "." + TargetProtocolAddress[2] + "." + TargetProtocolAddress[3]);
        }
    }
}
