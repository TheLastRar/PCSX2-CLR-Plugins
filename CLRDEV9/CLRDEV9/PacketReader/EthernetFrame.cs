using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRDEV9.PacketReader
{
    class EthernetFrame
    {
        private netHeader.NetPacket _pkt;
        public netHeader.NetPacket RawPacket
        {
            get{
            return _pkt;
            }
        }
        public byte[] DestinationMAC = new byte[6];
        public byte[] SourceMAC = new byte[6];

        Int16 proto;
        public Int16 Protocol
        {
            get
            {
                return proto;
            }
            set { proto = value; }
        }
        private int hlen = 0;
        public int HeaderLength
        {
            get
            {
                return hlen;
            }
        }
        public int Length
        {
            get
            {
                return _pkt.size;//Frame Check is not added to the frame
            }
        }
        EthernetPayload _pl;
        public EthernetPayload Payload
        {
            get
            {
                return _pl;
            }
        }

        public EthernetFrame(EthernetPayload ePL)
        {
            hlen = 14;
            _pl = ePL;
        }
        public netHeader.NetPacket CreatePacket()
        {
            netHeader.NetPacket nPK = new netHeader.NetPacket();
            byte[] PLbytes = _pl.GetBytes;

            byte[] rawbytes = new byte[PLbytes.Length + hlen];
            nPK.size = PLbytes.Length + hlen;
            Utils.memcpy(ref rawbytes, 0, DestinationMAC, 0, 6);
            Utils.memcpy(ref rawbytes, 6, SourceMAC, 0, 6);
            Utils.memcpy(ref rawbytes, 12, BitConverter.GetBytes(proto), 0, 2);

            Utils.memcpy(ref rawbytes, 14, PLbytes, 0, PLbytes.Length);
            Utils.memcpy(ref nPK.buffer, 0, rawbytes, 0, rawbytes.Length);
            return nPK;
        }

        public EthernetFrame(netHeader.NetPacket pkt)
        {
            _pkt = pkt;
            Utils.memcpy(ref DestinationMAC, 0, pkt.buffer, 0, 6);
            //Console.WriteLine("eth dst MAC :" + DestinationMAC[0] + ":" + DestinationMAC[1] + ":" + DestinationMAC[2] + ":" + DestinationMAC[3] + ":" + DestinationMAC[4] + ":" + DestinationMAC[5]);
            Utils.memcpy(ref SourceMAC, 0, pkt.buffer, 6, 6);
            //Console.WriteLine("src MAC :" + SourceMAC[0] + ":" + SourceMAC[1] + ":" + SourceMAC[2] + ":" + SourceMAC[3] + ":" + SourceMAC[4] + ":" + SourceMAC[5]);
            
            hlen = 14; //(6+6+12)
            
            //NOTE: we don't have to worry about the Ethernet Frame CRC as it is not included in the packet

            proto = BitConverter.ToInt16(pkt.buffer,12);
            switch (proto) //Note, Diffrent Edian
            {
                case (int)EtherFrameType.NULL:
                    break;
                case (int)EtherFrameType.IPv4:
                    _pl = new IPPacket(this);
                    break;
                case (int)EtherFrameType.ARP:
                    _pl = new ARPPacket(this);
                    break;
                case (int)0x0081:
                    //Console.Error.WriteLine("VLAN-tagged frame (IEEE 802.1Q)");
                    throw new NotImplementedException();
                    //break;
                default:
                    Console.Error.WriteLine("Unkown Ethernet Protocol " + proto.ToString("X4"));
                    break;
            }
        }
    }
}
