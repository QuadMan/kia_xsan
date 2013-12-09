using EGSE.Utilites;
using kia_xan;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public struct HSIMessageStruct
        {
            public byte Status;
            //public fixed byte Time[6];
             [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] Time;
            public byte Flag;
             [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HSI_FRAME_SIZE_BYTES)]
            public byte[] data;
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

        public class KVVStatistics //: List<KVVChannelStruct>
        {
            public KVVChannelStruct[] Channels; 
            // описание битов байта параметров кадра данных ВСИ
            const int HSI_STATUS_CHANNEL_BIT_MASK = (1 << 6);
            const int HSI_STATUS_MARKER_ERROR_BIT_MASK = (1 << 3);
            const int HSI_STATUS_CRC_ERROR_BIT_MASK = (1 << 2);
            const int HSI_STATUS_STOPBIT_ERROR_BIT_MASK = (1 << 1);
            const int HSI_STATUS_PARITY_ERROR_BIT_MASK = 1;

            const int HSI_FLAG_BUSY_BIT_MASK = 4;
            const int HSI_FLAG_DATA_BIT_MASK = 16;
            const int HSI_FLAG_ME_BIT_MASK = 1;
            const int HSI_FLAG_SR_BIT_MASK = 2;

            public HSIMessageStruct HSIFrame;
            private int _curChannelId;

            public void Update(byte[] buf)
            {
                HSIFrame = ByteArrayToStructure.make<HSIMessageStruct>(buf);

                _curChannelId = 0;
                // узнаем, по какому каналу пришли данные
                if ((HSIFrame.Status & HSI_STATUS_CHANNEL_BIT_MASK) == 0) _curChannelId = 1;
                // подсчитаем статистику по текущему каналу
                Channels[_curChannelId].FramesCnt++;
                if ((HSIFrame.Status & HSI_STATUS_MARKER_ERROR_BIT_MASK) > 0) Channels[_curChannelId].ErrInMarkerCnt++;
                if ((HSIFrame.Status & HSI_STATUS_CRC_ERROR_BIT_MASK) > 0) Channels[_curChannelId].ErrInCRCCnt++;
                if ((HSIFrame.Status & HSI_STATUS_STOPBIT_ERROR_BIT_MASK) > 0) Channels[_curChannelId].ErrInStopBitCnt++;
                if ((HSIFrame.Status & HSI_STATUS_PARITY_ERROR_BIT_MASK) > 0) Channels[_curChannelId].ErrInParityCnt++;

                if ((HSIFrame.Flag & HSI_FLAG_BUSY_BIT_MASK) == HSI_FLAG_BUSY_BIT_MASK) Channels[_curChannelId].StatusBUSYCnt++;
                if ((HSIFrame.Flag & HSI_FLAG_DATA_BIT_MASK) == HSI_FLAG_DATA_BIT_MASK) Channels[_curChannelId].StatusDataFramesCnt++;
                if ((HSIFrame.Flag & HSI_FLAG_ME_BIT_MASK) == HSI_FLAG_ME_BIT_MASK) Channels[_curChannelId].StatusMECnt++;
                if ((HSIFrame.Flag & HSI_FLAG_SR_BIT_MASK) == HSI_FLAG_SR_BIT_MASK) Channels[_curChannelId].StatusSRCnt++;
            }

            public KVVStatistics()
                //: base()
            {
                Channels = new KVVChannelStruct[2];
                Channels[0].Name = "Основной";
                Channels[1].Name = "Резервный";
            }
        }

        public class BUKStatistics //: ObservableCollection<BUKChannel>
        {
            public BUKChannelStruct[] Channels; 

            private const int HSI_SR_FLAG = 3;
            private const int HSI_DR_FLAG = 4;
            private const int HSI_TIMESTAMP_FLAG = 5;
            private const int HSI_OBT_FLAG = 1;
            private const int HSI_UKS_FLAG = 2;

            public HSIMessageStruct HSIFrame;
            private int _curChannelId;
            
            public BUKStatistics()
                : base()
            {
                //Add(new BUKChannel("Основной_БУК"));
                //Add(new BUKChannel("Резервный_БУК"));
                Channels = new BUKChannelStruct[2];
                Channels[0].Name = "Основной";
                Channels[1].Name = "Резервный";
            }

            /// <summary>
            ///  Определение делегата обработки ошибок протокола
            /// </summary>
            /// <param name="errMsg">класс сообщения, порожденный от MsgBase, описывающиё ошибку</param>
            public delegate void onUKSFrameReceivedDelegate(byte[] buf, byte[] timeBuf);

            /// <summary>
            /// Делегат, вызываемый при возникновении ошибки в декодере
            /// </summary>
            public onUKSFrameReceivedDelegate onUKSFrameReceived;


            public void Update(byte[] buf, int bufLen)
            {
                HSIFrame = ByteArrayToStructure.make<HSIMessageStruct>(buf);

                // узнаем, по какому каналу пришли данные
                _curChannelId = HSIFrame.Status & 1;

                byte flag = HSIFrame.Flag;
                int sz = 0;
                if (bufLen > 8)
                { 
                    sz = (buf[7] << 8) | buf[8];
                    flag = HSIFrame.data[sz-1];
                }
                
                // подсчитаем статистику по текущему каналу
                switch (flag)
                {
                    case HSI_SR_FLAG:
                        Channels[_curChannelId].SRCnt++;
                        break;
                    case HSI_DR_FLAG:
                        Channels[_curChannelId].DRCnt++;
                        break;
                    case HSI_TIMESTAMP_FLAG:
                        Channels[_curChannelId].TimeStampCnt++;
                        break;
                    case HSI_OBT_FLAG:
                        Channels[_curChannelId].OBTCnt++;
                        break;
                    case HSI_UKS_FLAG:
                        Channels[_curChannelId].UKSCnt++;
                        if (onUKSFrameReceived != null)           // если есть обработчик УКС, вызовем его, чтобы вывести их на экран, к примеру
                        {
                            byte[] tmpUKSBuf = new byte[sz];
                            Array.Copy(buf, 9, tmpUKSBuf, 0, sz);
                            onUKSFrameReceived(tmpUKSBuf,HSIFrame.Time);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

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

        public class KVVChannel : INotifyPropertyChanged
        {
            public KVVChannelStruct GetData;

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
            }
        }
    }
}