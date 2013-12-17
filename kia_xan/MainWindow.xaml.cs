using EGSE.Utilites;
using EGSE.Utilites.ADC;
/*
 * Доработки: + в класс протокола ввести конструктур с нашим логгером
 *            - доработать класс логгера, чтобы он нормально сбрасывал данные
 *            - вывести строки в ресурсы
 *            - запоминать положение окон
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kia_xan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string SW_CAPTION = "КИА XSAN";
        private const string SW_VERSION = "0.0.1.0";
        private const string DEV_NAME = "БИ КИА XSAN";

        private XSAN Xsan;
        public HSIWindow hsiWin;

        private void InitAll()
        {
            Xsan = new XSAN();
        }

        private void CloseAll()
        {
            hsiWin.CanClose();
            hsiWin.Close();
            Xsan.Device.finishAll();
        }

        private void OnTimerWork()
        {
            // выведем значения АЦП
            try
            {
                U27VLabel.Content = Xsan.Tm.Adc.GetValue(XsanTm.ADC_CH_27V);
            }
            catch (ADCException adcE)
            {
                U27VLabel.Content = "---";
            }
            try
            {
                UXSANLabel.Content = Xsan.Tm.Adc.GetValue(XsanTm.ADC_CH_U);
            }
            catch (ADCException adcE)
            {
                UXSANLabel.Content = "---";
            }
            try
            {
                IXSANLabel.Content = Xsan.Tm.Adc.GetValue(XsanTm.ADC_CH_I);
            }
            catch (ADCException adcE)
            {
                IXSANLabel.Content = "---";
            }
            //
            UpdateTM();
        }

        public void UpdateTM()
        {
            // Индикация питания
            if (Xsan.Tm.IsPowerOn)
            {
                PowerLabel.Content = "Питание ВКЛ";
                PowerLabel.Background = Brushes.LightGreen;
                PwrOnOffBtn.Content = "ВЫКЛ ПИТАНИЕ";
            }
            else
            {
                PowerLabel.Content = "Питание ВЫКЛ";
                PowerLabel.Background = Brushes.Red;
                PwrOnOffBtn.Content = "ВКЛ ПИТАНИЕ";
            }
        }

        /// <summary>
        /// Панель управления, окно "Управление ВСИ"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HSIControlCb_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)HSIControlCb.IsChecked)
            {
                hsiWin.Owner = Window.GetWindow(this);
                hsiWin.Show();
            }
            else
            {
                hsiWin.Hide();
            }
        }

        /// <summary>
        /// Кнопка управления питанием
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Xsan.Tm.IsPowerOn)
            {
                Xsan.PowerControl.SetValue = 0;
            }
            else
            {
                Xsan.PowerControl.SetValue = 1;
            }
            Xsan.Device.CmdPowerOnOff((byte)Xsan.PowerControl.SetValue);
        }

        /// <summary>
        /// Скрытие телеметрической информации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (TMGrid.Visibility == System.Windows.Visibility.Visible) {
                    TMGrid.Visibility = System.Windows.Visibility.Hidden;
                }
                else {
                    TMGrid.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        /*********************************************************************************************************
         * 
         * СТАНДАРТНЫЕ ОБРАБОТЧИКИ
         * 
         * *******************************************************************************************************/
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private XsanAbout aboutWin;
        private List<ControlValue> _controlValuesList = new List<ControlValue>();

        public MainWindow()
        {
            InitializeComponent();
            this.Title = SW_CAPTION;// +"  " + SW_VERSION;

            InitAll();
            CreateAllWindows();
            InitControlValues();

            LogsClass.Instance.Files[(int)LogsClass.Idx.logMain].LogText = "Программа " + SW_VERSION + " загрузилась";
            
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(timerWork);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void InitControlValues()
        {
            Xsan.XSANControl.AddProperty(hsiWin.XSANReadyCb, 4, 1, Xsan.Device.CmdHSIXSANControl);
            Xsan.XSANControl.AddProperty(hsiWin.XSANBusyCb, 5, 1, Xsan.Device.CmdHSIXSANControl);
            Xsan.XSANControl.AddProperty(hsiWin.XSANMECb, 6, 1, Xsan.Device.CmdHSIXSANControl);
            Xsan.XSANControl.AddProperty(hsiWin.XSANCmdChannelCbb, 0, 2, Xsan.Device.CmdHSIXSANControl);
            Xsan.XSANControl.AddProperty(hsiWin.XSANDatChannelCbb, 2, 2, Xsan.Device.CmdHSIXSANControl);
            //
            Xsan.BUNIControl.AddProperty(hsiWin.BUNIOnCb, 0, 1, Xsan.Device.CmdHSIBUNIControl);
            Xsan.BUNIControl.AddProperty(hsiWin.BUNICmdChannelCbb, 1, 1, Xsan.Device.CmdHSIBUNIControl);
            Xsan.BUNIControl.AddProperty(hsiWin.BUNIDataChannelCbb, 2, 2, Xsan.Device.CmdHSIBUNIControl);
            //
            _controlValuesList.Add(Xsan.XSANControl);
            _controlValuesList.Add(Xsan.BUNIControl);
        }

        private void TestControlValuesOnTimeTick()
        {
            foreach (ControlValue cv in _controlValuesList)
            {
                cv.TimerTick();
            }
        }

        private void CreateAllWindows()
        {
            hsiWin = new HSIWindow(Xsan);            

            AppSettings.LoadWindow((Window)hsiWin);
            AppSettings.LoadWindow(Window.GetWindow(this));
            if (hsiWin.Visibility == System.Windows.Visibility.Visible)
            {
                HSIControlCb.IsChecked = true;
            }
            string powerLabelVisible = AppSettings.Load("PowerLabel");
            if (powerLabelVisible != null)
            {
                switch (powerLabelVisible)
                {
                    case "Visible" :
                        TMGrid.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "Hidden" :
                        TMGrid.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    default :
                        TMGrid.Visibility = System.Windows.Visibility.Visible;
                        break;
                }
            }
        }

        private void SaveAllWindows()
        {
            AppSettings.SaveWindow((Window)hsiWin);
            AppSettings.SaveWindow(Window.GetWindow(this));
            AppSettings.Save("PowerLabel", Convert.ToString(TMGrid.Visibility));
        }

        private void timerWork(object sender, EventArgs e)
        {
            OnTimerWork();
            //
            TestControlValuesOnTimeTick();
            // индикация подключения
            TimeLabel.Content = Xsan.eTime.ToString();
            if (Xsan.Connected)
            {
                ConnectionLabel.Background = Brushes.LightGreen;
                ConnectionLabel.Content = DEV_NAME + " подключен";
            }
            else
            {
                ConnectionLabel.Background = Brushes.Red;
                ConnectionLabel.Content = DEV_NAME + " отключен";
            }
            SpeedLabel.Content = Converter.SpeedToStr(Xsan.Device.speed) + " [" + Xsan.Device.globalBufSize.ToString() + "]";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveAllWindows();
            //
            CloseAll();
            //
            LogsClass.Instance.Files[(int)LogsClass.Idx.logMain].LogText = "Программа завершена";
            LogsClass.Instance.Files.FlushAll();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            aboutWin = new XsanAbout();
            aboutWin.Owner = Window.GetWindow(this);
            aboutWin.ShowDialog();
        }

        private void mouseLoggerEvent(object sender, MouseButtonEventArgs e)
        {
            string logEvent = EventClickToString.ElementClicked(e);
            if (logEvent != null)
            {
                LogsClass.Instance.Files[(int)LogsClass.Idx.logOperator].LogText = logEvent;
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if ((bool)HSIControlCb.IsChecked)
            {
                hsiWin.Visibility = System.Windows.Visibility.Visible;
            }
        }

    }
}
