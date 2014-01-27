/*
 * Модуль описания Модели КИА XSAN
 * Модель отвечает за внутреннее получение и обработку данных,
 * выдачу команд в USB (все, что должно быть незаметно обычному пользователю)
 * 
 * 
 * 
 * 
 */
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
    
    public class XsanModel
    {
        /// <summary>
        /// Модуль обработки ВСИ
        /// </summary>
        public HSIInterface _hsiInterface;

        /// <summary>
        /// Модуль обработки телеметрии
        /// </summary>
        public XsanTm Tm;

        //*********************************
        // адреса приема сообщений
        private const int TIME_ADDR_GET = 0x04;
        private const int HSI_XSAN_CTRL_GET = 0x09;
        private const int HSI_XSAN_DATA_GET = 0x0A;
        private const int HSI_BUNI_CTRL_GET = 0x06;
        private const int HSI_BUNI_DATA_GET = 0x07;
        private const int TM_DATA_GET = 5;

        //*********************************
        // адреса установки данных
        private const int TIME_RESET_ADDR = 0x01;
        private const int TIME_DATA_ADDR = 0x02;
        private const int TIME_SET_ADDR = 0x03;
        private const int TIME_OBT_ADDR = 0x12;
        //
        private const int POWER_SET_ADDR = 0x05;
        private const int HSI_BUNI_CTRL_ADDR = 0x06;
        private const int HSI_XSAN_CTRL_ADDR = 0x09;
        private const int HSI_UKS_ADDR = 0x08;

        /// <summary>
        /// протокол кодирования данных
        /// </summary>
        private ProtocolUSB5E4D _protocol;

        /// <summary>
        /// Устройство USB
        /// </summary>
        private Device _xsanDevice;

        /// <summary>
        /// Сюда пишутся пришедшие даные по USB управления питанием
        /// </summary>
        private int _powerControlValue;
        /// <summary>
        /// Сюда пишутся пришедшие даные по USB управления БУНИ
        /// </summary>
        private int _buniControlValue;
        /// <summary>
        /// Сюда пишутся пришедшие даные по USB управления XSAN
        /// </summary>
        private int _xsanControlValue;

        /// <summary>
        /// буфер отправки сообщений
        /// </summary>
        private byte[] buf;

        /// <summary>
        /// Скорость приема данных по USB
        /// </summary>
        public float Speed
        {
            get
            {
                return _xsanDevice.Speed;
            }
        }

        /// <summary>
        /// Максимальный размер "большого" буфера за последнюю секунду
        /// </summary>
        public uint GlobalBufferSize
        {
            get
            {
                return _xsanDevice.GlobalBufferSize;
            }
        }

        /// <summary>
        /// Завершаем работу по USB (закрываем все потоки)
        /// </summary>
        public void FinishAll()
        {
            _xsanDevice.FinishAll();
        }

        /// <summary>
        /// Управление питанием
        /// </summary>
        public int PowerControlValue 
        {
            get { return _powerControlValue; }
            private set { _powerControlValue = value; }
        }

        /// <summary>
        /// Управление БУНИ
        /// </summary>
        public int BuniControlValue 
        {
            get { return _buniControlValue; }
            private set { _buniControlValue = value; }
        }

        /// <summary>
        /// Управление XSAN
        /// </summary>
        public int XsanControlValue 
        {
            get { return _xsanControlValue; }
            private set { _xsanControlValue = value; }
        }

        /// <summary>
        /// куда записываем данные с XSAN
        /// </summary>
        private FileStream _xsanDataLogStream;
        
        /// <summary>
        /// с какого канала записываются данные (основного или резервного) 
        /// </summary>
        private uint _xsanChannelForWriting;

        /// <summary>
        /// Время, пришедшее от КИА
        /// </summary>       
        public EgseTime ETime;

        //TODO: переделать на событие
        public delegate void ChangeStateEventHandler(bool connected);
        public ChangeStateEventHandler DeviceStateChanged;

        /// <summary>
        /// Создаем модель
        /// - протокол
        /// - модуль телеметрии
        /// - модуль ВСИ
        /// - модуль работы по USB
        /// </summary>
        public XsanModel()
        {
            _protocol = new ProtocolUSB5E4D(null, LogsClass.Instance.Files[LogsClass.UsbIdx], false, true);
            _protocol.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(onMessageFunc);
            _protocol.GotProtocolError += new ProtocolUSBBase.ProtocolErrorEventHandler(onErrorFunc);

            _hsiInterface = new HSIInterface();
            Tm = new XsanTm();
            ETime = new EgseTime();

            _xsanDevice = new Device(XsanConst.XSANSerial, _protocol, new USBCfg(10));
            _xsanDevice.ChangeStateEvent = onChangeConnection;
        }

        /// <summary>
        /// Запускаем обмен по USB
        /// </summary>
        public void Start() {
            _xsanDevice.Start();
        }

        /// <summary>
        /// Метод вызывается, когда прибор подсоединяется или отсоединяется
        /// </summary>
        /// <param name="state">Текущее состояние прибора TRUE - подключен, FALSE - отключен</param>
        void onChangeConnection(bool connected)
        {
            if (connected)
            {
                CmdSendTime();
            }
            if (DeviceStateChanged != null) {
                DeviceStateChanged(connected);
            }
        }

        /// <summary>
        /// Размер файла данных с прибора XSAN
        /// </summary>
        public long XsanFileSize
        {
            get
            {
                if (_xsanDataLogStream != null)
                {
                    return _xsanDataLogStream.Length;
                }
                else return 0;
            }
        }

        /// <summary>
        /// Имя файла с данными прибора XSAN
        /// </summary>
        public string XsanFileName
        {
            get
            {
                if (_xsanDataLogStream != null)
                {
                    return _xsanDataLogStream.Name;
                }
                else return string.Empty;
            }
        }

        /// <summary>
        /// Начинаем записывать данных с прибора XSAN по ВСИ
        /// </summary>
        /// <param name="StartWrite">признак начала и окончания записи</param>
        /// <param name="_buniImitatorDatChannel">С какого канала пишем данные</param>
        public void WriteXsanData(bool StartWrite, int _buniImitatorDatChannel)
        {
            if (StartWrite)
            {
                string dataLogDir = Directory.GetCurrentDirectory().ToString() + "\\DATA\\";
                Directory.CreateDirectory(dataLogDir);
                string fileName = dataLogDir + "xsan_" + DateTime.Now.ToString("yyMMdd_HHmmss") + ".dat";
                _xsanDataLogStream = new FileStream(fileName, System.IO.FileMode.Create);

                // выбираем, по какому каналу записываем данные (по комбобоксу выбора приема данных)
                switch (_buniImitatorDatChannel)
                {
                    case 1: _xsanChannelForWriting = 0;
                        break;
                    case 2: _xsanChannelForWriting = 1;
                        break;
                    case 3:
                        _xsanChannelForWriting = 0;
                        break;
                    default:
                        _xsanChannelForWriting = 0;
                        break;
                }
            }
            else
            {
                if (_xsanDataLogStream != null)
                {
                    _xsanDataLogStream.Close();
                    _xsanDataLogStream = null;
                }
            }
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
                        Array.Copy(msg1.Data, 0, ETime.Data, 0, 6);
                        break;
                    case TM_DATA_GET:
                        Tm.Update(msg1.Data);
                        _powerControlValue = msg1.Data[6];
                        break;
                    case HSI_XSAN_DATA_GET:
                        _hsiInterface.XSANStat.Update(msg1.Data, msg1.DataLen);
                        break;
                    case HSI_BUNI_DATA_GET:
                        _hsiInterface.BUNIStat.Update(msg1.Data, _xsanDataLogStream, _xsanChannelForWriting);
                        break;
                    case HSI_BUNI_CTRL_GET:
                        _buniControlValue = msg1.Data[0];
                        break;
                    case HSI_XSAN_CTRL_GET:
                        _xsanControlValue = msg1.Data[0];
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

            LogsClass.Instance.Files[LogsClass.ErrorsIdx].LogText = msg.Msg + " (" + bufferStr + ", на позиции: " + msg.ErrorPos.ToString() + ")";
        }

        //*****************************************************************************************************************
        // Команды отправки сообщений 
        //*****************************************************************************************************************
        /// <summary>
        /// Команда управления ВСИ БУНИ
        /// </summary>
        /// <param name="HSIImitControl">байт управления</param>
        public void CmdHSIBUNIControl(UInt32 HSIImitControl)
        {
            buf = new byte[1] { (byte)HSIImitControl };
            _xsanDevice.SendCmd(HSI_BUNI_CTRL_ADDR, buf);
        }

        /// <summary>
        /// Команда управления ВСИ XSAN
        /// </summary>
        /// <param name="HSIImitControl">байт управления</param>
        public void CmdHSIXSANControl(UInt32 HSIControl)
        {
            int frameSize = 496;
            buf = new byte[3] { (byte)HSIControl, (byte)(frameSize >> 8), (byte)frameSize };
            _xsanDevice.SendCmd(HSI_XSAN_CTRL_ADDR, buf);
        }

        /// <summary>
        /// Команда выдачи УКС
        /// </summary>
        /// <param name="UKSBuf">данные УКС для выдачи</param>
        public void CmdSendUKS(byte[] UKSBuf)
        {
            _xsanDevice.SendCmd(HSI_UKS_ADDR, UKSBuf);
        }

        /// <summary>
        /// Команда включения/выключения питания
        /// </summary>
        /// <param name="turnOn">Включаем или выключаем питание</param>
        public void CmdPowerOnOff(UInt32 turnOn)
        {
            turnOn &= 1;
            _xsanDevice.SendCmd(POWER_SET_ADDR, new byte[1] { (byte)turnOn });
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

            _xsanDevice.SendCmd(TIME_RESET_ADDR, buf);
            _xsanDevice.SendCmd(TIME_DATA_ADDR, time.Data);
            _xsanDevice.SendCmd(TIME_OBT_ADDR, OBTData);
            _xsanDevice.SendCmd(TIME_SET_ADDR, buf);
        }
    }
}