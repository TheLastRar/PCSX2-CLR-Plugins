using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSE
{
    public abstract class CLR_PSE_DEV9 : CLR_PSE_Base
    {
        //This varies from plugin to plugin, 
        //though it's unlikely for this to be used for those pluings
        public abstract Int32 DEV9open(IntPtr hWnd);

        public abstract byte DEV9read8(UInt32 addr);
        public abstract UInt16 DEV9read16(UInt32 addr);
        public abstract UInt32 DEV9read32(UInt32 addr);
        public abstract void DEV9write8(UInt32 addr, byte value);
        public abstract void DEV9write16(UInt32 addr, UInt16 value);
        public abstract void DEV9write32(UInt32 addr, UInt32 value);
        public abstract void DEV9readDMA8Mem(System.IO.UnmanagedMemoryStream addr, int size);
        public abstract void DEV9writeDMA8Mem(System.IO.UnmanagedMemoryStream addr, int size);
        public abstract void DEV9irqCallback(CLR_PSE_Callbacks.CLR_CyclesCallback callback);
        public abstract Int32 _DEV9irqHandler();
        public abstract void DEV9async(UInt32 cycles);

    }
}
