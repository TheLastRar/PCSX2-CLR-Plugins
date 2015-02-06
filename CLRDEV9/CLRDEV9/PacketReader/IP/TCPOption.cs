using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CLRDEV9.PacketReader
{
    abstract class TCPOption
    {
        public abstract byte Length
        {
            get;
        }
        public abstract byte Code
        {
            get;
        }
        public abstract byte[] GetBytes();
    }
    //class TCPopEOO : TCPOption
    //{
    //    public TCPopEOO()
    //    {

    //    }
    //    public override byte Length
    //    {
    //        get
    //        {
    //            return 1;
    //        }
    //    }
    //    public override byte[] GetBytes()
    //    {
    //        return new byte[] { 0 };
    //    }
    //}
    class TCPopNOP : TCPOption
    {
        public TCPopNOP()
        {

        }
        public override byte Length { get { return 1; } }
        public override byte Code { get { return 1; } }

        public override byte[] GetBytes()
        {
            return new byte[] { Code };
        }
    }
    class TCPopMSS : TCPOption
    {
        Int16 NO_MMS;
        public UInt16 MaxSegmentSize
        {
            get
            {
                return (UInt16)IPAddress.NetworkToHostOrder(NO_MMS);
            }
            protected set
            {
                NO_MMS = IPAddress.HostToNetworkOrder((Int16)value);
            }
        }
        public TCPopMSS(UInt16 MSS) //Offset will include Kind and Len
        {
            //'(32 bits)'
            MaxSegmentSize = MSS;
        }
        public TCPopMSS(byte[] data, int offset) //Offset will include Kind and Len
        {
            //'(32 bits)'
            NO_MMS = (BitConverter.ToInt16(data, offset + 2));
            Console.Error.WriteLine("Got Maximum segment size of " + MaxSegmentSize);
        }
        public override byte Length { get { return 4; } }
        public override byte Code { get { return 2; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = Length;
            Utils.memcpy(ref ret, 2, BitConverter.GetBytes(NO_MMS), 0, 2);
            return ret;
        }
    }
    class TCPopWS : TCPOption
    {
        byte WindowScale;
        public TCPopWS(byte WS) //Offset will include Kind and Len
        {
            //'(24 bits)'
            WindowScale = WS;
        }
        public TCPopWS(byte[] data, int offset) //Offset will include Kind and Len
        {
            //'(24 bits)'
            WindowScale = data[offset + 2];
            Console.Error.WriteLine("Got Window scale of " + WindowScale);
        }
        public override byte Length { get { return 3; } }
        public override byte Code { get { return 3; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = Length;
            ret[2] = WindowScale;
            return ret;
        }
    }
    class TCPopTS : TCPOption
    {
        Int32 NO_senderTS;
        public UInt32 SenderTimeStamp
        {
            get
            {
                return (UInt32)IPAddress.NetworkToHostOrder(NO_senderTS);
            }
            protected set
            {
                NO_senderTS = IPAddress.HostToNetworkOrder((Int32)value);
            }
        }
        Int32 NO_echoTS;
        public UInt32 EchoTimeStamp
        {
            get
            {
                return (UInt32)IPAddress.NetworkToHostOrder(NO_echoTS);
            }
            protected set
            {
                NO_echoTS = IPAddress.HostToNetworkOrder((Int32)value);
            }
        }
        public TCPopTS(UInt32 SenderTS, UInt32 EchoTS)
        {
            SenderTimeStamp = SenderTS;
            EchoTimeStamp = EchoTS;
        }
        public TCPopTS(byte[] data, int offset) //Offset will include Kind and Len
        {
            //'(80 bits)'
            NO_senderTS = (BitConverter.ToInt32(data, offset + 2));
            NO_echoTS = (BitConverter.ToInt32(data, offset + 6));
        }
        public override byte Length { get { return 10; } }
        public override byte Code { get { return 8; } }

        public override byte[] GetBytes()
        {
            byte[] ret = new byte[Length];
            ret[0] = Code;
            ret[1] = Length;
            Utils.memcpy(ref ret, 2, BitConverter.GetBytes(NO_senderTS), 0, 4);
            Utils.memcpy(ref ret, 6, BitConverter.GetBytes(NO_echoTS), 0, 4);
            return ret;
        }
    }
}
