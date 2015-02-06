using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSE
{
    public class CLR_PSE_PluginLog
    {
        public CLR_PSE_Callbacks.Log_Open Open;
        public CLR_PSE_Callbacks.Close Close;
        public CLR_PSE_Callbacks.Log_Write LogWrite;

        public CLR_PSE_Callbacks.Log_Write LogWriteLine;
        public CLR_PSE_Callbacks.Log_SetValue SetWriteToFile;

        public void ErrorWrite(string str)
        {
            Console.Error.Write(str);
            //log to stderr
            this.LogWrite(str);
        }
        public void ErrorWriteLine(string str)
        {
            Console.Error.WriteLine(str);
            //log to stderr
            this.LogWriteLine(str);
        }

    }
}
