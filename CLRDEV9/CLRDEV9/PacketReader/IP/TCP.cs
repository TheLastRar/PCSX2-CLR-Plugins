using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CLRDEV9.PacketReader
{
    class TCP : IPPayload
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
        Int32 NO_seqNum;
        public UInt32 SequenceNumber
        {
            get
            {
                return (UInt32)IPAddress.NetworkToHostOrder(NO_seqNum);
            }
            set
            {
                NO_seqNum = IPAddress.HostToNetworkOrder((Int32)value);
            }
        }
        Int32 NO_ackNum;
        public UInt32 AcknowledgementNumber
        {
            get
            {
                return (UInt32)IPAddress.NetworkToHostOrder(NO_ackNum);
            }
            set
            {
                NO_ackNum = IPAddress.HostToNetworkOrder((Int32)value);
            }
        }
        byte data_offset_and_NS_flag;
        protected int HeaderLength //Can have varying Header Len
            //Need to account for this at packet creation
        {
            get
            {
                return (data_offset_and_NS_flag>>4) << 2;
            }
            set
            {
                byte NS = (byte)(data_offset_and_NS_flag & 1);
                data_offset_and_NS_flag = (byte)((value >> 2) << 4);
                data_offset_and_NS_flag |= NS;
            }
        }
        byte flags;
        Int16 NO_winSize;
        public UInt16 WindowSize
        {
            get
            {
                return (UInt16)IPAddress.NetworkToHostOrder(NO_winSize);
            }
            set
            {
                NO_winSize = IPAddress.HostToNetworkOrder((Int16)value);
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
        Int16 NO_urgPtr = 0;
        protected UInt16 UrgentPointer
        {
            get
            {
                return (UInt16)IPAddress.NetworkToHostOrder(NO_urgPtr);
            }
            set
            {
                NO_urgPtr = IPAddress.HostToNetworkOrder((Int16)value);
            }
        }
        public List<TCPOption> Options = new List<TCPOption>();
        byte[] data;
        public override byte Protocol
        {
            get { return (byte)IPType.TCP; }
        }
        public override ushort Length
        {
            get
            {
                ReComputeHeaderLen();
                return (UInt16)(data.Length + HeaderLength);
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }

        #region 'Flags'
        public bool ACK
        {
            get { return ((flags & (1 << 4)) != 0); }
            set
            {
                if (value) { flags |= (1 << 4); }
                else { flags &= unchecked((byte)(~(1 << 4))); }
            }
        }
        public bool PSH
        {
            get { return ((flags & (1 << 3)) != 0); }
            set
            {
                if (value) { flags |= (1 << 3); }
                else { flags &= unchecked((byte)(~(1 << 3))); }
            }
        }
        public bool RST
        {
            get { return ((flags & (1 << 2)) != 0); }
            set
            {
                if (value) { flags |= (1 << 2); }
                else { flags &= unchecked((byte)(~(1 << 2))); }
            }
        }
        public bool SYN
        {
            get{ return ((flags & (1 << 1)) != 0);}
            set{
                if (value) { flags |= (1 << 1); }
                else {flags &= unchecked((byte)(~(1 << 1)));}
            }
        }
        public bool FIN
        {
            get{ return ((flags & (1)) != 0);}
            set{
                if (value) { flags |= (1); }
                else {flags &= unchecked((byte)(~(1)));}
            }
        }
        #endregion

        private void ReComputeHeaderLen()
        {
            int opOffset = 20;
            for (int i = 0; i < Options.Count; i++)
            {
                opOffset += Options[i].Length;
            }
            opOffset += opOffset % 4; //needs to be a whole number of 32bits
            HeaderLength = opOffset;
        }

        public override byte[] GetPayload()
        {
            return data;
        }
        public TCP(byte[] payload) //Length = IP payload len
        {
            data = payload;
        }
        public TCP(EthernetFrame Ef, int offset, int parLength) //Length = IP payload len
        {
            //Bits 0-31
            NO_srcPort = BitConverter.ToInt16(Ef.RawPacket.buffer, offset);
            //Console.Error.WriteLine("src port=" + SourcePort); 
            NO_dstPort = BitConverter.ToInt16(Ef.RawPacket.buffer, offset + 2);
            //Console.Error.WriteLine("dts port=" + DestinationPort);
            //Bits 32-63
            NO_seqNum = BitConverter.ToInt32(Ef.RawPacket.buffer, offset + 4);
            //Console.Error.WriteLine("seq num=" + SequenceNumber); //Where in the stream the start of the payload is
            //Bits 64-95
            NO_ackNum = BitConverter.ToInt32(Ef.RawPacket.buffer, offset + 8);
            //Console.Error.WriteLine("ack num=" + AcknowledgmentNumber); //the next expected byte(seq) number
            //Bits 96-127
            data_offset_and_NS_flag = Ef.RawPacket.buffer[offset + 12];
            //Console.Error.WriteLine("TCP hlen=" + HeaderLength);
            flags = Ef.RawPacket.buffer[offset + 13];
            //flags
            //if ((data_offset_and_NS_flag & 1)!=0)
            //{
            //    Console.Error.WriteLine("NS is set"); //experimental flag
            //}
            //if (((flags & (1 << 7)) != 0))
            //{
            //    Console.Error.WriteLine("CWR is set");
            //}
            //if (((flags & (1 << 6)) != 0))
            //{
            //    Console.Error.WriteLine("ECE is set");
            //}
            //if ((flags & (1 << 5)) != 0)
            //{
            //    Console.Error.WriteLine("URG is set");
            //}
            //if ((flags & (1 << 4)) != 0)
            //{
            //    Console.Error.WriteLine("ACK is set"); //Acknowledgement of recived packet with Seqnumber
            //}
            //if ((flags & (1 << 3)) != 0)
            //{
            //    Console.Error.WriteLine("PSH is set");
            //}
            //if ((flags & (1 << 2)) != 0)
            //{
            //    Console.Error.WriteLine("RST is set");
            //}
            //if ((flags & (1<<1)) != 0)
            //{
            //    Console.Error.WriteLine("SYN is set");
            //}
            //if ((flags & 1)!=0)
            //{
            //    Console.Error.WriteLine("FIN is set");
            //}
            //endflags
            NO_winSize = BitConverter.ToInt16(Ef.RawPacket.buffer, offset + 14);
            //Console.Error.WriteLine("win Size=" + WindowSize);
            //Bits 127-159
            NO_csum = BitConverter.ToInt16(Ef.RawPacket.buffer, offset + 16);
            NO_urgPtr = BitConverter.ToInt16(Ef.RawPacket.buffer, offset + 18);
            //Console.Error.WriteLine("urg ptr=" + UrgentPointer);

            //Bits 160+
            if (HeaderLength > 20) //TCP options
            {
                bool opReadFin = false;
                int op_offset = offset + 20;
                do
                {
                    byte opKind = Ef.RawPacket.buffer[op_offset];
                    byte opLen = Ef.RawPacket.buffer[op_offset + 1];
                    switch (opKind)
                    {
                        case 0:
                            //Console.Error.WriteLine("Got End of Options List @ " + (op_offset-offset-1));
                            opReadFin = true;
                            break;
                        case 1:
                            //Console.Error.WriteLine("Got NOP");
                            Options.Add(new TCPopNOP());
                            op_offset += 1;
                            continue;
                        case 2:
                            //Console.Error.WriteLine("Got MMS");
                            Options.Add(new TCPopMSS(Ef.RawPacket.buffer,op_offset));
                            break;
                        case 3:
                            Options.Add(new TCPopWS(Ef.RawPacket.buffer, op_offset));
                            break;
                        case 8:
                            //Console.Error.WriteLine("Got Timestamp");
                            Options.Add(new TCPopTS(Ef.RawPacket.buffer, op_offset));
                            break;
                        default:
                            Console.Error.WriteLine("Got TCP Unknown Option " + opKind + "with len" + opLen);
                            break;
                    }
                    op_offset += opLen;
                    if (op_offset == offset+HeaderLength)
                    {
                        //Console.Error.WriteLine("Reached end of Options");
                        opReadFin = true;
                    }
                } while (opReadFin==false);
            }
            data = new byte[parLength-HeaderLength];
            Utils.memcpy(ref data, 0, Ef.RawPacket.buffer, offset + HeaderLength, parLength - HeaderLength);
            //AllDone
        }

        public override void CalculateCheckSum(byte[] srcIP, byte[] dstIP)
        {
            Int16 TCPLength = (Int16)(HeaderLength + data.Length);
            int pHeaderLen = (12 + TCPLength);
            if ((pHeaderLen & 1) != 0)
            {
                //Console.Error.WriteLine("OddSizedPacket");
                pHeaderLen += 1;
            }

            byte[] headerSegment = new byte[pHeaderLen];

            Utils.memcpy(ref headerSegment, 0, srcIP, 0, 4);
            Utils.memcpy(ref headerSegment, 4, dstIP, 0, 4);
            //[8] = 0
            headerSegment[9] = Protocol;
            Int16 NO_Length = IPAddress.HostToNetworkOrder(TCPLength);
            Utils.memcpy(ref headerSegment, 10, BitConverter.GetBytes(NO_Length), 0, 2);
            //Pseudo Header added
            //Rest of data is normal neader+data
            Utils.memcpy(ref headerSegment, 12, GetBytes(), 0, TCPLength);

            byte[] nullcsum = new byte[2];
            Utils.memcpy(ref headerSegment, 28, nullcsum, 0, 2);

            Checksum = IPPacket.InternetChecksum(headerSegment);
        }

        public bool VerifyCheckSum(byte[] srcIP, byte[] dstIP)
        {
            UInt16 TCPLength = (UInt16)(Length);
            int pHeaderLen = (12 + TCPLength);
            if ((pHeaderLen & 1) != 0)
            {
                //Console.Error.WriteLine("OddSizedPacket");
                pHeaderLen += 1;
            }

            byte[] headerSegment = new byte[pHeaderLen];

            Utils.memcpy(ref headerSegment, 0, srcIP, 0, 4);
            Utils.memcpy(ref headerSegment, 4, dstIP, 0, 4);
            //[8] = 0
            headerSegment[9] = Protocol;
            Int16 NO_Length = IPAddress.HostToNetworkOrder((Int16)TCPLength);
            Utils.memcpy(ref headerSegment, 10, BitConverter.GetBytes(NO_Length), 0, 2);
            //Pseudo Header added
            //Rest of data is normal neader+data
            Utils.memcpy(ref headerSegment, 12, GetBytes(), 0, TCPLength);

            UInt16 CsumCal = IPPacket.InternetChecksum(headerSegment);
            Console.Error.WriteLine("Checksum Good = " + (CsumCal == 0));
            return (CsumCal == 0);
        }

        public override byte[] GetBytes()
        {
            int len = Length;
            byte[] ret = new byte[len];

            Utils.memcpy(ref ret, 0, BitConverter.GetBytes(NO_srcPort), 0, 2);
            Utils.memcpy(ref ret, 2, BitConverter.GetBytes(NO_dstPort), 0, 2);
            Utils.memcpy(ref ret, 4, BitConverter.GetBytes(NO_seqNum), 0, 4);
            Utils.memcpy(ref ret, 8, BitConverter.GetBytes(NO_ackNum), 0, 4);
            ret[12] = data_offset_and_NS_flag;
            ret[13] = flags;
            Utils.memcpy(ref ret, 14, BitConverter.GetBytes(NO_winSize), 0, 2);
            Utils.memcpy(ref ret, 16, BitConverter.GetBytes(NO_csum), 0, 2);
            Utils.memcpy(ref ret, 18, BitConverter.GetBytes(NO_urgPtr), 0, 2);
            
            //options
            int opOffset = 20;
            for (int i = 0; i < Options.Count; i++)
            {
                Utils.memcpy(ref ret, opOffset, Options[i].GetBytes(), 0, Options[i].Length);
                opOffset += Options[i].Length;
            }
            Utils.memcpy(ref ret, HeaderLength, data, 0, data.Length);
            return ret;
        }
    }
}
