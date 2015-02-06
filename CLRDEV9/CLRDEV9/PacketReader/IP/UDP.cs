using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CLRDEV9.PacketReader
{
    class UDP : IPPayload
    {
        Int16 NO_srcPort;
        public UInt16 SourcePort
        {
            get
            {
                return (UInt16)IPAddress.NetworkToHostOrder(NO_srcPort);
            }
            set
            {
                NO_srcPort = IPAddress.HostToNetworkOrder((Int16)value);
            }
        }
        Int16 NO_dstPort;
        public UInt16 DestinationPort
        {
            get
            {
                return (UInt16)IPAddress.NetworkToHostOrder(NO_dstPort);
            }
            set
            {
                NO_dstPort = IPAddress.HostToNetworkOrder((Int16)value);
            }
        }
        Int16 NO_Length;
        public override UInt16 Length
        {
            get
            {
                return (UInt16)IPAddress.NetworkToHostOrder(NO_Length);
            }
            protected set
            {
                NO_Length = IPAddress.HostToNetworkOrder((Int16)value);
            }
        }
        Int16 NO_csum = 0;
        protected UInt16 Checksum
        {
            get
            {
                return (UInt16)IPAddress.NetworkToHostOrder(NO_csum);
            }
            set
            {
                NO_csum = IPAddress.HostToNetworkOrder((Int16)value);
            }
        }
        protected int HeaderLength
        {
            get
            {
                return 8;
            }
        }
        byte[] data;
        public override byte Protocol
        {
            get { return (byte)IPType.UDP; }
        }
        public override byte[] GetPayload()
        {
            return data;
        }
        public UDP(byte[] parData)
        {
            data = parData;
            Length = (UInt16)(data.Length + HeaderLength);
        }
        public UDP(EthernetFrame Ef, int offset)
        {
            //Bits 0-31
            NO_srcPort = BitConverter.ToInt16(Ef.RawPacket.buffer, offset);
            //Console.Error.WriteLine("src port=" + SourcePort); 
            NO_dstPort = BitConverter.ToInt16(Ef.RawPacket.buffer, offset + 2);
            //Console.Error.WriteLine("dts port=" + DestinationPort);
            //Bits 32-63
            NO_Length = BitConverter.ToInt16(Ef.RawPacket.buffer, offset + 4); //includes header length
            NO_csum = BitConverter.ToInt16(Ef.RawPacket.buffer, offset + 6);

            //Bits 64+
            data = new byte[Length-HeaderLength];
            Utils.memcpy(ref data, 0, Ef.RawPacket.buffer, offset + HeaderLength, Length - HeaderLength);
            //AllDone
        }
        public override void CalculateCheckSum(byte[] srcIP, byte[] dstIP)
        {
            int pHeaderLen = (12) + HeaderLength + data.Length;
            if ((pHeaderLen & 1) !=0)
            {
                pHeaderLen += 1;
            }
            byte[] headerSegment = new byte[pHeaderLen];
            byte[] nullBytes = new byte[2];

            Utils.memcpy(ref headerSegment, 0, srcIP, 0, 4);
            Utils.memcpy(ref headerSegment, 4, dstIP, 0, 4);
            //[8] = 0
            headerSegment[9] = 0x11;
            Utils.memcpy(ref headerSegment, 10, BitConverter.GetBytes(NO_Length), 0, 2);
            //Pseudo Header added

            //Rest of data is normal neader+data
            Utils.memcpy(ref headerSegment, 12, BitConverter.GetBytes(NO_srcPort), 0, 2);
            Utils.memcpy(ref headerSegment, 14, BitConverter.GetBytes(NO_dstPort), 0, 2);
            Utils.memcpy(ref headerSegment, 16, BitConverter.GetBytes(NO_Length), 0, 2);
            Utils.memcpy(ref headerSegment, 18, nullBytes, 0, 2); //zero out csum

            Utils.memcpy(ref headerSegment, 20, data, 0, data.Length);
            
            Checksum = IPPacket.InternetChecksum(headerSegment); //For performance, we can set this to = zero
            Console.Error.WriteLine();
        }

        public bool VerifyCheckSum(byte[] srcIP, byte[] dstIP)
        {
            int pHeaderLen = (12) + HeaderLength + data.Length;
            if ((pHeaderLen & 1) != 0)
            {
                //Console.Error.WriteLine("OddSizedPacket");
                pHeaderLen += 1;
            }

            byte[] headerSegment = new byte[pHeaderLen];

            Utils.memcpy(ref headerSegment, 0, srcIP, 0, 4);
            Utils.memcpy(ref headerSegment, 4, dstIP, 0, 4);
            //[8] = 0
            headerSegment[9] = 0x11;
            Utils.memcpy(ref headerSegment, 10, BitConverter.GetBytes(NO_Length), 0, 2);
            //Pseudo Header added

            //Rest of data is normal neader+data
            Utils.memcpy(ref headerSegment, 12, BitConverter.GetBytes(NO_srcPort), 0, 2);
            Utils.memcpy(ref headerSegment, 14, BitConverter.GetBytes(NO_dstPort), 0, 2);
            Utils.memcpy(ref headerSegment, 16, BitConverter.GetBytes(NO_Length), 0, 2);
            Utils.memcpy(ref headerSegment, 18, BitConverter.GetBytes(NO_csum), 0, 2);

            Utils.memcpy(ref headerSegment, 20, data, 0, data.Length);

            UInt16 CsumCal = IPPacket.InternetChecksum(headerSegment);
            Console.Error.WriteLine("UDP Checksum Good = " + (CsumCal == 0));
            return (CsumCal == 0);
        }
        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];

            Utils.memcpy(ref ret, 0, BitConverter.GetBytes(NO_srcPort), 0, 2);
            Utils.memcpy(ref ret, 2, BitConverter.GetBytes(NO_dstPort), 0, 2);
            Utils.memcpy(ref ret, 4, BitConverter.GetBytes(NO_Length), 0, 2);
            Utils.memcpy(ref ret, 6, BitConverter.GetBytes(NO_csum), 0, 2);

            Utils.memcpy(ref ret, 8, data, 0, data.Length);

            return ret;
        }
    }
}
