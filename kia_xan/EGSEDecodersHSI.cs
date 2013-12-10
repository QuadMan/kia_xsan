/*
 * EDGEDecodersHSI.cs
 * 
 * Copyright(c) 2013 ИКИ РАН
 * 
 * Author: Семенов Александр
 * Project: EDGE
 * Module: EDGEDecodersHSI
 * Comments:
 *          Класс HSIInterface предназначен для получения статистики по кадрам, принятым по интерфейсу ВСИ.
 *          Первоначально использовался в проекте КИА КВВ для получения данных от БУК и КВВ
 *          HSIMessageStruct - общая структура данных на входе декодера
 *          XXXChannelStruct - структуры описания статистики по каналам (основным или резервным) КВВ и БУК
 *          XXXStatistics - классы, через которые обновляется статистика (используя функцию Update), статистика доступна 
 *          через свойство Channels этого класса
 *          XXXChannel - классы для использования в ObservationCollection в классе отображения         
 * 
 */

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
    /// <summary>
    /// Класс декодера данных по ВСИ интерфейсу для КИА КВВ
    /// </summary>
    public class HSIInterface
    {
        /// <summary>
        /// Статисика принятых кадров по каналам БУК (основному и резервному)
        /// </summary>
        public BUKStatistics BUKStat;
        /// <summary>
        /// Статистика принятых кадров по каналам КВВ (основному и резервному)
        /// </summary>
        public KVVStatistics KVVStat;

        // максимальный размер структуры на вход декодера
        const int HSI_FRAME_SIZE_BYTES = 496;
        // описание структуры, приходящей на вход декодера        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct HSIMessageStruct
        {
            public byte Status;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] Time;
            public byte Flag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HSI_FRAME_SIZE_BYTES)]
            public byte[] data;
        }

        /// <summary>
        /// Описание статистики по каналу КВВ
        /// </summary>
        public struct KVVChannelStruct
        {
            /// <summary>
            /// Имя канала - основной или резервный
            /// </summary>
            public string Name;

            // Всего кадров от КВВ принято
            public uint FramesCnt;
            // Кадров с признаком в статусе ME
            public uint StatusMECnt;
            // Кадров с признаком в статусе SR
            public uint StatusSRCnt;
            // Кадров с признаком в статусе BUSY
            public uint StatusBUSYCnt;
            // Кадров с признаком в статусе "кадр данных"
            public uint StatusDataFramesCnt;
            // Кадров с признаком ошибки маркера
            public uint ErrInMarkerCnt;
            // Кадров с признаком ошибки CRC
            public uint ErrInCRCCnt;
            // Кадров с признаком ошибки стопового бита
            public uint ErrInStopBitCnt;
            // Кадров с признаком ошибки четности
            public uint ErrInParityCnt;

            /// <summary>
            /// Сброс статистики
            /// </summary>
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

        /// <summary>
        /// Описание статистики по каналу БУК
        /// </summary>
        public struct BUKChannelStruct
        {
            /// <summary>
            /// Имя канала - основной или резервный
            /// </summary>
            public string Name;

            // Всего запросов кадров получено
            public uint SRCnt;
            // Всего запросов данных получено
            public uint DRCnt;
            // Всего меток времени получено
            public uint TimeStampCnt;
            // Всего КБВ получено
            public uint OBTCnt;
            // Всего УКС получено
            public uint UKSCnt;

            /// <summary>
            /// Сброс статистики
            /// </summary>
            public void Reset()
            {
                SRCnt = 0;
                DRCnt = 0;
                TimeStampCnt = 0;
                OBTCnt = 0;
                UKSCnt = 0;
            }
        }

        /// <summary>
        /// Класс статистики по каналам КВВ
        /// </summary>
        public class KVVStatistics 
        {
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

            /// <summary>
            /// Два канала - основной и резервный
            /// </summary>
            public KVVChannelStruct[] Channels; 

            // Проекция буфера из декодера на структуру данных
            private HSIMessageStruct _hsiFrame;

            // текущий канал
            private int _curChannelId;

            /// <summary>
            /// Функция обновления статистики по данным декодера
            /// </summary>
            /// <param name="buf">Буфер данных декодера КВВ</param>
            public void Update(byte[] buf)
            {
                _hsiFrame = ByteArrayToStructure.make<HSIMessageStruct>(buf);

                _curChannelId = 0;
                // узнаем, по какому каналу пришли данные
                if ((_hsiFrame.Status & HSI_STATUS_CHANNEL_BIT_MASK) == 0) _curChannelId = 1;
                // подсчитаем статистику по текущему каналу
                Channels[_curChannelId].FramesCnt++;
                if ((_hsiFrame.Status & HSI_STATUS_MARKER_ERROR_BIT_MASK) > 0) Channels[_curChannelId].ErrInMarkerCnt++;
                if ((_hsiFrame.Status & HSI_STATUS_CRC_ERROR_BIT_MASK) > 0) Channels[_curChannelId].ErrInCRCCnt++;
                if ((_hsiFrame.Status & HSI_STATUS_STOPBIT_ERROR_BIT_MASK) > 0) Channels[_curChannelId].ErrInStopBitCnt++;
                if ((_hsiFrame.Status & HSI_STATUS_PARITY_ERROR_BIT_MASK) > 0) Channels[_curChannelId].ErrInParityCnt++;

                if ((_hsiFrame.Flag & HSI_FLAG_BUSY_BIT_MASK) == HSI_FLAG_BUSY_BIT_MASK) Channels[_curChannelId].StatusBUSYCnt++;
                if ((_hsiFrame.Flag & HSI_FLAG_DATA_BIT_MASK) == HSI_FLAG_DATA_BIT_MASK) Channels[_curChannelId].StatusDataFramesCnt++;
                if ((_hsiFrame.Flag & HSI_FLAG_ME_BIT_MASK) == HSI_FLAG_ME_BIT_MASK) Channels[_curChannelId].StatusMECnt++;
                if ((_hsiFrame.Flag & HSI_FLAG_SR_BIT_MASK) == HSI_FLAG_SR_BIT_MASK) Channels[_curChannelId].StatusSRCnt++;
            }

            /// <summary>
            /// Конструктор статистики канала КВВ
            /// </summary>
            public KVVStatistics()
            {
                Channels = new KVVChannelStruct[2];
                Channels[0].Name = "Основной";
                Channels[1].Name = "Резервный";
            }

            /// <summary>
            /// Функция очистки статистики
            /// </summary>
            public void Clear()
            {
                Channels[0].Reset();
                Channels[1].Reset();
            }
        }

        /// <summary>
        /// Класс статистики по каналам БУК
        /// </summary>
        public class BUKStatistics 
        {
            private const int HSI_SR_FLAG = 3;
            private const int HSI_DR_FLAG = 4;
            private const int HSI_TIMESTAMP_FLAG = 5;
            private const int HSI_OBT_FLAG = 1;
            private const int HSI_UKS_FLAG = 2;

            /// <summary>
            /// Два канала - основной и резервный
            /// </summary>
            public BUKChannelStruct[] Channels;

            // Проекция буфера из декодера на структуру данных
            private HSIMessageStruct HSIFrame;
            
            // текущий канал
            private int _curChannelId;

           
            /// <summary>
            /// Определение делегата обработки УКС
            /// </summary>
            /// <param name="buf">Буфер УКС</param>
            /// <param name="timeBuf">Время в формате EgseTime</param>
            public delegate void onUKSFrameReceivedDelegate(byte[] buf, byte[] timeBuf);

            /// <summary>
            /// Делегат, вызываемый при получении УКС
            /// </summary>
            public onUKSFrameReceivedDelegate onUKSFrameReceived;

            /// <summary>
            /// Функция обновления статистики по данным декодера
            /// </summary>
            /// <param name="buf">Буфер данных</param>
            /// <param name="bufLen">Длина буфера</param>
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

            /// <summary>
            /// Конструктор статистики каналов БУК
            /// </summary>
            public BUKStatistics()
            {
                Channels = new BUKChannelStruct[2];
                Channels[0].Name = "Основной";
                Channels[1].Name = "Резервный";
            }

            /// <summary>
            /// Функция очистки статистики
            /// </summary>
            public void Clear()
            {
                Channels[0].Reset();
                Channels[1].Reset();
            }
        }

        /// <summary>
        /// Конструктор 
        /// </summary>
        public HSIInterface()
        {
            BUKStat = new BUKStatistics();
            KVVStat = new KVVStatistics();
        }

        /// <summary>
        /// Класс для доступа к статистике канала КВВ через ObservationCollection
        /// </summary>
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

        /// <summary>
        /// Класс для доступа к статистике канала БУК через ObservationCollection
        /// </summary>
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
                Name = GetData.Name;
                SRCnt = GetData.SRCnt;
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