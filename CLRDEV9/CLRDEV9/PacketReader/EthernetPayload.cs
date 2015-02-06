using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRDEV9.PacketReader
{
    abstract class EthernetPayload
    {
        //abstract public byte[] GetPayload();
        public abstract UInt16 Length
        {
            get;
            protected set;
        }
        public abstract byte[] GetBytes
        {
            get;
        }
    }
}
