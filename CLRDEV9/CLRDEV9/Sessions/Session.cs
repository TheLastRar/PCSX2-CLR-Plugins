using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CLRDEV9.PacketReader;

namespace CLRDEV9.Sessions
{
    abstract class Session
    {
        public byte[] SourceIP;
        public byte[] DestIP;

        public abstract IPPayload recv();
        public abstract bool send(IPPayload payload);
        public abstract bool isOpen();
        public abstract void forceClose();
    }
}
