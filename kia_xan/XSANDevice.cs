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
using System.Windows;

namespace kia_xan
{
    /// <summary>
    /// Прописываются команды управления прибором по USB
    /// </summary>
    public class XSANDevice : Device
    {
        private const int TIME_RESET_ADDR = 0x01;
        private const int TIME_DATA_ADDR = 0x02;
        private const int TIME_SET_ADDR = 0x03;
        //
        private const int POWER_SET_ADDR = 0x05;
        private const int HSI_BUNI_CTRL_ADDR = 0x06;
        private const int HSI_XSAN_CTRL_ADDR = 0x09;
        private const int HSI_UKS_ADDR = 0x08;

        private byte[] buf;

        public XSANDevice(string Serial, ProtocolUSBBase dec)
            : base(Serial, dec, new USBCfg(10))
        {
        }

        public void CmdHSIBUNIControl(byte HSIImitControl)
        {
            buf = new byte[1] { HSIImitControl };
            base.SendCmd(HSI_BUNI_CTRL_ADDR, buf);
        }

        public void CmdHSIXSANControl(byte HSIControl,int frameSize)
        {
            buf = new byte[3] { HSIControl, (byte)(frameSize >> 8), (byte)frameSize };
            base.SendCmd(HSI_XSAN_CTRL_ADDR, buf);
        }

        public void CmdSendUKS(byte[] UKSBuf)
        {
            base.SendCmd(HSI_UKS_ADDR, UKSBuf);
        }

        public void CmdPowerOnOff(byte turnOn)
        {
            turnOn &= 1;
            base.SendCmd(POWER_SET_ADDR, new byte[1] { turnOn });
        }

        /// <summary>
        /// Стандарная команда установки времени
        /// </summary>
        public void CmdSendTime()
        {
            EgseTime time = new EgseTime();
            time.Encode();

            buf = new byte[1] { 1 };

            base.SendCmd(TIME_RESET_ADDR, buf);
            base.SendCmd(TIME_DATA_ADDR, time.data);
            base.SendCmd(TIME_SET_ADDR, buf);
        }
    }

    /// <summary>
    /// Общий объект, позволяющий управлять прибором (принимать данные, выдавать команды)
    /// </summary>
    public class XSAN
    {
        private const int TIME_ADDR_GET = 0x04;
        private const int HSI_XSAN_CTRL_GET = 0x09;
        private const int HSI_XSAN_DATA_GET = 0x0A;
        private const int HSI_BUNI_CTRL_GET = 0x06;
        private const int HSI_BUNI_DATA_GET = 0x07;
        private const int TM_DATA_GET = 5;

        const string XSANSerial = "KIA_LINA";
        private ProtocolUSB5E4D _decoder;
        private FileStream _xsanDataLogStream;
        private uint _xsanChannelForWriting;

        public XSANDevice Device;
        public bool Connected;
        public string time;
        public EgseTime eTime;
        public HSIInterface HSIInt;
        public XsanTm Tm;
        public ControlValue XSANControl;
        public ControlValue BUNIControl;
        public ControlValue PowerControl;

        public XSAN()
        {
            _decoder = new ProtocolUSB5E4D(null, LogsClass.Instance.Files[(int)LogsClass.Idx.logUSB], false, true);
            _decoder.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(onMessageFunc);
            _decoder.GotProtocolError += new ProtocolUSBBase.ProtocolErrorEventHandler(onErrorFunc);

            _xsanDataLogStream = null;
            _xsanChannelForWriting = 0;

            Device = new XSANDevice(XSANSerial, _decoder);
            Device.onNewState = onChangeConnection;
            //

            Connected = false;
            eTime = new EgseTime();
            HSIInt = new HSIInterface();
            Tm = new XsanTm();

            XSANControl = new ControlValue();
            BUNIControl = new ControlValue();
            PowerControl = new ControlValue();
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Указываем какой файл использовать для записи данных и какой канал
        /// </summary>
        /// <param name="fStream"></param>
        /// <param name="channel"></param>
        public void SetFileAndChannelForLogXSANData(FileStream fStream, uint channel)
        {
            _xsanDataLogStream = fStream;
            _xsanChannelForWriting = channel;
        }

        void onChangeConnection(bool state)
        {
            Connected = state;
            if (Connected)
            {
                Device.CmdSendTime();
                XSANControl.RefreshGetValue();
                BUNIControl.RefreshGetValue();
                PowerControl.RefreshGetValue();
                //
                LogsClass.Instance.Files[(int)LogsClass.Idx.logMain].LogText = "КИА XSAN подключен";
            }
            else
            {
                LogsClass.Instance.Files[(int)LogsClass.Idx.logMain].LogText = "КИА XSAN отключен";
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
                    case TM_DATA_GET :
                        Tm.Update(msg1.Data);
                        PowerControl.GetValue = msg1.Data[6];
                        break;
                    case HSI_XSAN_DATA_GET:
                        HSIInt.XSANStat.Update(msg1.Data, msg1.DataLen);
                        break;
                    case HSI_BUNI_DATA_GET:
                        HSIInt.BUNIStat.Update(msg1.Data, _xsanDataLogStream, _xsanChannelForWriting);
                        break;
                    case HSI_BUNI_CTRL_GET:
                        BUNIControl.GetValue = msg1.Data[0];
                        break;
                    case HSI_XSAN_CTRL_GET:
                        XSANControl.GetValue = msg1.Data[0];
                        break;
                }
            }
        }

        /// <summary>
        /// Обработчик ошибок протокола декодера USB
        /// </summary>
        /// <param name="errMsg"></param>
        void onErrorFunc(MsgBase errMsg)
        {
            ProtocolErrorEventArgs msg = errMsg as ProtocolErrorEventArgs;
            string bufferStr = Converter.ByteArrayToHexStr(msg.Data);

            LogsClass.Instance.Files[(int)LogsClass.Idx.logErrors].LogText = msg.Msg+" ("+bufferStr+", на позиции: "+msg.ErrorPos.ToString()+")";
        }
    }
}
