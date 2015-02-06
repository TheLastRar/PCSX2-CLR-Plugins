using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CLRDEV9.PacketReader;
using System.Net;
using System.Net.NetworkInformation;

namespace CLRDEV9.Sessions
{
    class ICMPSession : Session
    {
        Object sentry = new Object();

        List<ICMP> recvbuff = new List<ICMP>();

        Ping ping = new Ping();
        public ICMPSession()
        {
            ping.PingCompleted += PingCompleate;
        }

        struct PingData
        {
            public byte[] HeaderData;
            public byte[] Data;
        }

        public void PingCompleate(object sender, System.Net.NetworkInformation.PingCompletedEventArgs e)
        {
            PingData Seq = (PingData)e.UserState;
            PingReply rep = e.Reply;
            lock (sentry)
            {
                switch (rep.Status)
                {
                    case IPStatus.Success:
                        ICMP retICMP = new ICMP(Seq.Data);
                        retICMP.HeaderData = Seq.HeaderData;
                        retICMP.Type = 0; //echo reply
                        recvbuff.Add(retICMP);
                        break;
                    default:
                        open -= 1;
                        break;
                }
            }
        }

        public override IPPayload recv()
        {
            //Console.Error.WriteLine("UDP Recive");
            if (recvbuff.Count != 0)
            {
                ICMP ret;
                lock (sentry)
                {
                    ret = recvbuff[0];
                    recvbuff.RemoveAt(0);
                    open -= 1;
                }
                return ret;
                //}
            }

            return null;
        }
        public override bool send(IPPayload payload)
        {
            ICMP icmp = (ICMP)payload;

            switch (icmp.Type)
            {
                case 8:
                    //Code == zero
                    Console.Error.WriteLine("Send Ping");
                    lock (sentry)
                    {
                        open += 1;
                    }
                    PingData PD;
                    PD.Data = icmp.Data;
                    PD.HeaderData = icmp.HeaderData;
                    ping.SendAsync(new IPAddress(DestIP),PD);
                    break;
                default:
                    throw new NotImplementedException("Unsupported ICMP Type" + icmp.Type);
            }

            return true;
        }

        int open = 0;
        public override bool isOpen()
        {
            return (open != 0);
        }
        public override void forceClose()
        {
            open = 0;
            ping.SendAsyncCancel();
            ping.Dispose();
        }
    }
}
