using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CLRDEV9.PacketReader
{
    class DHCP
    {
        public byte OP;
        public byte HardwareType;
        public byte HardwareAddressLength;
        public byte Hops;
        public Int32 TransactionID; //xid
        Int16 NO_sec; //seconds
        public UInt16 Seconds
        {
            get
            {
                return (UInt16)IPAddress.NetworkToHostOrder(NO_sec);
            }
            set
            {
                NO_sec = IPAddress.HostToNetworkOrder((Int16)value);
            }
        }
        public Int16 Flags;
        public byte[] ClientIP = new byte[4];
        public byte[] YourIP = new byte[4];
        public byte[] ServerIP = new byte[4];
        public byte[] GatewayIP = new byte[4]; //NOT the router IP
        public byte[] ClientHardwareAddress = new byte[16];
        //192 bytes of padding
        public Int32 MagicCookie;
        public List<TCPOption> Options = new List<TCPOption>();
        public DHCP()
        {

        }
        public DHCP(byte[] data)
        {
            //Bits 0-31 //Bytes 0-3
            OP = data[0];
            Console.Error.WriteLine("OP " + OP);
            HardwareType = data[1];
            //Console.Error.WriteLine("HWt " + HardwareType);
            HardwareAddressLength = data[2];
            //Console.Error.WriteLine("HWaddrlen " + HardwareAddressLength);
            Hops = data[3];
            //Console.Error.WriteLine("Hops " + Hops);
            //Bits 32-63 //Bytes 4-7
            TransactionID = BitConverter.ToInt32(data, 4);
            Console.Error.WriteLine("xid " + TransactionID);
            //Bits 64-95 //Bytes 8-11
            NO_sec = BitConverter.ToInt16(data, 8);
            //Console.Error.WriteLine("sec " + Seconds);
            Flags = BitConverter.ToInt16(data, 10);
            //Console.Error.WriteLine("Flags " + Flags);
            //Bits 96-127 //Bytes 12-15
            Utils.memcpy(ref ClientIP, 0, data, 12, 4);
            Console.Error.WriteLine("CIP " + ClientIP[0] + "." + ClientIP[1] + "." + ClientIP[2] + "." + ClientIP[3]);
            //Bits 128-159 //Bytes 16-19
            Utils.memcpy(ref YourIP, 0, data, 16, 4);
            Console.Error.WriteLine("YIP " + YourIP[0] + "." + YourIP[1] + "." + YourIP[2] + "." + YourIP[3]);
            //Bits 160-191 //Bytes 20-23
            Utils.memcpy(ref ServerIP, 0, data, 20, 4);
            Console.Error.WriteLine("SIP " + ServerIP[0] + "." + ServerIP[1] + "." + ServerIP[2] + "." + ServerIP[3]);
            //Bits 192-223 //Bytes 24-27
            Utils.memcpy(ref GatewayIP, 0, data, 24, 4);
            Console.Error.WriteLine("GIP " + GatewayIP[0] + "." + GatewayIP[1] + "." + GatewayIP[2] + "." + GatewayIP[3]);
            //Bits 192+ //Bytes 28-43
            Utils.memcpy(ref ClientHardwareAddress, 0, data, 28, 16);
            //Bytes 44-107
            byte[] sNamebytes = new byte[64];
            Utils.memcpy(ref sNamebytes, 0, data, 44, 64);


            //Bytes 108-235
            byte[] filebytes = new byte[128];
            Utils.memcpy(ref filebytes, 0, data, 108, 128);

            //Bytes 236-239
            MagicCookie = BitConverter.ToInt32(data, 236);
            //Console.Error.WriteLine("Cookie " + MagicCookie);
            bool opReadFin = false;
            int op_offset = 240;
            do
            {
                byte opKind = data[op_offset];
                if ((op_offset + 1) >= data.Length)
                {
                    Console.Error.WriteLine("Unexpected end of packet");
                    Options.Add(new DHCPopEND());
                    opReadFin = true;
                    continue;
                }
                byte opLen = data[op_offset + 1];
                switch (opKind)
                {
                    case 0:
                        //Console.Error.WriteLine("Got NOP");
                        Options.Add(new DHCPopNOP());
                        op_offset += 1;
                        continue;
                    case 1:
                        //Console.Error.WriteLine("Got Subnet");
                        Options.Add(new DHCPopSubnet(data,op_offset));
                        break;
                    case 3:
                        //Console.Error.WriteLine("Got Router");
                        Options.Add(new DHCPopRouter(data, op_offset));
                        break;
                    case 15:
                        //Console.Error.WriteLine("Got Domain Name (Not supported)");
                        Options.Add(new DHCPopDNSNAME(data, op_offset));
                        break;
                    case 28:
                        //Console.Error.WriteLine("Got broadcast");
                        Options.Add(new DHCPopBCIP(data, op_offset));
                        break;
                    case 50:
                        //Console.Error.WriteLine("Got Request IP");
                        Options.Add(new DHCPopREQIP(data,op_offset));
                        break;
                    case 53:
                        //Console.Error.WriteLine("Got MSG");
                        Options.Add(new DHCPopMSG(data,op_offset));
                        break;
                    case 54:
                        //Console.Error.WriteLine("Got Server IP");
                        Options.Add(new DHCPopSERVIP(data,op_offset));
                        break;
                    case 55:
                        //Console.Error.WriteLine("Got Request List");
                        Options.Add(new DHCPopREQLIST(data, op_offset));
                        break;
                    case 56:
                        Options.Add(new DHCPopMSGStr(data, op_offset));
                        break;
                    case 57:
                        //Console.Error.WriteLine("Got Max Message Size");
                        Options.Add(new DHCPopMMSGS(data, op_offset));
                        break;
                    case 61:
                        //Console.Error.WriteLine("Got Client ID");
                        Options.Add(new DHCPopCID(data, op_offset));
                        break;
                    case 255:
                        //Console.Error.WriteLine("Got END");
                        Options.Add(new DHCPopEND());
                        opReadFin = true;
                        break;
                    default:
                        Console.Error.WriteLine("Got Unknown Option " + opKind + "with len" + opLen);
                        break;
                }
                op_offset += opLen + 2;
                if (op_offset >= data.Length)
                {
                    Console.Error.WriteLine("Unexpected end of packet");
                    Options.Add(new DHCPopEND());
                    opReadFin = true;
                }
            } while (opReadFin==false);
        }
        public byte[] GetBytes(UInt16 MaxLen)
        {
            //int len = 576; //Min size;
            //We will create a message of the min size and hop it fits.
            byte[] ret = new byte[240]; //Fixed size section
            ret[0] = OP;
            ret[1] = HardwareType;
            ret[2] = HardwareAddressLength;
            ret[3] = Hops;

            Utils.memcpy(ref ret, 4, BitConverter.GetBytes(TransactionID), 0, 4);

            Utils.memcpy(ref ret, 8, BitConverter.GetBytes(NO_sec), 0, 2);
            Utils.memcpy(ref ret, 10, BitConverter.GetBytes(Flags), 0, 2);

            Utils.memcpy(ref ret, 12, ClientIP, 0, 4);
            Utils.memcpy(ref ret, 16, YourIP, 0, 4);
            Utils.memcpy(ref ret, 20, ServerIP, 0, 4);
            Utils.memcpy(ref ret, 24, GatewayIP, 0, 4);

            Utils.memcpy(ref ret, 28, ClientHardwareAddress, 0, 16);
            //empty bytes
            Utils.memcpy(ref ret, 236, BitConverter.GetBytes(MagicCookie), 0, 4);

            const UInt16 minOpLength = 64;
            UInt16 OpLength = minOpLength;
            byte[] retOp = new byte[minOpLength];
            int opOffset = 0;
            for (int i = 0; i < Options.Count; i++)
            {
                Utils.memcpy(ref retOp, opOffset, Options[i].GetBytes(), 0, Options[i].Length);
                opOffset += Options[i].Length;
            }

            //byte[] RetFinal = new byte[OpLength+240];
            byte[] RetFinal = new byte[MaxLen];
            Utils.memcpy(ref RetFinal, 0, ret, 0, 240);
            Utils.memcpy(ref RetFinal, 240, retOp, 0, OpLength);
            return RetFinal;
        }
    }
}
