using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRDEV9.PacketReader
{
    abstract class IPPayload
    {
        abstract public byte[] GetPayload();
        abstract public void CalculateCheckSum(byte[] srcIP, byte[] dstIP);
        abstract public byte[] GetBytes();
        abstract public byte Protocol
        {
            get;
        }
        abstract public UInt16 Length
        {
            get;
            protected set;
        }
    }
}
