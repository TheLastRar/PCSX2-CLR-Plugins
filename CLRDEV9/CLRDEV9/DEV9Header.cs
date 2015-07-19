﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRDEV9
{
    static class DEV9Header
    {
        public const string ETH_DEF = "winsock";//"eth0";
        public const string HDD_DEF = "DEV9hdd.raw";

        public struct Config
        {
            public string Eth;
            public string Hdd;
            public int HddSize;

            //public int hddEnable;
            public int ethEnable;
        }

        public static Config config;

        //public struct dev9Struct
        public class dev9DataClass
        {
            public byte[] dev9R = new byte[0x10000]; //changed to unsigned
            public byte eeprom_state;
            public byte eeprom_command;
            public byte eeprom_address;
            public byte eeprom_bit;
            public byte eeprom_dir;
            public ushort[] eeprom;//[32];

            public UInt32 rxbdi;
            public byte[] rxfifo = new byte[16 * 1024];
            public UInt16 rxfifo_wr_ptr;

            public UInt32 txbdi;
            public byte[] txfifo = new byte[16 * 1024];
            public UInt16 txfifo_rd_ptr;

            public byte bd_swap;
            public UInt16[] atabuf = new UInt16[1024];
            //public UInt32 atacount;
            //public UInt32 atasize;
            public UInt16[] phyregs = new UInt16[32];
            public int irqcause;
            //public byte atacmd;
            //public UInt32 atasector;
            //public UInt32 atansector;
        }

        //EEPROM states
        public const int EEPROM_READY = 0;
        public const int EEPROM_OPCD0 = 1;  //waiting for first bit of opcode
        public const int EEPROM_OPCD1 = 2;  //waiting for second bit of opcode
        public const int EEPROM_ADDR0 = 3;	//waiting for address bits
        public const int EEPROM_ADDR1 = 4;
        public const int EEPROM_ADDR2 = 5;
        public const int EEPROM_ADDR3 = 6;
        public const int EEPROM_ADDR4 = 7;
        public const int EEPROM_ADDR5 = 8;
        public const int EEPROM_TDATA = 9;	//ready to send/receive data

        public static dev9DataClass dev9;

        public static void dev9_rxfifo_write(byte x) { dev9.rxfifo[dev9.rxfifo_wr_ptr++] = x; }

        //public static sbyte dev9Rs8(int mem)	{return dev9.dev9R[mem & 0xffff];}
        //#define dev9Rs16(int mem)	{return dev9.dev9R[mem & 0xffff];}
        //#define dev9Rs32(mem)	(*(s32*)&dev9.dev9R[(mem) & 0xffff])
        public static byte dev9Ru8(int mem) { return dev9.dev9R[mem & 0xffff]; }
        public static UInt16 dev9Ru16(int mem) { return BitConverter.ToUInt16(dev9.dev9R, (mem) & 0xffff); }
        public static UInt32 dev9Ru32(int mem) { return BitConverter.ToUInt32(dev9.dev9R, (mem) & 0xffff); }

        public static void dev9Wu8(int mem, byte value) { dev9.dev9R[mem & 0xffff] = value; }
        public static void dev9Wu16(int mem, UInt16 value)
        {
            byte[] tmp = BitConverter.GetBytes(value);
            Utils.memcpy(ref dev9.dev9R, (mem & 0xffff), tmp, 0, tmp.Length);
        }
        public static void dev9Wu32(int mem, UInt32 value)
        {
            byte[] tmp = BitConverter.GetBytes(value);
            Utils.memcpy(ref dev9.dev9R, (mem & 0xffff), tmp, 0, tmp.Length);
        }

        public static PSE.CLR_PSE_Callbacks.CLR_CyclesCallback DEV9irq;

        public const uint DEV9_R_REV = 0x1f80146e;

        public const uint SPD_R_DMA_CTRL = (SPD_REGBASE + 0x24);
        public const uint SPD_R_INTR_STAT = (SPD_REGBASE + 0x28);
        public const uint SPD_R_INTR_MASK = (SPD_REGBASE + 0x2a);
        public const uint SPD_R_PIO_DIR = (SPD_REGBASE + 0x2c);
        public const uint SPD_R_PIO_DATA = (SPD_REGBASE + 0x2e);
        public const uint SPD_PP_DOUT = (1 << 4);	/* Data output, read port */
        public const uint SPD_PP_DIN = (1 << 5);	/* Data input,  write port */
        public const uint SPD_PP_SCLK = (1 << 6);	/* Clock,       write port */
        public const uint SPD_PP_CSEL = (1 << 7);	/* Chip select, write port */



        /*
            * SMAP (PS2 Network Adapter) register definitions.
            *
            * Copyright (c) 2003 Marcus R. Brown <mrbrown@0xd6.org>
            *
            * * code included from the ps2smap iop driver, modified by linuzappz  *
            */

        /* SMAP interrupt status bits (selected from the SPEED device).  */
        public const int SMAP_INTR_EMAC3 = (1 << 6);
        public const int SMAP_INTR_RXEND = (1 << 5);
        public const int SMAP_INTR_TXEND = (1 << 4);
        public const int SMAP_INTR_RXDNV = (1 << 3);	/* descriptor not valid */
        public const int SMAP_INTR_TXDNV = (1 << 2);	/* descriptor not valid */
        public const int SMAP_INTR_CLR_ALL = (SMAP_INTR_RXEND | SMAP_INTR_TXEND | SMAP_INTR_RXDNV);
        public const int SMAP_INTR_ENA_ALL = (SMAP_INTR_EMAC3 | SMAP_INTR_CLR_ALL);
        public const int SMAP_INTR_BITMSK = 0x7C;

        public const uint SPD_REGBASE = 0x10000000;

        public const uint SPD_R_REV = (SPD_REGBASE + 0x00);
        public const uint SPD_R_REV_1 = (SPD_REGBASE + 0x02);
        // bit 0: smap
        // bit 1: hdd
        // bit 5: flash
        public const uint SPD_R_REV_3 = (SPD_REGBASE + 0x04);
        public const uint SPD_R_0e = (SPD_REGBASE + 0x0e);

        /* SMAP Register Definitions.  */
        public const uint SMAP_REGBASE = (SPD_REGBASE + 0x100);

        public const uint SMAP_R_BD_MODE = (SMAP_REGBASE + 0x02);
        public const uint SMAP_BD_SWAP = (1 << 0);

        public const uint SMAP_R_INTR_CLR = (SMAP_REGBASE + 0x28);

        /* SMAP FIFO Registers.  */
        public const uint SMAP_R_TXFIFO_CTRL = (SMAP_REGBASE + 0xf00);
        public const uint SMAP_TXFIFO_RESET = (1 << 0);
        public const uint SMAP_TXFIFO_DMAEN = (1 << 1);
        public const uint SMAP_R_TXFIFO_WR_PTR = (SMAP_REGBASE + 0xf04);
        public const uint SMAP_R_TXFIFO_SIZE = (SMAP_REGBASE + 0xf08);
        public const uint SMAP_R_TXFIFO_FRAME_CNT = (SMAP_REGBASE + 0xf0C);
        public const uint SMAP_R_TXFIFO_FRAME_INC = (SMAP_REGBASE + 0xf10);
        public const uint SMAP_R_TXFIFO_DATA = (SMAP_REGBASE + 0x1000);

        public const uint SMAP_R_RXFIFO_CTRL = (SMAP_REGBASE + 0xf30);
        public const uint SMAP_RXFIFO_RESET = (1 << 0);
        public const uint SMAP_RXFIFO_DMAEN = (1 << 1);
        public const uint SMAP_R_RXFIFO_RD_PTR = (SMAP_REGBASE + 0xf34);
        public const uint SMAP_R_RXFIFO_SIZE = (SMAP_REGBASE + 0xf38);
        public const uint SMAP_R_RXFIFO_FRAME_CNT = (SMAP_REGBASE + 0xf3C);
        public const uint SMAP_R_RXFIFO_FRAME_DEC = (SMAP_REGBASE + 0xf40);
        public const uint SMAP_R_RXFIFO_DATA = (SMAP_REGBASE + 0x1100);

        /* EMAC3 Registers.  */
        public const uint SMAP_EMAC3_REGBASE = (SMAP_REGBASE + 0x1f00);

        public const uint SMAP_R_EMAC3_MODE0_L = (SMAP_EMAC3_REGBASE + 0x00);
        public const uint SMAP_E3_RXMAC_IDLE = unchecked((uint)(1 << (15 + 16)));
        public const uint SMAP_E3_TXMAC_IDLE = (1 << (14 + 16));
        public const uint SMAP_E3_SOFT_RESET = (1 << (13 + 16));
        public const uint SMAP_E3_TXMAC_ENABLE = (1 << (12 + 16));
        public const uint SMAP_E3_RXMAC_ENABLE = (1 << (11 + 16));
        public const uint SMAP_E3_WAKEUP_ENABLE = (1 << (10 + 16));
        public const uint SMAP_R_EMAC3_MODE0_H = (SMAP_EMAC3_REGBASE + 0x02);

        public const uint SMAP_R_EMAC3_TxMODE0_L = (SMAP_EMAC3_REGBASE + 0x08);
        public const uint SMAP_E3_TX_GNP_0 = unchecked((uint)(1 << (15 + 16)));/* get new packet */
        public const uint SMAP_E3_TX_GNP_1 = (1 << (14 + 16));	    /* get new packet */
        public const uint SMAP_E3_TX_GNP_DEPEND = (1 << (13 + 16));	/* get new packet */
        public const uint SMAP_E3_TX_FIRST_CHANNEL = (1 << (12 + 16));
        public const uint SMAP_R_EMAC3_TxMODE0_H = (SMAP_EMAC3_REGBASE + 0x0A);

        public const uint SMAP_R_EMAC3_MODE1 = (SMAP_EMAC3_REGBASE + 0x04);
        public const uint SMAP_R_EMAC3_MODE1_L = (SMAP_EMAC3_REGBASE + 0x04);
        public const uint SMAP_R_EMAC3_MODE1_H = (SMAP_EMAC3_REGBASE + 0x06);
        public const uint SMAP_E3_FDX_ENABLE = unchecked((uint)(1 << 31));
        public const uint SMAP_E3_INLPBK_ENABLE = (1 << 30);	/* internal loop back */
        public const uint SMAP_E3_VLAN_ENABLE = (1 << 29);
        public const uint SMAP_E3_FLOWCTRL_ENABLE = (1 << 28);  /* integrated flow ctrl(pause frame) */
        public const uint SMAP_E3_ALLOW_PF = (1 << 27);         /* allow pause frame */
        public const uint SMAP_E3_ALLOW_EXTMNGIF = (1 << 25);   /* allow external management IF */
        public const uint SMAP_E3_IGNORE_SQE = (1 << 24);
        public const uint SMAP_E3_MEDIA_FREQ_BITSFT = (22);
        public const uint SMAP_E3_MEDIA_10M = (0 << 22);
        public const uint SMAP_E3_MEDIA_100M = (1 << 22);
        public const uint SMAP_E3_MEDIA_1000M = (2 << 22);
        public const uint SMAP_E3_MEDIA_MSK = (3 << 22);
        public const uint SMAP_E3_RXFIFO_SIZE_BITSFT = (20);
        public const uint SMAP_E3_RXFIFO_512 = (0 << 20);
        public const uint SMAP_E3_RXFIFO_1K = (1 << 20);
        public const uint SMAP_E3_RXFIFO_2K = (2 << 20);
        public const uint SMAP_E3_RXFIFO_4K = (3 << 20);
        public const uint SMAP_E3_TXFIFO_SIZE_BITSFT = (18);
        public const uint SMAP_E3_TXFIFO_512 = (0 << 18);
        public const uint SMAP_E3_TXFIFO_1K = (1 << 18);
        public const uint SMAP_E3_TXFIFO_2K = (2 << 18);
        public const uint SMAP_E3_TXREQ0_BITSFT = (15);
        public const uint SMAP_E3_TXREQ0_SINGLE = (0 << 15);
        public const uint SMAP_E3_TXREQ0_MULTI = (1 << 15);
        public const uint SMAP_E3_TXREQ0_DEPEND = (2 << 15);
        public const uint SMAP_E3_TXREQ1_BITSFT = (13);
        public const uint SMAP_E3_TXREQ1_SINGLE = (0 << 13);
        public const uint SMAP_E3_TXREQ1_MULTI = (1 << 13);
        public const uint SMAP_E3_TXREQ1_DEPEND = (2 << 13);
        public const uint SMAP_E3_JUMBO_ENABLE = (1 << 12);

        public const uint SMAP_R_EMAC3_TxMODE1_L = (SMAP_EMAC3_REGBASE + 0x0C);
        public const uint SMAP_R_EMAC3_TxMODE1_H = (SMAP_EMAC3_REGBASE + 0x0E);
        public const uint SMAP_E3_TX_LOW_REQ_MSK = (0x1F);	/* low priority request */
        public const uint SMAP_E3_TX_LOW_REQ_BITSFT = (27);	/* low priority request */
        public const uint SMAP_E3_TX_URG_REQ_MSK = (0xFF);	/* urgent priority request */
        public const uint SMAP_E3_TX_URG_REQ_BITSFT = (16);	/* urgent priority request */

        public const uint SMAP_R_EMAC3_RxMODE = (SMAP_EMAC3_REGBASE + 0x10);
        public const uint SMAP_R_EMAC3_RxMODE_L = (SMAP_EMAC3_REGBASE + 0x10);
        public const uint SMAP_R_EMAC3_RxMODE_H = (SMAP_EMAC3_REGBASE + 0x12);
        public const uint SMAP_E3_RX_STRIP_PAD = unchecked((uint)(1 << 31));
        public const uint SMAP_E3_RX_STRIP_FCS = (1 << 30);
        public const uint SMAP_E3_RX_RX_RUNT_FRAME = (1 << 29);
        public const uint SMAP_E3_RX_RX_FCS_ERR = (1 << 28);
        public const uint SMAP_E3_RX_RX_TOO_LONG_ERR = (1 << 27);
        public const uint SMAP_E3_RX_RX_IN_RANGE_ERR = (1 << 26);
        public const uint SMAP_E3_RX_PROP_PF = (1 << 25);/* propagate pause frame */
        public const uint SMAP_E3_RX_PROMISC = (1 << 24);
        public const uint SMAP_E3_RX_PROMISC_MCAST = (1 << 23);
        public const uint SMAP_E3_RX_INDIVID_ADDR = (1 << 22);
        public const uint SMAP_E3_RX_INDIVID_HASH = (1 << 21);
        public const uint SMAP_E3_RX_BCAST = (1 << 20);
        public const uint SMAP_E3_RX_MCAST = (1 << 19);

        public const uint SMAP_R_EMAC3_INTR_STAT = (SMAP_EMAC3_REGBASE + 0x14);
        public const uint SMAP_R_EMAC3_INTR_STAT_L = (SMAP_EMAC3_REGBASE + 0x14);
        public const uint SMAP_R_EMAC3_INTR_STAT_H = (SMAP_EMAC3_REGBASE + 0x16);
        public const uint SMAP_R_EMAC3_INTR_ENABLE = (SMAP_EMAC3_REGBASE + 0x18);
        public const uint SMAP_R_EMAC3_INTR_ENABLE_L = (SMAP_EMAC3_REGBASE + 0x18);
        public const uint SMAP_R_EMAC3_INTR_ENABLE_H = (SMAP_EMAC3_REGBASE + 0x1A);
        public const uint SMAP_E3_INTR_OVERRUN = (1 << 25);/* this bit does NOT WORKED */
        public const uint SMAP_E3_INTR_PF = (1 << 24);
        public const uint SMAP_E3_INTR_BAD_FRAME = (1 << 23);
        public const uint SMAP_E3_INTR_RUNT_FRAME = (1 << 22);
        public const uint SMAP_E3_INTR_SHORT_EVENT = (1 << 21);
        public const uint SMAP_E3_INTR_ALIGN_ERR = (1 << 20);
        public const uint SMAP_E3_INTR_BAD_FCS = (1 << 19);
        public const uint SMAP_E3_INTR_TOO_LONG = (1 << 18);
        public const uint SMAP_E3_INTR_OUT_RANGE_ERR = (1 << 17);
        public const uint SMAP_E3_INTR_IN_RANGE_ERR = (1 << 16);
        public const uint SMAP_E3_INTR_DEAD_DEPEND = (1 << 9);
        public const uint SMAP_E3_INTR_DEAD_0 = (1 << 8);
        public const uint SMAP_E3_INTR_SQE_ERR_0 = (1 << 7);
        public const uint SMAP_E3_INTR_TX_ERR_0 = (1 << 6);
        public const uint SMAP_E3_INTR_DEAD_1 = (1 << 5);
        public const uint SMAP_E3_INTR_SQE_ERR_1 = (1 << 4);
        public const uint SMAP_E3_INTR_TX_ERR_1 = (1 << 3);
        public const uint SMAP_E3_INTR_MMAOP_SUCCESS = (1 << 1);
        public const uint SMAP_E3_INTR_MMAOP_FAIL = (1 << 0);
        public const uint SMAP_E3_INTR_ALL =
            (SMAP_E3_INTR_OVERRUN | SMAP_E3_INTR_PF | SMAP_E3_INTR_BAD_FRAME |
             SMAP_E3_INTR_RUNT_FRAME | SMAP_E3_INTR_SHORT_EVENT |
             SMAP_E3_INTR_ALIGN_ERR | SMAP_E3_INTR_BAD_FCS |
             SMAP_E3_INTR_TOO_LONG | SMAP_E3_INTR_OUT_RANGE_ERR |
             SMAP_E3_INTR_IN_RANGE_ERR |
             SMAP_E3_INTR_DEAD_DEPEND | SMAP_E3_INTR_DEAD_0 |
             SMAP_E3_INTR_SQE_ERR_0 | SMAP_E3_INTR_TX_ERR_0 |
             SMAP_E3_INTR_DEAD_1 | SMAP_E3_INTR_SQE_ERR_1 |
             SMAP_E3_INTR_TX_ERR_1 |
             SMAP_E3_INTR_MMAOP_SUCCESS | SMAP_E3_INTR_MMAOP_FAIL);
        public const uint SMAP_E3_DEAD_ALL =
            (SMAP_E3_INTR_DEAD_DEPEND | SMAP_E3_INTR_DEAD_0 |
             SMAP_E3_INTR_DEAD_1);

        public const uint SMAP_R_EMAC3_ADDR_HI = (SMAP_EMAC3_REGBASE + 0x1C);
        public const uint SMAP_R_EMAC3_ADDR_LO = (SMAP_EMAC3_REGBASE + 0x20);
        public const uint SMAP_R_EMAC3_ADDR_HI_L = (SMAP_EMAC3_REGBASE + 0x1C);
        public const uint SMAP_R_EMAC3_ADDR_HI_H = (SMAP_EMAC3_REGBASE + 0x1E);
        public const uint SMAP_R_EMAC3_ADDR_LO_L = (SMAP_EMAC3_REGBASE + 0x20);
        public const uint SMAP_R_EMAC3_ADDR_LO_H = (SMAP_EMAC3_REGBASE + 0x22);

        public const uint SMAP_R_EMAC3_VLAN_TPID = (SMAP_EMAC3_REGBASE + 0x24);
        public const uint SMAP_E3_VLAN_ID_MSK = 0xFFFF;

        public const uint SMAP_R_EMAC3_PAUSE_TIMER = (SMAP_EMAC3_REGBASE + 0x2C);
        public const uint SMAP_R_EMAC3_PAUSE_TIMER_L = (SMAP_EMAC3_REGBASE + 0x2C);
        public const uint SMAP_R_EMAC3_PAUSE_TIMER_H = (SMAP_EMAC3_REGBASE + 0x2E);
        public const uint SMAP_E3_PTIMER_MSK = 0xFFFF;

        public const uint SMAP_R_EMAC3_INDIVID_HASH1 = (SMAP_EMAC3_REGBASE + 0x30);
        public const uint SMAP_R_EMAC3_INDIVID_HASH2 = (SMAP_EMAC3_REGBASE + 0x34);
        public const uint SMAP_R_EMAC3_INDIVID_HASH3 = (SMAP_EMAC3_REGBASE + 0x38);
        public const uint SMAP_R_EMAC3_INDIVID_HASH4 = (SMAP_EMAC3_REGBASE + 0x3C);
        public const uint SMAP_R_EMAC3_GROUP_HASH1 = (SMAP_EMAC3_REGBASE + 0x40);
        public const uint SMAP_R_EMAC3_GROUP_HASH2 = (SMAP_EMAC3_REGBASE + 0x44);
        public const uint SMAP_R_EMAC3_GROUP_HASH3 = (SMAP_EMAC3_REGBASE + 0x48);
        public const uint SMAP_R_EMAC3_GROUP_HASH4 = (SMAP_EMAC3_REGBASE + 0x4C);
        public const uint SMAP_E3_HASH_MSK = 0xFFFF;

        public const uint SMAP_R_EMAC3_LAST_SA_HI = (SMAP_EMAC3_REGBASE + 0x50);
        public const uint SMAP_R_EMAC3_LAST_SA_LO = (SMAP_EMAC3_REGBASE + 0x54);

        public const uint SMAP_R_EMAC3_INTER_FRAME_GAP = (SMAP_EMAC3_REGBASE + 0x58);
        public const uint SMAP_R_EMAC3_INTER_FRAME_GAP_L = (SMAP_EMAC3_REGBASE + 0x58);
        public const uint SMAP_R_EMAC3_INTER_FRAME_GAP_H = (SMAP_EMAC3_REGBASE + 0x5A);
        public const uint SMAP_E3_IFGAP_MSK = 0x3F;

        public const uint SMAP_R_EMAC3_STA_CTRL_L = (SMAP_EMAC3_REGBASE + 0x5C);
        public const uint SMAP_R_EMAC3_STA_CTRL_H = (SMAP_EMAC3_REGBASE + 0x5E);
        public const uint SMAP_E3_PHY_DATA_MSK = (0xFFFF);
        public const uint SMAP_E3_PHY_DATA_BITSFT = (16);
        public const uint SMAP_E3_PHY_OP_COMP = (1 << 15);/* operation complete */
        public const uint SMAP_E3_PHY_ERR_READ = (1 << 14);
        public const uint SMAP_E3_PHY_STA_CMD_BITSFT = (12);
        public const uint SMAP_E3_PHY_READ = (1 << 12);
        public const uint SMAP_E3_PHY_WRITE = (2 << 12);
        public const uint SMAP_E3_PHY_OPBCLCK_BITSFT = (10);
        public const uint SMAP_E3_PHY_50M = (0 << 10);
        public const uint SMAP_E3_PHY_66M = (1 << 10);
        public const uint SMAP_E3_PHY_83M = (2 << 10);
        public const uint SMAP_E3_PHY_100M = (3 << 10);
        public const uint SMAP_E3_PHY_ADDR_MSK = (0x1F);
        public const uint SMAP_E3_PHY_ADDR_BITSFT = (5);
        public const uint SMAP_E3_PHY_REG_ADDR_MSK = (0x1F);

        public const uint SMAP_R_EMAC3_TX_THRESHOLD = (SMAP_EMAC3_REGBASE + 0x60);
        public const uint SMAP_R_EMAC3_TX_THRESHOLD_L = (SMAP_EMAC3_REGBASE + 0x60);
        public const uint SMAP_R_EMAC3_TX_THRESHOLD_H = (SMAP_EMAC3_REGBASE + 0x62);
        public const uint SMAP_E3_TX_THRESHLD_MSK = (0x1F);
        public const uint SMAP_E3_TX_THRESHLD_BITSFT = (27);

        public const uint SMAP_R_EMAC3_RX_WATERMARK = (SMAP_EMAC3_REGBASE + 0x64);
        public const uint SMAP_R_EMAC3_RX_WATERMARK_L = (SMAP_EMAC3_REGBASE + 0x64);
        public const uint SMAP_R_EMAC3_RX_WATERMARK_H = (SMAP_EMAC3_REGBASE + 0x66);
        public const uint SMAP_E3_RX_LO_WATER_MSK = (0x1FF);
        public const uint SMAP_E3_RX_LO_WATER_BITSFT = (23);
        public const uint SMAP_E3_RX_HI_WATER_MSK = (0x1FF);
        public const uint SMAP_E3_RX_HI_WATER_BITSFT = (7);

        public const uint SMAP_R_EMAC3_TX_OCTETS = (SMAP_EMAC3_REGBASE + 0x68);
        public const uint SMAP_R_EMAC3_RX_OCTETS = (SMAP_EMAC3_REGBASE + 0x6C);
        public const uint SMAP_EMAC3_REGEND = (SMAP_EMAC3_REGBASE + 0x6C + 4);

        /* Buffer descriptors.  */
        public class smap_bd
        {
            int _startoff = 0;
            byte[] basedata;
            public smap_bd(byte[] data, int startoffset)
            {
                basedata = data;
                _startoff = startoffset;
            }
            public UInt16 ctrl_stat
            {
                get
                {
                    return BitConverter.ToUInt16(basedata, _startoff);
                }
                set
                {
                    byte[] var = BitConverter.GetBytes(value);
                    Utils.memcpy(ref basedata, _startoff, var, 0, var.Length);
                }
            }
            public UInt16 reserved
            {
                get
                {
                    return BitConverter.ToUInt16(basedata, _startoff + 2);
                }
                set
                {
                    byte[] var = BitConverter.GetBytes(value);
                    Utils.memcpy(ref basedata, _startoff + 2, var, 0, var.Length);
                }
            }
            public UInt16 length
            {
                get
                {
                    return BitConverter.ToUInt16(basedata, _startoff + 4);
                }
                set
                {
                    byte[] var = BitConverter.GetBytes(value);
                    Utils.memcpy(ref basedata, _startoff + 4, var, 0, var.Length);
                }
            }
            public UInt16 pointer
            {
                get
                {
                    return BitConverter.ToUInt16(basedata, _startoff + 6);
                }
                set
                {
                    byte[] var = BitConverter.GetBytes(value);
                    Utils.memcpy(ref basedata, _startoff + 6, var, 0, var.Length);
                }
            }

            public static int GetSize()
            {
                return ((16 * 4) / 8);
            }
        }

        //public struct smap_bd_t {
        //    public UInt16 ctrl_stat;
        //    public UInt16 reserved;	/* must be zero */
        //    public UInt16 length;		/* number of bytes in pkt */
        //    public UInt16 pointer;
        //    public void FromBytes(byte[] data, int startoffset)
        //    {
        //        ctrl_stat = BitConverter.ToUInt16(data,startoffset);
        //        reserved = BitConverter.ToUInt16(data, startoffset+2);
        //        length = BitConverter.ToUInt16(data, startoffset+4);
        //        pointer = BitConverter.ToUInt16(data, startoffset+6);
        //    }
        //    public static int GetSize()
        //    {
        //        return ((16 * 4) / 8);
        //    }
        //}

        public const uint SMAP_BD_REGBASE = (SMAP_REGBASE + 0x2f00);
        public const uint SMAP_BD_TX_BASE = (SMAP_BD_REGBASE + 0x0000);
        public const uint SMAP_BD_RX_BASE = (SMAP_BD_REGBASE + 0x0200);
        public const uint SMAP_BD_SIZE = 512;
        public const uint SMAP_BD_MAX_ENTRY = 64;

        /* TX Status */
        public const uint SMAP_BD_TX_READY = (1 << 15); /* set:driver, clear:HW */
        public const int SMAP_BD_TX_BADFCS = (1 << 9);	/* bad FCS */
        public const int SMAP_BD_TX_BADPKT = (1 << 8);	/* bad previous pkt in dependent mode */
        public const int SMAP_BD_TX_LOSSCR = (1 << 7);	/* loss of carrior sense */
        public const int SMAP_BD_TX_EDEFER = (1 << 6);	/* excessive deferal */
        public const int SMAP_BD_TX_ECOLL = (1 << 5);	/* excessive collision */
        public const int SMAP_BD_TX_LCOLL = (1 << 4);	/* late collision */
        public const int SMAP_BD_TX_MCOLL = (1 << 3);	/* multiple collision */
        public const int SMAP_BD_TX_SCOLL = (1 << 2);	/* single collision */
        public const int SMAP_BD_TX_UNDERRUN = (1 << 1);	/* underrun */
        public const int SMAP_BD_TX_SQE = (1 << 0);	/* SQE */

        /* RX Status */
        public const uint SMAP_BD_RX_EMPTY = (1 << 15);	/* set:driver, clear:HW */
        public const int SMAP_BD_RX_OVERRUN = (1 << 9);	/* overrun */
        public const int SMAP_BD_RX_PFRM = (1 << 8);	/* pause frame */
        public const int SMAP_BD_RX_BADFRM = (1 << 7);	/* bad frame */
        public const int SMAP_BD_RX_RUNTFRM = (1 << 6);	/* runt frame */
        public const int SMAP_BD_RX_SHORTEVNT = (1 << 5);/* short event */
        public const int SMAP_BD_RX_ALIGNERR = (1 << 4);/* alignment error */
        public const int SMAP_BD_RX_BADFCS = (1 << 3);  /* bad FCS */
        public const int SMAP_BD_RX_FRMTOOLONG = (1 << 2);/* frame too long */
        public const int SMAP_BD_RX_OUTRANGE = (1 << 1);/* out of range error */
        public const int SMAP_BD_RX_INRANGE = (1 << 0);	/* in range error */

        public const int SMAP_DsPHYTER_BMCR = 0x00;
        public const int SMAP_PHY_BMCR_RST = (1 << 15);	/* ReSeT */
        public const int SMAP_PHY_BMCR_LPBK = (1 << 14);/* LooPBacK */
        public const int SMAP_PHY_BMCR_100M = (1 << 13);/* speed select, 1:100M, 0:10M */
        public const int SMAP_PHY_BMCR_10M = (0 << 13);	/* speed select, 1:100M, 0:10M */
        public const int SMAP_PHY_BMCR_ANEN = (1 << 12);/* Auto-Negotiation ENable */
        public const int SMAP_PHY_BMCR_PWDN = (1 << 11);/* PoWer DowN */
        public const int SMAP_PHY_BMCR_ISOL = (1 << 10);/* ISOLate */
        public const int SMAP_PHY_BMCR_RSAN = (1 << 9);	/* ReStart Auto-Negotiation */
        public const int SMAP_PHY_BMCR_DUPM = (1 << 8);	/* DUPlex Mode, 1:FDX, 0:HDX */
        public const int SMAP_PHY_BMCR_COLT = (1 << 7);	/* COLlision Test */

        public const int SMAP_DsPHYTER_BMSR = 0x01;
        public const int SMAP_PHY_BMSR_ANCP = (1 << 5);	/* Auto-Negotiation ComPlete */
        public const int SMAP_PHY_BMSR_LINK = (1 << 2); /* LINK status */

        /* Extended registers.  */
        public const int SMAP_DsPHYTER_PHYSTS = 0x10;
        public const int SMAP_PHY_STS_REL = (1 << 13);  /* Receive Error Latch */
        public const int SMAP_PHY_STS_POST = (1 << 12); /* POlarity STatus */
        public const int SMAP_PHY_STS_FCSL = (1 << 11); /* False Carrier Sense Latch */
        public const int SMAP_PHY_STS_SD = (1 << 10);   /* 100BT unconditional Signal Detect */
        public const int SMAP_PHY_STS_DSL = (1 << 9);   /* 100BT DeScrambler Lock */
        public const int SMAP_PHY_STS_PRCV = (1 << 8);  /* Page ReCeiVed */
        public const int SMAP_PHY_STS_RFLT = (1 << 6);  /* Remote FauLT */
        public const int SMAP_PHY_STS_JBDT = (1 << 5);  /* JaBber DetecT */
        public const int SMAP_PHY_STS_ANCP = (1 << 4);  /* Auto-Negotiation ComPlete */
        public const int SMAP_PHY_STS_LPBK = (1 << 3);  /* LooPBacK status */
        public const int SMAP_PHY_STS_DUPS = (1 << 2);  /* DUPlex Status,1:FDX,0:HDX */
        public const int SMAP_PHY_STS_FDX = (1 << 2);   /* Full Duplex */
        public const int SMAP_PHY_STS_HDX = (0 << 2);   /* Half Duplex */
        public const int SMAP_PHY_STS_SPDS = (1 << 1);  /* SPeeD Status */
        public const int SMAP_PHY_STS_10M = (1 << 1);   /* 10Mbps */
        public const int SMAP_PHY_STS_100M = (0 << 1);  /* 100Mbps */
        public const int SMAP_PHY_STS_LINK = (1 << 0);  /* LINK status */
        public const int SMAP_DsPHYTER_FCSCR = 0x14;
        public const int SMAP_DsPHYTER_RECR = 0x15;
        public const int SMAP_DsPHYTER_PCSR = 0x16;
        public const int SMAP_DsPHYTER_PHYCTRL = 0x19;
        public const int SMAP_DsPHYTER_10BTSCR = 0x1A;
        public const int SMAP_DsPHYTER_CDCTRL = 0x1B;

        /*
            * ATA hardware types and definitions.
            *
            * Copyright (c) 2003 Marcus R. Brown <mrbrown@0xd6.org>
            *
            * * code included from the ps2drv iop driver, modified by linuzappz  *
            */

        public const uint ATA_DEV9_HDD_BASE = (SPD_REGBASE + 0x40);
        //
        public const uint ATA_R_CONTROL = (ATA_DEV9_HDD_BASE + 0x1c);
        //
        public const uint ATA_DEV9_HDD_END = (ATA_R_CONTROL + 4);

        public const int FLASH_ID_64MBIT = 0xe6;
        //
        //
        //

        /* SmartMedia commands.  */
        public const int SM_CMD_READ1 = 0x00;
        public const int SM_CMD_READ2 = 0x01;
        public const int SM_CMD_READ3 = 0x50;
        public const int SM_CMD_RESET = 0xff;
        public const int SM_CMD_WRITEDATA = 0x80;
        public const int SM_CMD_PROGRAMPAGE = 0x10;
        public const int SM_CMD_ERASEBLOCK = 0x60;
        public const int SM_CMD_ERASECONFIRM = 0xd0;
        public const int SM_CMD_GETSTATUS = 0x70;
        public const int SM_CMD_READID = 0x90;

        public const int FLASH_REGBASE = 0x10004800;

        public const int FLASH_R_DATA = (FLASH_REGBASE + 0x00);
        public const int FLASH_R_CMD = (FLASH_REGBASE + 0x04);
        public const int FLASH_R_ADDR = (FLASH_REGBASE + 0x08);
        public const int FLASH_R_CTRL = (FLASH_REGBASE + 0x0C);
        public const int FLASH_PP_READY = (1 << 0);	// r/w	/BUSY
        public const int FLASH_PP_WRITE = (1 << 7);	// -/w	WRITE data
        public const int FLASH_PP_CSEL = (1 << 8);	// -/w	CS
        public const int FLASH_PP_READ = (1 << 11);	// -/w	READ data
        public const int FLASH_PP_NOECC = (1 << 12);	// -/w	ECC disabled
        public const int FLASH_R_ID = (FLASH_REGBASE + 0x14);

        public const int FLASH_REGSIZE = 0x20;
    }
}
