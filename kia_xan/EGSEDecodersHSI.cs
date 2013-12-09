using EGSE.Utilites;
using kia_xan;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace kia_xan
{
    public class HSIInterface
    {
        const int HSI_FRAME_SIZE_BYTES = 496;
        // описание битов байта управления ВСИ
        public enum HSI_CTRLS { hcOn, hcCmdChannel, hcDatChannel, hcHz = 4, hcOBT, hcErrReg };
        private int HSI_DAT_CHANNEL_MASK = 0xC; //1100    
        ////
        //const uint HSI_MAIN_CHANNEL_IDX = 1;
        //const uint HSI_RED_CHANNEL_IDX = 0;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe public struct HSIMessageStruct
        {
            public byte Status;
            public fixed byte Time[6];
            public byte Flag;
            public fixed byte Size[2];
            public fixed byte data[HSI_FRAME_SIZE_BYTES];
        }


        public struct KVVChannelStruct
        {
            public string Name;

            public uint FramesCnt;
            public uint StatusMECnt;
            public uint StatusSRCnt;
            public uint StatusBUSYCnt;
            public uint StatusDataFramesCnt;
            public uint ErrInMarkerCnt;
            public uint ErrInCRCCnt;
            public uint ErrInStopBitCnt;
            public uint ErrInParityCnt;

            public void Reset()
            {
                FramesCnt = 0;
                StatusMECnt = 0;
                StatusSRCnt = 0;
                StatusBUSYCnt = 0;
                StatusDataFramesCnt = 0;
                ErrInMarkerCnt = 0;
                ErrInCRCCnt = 0;
                ErrInStopBitCnt = 0;
                ErrInParityCnt = 0;
            }
        }

        public struct BUKChannelStruct
        {
            public string Name;

            public uint SRCnt;
            public uint DRCnt;
            public uint TimeStampCnt;
            public uint OBTCnt;
            public uint UKSCnt;

            public void Reset()
            {
                SRCnt = 0;
                DRCnt = 0;
                TimeStampCnt = 0;
                OBTCnt = 0;
                UKSCnt = 0;
            }
        }

        public class KVVChannel : INotifyPropertyChanged
        {
            public KVVChannelStruct GetData;
            //public KVVChannelStruct TimeEventData;

            private string _Name;
            private uint _FramesCnt;
            private uint _StatusMECnt;
            private uint _StatusSRCnt;
            private uint _StatusBUSYCnt;
            private uint _StatusDataFramesCnt;
            private uint _ErrInMarkerCnt;
            private uint _ErrInCRCCnt;
            private uint _ErrInStopBitCnt;
            private uint _ErrInParityCnt;

            public string Name { get { return _Name; } set { _Name = value; FirePropertyChangedEvent("Name"); } }
            public uint FramesCnt { get { return _FramesCnt; } set { _FramesCnt = value; FirePropertyChangedEvent("FramesCnt"); } }
            public uint StatusMECnt { get { return _StatusMECnt; } set { _StatusMECnt = value; FirePropertyChangedEvent("StatusMECnt"); } }
            public uint StatusSRCnt { get { return _StatusSRCnt; } set { _StatusSRCnt = value; FirePropertyChangedEvent("StatusSRCnt"); } }
            public uint StatusBUSYCnt { get { return _StatusBUSYCnt; } set { _StatusBUSYCnt = value; FirePropertyChangedEvent("StatusBUSYCnt"); } }
            public uint StatusDataFramesCnt { get { return _StatusDataFramesCnt; } set { _StatusDataFramesCnt = value; FirePropertyChangedEvent("StatusDataFramesCnt"); } }
            public uint ErrInMarkerCnt { get { return _ErrInMarkerCnt; } set { _ErrInMarkerCnt = value; FirePropertyChangedEvent("ErrInMarkerCnt"); } }
            public uint ErrInCRCCnt { get { return _ErrInCRCCnt; } set { _ErrInCRCCnt = value; FirePropertyChangedEvent("ErrInCRCCnt"); } }
            public uint ErrInStopBitCnt { get { return _ErrInStopBitCnt; } set { _ErrInStopBitCnt = value; FirePropertyChangedEvent("ErrInStopBitCnt"); } }
            public uint ErrInParityCnt { get { return _ErrInParityCnt; } set { _ErrInParityCnt = value; FirePropertyChangedEvent("ErrInParityCnt"); } }

            public KVVChannel(string chName)
            {
                GetData.Name = chName;
            }

            public void UpdateTimeEventData()
            {
                //            TimeEventData = GetData;
                Name = GetData.Name;
                FramesCnt = GetData.FramesCnt;
                StatusMECnt = GetData.StatusMECnt;
                StatusSRCnt = GetData.StatusSRCnt;
                StatusBUSYCnt = GetData.StatusBUSYCnt;
                StatusDataFramesCnt = GetData.StatusDataFramesCnt;
                ErrInMarkerCnt = GetData.ErrInMarkerCnt;
                ErrInCRCCnt = GetData.ErrInCRCCnt;
                ErrInStopBitCnt = GetData.ErrInStopBitCnt;
                ErrInParityCnt = GetData.ErrInParityCnt;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void FirePropertyChangedEvent(string propertyName)
            {
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            public void Reset()
            {
                GetData.Reset();
                //TimeEventData.Reset();
                FramesCnt = 0;
                StatusMECnt = 0;
                StatusSRCnt = 0;
                StatusBUSYCnt = 0;
                StatusDataFramesCnt = 0;
                ErrInMarkerCnt = 0;
                ErrInCRCCnt = 0;
                ErrInStopBitCnt = 0;
                ErrInParityCnt = 0;
            }
        }

        public class BUKChannel : INotifyPropertyChanged
        {
            public BUKChannelStruct GetData;
            //public BUKChannelStruct TimeEventData
            //{
            //    get { return _timeEventData; }
            //    set
            //    {
            //        _timeEventData = value;
            //        FirePropertyChangedEvent("TimeEventData1");
            //    }
            //}
            private string _name;
            private uint _srCnt;
            private uint _drCnt;
            private uint _timeStampCnt;
            private uint _obtCnt;
            private uint _uksCnt;
            public string Name { get { return _name; } set { _name = value; FirePropertyChangedEvent("Name"); } }
            public uint SRCnt { get { return _srCnt; } set { _srCnt = value; FirePropertyChangedEvent("SRCnt"); } }
            public uint DRCnt { get { return _drCnt; } set { _drCnt = value; FirePropertyChangedEvent("DRCnt"); } }
            public uint TimeStampCnt { get { return _timeStampCnt; } set { _timeStampCnt = value; FirePropertyChangedEvent("TimeStampCnt"); } }
            public uint ObtCnt { get { return _obtCnt; } set { _obtCnt = value; FirePropertyChangedEvent("ObtCnt"); } }
            public uint UksCnt { get { return _uksCnt; } set { _uksCnt = value; FirePropertyChangedEvent("UksCnt"); } }

            public BUKChannel(string chName)
            {
                GetData.Name = chName;
            }

            public void UpdateTimeEventData()
            {
                //Name = GetData.Name;
                Name = GetData.Name;
                SRCnt = GetData.SRCnt + 1;
                DRCnt = GetData.DRCnt;
                TimeStampCnt = GetData.TimeStampCnt;
                ObtCnt = GetData.OBTCnt;
                UksCnt = GetData.UKSCnt;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void FirePropertyChangedEvent(string propertyName)
            {
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            public void Reset()
            {
                GetData.Reset();

                SRCnt = 0;
                DRCnt = 0;
                TimeStampCnt = 0;
                ObtCnt = 0;
                UksCnt = 0;
                //TimeEventData.Reset();
            }
        }

        public class KVVStatistics : ObservableCollection<KVVChannel>
        {
            // описание битов байта параметров кадра данных ВСИ
            const int HSI_STATUS_CHANNEL_BIT_POS = 6;
            const int HSI_STATUS_MARKER_ERROR_BIT_POS = 3;
            const int HSI_STATUS_CRC_ERROR_BIT_POS = 2;
            const int HSI_STATUS_STOPBIT_ERROR_BIT_POS = 1;
            const int HSI_STATUS_PARITY_ERROR_BIT_POS = 0;

            const int HSI_FLAG_BUSY_BIT_POS = 2;
            const int HSI_FLAG_DATA_BIT_POS = 4;
            const int HSI_FLAG_ME_BIT_POS = 0;
            const int HSI_FLAG_SR_BIT_POS = 1;

            public HSIMessageStruct HSIFrame;
            private int _curChannelId;
            private BitVector32 _statusVector;
            private BitVector32 _flagVector;

            public void Update(byte[] buf)
            {
                HSIFrame = ByteArrayToStructure.make<HSIMessageStruct>(buf);
                _statusVector = new BitVector32(HSIFrame.Status);
                _flagVector = new BitVector32(HSIFrame.Flag);

                // узнаем, по какому каналу пришли данные
                if (_statusVector[HSI_STATUS_CHANNEL_BIT_POS])
                {
                    _curChannelId = 0;
                }
                else
                {
                    _curChannelId = 1;
                }

                // подсчитаем статистику по текущему каналу
                Items[_curChannelId].GetData.FramesCnt++;
                Items[_curChannelId].GetData.ErrInMarkerCnt += (_statusVector[HSI_STATUS_MARKER_ERROR_BIT_POS]) ? (uint)1 : 0;
                Items[_curChannelId].GetData.ErrInCRCCnt += (_statusVector[HSI_STATUS_CRC_ERROR_BIT_POS]) ? (uint)1 : 0;
                Items[_curChannelId].GetData.ErrInStopBitCnt += (_statusVector[HSI_STATUS_STOPBIT_ERROR_BIT_POS]) ? (uint)1 : 0;
                Items[_curChannelId].GetData.ErrInParityCnt += (_statusVector[HSI_STATUS_PARITY_ERROR_BIT_POS]) ? (uint)1 : 0;

                Items[_curChannelId].GetData.StatusBUSYCnt += (_flagVector[HSI_FLAG_BUSY_BIT_POS]) ? (uint)1 : 0;
                Items[_curChannelId].GetData.StatusDataFramesCnt += (_flagVector[HSI_FLAG_DATA_BIT_POS]) ? (uint)1 : 0;
                Items[_curChannelId].GetData.StatusMECnt += (_flagVector[HSI_FLAG_ME_BIT_POS]) ? (uint)1 : 0;
                Items[_curChannelId].GetData.StatusSRCnt += (_flagVector[HSI_FLAG_SR_BIT_POS]) ? (uint)1 : 0;
            }

            public KVVStatistics()
                : base()
            {
                Add(new KVVChannel("Основной_КВВ"));
                Add(new KVVChannel("Резервный_КВВ"));
            }
        }

        public class BUKStatistics : ObservableCollection<BUKChannel>
        {
            private const int HSI_SR_FLAG = 3;
            private const int HSI_DR_FLAG = 4;
            private const int HSI_TIMESTAMP_FLAG = 5;
            private const int HSI_OBT_FLAG = 1;
            private const int HSI_UKS_FLAG = 2;

            public HSIMessageStruct HSIFrame;
            private int _curChannelId;
            private BitVector32 _flagVector;

            public BUKStatistics()
                : base()
            {
                Add(new BUKChannel("Основной_БУК"));
                Add(new BUKChannel("Резервный_БУК"));
            }

            /// <summary>
            ///  Определение делегата обработки ошибок протокола
            /// </summary>
            /// <param name="errMsg">класс сообщения, порожденный от MsgBase, описывающиё ошибку</param>
            public delegate void onUKSFrameReceivedDelegate(byte[] buf);

            /// <summary>
            /// Делегат, вызываемый при возникновении ошибки в декодере
            /// </summary>
            public onUKSFrameReceivedDelegate onUKSFrameReceived;


            public void Update(byte[] buf)
            {
                HSIFrame = ByteArrayToStructure.make<HSIMessageStruct>(buf);
                _flagVector = new BitVector32(HSIFrame.Flag);

                // узнаем, по какому каналу пришли данные
                _curChannelId = HSIFrame.Status & 1;

                // подсчитаем статистику по текущему каналу
                switch (HSIFrame.Flag)
                {
                    case HSI_SR_FLAG:
                        Items[_curChannelId].GetData.SRCnt++;
                        break;
                    case HSI_DR_FLAG:
                        Items[_curChannelId].GetData.DRCnt++;
                        break;
                    case HSI_TIMESTAMP_FLAG:
                        Items[_curChannelId].GetData.TimeStampCnt++;
                        break;
                    case HSI_OBT_FLAG:
                        Items[_curChannelId].GetData.OBTCnt++;
                        break;
                    case HSI_UKS_FLAG:
                        Items[_curChannelId].GetData.UKSCnt++;
                        if (onUKSFrameReceived != null)           // если есть обработчик УКС, вызовем его, чтобы вывести их на экран, к примеру
                        {
                            onUKSFrameReceived(buf);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /*
        public struct BUKControlStruct
        {
            public byte Control;

            private BitVector32 _controlV;
            private BitVector32.Section _onSect;
            private BitVector32.Section _cmdChSect;
            private BitVector32.Section _datChSect;
            private BitVector32.Section _HZSect;
            private BitVector32.Section _OBTSect;
            private BitVector32.Section _ErrRegSect;

            public void Init()
            {
                _controlV = new BitVector32(Control);

                _onSect = BitVector32.CreateSection(1);
                _cmdChSect = BitVector32.CreateSection(1, _onSect);
                _datChSect = BitVector32.CreateSection(2, _cmdChSect);
                _HZSect = BitVector32.CreateSection(1, _datChSect);
                _OBTSect = BitVector32.CreateSection(1, _HZSect);
                _ErrRegSect = BitVector32.CreateSection(1, _OBTSect);
            }

            public int OnInterface { get { return _controlV[_onSect]; } set { _controlV[_onSect] = value; } }
            public int CommandChannel { get { return _controlV[_cmdChSect]; } set { _controlV[_cmdChSect] = value; } }
            public int DataChannel { get { return _controlV[_datChSect]; } set { _controlV[_datChSect] = value; } }
            public int HZOn { get { return _controlV[_HZSect]; } set { _controlV[_HZSect] = value; } }
            public int OBTOn { get { return _controlV[_OBTSect]; } set { _controlV[_OBTSect] = value; } }
            public int ErrRegOn { get { return _controlV[_ErrRegSect]; } set { _controlV[_ErrRegSect] = value; } }
        }
         */

        //private uint _curChannelId;

        /// <summary>
        /// Байт управления ВСИ
        /// </summary>
        //public BUKControlStruct BUKControl;

        public BUKStatistics BUKStat;
        public KVVStatistics KVVStat;
        public HSIInterface()
        {
            BUKStat = new BUKStatistics();
            KVVStat = new KVVStatistics();

            //BUKControl.Init();

            //BUKControl.OBTOn = 1;
            //BUKControl.ErrRegOn = 1;
        }

        public void MakeControl(HSI_CTRLS CtrlBitPos, int Value)
        {

        }
    }
}