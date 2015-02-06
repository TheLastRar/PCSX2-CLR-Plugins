using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRDEV9.PacketReader
{
    enum EtherFrameType: int
    {
        NULL = 0x0000,
        IPv4 = 0x0008,
        ARP = 0x0608,
    }
}
