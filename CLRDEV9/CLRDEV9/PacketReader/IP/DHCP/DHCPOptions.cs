using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CLRDEV9.PacketReader
{
    class DHCPopNOP : TCPOption //unlike TCP options, DCHP length feild does not count the option header
    {
        public DHCPopNOP()
        {

        }
        public override byte Length { get { return 1; } }
        public override byte Code { get { return 0; } }

        public override byte[] GetBytes()
        {
            return new byte[] { Code };
        }
    }
    class DHCPopSubnet : TCPOption
    {
        byte[] mask = new byte[4];
        public DHCPopSubnet(byte[] data)
        {
            mask = data;
        }
        public DHCPopSubnet(byte[] data, int offset) //Offset will include Kind and Len
        {
            Utils.memcpy(ref mask, 0, data, offset + 2, 4);   
        }
        public override byte Length { get { return 6; } }
        public override byte Code { get { return 1; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, mask, 0, 4);
            return ret;
        }
    }
    class DHCPopRouter : TCPOption //can be longer then 1 address (not supported)
    {
        byte[] routerip = new byte[4];
        public DHCPopRouter(byte[] data)
        {
            routerip = data;
        }
        public DHCPopRouter(byte[] data, int offset) //Offset will include Kind and Len
        {
            Utils.memcpy(ref routerip, 0, data, offset + 2, 4);
        }
        public override byte Length { get { return 6; } }
        public override byte Code { get { return 3; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, routerip, 0, 4);
            return ret;
        }
    }
    class DHCPopDNS : TCPOption //can be longer then 1 address (not supported)
    {
        byte[] dnsip = new byte[4];
        public DHCPopDNS(IPAddress ip)
        {
            dnsip = ip.GetAddressBytes();
        }
        public DHCPopDNS(byte[] data, int offset) //Offset will include Kind and Len
        {
            Utils.memcpy(ref dnsip, 0, data, offset + 2, 4);
        }
        public override byte Length { get { return 6; } }
        public override byte Code { get { return 6; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, dnsip, 0, 4);
            return ret;
        }
    }
    class DHCPopBCIP : TCPOption //The IP to send broadcasts to
    {
        byte[] ip = new byte[4];
        public DHCPopBCIP(byte[] data) //ip provided as byte array
        {
            ip = data;
        }
        public DHCPopBCIP(byte[] data, int offset) //Offset will include Kind and Len
        {
            Utils.memcpy(ref ip, 0, data, offset + 2, 4);
        }
        public override byte Length { get { return 6; } }
        public override byte Code { get { return 28; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, ip, 0, 4);
            return ret;
        }
    }
    class DHCPopDNSNAME : TCPOption
    {
        byte len;
        byte[] DomainNameBytes;
        public DHCPopDNSNAME(string name)
        {
            DomainNameBytes = ASCIIEncoding.ASCII.GetBytes(name);
            len = (byte)DomainNameBytes.Length;
            if (DomainNameBytes.Length > len)
            {
                throw new Exception("Domain Name Overflow");
            }
        }
        public DHCPopDNSNAME(byte[] data, int offset) //Offset will include Kind and Len
        {
            throw new NotImplementedException();
            //len = data[offset + 1];
            //DomainNameBytes = new byte[len];
            //Utils.memcpy(ref DomainNameBytes, 0, data, offset + 2, len);
        }
        public override byte Length { get { return (byte)(2 + len); } }
        public override byte Code { get { return 15; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, DomainNameBytes, 0, len);
            return ret;
        }
    }
    class DHCPopREQIP : TCPOption //can be longer then 1 address (not supported)
    {
        byte[] ip = new byte[4];
        public byte[] IPaddress
        {
            get
            {
                return ip;
            }
        }
        public DHCPopREQIP(byte[] data, int offset) //Offset will include Kind and Len
        {
            Utils.memcpy(ref ip, 0, data, offset + 2, 4);
        }
        public override byte Length { get { return 6; } }
        public override byte Code { get { return 50; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, ip, 0, 4);
            return ret;
        }
    }
    class DHCPopIPLT : TCPOption
    {
        Int32 NO_Time;
        public UInt32 IPLeaseTime
        {
            get
            {
                return (UInt32)IPAddress.NetworkToHostOrder(NO_Time);
            }
            protected set
            {
                NO_Time = IPAddress.HostToNetworkOrder((Int32)value);
            }
        }
        public DHCPopIPLT(UInt32 LeaseTime)
        {
            IPLeaseTime = LeaseTime;
        }
        public DHCPopIPLT(byte[] data, int offset) //Offset will include Kind and Len
        {
            NO_Time = (BitConverter.ToInt32(data, offset + 2));
        }
        public override byte Length { get { return 6; } }
        public override byte Code { get { return 51; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, BitConverter.GetBytes(NO_Time), 0, 4);
            return ret;
        }
    }
    class DHCPopMSG : TCPOption
    {
        byte msg;
        public byte Message
        {
            get
            {
                return msg;
            }
        }
        public DHCPopMSG(byte parMsg)
        {
            msg = parMsg;
        }
        public DHCPopMSG(byte[] data, int offset) //Offset will include Kind and Len
        {
            msg = data[offset + 2];
        }
        public override byte Length { get { return 3; } }
        public override byte Code { get { return 53; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            ret[2] = msg;
            return ret;
        }
    }
    class DHCPopSERVIP : TCPOption //DHCP server ip
    {
        byte[] ip = new byte[4];
        public byte[] IPaddress
        {
            get
            {
                return ip;
            }
        }
        public DHCPopSERVIP(byte[] data) //ip provided as byte array
        {
            ip = data;
        }
        public DHCPopSERVIP(byte[] data, int offset) //Offset will include Kind and Len
        {
            Utils.memcpy(ref ip, 0, data, offset + 2, 4);
        }
        public override byte Length { get { return 6; } }
        public override byte Code { get { return 54; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, ip, 0, 4);
            return ret;
        }
    }
    class DHCPopREQLIST : TCPOption
    {
        byte len;
        byte[] Requests;
        public byte[] RequestList
        {
            get
            {
                return Requests;
            }
        }
        public DHCPopREQLIST(byte[] data, int offset) //Offset will include Kind and Len
        {
            len = data[offset + 1];
            Requests = new byte[len];
            Utils.memcpy(ref Requests, 0, data, offset + 2, len);
        }
        public override byte Length { get { return (byte)(2 + len); } }
        public override byte Code { get { return 55; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, Requests, 0, len);
            return ret;
        }
    }
    class DHCPopMSGStrOld : TCPOption
    {
        byte len;
        byte[] MsgBytes;
        public DHCPopMSGStrOld(byte[] data, int offset) //Offset will include Kind and Len
        {
            len = data[offset + 1];
            MsgBytes = new byte[len];
            Utils.memcpy(ref MsgBytes, 0, data, offset + 2, len);
            Encoding enc = Encoding.ASCII;
            Console.Error.WriteLine(enc.GetString(MsgBytes));
        }
        public override byte Length { get { return (byte)(2 + len); } }
        public override byte Code { get { return 56; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, MsgBytes, 0, len);
            return ret;
        }
    }
    class DHCPopMSGStr : TCPOption
    {
        byte len;
        byte[] MsgBytes;
        public DHCPopMSGStr(byte[] data, int offset) //Offset will include Kind and Len
        {
            len = data[offset + 1];
            MsgBytes = new byte[len];
            Utils.memcpy(ref MsgBytes, 0, data, offset + 2, len);
            Encoding enc = Encoding.ASCII;
            Console.Error.WriteLine(enc.GetString(MsgBytes));
            //Console.Error.WriteLine(BitConverter.ToString(MsgBytes, 0, MsgBytes.Length));
        }
        public override byte Length { get { return (byte)(2 + len); } }
        public override byte Code { get { return 56; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, MsgBytes, 0, len);
            return ret;
        }
    }
    class DHCPopMMSGS : TCPOption
    {
        Int16 NO_MMSGS;
        public UInt16 MaxMessageSize
        {
            get
            {
                return (UInt16)IPAddress.NetworkToHostOrder(NO_MMSGS);
            }
            protected set
            {
                NO_MMSGS = IPAddress.HostToNetworkOrder((Int16)value);
            }
        }
        public DHCPopMMSGS(byte[] data, int offset) //Offset will include Kind and Len
        {
            NO_MMSGS = (BitConverter.ToInt16(data, offset + 2));
        }
        public override byte Length { get { return 4; } }
        public override byte Code { get { return 57; } }
        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, BitConverter.GetBytes(NO_MMSGS), 0, 2);
            return ret;
        }
    }
    class DHCPopCID : TCPOption
    {
        byte len;
        byte[] ClientID;
        public DHCPopCID(byte[] data, int offset) //Offset will include Kind and Len
        {
            len = data[offset + 1];
            ClientID = new byte[len];
            Utils.memcpy(ref ClientID, 0, data, offset + 2, len);
        }
        public override byte Length { get { return (byte)(2 + len); } }
        public override byte Code { get { return 61; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = (byte)(Length - 2);
            Utils.memcpy(ref ret, 2, ClientID, 0, len);
            return ret;
        }
    }
    class DHCPopEND : TCPOption
    {
        public DHCPopEND()
        {

        }
        public override byte Length { get { return 1; } }
        public override byte Code { get { return 255; } }

        public override byte[] GetBytes()
        {
            return new byte[] { Code };
        }
    }
}
