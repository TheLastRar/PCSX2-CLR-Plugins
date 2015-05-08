using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CLRDEV9.PacketReader
{
    class IPPacket : EthernetPayload //IPv4 Only
    {
        const byte _verHi = 4<<4; //Assume it is always 4
        int hlen; //convert this back to num of 32bit words
        byte typeofservice; //TODO, Implement this
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
        Int16 NO_id;
        protected UInt16 ID
        {
            get
            {
                return (UInt16)IPAddress.NetworkToHostOrder(NO_id);
            }
        }
        Int16 NO_fragmented_flags;
        #region "Fragment"
        protected Int16 HostO_fragment_flags
        {
            get
            {
                return IPAddress.NetworkToHostOrder(NO_fragmented_flags);
            }
            set
            {
                NO_fragmented_flags = IPAddress.HostToNetworkOrder(value);
            }
        }
        public UInt16 FragmentOffset
        {
            get
            {
                return (UInt16)(HostO_fragment_flags & ~(0x7 << 13));
            }
        }
        //1st bit is reserved
        public bool MoreFragments
        {
            get { return ((HostO_fragment_flags & (1 << 15)) != 0); }
            set
            {
                if (value) { HostO_fragment_flags |= unchecked((Int16)(1 << 15)); }
                else { HostO_fragment_flags &= unchecked((Int16)(~(1 << 15))); }
            }
        }
        public bool DoNotFragment
        {
            get { return ((HostO_fragment_flags & (1 << 14)) != 0); }
            set
            {
                if (value) { HostO_fragment_flags |= (1 << 14); }
                else { HostO_fragment_flags &= unchecked((byte)(~(1 << 14))); }
            }
        }
        #endregion
        byte ttl = 128;
        public byte Protocol;
        Int16 NO_csum;
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
        public byte[] SourceIP = new byte[4];
        public byte[] DestinationIP = new byte[4];

        IPPayload _pl;
        public IPPayload Payload
        {
            get
            {
                return _pl;
            }
        }
        public override byte[] GetBytes
        {
            get {
                CalculateCheckSum();
                _pl.CalculateCheckSum(SourceIP, DestinationIP);

                byte[] ret = new byte[Length];
                ret[0] = (byte)(_verHi + (hlen >> 2));
                ret[1] = typeofservice;//DSCP/ECN
                Utils.memcpy(ref ret, 2, BitConverter.GetBytes(NO_Length), 0, 2);

                Utils.memcpy(ref ret, 4, BitConverter.GetBytes(NO_id), 0, 2);
                Utils.memcpy(ref ret, 6, BitConverter.GetBytes(NO_fragmented_flags), 0, 2);

                ret[8] = ttl;
                ret[9] = Protocol;
                Utils.memcpy(ref ret, 10, BitConverter.GetBytes(NO_csum), 0, 2);

                Utils.memcpy(ref ret, 12, SourceIP, 0, 4);
                Utils.memcpy(ref ret, 16, DestinationIP, 0, 4);

                byte[] plBytes = _pl.GetBytes();
                Utils.memcpy(ref ret,20,plBytes,0,plBytes.Length);
                return ret;
            }
        }
        //source ip
        //dest ip
        public IPPacket(IPPayload pl)
        {
            _pl = pl;
            hlen = 20;
            Length = (UInt16)(pl.Length + hlen);
            Protocol = _pl.Protocol;
        }
        public IPPacket(EthernetFrame Ef)
        {
            int pktoffset = Ef.HeaderLength;

            //Bits 0-31
            byte v_hl = Ef.RawPacket.buffer[pktoffset];
            hlen = ((v_hl & 0xF) << 2);
            typeofservice = Ef.RawPacket.buffer[pktoffset + 1]; //TODO, Implement this
            NO_Length = (BitConverter.ToInt16(Ef.RawPacket.buffer, pktoffset + 2));
            //Console.Error.WriteLine("len=" + Length); //Includes hlen

            //Bits 32-63
            NO_id = (BitConverter.ToInt16(Ef.RawPacket.buffer, pktoffset + 4)); //Send packets with unique IDs
            NO_fragmented_flags = BitConverter.ToInt16(Ef.RawPacket.buffer, pktoffset + 6);

            if (MoreFragments)
            {
                Console.Error.WriteLine("FragmentedPacket");
            }

            //Bits 64-95
            ttl = Ef.RawPacket.buffer[pktoffset + 8];
            Protocol = Ef.RawPacket.buffer[pktoffset + 9];
            NO_csum = (BitConverter.ToInt16(Ef.RawPacket.buffer, pktoffset + 10));
            //bool ccsum = verifyCheckSum(Ef.RawPacket.buffer, pktoffset);
            //Console.Error.WriteLine("IP Checksum Good? " + ccsum);//Should ALWAYS be true

            //Bits 96-127
            Utils.memcpy(ref SourceIP, 0, Ef.RawPacket.buffer, pktoffset + 12, 4);
            //Bits 128-159
            Utils.memcpy(ref DestinationIP, 0, Ef.RawPacket.buffer, pktoffset + 16, 4);
            //Console.WriteLine("Target IP :" + DestinationIP[0] + "." + DestinationIP[1] + "." + DestinationIP[2] + "." + DestinationIP[3]);

            //Bits 160+
            if (hlen > 20) //IP options (if any)
            {
                Console.Error.WriteLine("hlen=" + hlen +" > 20");
                Console.Error.WriteLine("IP options are not supported");
                throw new NotImplementedException("IP options are not supported");
            }
            switch (Protocol) //(Prase Payload)
            {
                case (byte)IPType.ICMP:
                    _pl = new ICMP(Ef, pktoffset + hlen, Length - hlen);
                    //((ICMP)_pl).VerifyCheckSum(SourceIP, DestinationIP);
                    break;
                case (byte)IPType.TCP:
                    _pl = new TCP(Ef, pktoffset + hlen, Length - hlen);
                    //((TCP)_pl).VerifyCheckSum(SourceIP, DestinationIP);
                    break;
                case (byte)IPType.UDP:
                    _pl = new UDP(Ef, pktoffset + hlen);
                    //((UDP)_pl).VerifyCheckSum(SourceIP, DestinationIP);
                    break;
                default:
                    throw new NotImplementedException("Unkown IPv4 Protocol " + Protocol.ToString("X2"));
                    //break;
            }
        }
        private void CalculateCheckSum()
        {
            //if (!(i == 5)) //checksum feild is 10-11th byte (5th short), which is skipped
            byte[] headerSegment = new byte[hlen];
            headerSegment[0] = (byte)(_verHi + (hlen >> 2));
            headerSegment[1] = typeofservice;//DSCP/ECN
            Utils.memcpy(ref headerSegment, 2, BitConverter.GetBytes(NO_Length), 0, 2);

            Utils.memcpy(ref headerSegment, 4, BitConverter.GetBytes(NO_id), 0, 2);
            Utils.memcpy(ref headerSegment, 6, BitConverter.GetBytes(NO_fragmented_flags), 0, 2);

            headerSegment[8] = ttl;
            headerSegment[9] = Protocol;
            //header csum

            Utils.memcpy(ref headerSegment, 12, SourceIP, 0, 4);
            Utils.memcpy(ref headerSegment, 16, DestinationIP, 0, 4);

            Checksum = InternetChecksum(headerSegment);
        }
        public bool VerifyCheckSum()
        {
            byte[] headerSegment = new byte[hlen];
            headerSegment[0] = (byte)(_verHi + (hlen >> 2));
            headerSegment[1] = typeofservice;//DSCP/ECN
            Utils.memcpy(ref headerSegment, 2, BitConverter.GetBytes(NO_Length), 0, 2);

            Utils.memcpy(ref headerSegment, 4, BitConverter.GetBytes(NO_id), 0, 2);
            Utils.memcpy(ref headerSegment, 6, BitConverter.GetBytes(NO_fragmented_flags), 0, 2);

            headerSegment[8] = ttl;
            headerSegment[9] = Protocol;
            Utils.memcpy(ref headerSegment, 10, BitConverter.GetBytes(NO_csum), 0, 2);

            Utils.memcpy(ref headerSegment, 12, SourceIP, 0, 4);
            Utils.memcpy(ref headerSegment, 16, DestinationIP, 0, 4);

            UInt16 CsumCal = InternetChecksum(headerSegment);
            return (CsumCal == 0);
        }
        public static ushort InternetChecksum(byte[] buffer)
        {
            //source http://stackoverflow.com/a/2201090
            //byte[] buffer = value.ToArray();
            int length = buffer.Length;
            int i = 0;
            UInt32 sum = 0;
            UInt32 data = 0;
            while (length > 1)
            {
                data = 0;
                data = (UInt32)(
                ((UInt32)(buffer[i]) << 8) | ((UInt32)(buffer[i + 1]) & 0xFF)
                );

                sum += data;
                if ((sum & 0xFFFF0000) > 0)
                {
                    sum = sum & 0xFFFF;
                    sum += 1;
                }

                i += 2;
                length -= 2;
            }

            if (length > 0)
            {
                sum += (UInt32)(buffer[i] << 8);
                //sum += (UInt32)(buffer[i]);
                if ((sum & 0xFFFF0000) > 0)
                {
                    sum = sum & 0xFFFF;
                    sum += 1;
                }
            }
            sum = ~sum;
            sum = sum & 0xFFFF;
            return (UInt16)sum;
        }
    }
}
