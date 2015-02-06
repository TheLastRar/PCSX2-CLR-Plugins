using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CLRDEV9.PacketReader;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace CLRDEV9.Sessions
{
    class TCPSession : Session
    {
        private enum TCPState
        {
            None,
            SentSYN_ACK,
            Connected,
            ConnectionClosedByPS2,
            ConnectionClosedByPS2AndRemote,
            ConnectionClosedByRemote,
            Closed
        }
        private enum NumCheckResult
        {
            OK,
            GotOldData,
            Bad
        }

        List<TCP> recvbuff = new List<TCP>();
        //TCP LastDataPacket = null; //Only 1 outstanding data packet from the remote source can exist at a time
        TcpClient client;
        TCPState state = TCPState.None;

        UInt16 SrcPort = 0;
        UInt16 DestPort = 0;
        
        //UInt16 WindowSize; //assume zero scale
        UInt16 MaxSegmentSize = 1460;

        UInt32 LastRecivedTimeStamp;
        Stopwatch TimeStamp = new Stopwatch();
        bool SendTimeStamps = false;

        UInt32 ExpectedSequenceNumber;
        List<UInt32> ReceivedSequenceNumbers = new List<UInt32>();

        UInt32 MySequenceNumber = 1;
        UInt32 OldMyNumber = 1;
        bool ACKMyNumber = true;

        Object sentry = new Object();

        public override IPPayload recv()
        {
            lock (sentry)
            {
                if (recvbuff.Count != 0)
                {
                    TCP ret = recvbuff[0];
                    recvbuff.RemoveAt(0);
                    return ret;
                }
            }

            if (client == null) { return null; }

            if (client.Connected == false) { return null; }

            if (client.Available != 0 && ACKMyNumber==true)
            {
                int avaData = client.Available;
                if (avaData > (MaxSegmentSize-16))
                {
                    Console.Error.WriteLine("TOO MUCH DATA");
                    avaData = MaxSegmentSize-16;
                }

                byte[] recived = new byte[avaData];
                //Console.Error.WriteLine("Received " + avaData);
                client.GetStream().Read(recived, 0, avaData);

                TCP iRet = CreateBasePacket(recived);

                iRet.ACK = true;
                iRet.PSH = true;
                iRet.WindowSize = 16 * 1024;

                lock (sentry)
                {
                    OldMyNumber = MySequenceNumber;
                    unchecked
                    {
                        MySequenceNumber += ((uint)avaData);
                    }
                    ACKMyNumber = false;
                }
                return iRet;
            }

            if (client.Client.Poll(1, SelectMode.SelectRead) && client.Client.Available == 0 && state == TCPState.Connected)
            {
                Console.Error.WriteLine("Detected Closed By Remote Connection");
                PerformCloseByRemote();
                client.Close();
            }

            return null;
        }
        public override bool send(IPPayload payload)
        {
            TCP tcp = (TCP)payload;
            if (DestPort != 0)
            {
                if (!(tcp.DestinationPort == DestPort && tcp.SourcePort == SrcPort))
                {
                    Console.Error.WriteLine("TCP packet invalid for current session (Duplicate key?)");
                    return false;
                }
            }

            switch (state)
            {
                case TCPState.None:
                    #region "SYN"
                    DestPort = tcp.DestinationPort;
                    SrcPort = tcp.SourcePort;
                    if (tcp.SYN == false)
                    {
                        PerformRST();
                        Console.Error.WriteLine("Connection Not in Connected State");
                        return true;
                    }
                    ExpectedSequenceNumber = tcp.SequenceNumber + 1;
                    //Fill out last received numbers
                    ReceivedSequenceNumbers.Add(tcp.SequenceNumber);
                    ReceivedSequenceNumbers.Add(tcp.SequenceNumber);
                    ReceivedSequenceNumbers.Add(tcp.SequenceNumber);
                    ReceivedSequenceNumbers.Add(tcp.SequenceNumber);
                    ReceivedSequenceNumbers.Add(tcp.SequenceNumber);

                    for (int i = 0; i < tcp.Options.Count; i++)
                    {
                        switch (tcp.Options[i].Code)
                        {
                            case 0:
                                //Console.Error.WriteLine("Got END");
                                continue;
                            case 1:
                                //Console.Error.WriteLine("Got NOP");
                                continue;
                            case 2:
                                //Console.Error.WriteLine("Got MSS");
                                MaxSegmentSize = ((TCPopMSS)(tcp.Options[i])).MaxSegmentSize;
                                break;
                            case 3:
                                //Console.Error.WriteLine("Got WinScale");
                                // = ((TCPopWS)(tcp.Options[i])).WindowScale;
                                break;
                            case 8:
                                //Console.Error.WriteLine("Got TimeStamp");
                                LastRecivedTimeStamp = ((TCPopTS)(tcp.Options[i])).SenderTimeStamp;
                                SendTimeStamps = true;
                                TimeStamp.Start();
                                break;
                            default:
                                Console.Error.WriteLine("Got Unknown Option " + tcp.Options[i].Code);
                                throw new Exception();
                            //break;
                        }   
                    }

                    client = new TcpClient();
                    IPAddress address = new IPAddress(DestIP);
                    client.Connect(address, DestPort); //address to send to
                    client.NoDelay = true;
                    if (client.Connected)
                    {
                        open = true;
                        state = TCPState.SentSYN_ACK;
                        byte[] emptyByte = new byte[0];
                        TCP ret = new TCP(emptyByte);
                        //and now to setup THE ENTIRE THING
                        ret.SourcePort = tcp.DestinationPort;
                        ret.DestinationPort = tcp.SourcePort;

                        ret.SequenceNumber = MySequenceNumber;
                        MySequenceNumber += 1;
                        ret.AcknowledgementNumber = ExpectedSequenceNumber;

                        ret.SYN = true;
                        ret.ACK = true;
                        ret.WindowSize = 16 * 1024;
                        ret.Options.Add(new TCPopMSS(MaxSegmentSize));

                        ret.Options.Add(new TCPopNOP());
                        ret.Options.Add(new TCPopWS(0));

                        if (SendTimeStamps)
                        {
                            ret.Options.Add(new TCPopNOP());
                            ret.Options.Add(new TCPopNOP());
                            ret.Options.Add(new TCPopTS((UInt32)TimeStamp.Elapsed.Seconds, LastRecivedTimeStamp));
                        }
                        recvbuff.Add(ret);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    #endregion
                case TCPState.SentSYN_ACK:
                    #region "Syn-Ack"
                    lock (sentry)
                    {
                        if (tcp.SYN == true)
                        {
                            throw new Exception("Attempt to Connect to an operning Port");
                        }
                        NumCheckResult Result = CheckNumbers(tcp);
                        if (Result == NumCheckResult.Bad) { throw new Exception("Bad TCP Number Received"); }

                        for (int i = 0; i < tcp.Options.Count; i++)
                        {
                            switch (tcp.Options[i].Code)
                            {
                                case 0:
                                    //Console.Error.WriteLine("Got END");
                                    continue;
                                case 1:
                                    //Console.Error.WriteLine("Got NOP");
                                    continue;
                                case 8:
                                    //Console.Error.WriteLine("Got TimeStamp");
                                    LastRecivedTimeStamp = ((TCPopTS)(tcp.Options[i])).SenderTimeStamp;
                                    break;
                                default:
                                    Console.Error.WriteLine("Got Unknown Option " + tcp.Options[i].Code);
                                    throw new Exception();
                                //break;
                            }
                        }
                        //Next packet will be data
                        state = TCPState.Connected;
                    }
                    return true;
                    #endregion
                case TCPState.Connected:
                    #region "Connected"
                    if (tcp.SYN == true)
                    {
                        throw new Exception("Attempt to Connect to an open Port");
                    }
                    lock (sentry)
                    {
                        for (int i = 0; i < tcp.Options.Count; i++)
                        {
                            switch (tcp.Options[i].Code)
                            {
                                case 0:
                                    //Console.Error.WriteLine("Got END");
                                    continue;
                                case 1:
                                    //Console.Error.WriteLine("Got NOP");
                                    continue;
                                case 8:
                                    //Console.Error.WriteLine("Got TimeStamp");
                                    LastRecivedTimeStamp = ((TCPopTS)(tcp.Options[i])).SenderTimeStamp;
                                    break;
                                default:
                                    Console.Error.WriteLine("Got Unknown Option " + tcp.Options[i].Code);
                                    throw new Exception();
                                //break;
                            }
                        }
                        NumCheckResult Result = CheckNumbers(tcp);
                        if (Result == NumCheckResult.GotOldData) 
                        {
                            throw new NotImplementedException();
                            //return true;
                        }
                        if (Result == NumCheckResult.Bad) { throw new Exception("Bad TCP Number Received"); }
                        if (tcp.FIN == true) //Connection Close Part 1, receive FIN from PS2
                        {
                            PerformCloseByPS2();
                            return true;
                        }
                        if (tcp.GetPayload().Length != 0)
                        {
                            ReceivedSequenceNumbers.RemoveAt(0);
                            ReceivedSequenceNumbers.Add(ExpectedSequenceNumber);
                            //Send the Data
                            try
                            {
                                client.GetStream().Write(tcp.GetPayload(), 0, tcp.GetPayload().Length);
                            } catch (Exception e)
                            {
                                System.Windows.Forms.MessageBox.Show("Got IO Error :" + e.ToString());
                                //Connection Lost
                                //Send Shutdown (Untested)
                                PerformRST();
                                open = false;
                                return true;
                            }
                            unchecked
                            {
                                ExpectedSequenceNumber += ((uint)tcp.GetPayload().Length);
                            }
                            //Done send

                            //ACK data
                            TCP ret = CreateBasePacket();
                            ret.ACK = true;
                            recvbuff.Add(ret);
                        }
                    }
                    return true;
                    #endregion
                case TCPState.ConnectionClosedByPS2AndRemote:
                    #region "Closing"
                    //Close Part 4, Recive ACK from PS2
                    Console.Error.WriteLine("Compleated Close By PS2");
                    NumCheckResult ResultFIN = CheckNumbers(tcp);
                    if (ResultFIN == NumCheckResult.GotOldData) { return false; }
                    if (ResultFIN == NumCheckResult.Bad) { throw new Exception("Bad TCP Number Received"); }
                    state = TCPState.Closed;
                    open = false;
                    return true;
                #endregion
                case TCPState.ConnectionClosedByRemote:
                    #region "Closing"
                    //Expect fin+ack
                    if (tcp.FIN == true)
                    {
                        Console.Error.WriteLine("Compleated Close By Remote");
                        ReceivedSequenceNumbers.RemoveAt(0);
                        ReceivedSequenceNumbers.Add(ExpectedSequenceNumber);
                        unchecked
                        {
                            ExpectedSequenceNumber += 1;
                        }
                        TCP ret = CreateBasePacket();

                        ret.ACK = true;

                        recvbuff.Add(ret);
                        state = TCPState.ConnectionClosedByPS2AndRemote;
                        open = false;
                        return true;
                    }
                    //throw new Exception("Invalid Packet");
                    return false;
                    //break;
                    #endregion
                default:
                    throw new Exception("Invalid State");
            }
        }

        bool open = false;
        public override bool isOpen()
        {
            return open;
        }
        public override void forceClose()
        {
            open = false;
            client.Close();
        }

        private NumCheckResult CheckNumbers(TCP tcp)
        {
            ACKMyNumber = true;
            if (tcp.AcknowledgementNumber != MySequenceNumber)
            {
                ACKMyNumber = false;
                Console.Error.WriteLine("Unexpected Acknowledgement Number, Got " + tcp.AcknowledgementNumber + " Expected " + MySequenceNumber);
                if (tcp.AcknowledgementNumber != OldMyNumber)
                {
                    throw new Exception("Unexpected Acknowledgement Number did not Match Old Number of " + OldMyNumber);
                }
            }

            if (tcp.SequenceNumber != ExpectedSequenceNumber)
            {
                if (tcp.GetPayload().Length == 0)
                {
                    Console.Error.WriteLine("Unexpected Sequence Number From Act Packet, Got " + tcp.SequenceNumber + " Expected " + ExpectedSequenceNumber);
                }
                else
                {
                    if (ReceivedSequenceNumbers.Contains(tcp.SequenceNumber))
                    {
                        Console.Error.WriteLine("Got an Old Seq Number on an Data packet");
                        return NumCheckResult.GotOldData;
                    }
                    else
                        throw new Exception("Unexpected Sequence Number From Data Packet, Got " + tcp.SequenceNumber + " Expected " + ExpectedSequenceNumber);
                }
            }

            return NumCheckResult.OK;
        }

        private void PerformCloseByPS2()
        {
            client.Close();
            Console.Error.WriteLine("PS2 has closed connection");
            //Connection Close Part 2, Send ACK to PS2
            ReceivedSequenceNumbers.RemoveAt(0);
            ReceivedSequenceNumbers.Add(ExpectedSequenceNumber);
            unchecked
            {
                ExpectedSequenceNumber += 1;
            }
            TCP ret = CreateBasePacket();

            ret.ACK = true;
            ret.FIN = true;

            recvbuff.Add(ret);
            state = TCPState.ConnectionClosedByPS2AndRemote;

            lock (sentry)
            {
                OldMyNumber = MySequenceNumber;
                unchecked
                {
                    MySequenceNumber += (1);
                }
            }
        }
        private void PerformCloseByRemote()
        {
            client.Close();
            Console.Error.WriteLine("Remote has closed connection");
            TCP ret = CreateBasePacket();

            ret.ACK = true;
            ret.FIN = true;

            recvbuff.Add(ret);
            state = TCPState.ConnectionClosedByRemote;

            lock (sentry)
            {
                OldMyNumber = MySequenceNumber;
                unchecked
                {
                    MySequenceNumber += (1);
                }
            }
        }

        private void PerformRST()
        {
            byte[] emptyByteerr = new byte[0];
            TCP reterr = CreateBasePacket();
            reterr.RST = true;
            recvbuff.Add(reterr);
        }
        private TCP CreateBasePacket(byte[] data = null)
        {
            if (data == null) { data = new byte[0];}
            TCP ret = new TCP(data);

            //and now to setup THE ENTIRE THING
            ret.SourcePort = DestPort;
            ret.DestinationPort = SrcPort;
            ret.SequenceNumber = MySequenceNumber;
            ret.AcknowledgementNumber = ExpectedSequenceNumber;

            ret.WindowSize = 16 * 1024;

            if (SendTimeStamps)
            {
                ret.Options.Add(new TCPopNOP());
                ret.Options.Add(new TCPopNOP());
                ret.Options.Add(new TCPopTS((UInt32)TimeStamp.Elapsed.TotalSeconds, LastRecivedTimeStamp));
            }
            return ret;
        }
    }
}
