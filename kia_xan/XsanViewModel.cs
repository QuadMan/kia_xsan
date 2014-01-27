/*
 * Реализация ViewModel - представления, которое связывает модель и представление
 * Оно ответственно за подготовку данных для отображения
 * Все отображение и управление моделью осуществляется через этот модуль 
 * 
 * 
 */
using EGSE.Utilites;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kia_xan
{
     /// <summary>
     /// Класс модели
     /// Обязательно наследуюется от InotifyPropertyChanged, так как передается
     /// в качестве DataContext в окна WPF для управления интерфейсом
     /// </summary>
    public class XsanViewModel : INotifyPropertyChanged
    {
        private XsanModel _xsanModel;

        //********************************************
        // привытные переменные управления моделью
        private bool _connected;
        private int _buniImitatorCmdChannel;
        private int _buniImitatorDatChannel;
        private bool _buniImitatorOn;
        private bool _buniImitatorTimeStampOn;
        private bool _buniImitatorObtOn;

        private int _xsanImitatorCmdChannel;
        private int _xsanImitatorDatChannel;
        private bool _xsanImitatorReady;
        private bool _xsanImitatorBusyOn;
        private bool _xsanImitatorMeOn;

        private bool _writeXsanDataToFile;

        private string _u27Value;
        private string _iValue;
        private string _xsanTime;
        private string _xsanSpeed;
        //********************************************
        /// <summary>
        /// cписок управляющих элементов
        /// </summary>
        public List<ControlValue> ControlValuesList = new List<ControlValue>();

        /// <summary>
        /// Признак подсоединения по USB
        /// </summary>
        public bool Connected
        {
            get { return _connected; }
            set
            {
                if (_connected != value)
                {
                    _connected = value;
                    FirePropertyChangedEvent("Connected");
                }
            }
        }

        /// <summary>
        /// Управление каналом команд БУНИ
        /// </summary>
        public int BuniImitatorCmdChannel
        {
            get { return _buniImitatorCmdChannel; }
            set
            {
                _buniImitatorCmdChannel = value;
                ControlValuesList[XsanConst.BUNI_CTRL_IDX].SetProperty(XsanConst.PROPERTY_BUNI_CMD_CH_IDX, value);
                FirePropertyChangedEvent("BuniImitatorCmdChannel");
            }
        }

        /// <summary>
        /// Управление каналом данных БУНИ
        /// </summary>
        public int BuniImitatorDatChannel
        {
            get { return _buniImitatorDatChannel; }
            set
            {
                _buniImitatorDatChannel = value;
                ControlValuesList[XsanConst.BUNI_CTRL_IDX].SetProperty(XsanConst.PROPERTY_BUNI_DAT_CH_IDX, value);
                FirePropertyChangedEvent("BuniImitatorDatChannel");
            }
        }

        /// <summary>
        /// Включение или выключение имитатора БУНИ
        /// </summary>
        public bool BuniImitatorOn
        {
            get { return _buniImitatorOn; }
            set
            {
                _buniImitatorOn = value;
                ControlValuesList[XsanConst.BUNI_CTRL_IDX].SetProperty(XsanConst.PROPERTY_BUNI_ON_IDX, Convert.ToInt32(value));
                FirePropertyChangedEvent("BuniImitatorOn");
            }
        }

        /// <summary>
        /// Управлением выдачей меток времени
        /// </summary>
        public bool BuniImitatorTimeStampOn
        {
            get { return _buniImitatorTimeStampOn; }
            set
            {
                _buniImitatorTimeStampOn = value;
                ControlValuesList[XsanConst.BUNI_CTRL_IDX].SetProperty(XsanConst.PROPERTY_BUNI_HZ_IDX, Convert.ToInt32(value));
                FirePropertyChangedEvent("BuniImitatorTimeStampOn");
            }
        }

        /// <summary>
        /// Управлением выдачей КБВ
        /// </summary>
        public bool BuniImitatorObtOn
        {
            get { return _buniImitatorObtOn; }

            set
            {
                _buniImitatorObtOn = value;
                ControlValuesList[XsanConst.BUNI_CTRL_IDX].SetProperty(XsanConst.PROPERTY_BUNI_KBV_IDX, Convert.ToInt32(value));
                FirePropertyChangedEvent("BuniImitatorObtOn");
            }
        }

        /// <summary>
        /// Управление каналом команд XSAN
        /// </summary>
        public int XsanImitatorCmdChannel
        {
            get { return _xsanImitatorCmdChannel; }
            set
            {
                _xsanImitatorCmdChannel = value;
                ControlValuesList[XsanConst.XSAN_CTRL_IDX].SetProperty(XsanConst.PROPERTY_XSAN_CMD_CH_IDX, value);
                FirePropertyChangedEvent("XsanImitatorCmdChannel");
            }
        }

        /// <summary>
        /// Управление каналом данных XSAN
        /// </summary>
        public int XsanImitatorDatChannel
        {
            get { return _xsanImitatorDatChannel; }
            set
            {
                _xsanImitatorDatChannel = value;
                ControlValuesList[XsanConst.XSAN_CTRL_IDX].SetProperty(XsanConst.PROPERTY_XSAN_DAT_CH_IDX, value);
                FirePropertyChangedEvent("XsanImitatorDatChannel");
            }
        }

        /// <summary>
        /// Управление готовностью имитатора XSAN
        /// </summary>
        public bool XsanImitatorReady
        {
            get { return _xsanImitatorReady; }
            set
            {
                _xsanImitatorReady = value;
                ControlValuesList[XsanConst.XSAN_CTRL_IDX].SetProperty(XsanConst.PROPERTY_XSAN_READY_IDX, Convert.ToInt32(value));
                FirePropertyChangedEvent("XsanImitatorReady");
            }
        }

        /// <summary>
        /// Управление занятостью имитатора XSAN
        /// </summary>
        public bool XsanImitatorBusyOn
        {
            get { return _xsanImitatorBusyOn; }
            set
            {
                _xsanImitatorBusyOn = value;
                ControlValuesList[XsanConst.XSAN_CTRL_IDX].SetProperty(XsanConst.PROPERTY_XSAN_BUSY_IDX, Convert.ToInt32(value));
                FirePropertyChangedEvent("XsanImitatorBusyOn");
            }
        }

        /// <summary>
        /// Управление признаком ошибок имитатора XSAN
        /// </summary>
        public bool XsanImitatorMeOn
        {
            get { return _xsanImitatorMeOn; }
            set
            {
                _xsanImitatorMeOn = value;
                ControlValuesList[XsanConst.XSAN_CTRL_IDX].SetProperty(XsanConst.PROPERTY_XSAN_ME_IDX, Convert.ToInt32(value));
                FirePropertyChangedEvent("XsanImitatorMeOn");
            }
        }

        /// <summary>
        /// Начать запись в файл данных от XSAN
        /// </summary>
        public bool WriteXsanDataToFile
        {
            get { return _writeXsanDataToFile; }
            set
            {
                _writeXsanDataToFile = value;
                _xsanModel.WriteXsanData(value, _buniImitatorDatChannel);
                FirePropertyChangedEvent("WriteXsanDataToFile");
            }
        }

        /// <summary>
        /// Значение напряжения 27В
        /// </summary>
        public string U27Value
        {
            get
            {
                return _u27Value;
            }
            private set
            {
                _u27Value = value;
                FirePropertyChangedEvent("U27Value");
            }
        }

        /// <summary>
        /// Значение тока потребления
        /// </summary>
        public string IValue
        {
            get
            {
                return _iValue;
            }
            private set
            {
                _iValue = value;
                FirePropertyChangedEvent("IValue");
            }
        }

        /// <summary>
        /// Время из КИА
        /// </summary>
        public string XsanTime
        {
            get
            {
                return _xsanTime;
            }
            private set
            {
                _xsanTime = value;
                FirePropertyChangedEvent("XsanTime");
            }
        }

        /// <summary>
        /// Скорость работы КИА
        /// </summary>
        public string XsanSpeed
        {
            get
            {
                return _xsanSpeed;
            }
            private set
            {
                _xsanSpeed = value;
                FirePropertyChangedEvent("XsanSpeed");
            }
        }

        /// <summary>
        /// Механизм уведомления интерфейс окон об изменениях в свойствах
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Отправляем УКС
        /// </summary>
        /// <param name="uksData">Данные УКС</param>
        public void SendUks(byte[] uksData)
        {
            _xsanModel.CmdSendUKS(uksData);
        }

        /// <summary>
        /// Метод получает время и скорость работы КИА
        /// </summary>
        public void GetTimeAndSpeed()
        {
            XsanTime = _xsanModel.ETime.ToString();
            XsanSpeed = GetDeviceSpeed() + " [" + GetBuferSize() + "]";
        }

        /// <summary>
        /// Метод обновления значений телеметрии
        /// Должен вызываться 1 раз в секунду
        /// Автоматически через привязки обновляется экран
        /// </summary>
        public void GetTmValues()
        {
            _xsanModel.Tm.GetAdcValue();
            U27Value = _xsanModel.Tm.U27Value;
            IValue = _xsanModel.Tm.IXsanValue;
        }

        /// <summary>
        /// Создаем Модель-Представление на основе модели
        /// </summary>
        /// <param name="model"></param>
        public XsanViewModel(XsanModel model)
        {
            _xsanModel = model;
            _xsanModel.DeviceStateChanged = StateChanged;

            // создаем переменные управления
            ControlValuesList.Add(new ControlValue()); // XSAN_CTRL_IDX
            ControlValuesList.Add(new ControlValue()); // BUNI_CTRL_IDX
            ControlValuesList.Add(new ControlValue()); // POWER_CTRL_IDX

            // запоняем их значениями
            ControlValuesList[XsanConst.XSAN_CTRL_IDX].AddProperty(XsanConst.PROPERTY_XSAN_READY_IDX, 4, 1, _xsanModel.CmdHSIXSANControl, delegate(UInt32 value)
            {
                XsanImitatorReady = (value == 1);
            });
            ControlValuesList[XsanConst.XSAN_CTRL_IDX].AddProperty(XsanConst.PROPERTY_XSAN_BUSY_IDX, 5, 1, _xsanModel.CmdHSIXSANControl, delegate(UInt32 value)
            {
                XsanImitatorBusyOn = (value == 1);
            });
            ControlValuesList[XsanConst.XSAN_CTRL_IDX].AddProperty(XsanConst.PROPERTY_XSAN_ME_IDX, 6, 1, _xsanModel.CmdHSIXSANControl, delegate(UInt32 value)
            {
                XsanImitatorMeOn = (value == 1);
            });
            ControlValuesList[XsanConst.XSAN_CTRL_IDX].AddProperty(XsanConst.PROPERTY_XSAN_CMD_CH_IDX, 0, 2, _xsanModel.CmdHSIXSANControl, delegate(UInt32 value)
            {
                XsanImitatorCmdChannel = (int)value;

            });
            ControlValuesList[XsanConst.XSAN_CTRL_IDX].AddProperty(XsanConst.PROPERTY_XSAN_DAT_CH_IDX, 2, 2, _xsanModel.CmdHSIXSANControl, delegate(UInt32 value)
            {
                XsanImitatorDatChannel = (int)value;

            });
            //
            ControlValuesList[XsanConst.BUNI_CTRL_IDX].AddProperty(XsanConst.PROPERTY_BUNI_ON_IDX, 0, 1, _xsanModel.CmdHSIBUNIControl, delegate(UInt32 value)
            {
                BuniImitatorOn = (value == 1);
            });
            ControlValuesList[XsanConst.BUNI_CTRL_IDX].AddProperty(XsanConst.PROPERTY_BUNI_CMD_CH_IDX, 1, 1, _xsanModel.CmdHSIBUNIControl, delegate(UInt32 value)
            {
                BuniImitatorCmdChannel = (int)value;
            });
            ControlValuesList[XsanConst.BUNI_CTRL_IDX].AddProperty(XsanConst.PROPERTY_BUNI_DAT_CH_IDX, 2, 2, _xsanModel.CmdHSIBUNIControl, delegate(UInt32 value)
            {
                BuniImitatorDatChannel = (int)value;
            });
            ControlValuesList[XsanConst.BUNI_CTRL_IDX].AddProperty(XsanConst.PROPERTY_BUNI_HZ_IDX, 4, 1, _xsanModel.CmdHSIBUNIControl, delegate(UInt32 value)
            {
                BuniImitatorTimeStampOn = (value == 1);
            });
            ControlValuesList[XsanConst.BUNI_CTRL_IDX].AddProperty(XsanConst.PROPERTY_BUNI_KBV_IDX, 5, 1, _xsanModel.CmdHSIBUNIControl, delegate(UInt32 value)
            {
                BuniImitatorObtOn = (value == 1);
            });
            //
            ControlValuesList[XsanConst.POWER_CTRL_IDX].AddProperty(XsanConst.PROPERTY_POWER_IDX, 0, 1, _xsanModel.CmdPowerOnOff, delegate(UInt32 value) { });
        }

        /// <summary>
        /// Для каждого элемента управления тикаем временем
        /// </summary>
        public void TickAllControlsValues()
        {
            //if (ControlValuesList == null) return;
            Debug.Assert(ControlValuesList != null, "ControlValuesList не должны быть равны null!");

            foreach (ControlValue cv in ControlValuesList)
            {
                cv.TimerTick();
            }
            //TODO: переделать
            ControlValuesList[XsanConst.BUNI_CTRL_IDX].UsbValue = _xsanModel.BuniControlValue;
            ControlValuesList[XsanConst.XSAN_CTRL_IDX].UsbValue = _xsanModel.XsanControlValue;
            ControlValuesList[XsanConst.POWER_CTRL_IDX].UsbValue = _xsanModel.PowerControlValue;
        }

        /// <summary>
        /// Вызывается при подключении прибора, чтобы все элементы управления обновили свои значения
        /// </summary>
        private void refreshAllControlsValues()
        {
            //            if (ControlValuesList == null) return;
            Debug.Assert(ControlValuesList != null, "ControlValuesList не должны быть равны null!");

            foreach (ControlValue cv in ControlValuesList)
            {
                cv.RefreshGetValue();
            }
        }

        /// <summary>
        /// Начинаем работу по USB
        /// </summary>
        public void Start() {
            _xsanModel.Start();
        }

        /// <summary>
        /// Получаем скорость USB
        /// </summary>
        /// <returns></returns>
        public string GetDeviceSpeed()
        {
            return Converter.SpeedToStr(_xsanModel.Speed);
        }

        /// <summary>
        /// Получаем максимальный размер большого буфера с момента последнего вызова
        /// </summary>
        /// <returns></returns>
        public string GetBuferSize()
        {
            return _xsanModel.GlobalBufferSize.ToString();
        }

        /// <summary>
        /// Изменилось состояние подключения
        /// </summary>
        /// <param name="state"></param>
        public void StateChanged(bool state)
        {
            if (state)
            {
                refreshAllControlsValues();
            }
            Connected = state;
        }
    }
}
