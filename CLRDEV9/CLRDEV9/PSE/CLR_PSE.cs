using System;
using CLRDEV9;

namespace PSE
{
    public class CLR_PSE
    {
        //Multi-in-one is not supported
        private enum CLR_Type : int
        {
            GS = 1,
            PAD = 2,
            SPU2 = 4,
            CDVD = 8,
            USB = 16,
            FW = 32,
            DEV9 = 64
        }
        //major
        public const byte revision = 0;
        //minor
        public const byte build = 1;

        private const string libraryName = "CLR DEV9 Test";
        public static string PS2EgetLibName()
        {
            return libraryName;
        }

        public static int PS2EgetLibType()
        {
            return (int)CLR_Type.DEV9;
        }

        public static CLR_PSE_DEV9 NewDEV9()
        {
            return new DEV9();
        }

        public static CLR_PSE_PluginLog NewLog()
        {
            return new CLR_PSE_PluginLog();
        }

        public static CLR_PSE_Config NewConfig()
        {
            //return new Config.CLR_Config();
            return new dummyConfig();
        }
    }

    public abstract class CLR_PSE_Base
    {
        public CLR_PSE_PluginLog PluginLog;

        protected CLR_PSE_Config PluginConf;
        public abstract Int32 Init(byte wrapperRev, byte wrapperBuid);
        public abstract void Shutdown();

        public abstract void Close();

        public abstract void SetSettingsDir(string dir);
        public abstract void SetLogDir(string dir);

        public abstract byte[] FreezeSave();
        public abstract Int32 FreezeLoad(byte[] data);
        public abstract Int32 FreezeSize();

        public abstract Int32 Test();

        public void SetConfig(CLR_PSE_Config pPluginConf)
        {
            PluginConf = pPluginConf;
            PluginConf.SetBase(this);
        }
    }

    public abstract class CLR_PSE_Config
    {

        protected CLR_PSE_Base Base;
        public string IniFolderPath = "inis";
        //PluginConf calls
        public CLR_PSE_Callbacks.Config_Open Open;
        public CLR_PSE_Callbacks.Close Close;
        public CLR_PSE_Callbacks.Config_WriteInt WriteInt;

        public CLR_PSE_Callbacks.Config_ReadInt ReadInt;
        public void SetBase(CLR_PSE_Base common)
        {
            Base = common;
        }

        public abstract void About();
        public abstract void Configure();

        public abstract void LoadConfig();
        protected abstract void SaveConfig();
    }
}
