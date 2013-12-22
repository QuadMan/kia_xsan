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
        private const int TIME_OBT_ADDR = 0x12;
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

        public void CmdHSIBUNIControl(UInt32 HSIImitControl)
        {
            buf = new byte[1] { (byte)HSIImitControl };
            base.SendCmd(HSI_BUNI_CTRL_ADDR, buf);
        }

        public void CmdHSIXSANControl(UInt32 HSIControl)
        {
            int frameSize = 496;
            buf = new byte[3] { (byte)HSIControl, (byte)(frameSize >> 8), (byte)frameSize };
            base.SendCmd(HSI_XSAN_CTRL_ADDR, buf);
        }

        public void CmdSendUKS(byte[] UKSBuf)
        {
            base.SendCmd(HSI_UKS_ADDR, UKSBuf);
        }

        public void CmdPowerOnOff(UInt32 turnOn)
        {
            turnOn &= 1;
            base.SendCmd(POWER_SET_ADDR, new byte[1] { (byte)turnOn });
        }

        /// <summary>
        /// Стандарная команда установки времени
        /// </summary>
        public void CmdSendTime()
        {
            EgseTime time = new EgseTime();
            time.Encode();

            byte[] OBTData = new byte[5];
            OBTData[0] = 0;
            OBTData[1] = 0;
            OBTData[2] = 0;
            OBTData[3] = 0;
            OBTData[4] = 10;

            buf = new byte[1] { 1 };

            base.SendCmd(TIME_RESET_ADDR, buf);
            base.SendCmd(TIME_DATA_ADDR, time.data);
            base.SendCmd(TIME_OBT_ADDR, OBTData);
            base.SendCmd(TIME_SET_ADDR, buf);
        }
    }
    /*
    public class EGSEBase
    {
        private string _serial;
        private TProt _protocol;
        private TDev _device;
        private bool _connected;
        private EgseTime _eTime;

        public TDev Device
        {
            get
            {
                return _device;
            }
        }

        public bool Connected
        {
            get { return _connected; }
        }

        public EgseTime ETime
        {
            get { return _eTime; }
        }

        public EGSEBase(TProt _prot, TDev _dev) 
        {
            //_protocol = new TProt();
        }
    }
    */
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

        // *************************************************
        private ProtocolUSB5E4D _decoder;
        //**************************************************

        private FileStream _xsanDataLogStream;
        private uint _xsanChannelForWriting;

        public XSANDevice Device;

        public bool Connected { get; private set; }
        public EgseTime ETime;

        public HSIInterface HSIInt;
        public XsanTm Tm;

        /// <summary>
        /// ссылка насписок управляющих элементов, передается из MainWindow
        /// </summary>
        private List<ControlValue> ControlValuesList = new List<ControlValue>();

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public XSAN(List<ControlValue> cvl)
        {
            Connected = false;
            ControlValuesList = cvl;

            _decoder = new ProtocolUSB5E4D(null, LogsClass.Instance.Files[LogsClass.UsbIdx], false, true);
            _decoder.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(onMessageFunc);
            _decoder.GotProtocolError += new ProtocolUSBBase.ProtocolErrorEventHandler(onErrorFunc);

            ETime = new EgseTime();

            Device = new XSANDevice(XsanConst.XSANSerial, _decoder);
            Device.onNewState = onChangeConnection;
            
            //
            _xsanDataLogStream = null;
            _xsanChannelForWriting = 0;
            
            HSIInt = new HSIInterface();
            Tm = new XsanTm();
        }

        /// <summary>
        /// Вызываетя при подключении прибора, чтобы все элементы управления обновили свои значения
        /// </summary>
        private void refreshAllControlsValues()
        {
            if (ControlValuesList == null) return;
            
            foreach (ControlValue cv in ControlValuesList)
            {
                cv.RefreshGetValue();
            }
        }

        /// <summary>
        /// Метод вызывается, когда прибор подсоединяется или отсоединяется
        /// </summary>
        /// <param name="state">Текущее состояние прибора TRUE - подключен, FALSE - отключен</param>
        void onChangeConnection(bool connected)
        {
            Connected = connected;
            if (Connected)
            {
                Device.CmdSendTime();
                refreshAllControlsValues();
                //
                LogsClass.Instance.Files[LogsClass.MainIdx].LogText = "КИА XSAN подключен";
            }
            else
            {
                LogsClass.Instance.Files[LogsClass.MainIdx].LogText = "КИА XSAN отключен";
            }
        }

        /// <summary>
        /// Указываем какой файл использовать для записи данных от прибора XSAN и по какому каналу
        /// </summary>
        /// <param name="fStream">Поток для записи данных</param>
        /// <param name="channel">По какому каналу</param>
        public void SetFileAndChannelForLogXSANData(FileStream fStream, uint channel)
        {
            _xsanDataLogStream = fStream;
            _xsanChannelForWriting = channel;
        }

        /// <summary>
        /// Метод обрабатывающий сообщения от декодера USB
        /// </summary>
        /// <param name="msg">Сообщение</param>
        void onMessageFunc(MsgBase msg)
        {
            ProtocolMsgEventArgs msg1 = msg as ProtocolMsgEventArgs;
            if (msg1 != null)
            {
                switch (msg1.Addr)
                {
                    case TIME_ADDR_GET:
                        Array.Copy(msg1.Data, 0, ETime.data, 0, 6);
                        break;
                    case TM_DATA_GET :
                        Tm.Update(msg1.Data);
                        ControlValuesList[XsanConst.POWER_CTRL_IDX].GetValue = msg1.Data[6];
                        break;
                    case HSI_XSAN_DATA_GET:
                        HSIInt.XSANStat.Update(msg1.Data, msg1.DataLen);
                        break;
                    case HSI_BUNI_DATA_GET:
                        HSIInt.BUNIStat.Update(msg1.Data, _xsanDataLogStream, _xsanChannelForWriting);
                        break;
                    case HSI_BUNI_CTRL_GET:
                        ControlValuesList[XsanConst.BUNI_CTRL_IDX].GetValue = msg1.Data[0];
                        break;
                    case HSI_XSAN_CTRL_GET:
                        ControlValuesList[XsanConst.XSAN_CTRL_IDX].GetValue = msg1.Data[0];
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

            LogsClass.Instance.Files[LogsClass.ErrorsIdx].LogText = msg.Msg+" ("+bufferStr+", на позиции: "+msg.ErrorPos.ToString()+")";
        }
    }
}
