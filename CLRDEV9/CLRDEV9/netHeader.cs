using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRDEV9
{
    class netHeader
    {
        //public struct NetPacket
        public class NetPacket
        {
            public NetPacket()
            {
                size = 0;
            }
            public NetPacket(byte[] bytes, int offset, int sz)
            {
                size = sz;
                Utils.memcpy(ref buffer, 0, bytes, offset, sz);
            }

            public int size;
            public byte[] buffer = new byte[2048 - sizeof(int)];//1536 is realy needed, just pad up to 2048 bytes :)
        };

        public abstract class NetAdapter
        {
            public virtual bool blocks()
            {
                return false;
            }
            public virtual bool recv(ref NetPacket pkt) //gets a packet
            {
                return false;
            }
            public virtual bool send(NetPacket pkt)	//sends the packet and deletes it when done
            {
                return false;
            }
            public abstract void shutdown();	//sends the packet and deletes it when done
        }
    }
}
