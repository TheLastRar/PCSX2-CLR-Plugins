using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace PSE.CLR_PSE_Callbacks
{
    public delegate void Close();
    //Logs+config
    //Logs
    public delegate bool Log_Open(string logname);
    public delegate void Log_Write(string fmt);
    public delegate void Log_SetValue(bool par);

    //USBCallback
    public delegate void CLR_CyclesCallback(int cycles);

    //Config
    public delegate bool Config_Open(string filename, bool writeAccess);
    public delegate int Config_ReadInt(string item, int DefualtValue);
    public delegate void Config_WriteInt(string item, int Value);
}
