using EGSE;
using EGSE.Protocols;
using EGSE.USB;
using EGSE.Utilites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kia_xan
{
     /// <summary>
     /// Класс логгеров (синглетон)
     /// </summary>
    public sealed class LogsClass
    {
        private static string[] LogsFiles = new string[5]{ "main.log", "operator.log", "hsi.log", "errors.log", "usb.log" };
        public enum Idx { logMain, logOperator, logHSI, logErrors, logUSB };
        
        private static volatile LogsClass instance;
        private static object syncRoot = new Object();

        public TxtLoggers Files;

        private LogsClass()
        { 
            Files = new TxtLoggers();
            foreach (string FName in LogsFiles) {
                Files.AddFile(FName);
            }
        }

        public static LogsClass Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new LogsClass();
                    }
                }

                return instance;
            }
        }
    }

    public class XSANDevice : Device
    {
        private const int TIME_RESET_ADDR = 0x01;
        private const int TIME_DATA_ADDR = 0x02;
        private const int TIME_SET_ADDR = 0x03;

        private const int HSI_BUK_CTRL_ADDR = 0x0A;
        private const int HSI_KVV_CTRL_ADDR = 0x0D;

        private byte[] buf;

        public XSANDevice(string Serial, ProtocolUSBBase dec)
            : base(Serial, dec, new USBCfg(10))
        {
        }

        public void CmdHSIBUKControl(byte HSIImitControl)
        {
            buf = new byte[1] { HSIImitControl };
            base.SendCmd(HSI_BUK_CTRL_ADDR, buf);
        }

        public void CmdHSIKVVControl(byte HSIControl,int frameSize)
        {
            buf = new byte[3] { HSIControl, (byte)(frameSize >> 8), (byte)frameSize };
            base.SendCmd(HSI_KVV_CTRL_ADDR, buf);
        }

        public void CmdSendTime()
        {
            EgseTime time = new EgseTime();
            time.Encode();

            buf = new byte[1]{1};

            base.SendCmd(TIME_RESET_ADDR, buf);
            base.SendCmd(TIME_DATA_ADDR, time.data);
            base.SendCmd(TIME_SET_ADDR, buf);
        }

    }

    public class XSAN //: IDisposable
    {
        private const int TIME_ADDR_GET = 0x04;

        private const int HSI_BUK_DATA_GET = 0x0B;
        private const int HSI_KVV_DATA_GET = 0x0E;
        private const int HSI_KVV_CTRL_GET = 0x0D;
        private const int HSI_BUK_CTRL_GET = 0x0A;

        const string XSANSerial = "KIA_KVV";
        public XSANDevice Device;
        ProtocolUSB5E4DNoCrc _decoder;
        public bool Connected;
        private StreamWriter fTxtWriter;
        private bool disposed = false;
        public string time;
        public EgseTime eTime;
        public HSIInterface HSIInt;

        public ControlValue KVVControl;
        public ControlValue BUKControl;

        public XSAN()
        {
            fTxtWriter = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\usb_log.txt");
            fTxtWriter.AutoFlush = true;

            _decoder = new ProtocolUSB5E4DNoCrc(null, fTxtWriter, false, true);// new ProtocolUSB5E4D();
            _decoder.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(onMessageFunc);
            _decoder.GotProtocolError += new ProtocolUSBBase.ProtocolErrorEventHandler(onErrorFunc);

            Device = new XSANDevice(XSANSerial, _decoder);
            Device.onNewState = onChangeConnection;

            Connected = false;
            eTime = new EgseTime();
            HSIInt = new HSIInterface();

            KVVControl = new ControlValue();
            BUKControl = new ControlValue();
        }

        public void Dispose()
        {
        }

        void onChangeConnection(bool state)
        {
            Connected = state;
            if (Connected)
            {
                Device.CmdSendTime();
            }

        }

        void onMessageFunc(MsgBase msg)
        {
            ProtocolMsgEventArgs msg1 = msg as ProtocolMsgEventArgs;
            if (msg1 != null)
            {
                switch (msg1.Addr)
                {
                    case TIME_ADDR_GET:
                        Array.Copy(msg1.Data, 0, eTime.data, 0, 6);
                        break;
                    case HSI_KVV_DATA_GET:
                        HSIInt.KVVStat.Update(msg1.Data);
                        break;
                    case HSI_BUK_DATA_GET:
                        HSIInt.BUKStat.Update(msg1.Data);
                        break;
                    case HSI_BUK_CTRL_GET:
                        BUKControl.GetValue = msg1.Data[0];
                        break;
                    case HSI_KVV_CTRL_GET:
                        KVVControl.GetValue = msg1.Data[0];
                        break;
                }
            }
        }

        void onErrorFunc(MsgBase errMsg)
        {

        }
    }
}
