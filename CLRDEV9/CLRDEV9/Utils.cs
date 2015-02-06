using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRDEV9
{
    static class Utils
    {
        public static void memcpy(ref byte[] target, int targetstartindex, byte[] source, int sourcestartindex, int num)
        {
            for (int x = 0; x <= num - 1; x++)
            {
                target[targetstartindex + x] = source[sourcestartindex + x];
            }
        }
        public static void memset(ref byte[] data, int offset, byte value, int length)
        {
            for (int x = offset; x <= (offset + length) - 1; x++)
            {
                data[x] = value;
            }
        }
        public static bool memcmp(byte[] target, int targetstartindex, byte[] source, int sourcestartindex, int num)
        {
            bool match = true;
            for (int x = 0; x <= num - 1; x++)
            {
                if (!(target[targetstartindex + x] == source[sourcestartindex + x]))
                {
                    match = false;
                    return match;
                }
            }
            return match;
        }
        public static bool memcmp(byte[] target, int targetstartindex, ushort[] source, int sourcestartindex, int num)
        {
            bool match = true;
            for (int x = 0; x <= num - 1; x++)
            {
                if (!(target[targetstartindex + x] == source[sourcestartindex + x]))
                {
                    match = false;
                    return match;
                }
            }
            return match;
        }
    }
}
