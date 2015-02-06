using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CLRDEV9.PacketReader;
using CLRDEV9.Sessions;
using System.Net;

namespace CLRDEV9
{
    class Winsock : netHeader.NetAdapter
    {
        List<netHeader.NetPacket> vRecBuffer = new List<netHeader.NetPacket>(); //Non IP packets
        UDP_DHCPsession DCHP_server = new UDP_DHCPsession();
        //List<Session> Connections = new List<Session>();
        Object sentry = new Object();
        Dictionary<string, Session> Connections = new Dictionary<string, Session>();
        public Winsock()
        {
            //Add allways on connections
            DCHP_server.SourceIP = new byte[] { 255, 255, 255, 255 };
            DCHP_server.DestIP = UDP_DHCPsession.DHCP_IP;
            Connections.Add("DHCP", DCHP_server);
        }

        public override bool blocks()
        {
            return true;	//we use blocking io
        }

        byte[] gateway_mac = { 0x76, 0x6D, 0xF4, 0x63, 0x30, 0x31 };
        byte[] ps2_mac;
        byte[] broadcast_adddrrrr = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        //gets a packet.rv :true success
        public override bool recv(ref netHeader.NetPacket pkt)
        {
            //return false;
            bool result = false;

            
            if (ps2_mac == null)
            {
                ps2_mac = new byte[6];
                byte[] eeprombytes = new byte[6];
                for (int i = 0; i < 3; i++)
                {
                    byte[] tmp = BitConverter.GetBytes(DEV9Header.dev9.eeprom[i]);
                    Utils.memcpy(ref eeprombytes, i * 2, tmp, 0, 2);
                }
                Utils.memcpy(ref ps2_mac, 0, eeprombytes, 0, 6);
            }

            if (vRecBuffer.Count == 0)
            {
                List<string> DeadConnections = new List<string>();
                lock (sentry)
                {
                    foreach (string key in Connections.Keys) //ToDo better multi-connection stuff
                    {
                        IPPayload PL;
                        PL = Connections[key].recv();
                        if (!(PL == null))
                        {
                            IPPacket ippkt = new IPPacket(PL);
                            ippkt.DestinationIP = Connections[key].SourceIP;
                            ippkt.SourceIP = Connections[key].DestIP;
                            EthernetFrame eF = new EthernetFrame(ippkt);
                            eF.SourceMAC = gateway_mac;
                            eF.DestinationMAC = ps2_mac;
                            eF.Protocol = (Int16)EtherFrameType.IPv4;
                            pkt = eF.CreatePacket();
                            result = true;
                            break;
                        }
                        if (Connections[key].isOpen() == false)
                        {
                            Console.Error.WriteLine("Removing Closed Connection : " + key);
                            DeadConnections.Add(key);
                        }
                    }
                    foreach (string key in DeadConnections)
                    {
                        Connections.Remove(key);
                    }
                }
            } 
            else
            {
                pkt = vRecBuffer[0];
                vRecBuffer.RemoveAt(0);
                result = true;
            }

            if (result)
            {
                byte[] eeprombytes = new byte[6];
                for (int i = 0; i < 3; i++)
                {
                    byte[] tmp = BitConverter.GetBytes(DEV9Header.dev9.eeprom[i]);
                    Utils.memcpy(ref eeprombytes, i * 2, tmp, 0, 2);
                }
                //original memcmp returns 0 on perfect match
                //the if statment check if !=0
                if ((Utils.memcmp(pkt.buffer, 0, eeprombytes, 0, 6) == false) & (Utils.memcmp(pkt.buffer, 0, broadcast_adddrrrr, 0, 6) == false))
                {
                    //ignore strange packets
                    Console.Error.WriteLine("Dropping Strange Packet");
                    return false;
                }
                return true;
            }
            else
                return false;
        }
        //sends the packet and deletes it when done (if successful).rv :true success
        public override bool send(netHeader.NetPacket pkt)
        {
            bool result = false;

            PacketReader.EthernetFrame ef = new PacketReader.EthernetFrame(pkt);

            switch (ef.Protocol)
            {
                case (int)EtherFrameType.NULL:
                    //Adapter Reset
                    //TODO close all open connections
                    break;
                case (int)EtherFrameType.IPv4:
                    //Console.Error.WriteLine("IPv4");
                    IPPacket ippkt = ((IPPacket)ef.Payload);

                    string Key = (ippkt.DestinationIP[0]) + "." + (ippkt.DestinationIP[1]) + "." + (ippkt.DestinationIP[2]) + "." + ((UInt64)ippkt.DestinationIP[3])
                        + "-" + (ippkt.Protocol);

                    switch (ippkt.Protocol) //(Prase Payload)
                    {
                        case (byte)IPType.ICMP:
                            Console.Error.WriteLine("ICMP");
                            lock (sentry)
                            {
                                if (Connections.ContainsKey(Key))
                                {
                                    if (Connections[Key].isOpen() == false)
                                        throw new Exception("Attempt to send on Closed Connection");
                                    Console.Error.WriteLine("Found Open Connection");
                                    result = Connections[Key].send(ippkt.Payload);
                                }
                                else
                                {
                                    Console.Error.WriteLine("Creating New Connection with key " + Key);
                                    ICMPSession s = new ICMPSession();
                                    s.DestIP = ippkt.DestinationIP;
                                    s.SourceIP = UDP_DHCPsession.PS2_IP;
                                    result = s.send(ippkt.Payload);
                                    Connections.Add(Key, s);
                                }
                            }
                            break;
                        case (byte)IPType.TCP:
                            //Console.Error.WriteLine("TCP");
                            TCP tcp = (TCP)ippkt.Payload;

                            Key += "-" + ((UInt64)tcp.SourcePort) + ":" + ((UInt64)tcp.DestinationPort);
                            lock (sentry)
                            {
                                if (Connections.ContainsKey(Key))
                                {
                                    if (Connections[Key].isOpen() == false)
                                    {
                                        throw new Exception("Attempt to send on Closed TCP Connection of Key : " + Key + "");
                                    }
                                    //Console.Error.WriteLine("Found Open Connection");
                                    result = Connections[Key].send(ippkt.Payload);
                                }
                                else
                                {
                                    //Console.Error.WriteLine("Creating New Connection with key " + Key);
                                    Console.Error.WriteLine("Creating New TCP Connection with Dest Port " + tcp.DestinationPort);
                                    TCPSession s = new TCPSession();
                                    s.DestIP = ippkt.DestinationIP;
                                    s.SourceIP = UDP_DHCPsession.PS2_IP;
                                    result = s.send(ippkt.Payload);
                                    Connections.Add(Key, s);
                                }
                            }
                            break;
                        case (byte)IPType.UDP:
                            //Console.Error.WriteLine("UDP");
                            UDP udp = (UDP)ippkt.Payload;

                            Key += "-" + ((UInt64)udp.SourcePort) + ":" +  ((UInt64)udp.DestinationPort);
                            if (udp.DestinationPort == 67)
                            { //DHCP
                                result = DCHP_server.send(ippkt.Payload);
                                break;
                            }
                            lock (sentry)
                            {
                                if (Connections.ContainsKey(Key))
                                {
                                    if (Connections[Key].isOpen() == false)
                                        throw new Exception("Attempt to send on Closed Connection");
                                    //Console.Error.WriteLine("Found Open Connection");
                                    result = Connections[Key].send(ippkt.Payload);
                                }
                                else
                                {
                                    //Console.Error.WriteLine("Creating New Connection with key " + Key);
                                    Console.Error.WriteLine("Creating New UDP Connection with Dest Port " + udp.DestinationPort);
                                    UDPSession s = new UDPSession();
                                    s.DestIP = ippkt.DestinationIP;
                                    s.SourceIP = UDP_DHCPsession.PS2_IP;
                                    result = s.send(ippkt.Payload);
                                    Connections.Add(Key, s);
                                }
                            }
                            break;
                        default:
                            Console.Error.WriteLine("Unkown Protocol");
                            //throw new NotImplementedException();
                            break;
                    }
                    //Console.Error.WriteLine("Key = " + Key);
                    break;
#region "ARP"
                case (int)EtherFrameType.ARP:
                    Console.Error.WriteLine("ARP (Ignoring)");
                    ARPPacket arppkt = ((ARPPacket)ef.Payload);

                    ////Detect ARP Packet Types
                    //if (Utils.memcmp(arppkt.SenderProtocolAddress, 0, new byte[] { 0, 0, 0, 0 }, 0, 4))
                    //{
                    //    Console.WriteLine("ARP Probe"); //(Who has my IP?)
                    //    break;
                    //}
                    //if (Utils.memcmp(arppkt.SenderProtocolAddress, 0, arppkt.TargetProtocolAddress, 0, 4))
                    //{
                    //    if (Utils.memcmp(arppkt.TargetHardwareAddress, 0, new byte[] { 0, 0, 0, 0, 0, 0 }, 0, 6) & arppkt.OP == 1)
                    //    {
                    //        Console.WriteLine("ARP Announcement Type 1");
                    //        break;
                    //    }
                    //    if (Utils.memcmp(arppkt.SenderHardwareAddress, 0, arppkt.TargetHardwareAddress, 0, 6) & arppkt.OP == 2)
                    //    {
                    //        Console.WriteLine("ARP Announcement Type 2");
                    //        break;
                    //    }
                    //}

                    ////if (arppkt.OP == 1) //ARP request
                    ////{
                    ////    //This didn't work for whatever reason.
                    ////    if (Utils.memcmp(arppkt.TargetProtocolAddress,0,UDP_DHCPsession.GATEWAY_IP,0,4))
                    ////    //it's trying to resolve the virtual gateway's mac addr
                    ////    {
                    ////        Console.Error.WriteLine("ARP Attempt to Resolve Gateway Mac");
                    ////        arppkt.TargetHardwareAddress = arppkt.SenderHardwareAddress;
                    ////        arppkt.SenderHardwareAddress = gateway_mac;
                    ////        arppkt.TargetProtocolAddress = arppkt.SenderProtocolAddress;
                    ////        arppkt.SenderProtocolAddress = UDP_DHCPsession.GATEWAY_IP;
                    ////        arppkt.OP = 2;

                    ////        EthernetFrame retARP = new EthernetFrame(arppkt);
                    ////        retARP.DestinationMAC = ps2_mac;
                    ////        retARP.SourceMAC = gateway_mac;
                    ////        retARP.Protocol = (Int16)EtherFrameType.ARP;
                    ////        vRecBuffer.Add(retARP.CreatePacket());
                    ////        break;
                    ////    }
                    ////}
                    //Console.Error.WriteLine("Unhandled ARP packet");

                    result = true;
                    break;
#endregion
                case (int)0x0081:
                    Console.Error.WriteLine("VLAN-tagged frame (IEEE 802.1Q)");
                    throw new NotImplementedException();
                    //break;
                default:
                    Console.Error.WriteLine("Unkown EtherframeType " + ef.Protocol.ToString("X4"));
                    break;
            }

            return result;
        }
        
        public override void shutdown()
        {
            //TODO close all open connections
            foreach (string key in Connections.Keys) //ToDo better multi-connection stuff
            {
                Connections[key].forceClose();
            }
            vRecBuffer.Clear();
            Connections.Clear();
            //Connections.Add("DHCP", DCHP_server);
        }
    }
}
