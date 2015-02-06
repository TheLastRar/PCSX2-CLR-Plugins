using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CLRDEV9.PacketReader
{
    class ICMP : IPPayload
    {
        public byte Type;
        public byte Code;
        Int16 NO_csum = 0; //internet checksum
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
        public override byte Protocol
        {
            get { return 0x01; }
        }
        
        public byte[] HeaderData = new byte[4];
        public byte[] Data = new byte[0];

        public override ushort Length
        {
            get
            {
                return (UInt16)(4 + HeaderData.Length + Data.Length);
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }
        public override byte[] GetPayload()
        {
            throw new NotImplementedException();
        }
        public ICMP(byte[] data)
        {
            Data = data;
        }
        
        public ICMP(EthernetFrame Ef, int offset, int Length) //Length = IP payload len
        {
            Type = Ef.RawPacket.buffer[offset];
            //Console.Error.WriteLine("Type = " + Type);
            Code = Ef.RawPacket.buffer[offset + 1];
            //Console.Error.WriteLine("Code = " + Code);
            NO_csum = BitConverter.ToInt16(Ef.RawPacket.buffer, offset + 2);
            Utils.memcpy(ref HeaderData, 0, Ef.RawPacket.buffer, offset + 4, 4);

            Data = new byte[Length - 8];
            Utils.memcpy(ref Data, 0, Ef.RawPacket.buffer, offset + 8, Length - 8);
        }
        public override void CalculateCheckSum(byte[] srcIP, byte[] dstIP)
        {
            int pHeaderLen = ((Length));
            if ((pHeaderLen & 1) != 0)
            {
                //Console.Error.WriteLine("OddSizedPacket");
                pHeaderLen += 1;
            }

            byte[] headerSegment = new byte[pHeaderLen];
            Utils.memcpy(ref headerSegment, 0, GetBytes(), 0, Length);

            byte[] nullcsum = new byte[2];
            Utils.memcpy(ref headerSegment, 2, nullcsum, 0, 2);

            Checksum = IPPacket.InternetChecksum(headerSegment);
        }
        public bool VerifyCheckSum(byte[] srcIP, byte[] dstIP)
        {
            int pHeaderLen = ((Length));
            if ((pHeaderLen & 1) != 0)
            {
                //Console.Error.WriteLine("OddSizedPacket");
                pHeaderLen += 1;
            }

            byte[] headerSegment = new byte[pHeaderLen];
            Utils.memcpy(ref headerSegment, 0, GetBytes(), 0, Length);

            UInt16 CsumCal = IPPacket.InternetChecksum(headerSegment);
            Console.Error.WriteLine("IMCP Checksum Good = " + (CsumCal == 0));
            return (CsumCal == 0);
        }
        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Type;
            ret[1] = Code;
            Utils.memcpy(ref ret, 2, BitConverter.GetBytes(NO_csum), 0, 2);
            Utils.memcpy(ref ret, 4, HeaderData, 0, 4);
            Utils.memcpy(ref ret, 8, Data, 0, Data.Length);
            return ret;
        }
    }
}
