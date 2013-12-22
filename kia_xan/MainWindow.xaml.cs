using EGSE.Utilites;
using EGSE.Utilites.ADC;
/*
 * Доработки: 
 *            - доработать класс логгера, чтобы он нормально сбрасывал данные
 *            - вывести строки в ресурсы
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
        private const string SW_CAPTION = XsanConst.SW_CAPTION;
        private const string SW_VERSION = XsanConst.SW_VERSION;
        private const string DEV_NAME = XsanConst.DEV_NAME;

        private XsanCyclogramCommands _xsanCycCommands = new XsanCyclogramCommands();
        private HSIWindow hsiWin;
        private const int WIN_HSI_IDX = 1;

        private XSAN EGSE;

        private void initControlValues()
        {
            _controlValuesList.Add(new ControlValue()); // XSAN_CTRL_IDX
            _controlValuesList.Add(new ControlValue()); // BUNI_CTRL_IDX
            _controlValuesList.Add(new ControlValue()); // POWER_CTRL_IDX

            _controlValuesList[XsanConst.XSAN_CTRL_IDX].AddProperty(XsanConst.CTRL_XSAN_READY_IDX, hsiWin.XSANReadyCb, 4, 1, EGSE.Device.CmdHSIXSANControl);
            _controlValuesList[XsanConst.XSAN_CTRL_IDX].AddProperty(XsanConst.CTRL_XSAN_BUSY_IDX, hsiWin.XSANBusyCb, 5, 1, EGSE.Device.CmdHSIXSANControl);
            _controlValuesList[XsanConst.XSAN_CTRL_IDX].AddProperty(XsanConst.CTRL_XSAN_ME_IDX, hsiWin.XSANMECb, 6, 1, EGSE.Device.CmdHSIXSANControl);
            _controlValuesList[XsanConst.XSAN_CTRL_IDX].AddProperty(XsanConst.CTRL_XSAN_CMD_CH_IDX, hsiWin.XSANCmdChannelCbb, 0, 2, EGSE.Device.CmdHSIXSANControl);
            _controlValuesList[XsanConst.XSAN_CTRL_IDX].AddProperty(XsanConst.CTRL_XSAN_DAT_CH_IDX, hsiWin.XSANDatChannelCbb, 2, 2, EGSE.Device.CmdHSIXSANControl);
            //
            _controlValuesList[XsanConst.BUNI_CTRL_IDX].AddProperty(XsanConst.CTRL_BUNI_ON_IDX, hsiWin.BUNIOnCb, 0, 1, EGSE.Device.CmdHSIBUNIControl);
            _controlValuesList[XsanConst.BUNI_CTRL_IDX].AddProperty(XsanConst.CTRL_BUNI_CMD_CH_IDX, hsiWin.BUNICmdChannelCbb, 1, 1, EGSE.Device.CmdHSIBUNIControl);
            _controlValuesList[XsanConst.BUNI_CTRL_IDX].AddProperty(XsanConst.CTRL_BUNI_DAT_CH_IDX, hsiWin.BUNIDataChannelCbb, 2, 2, EGSE.Device.CmdHSIBUNIControl);
            _controlValuesList[XsanConst.BUNI_CTRL_IDX].AddProperty(XsanConst.CTRL_BUNI_HZ_IDX, hsiWin.BUNIHzOn, 4, 1, EGSE.Device.CmdHSIBUNIControl);
            _controlValuesList[XsanConst.BUNI_CTRL_IDX].AddProperty(XsanConst.CTRL_BUNI_KBV_IDX, hsiWin.BUNIKbvOn, 5, 1, EGSE.Device.CmdHSIBUNIControl);
            //
            _controlValuesList[XsanConst.POWER_CTRL_IDX].AddProperty(XsanConst.CTRL_POWER_IDX, null, 0, 1, EGSE.Device.CmdPowerOnOff);

            _xsanCycCommands.ContolValuesList = _controlValuesList;
        }

        private void initDevice()
        {
            EGSE = new XSAN(_controlValuesList);
        }

        private void initWindows()
        {
            _windowsList.Add(new HSIWindow());
            hsiWin = (HSIWindow)_windowsList[WIN_HSI_IDX];
        }

        private void OnTimerWork()
        {
            // выведем значения АЦП
            if (EGSE.Tm.IsPowerOn)
            {
                try
                {
                    U27VLabel.Content = Math.Round(EGSE.Tm.Adc.GetValue(XsanTm.ADC_CH_U)).ToString();
                }
                catch (ADCException)
                {
                    U27VLabel.Content = "---";
                }
                try
                {
                    IXSANLabel.Content = Math.Round(EGSE.Tm.Adc.GetValue(XsanTm.ADC_CH_I)).ToString();
                }
                catch (ADCException)
                {
                    IXSANLabel.Content = "---";
                }
            }
            else
            {
                U27VLabel.Content = "---";
                IXSANLabel.Content = "---";
            }
            
            updateTM();
        }

        private void updateTM()
        {
            // Индикация питания
            if (EGSE.Tm.IsPowerOn)
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
        /// Метод инициализируеющий дополнительные модули (если это необходимо)
        /// </summary>
        private void initModules()
        {
            CycloGrid.cycCommandsAvailable = _xsanCycCommands.CycCommandsAvailable;
            hsiWin.Init(EGSE);
        }

        /// <summary>
        /// Метод проверяет, если чекбокс управляющий окном управления выбран, чтобы было активно окно, ему соответсвующее
        /// </summary>
        private void checkWindowsActivation()
        {
            if ((bool)HSIControlCb.IsChecked)
            {
                _windowsList[WIN_HSI_IDX].Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// Загружаем специфичные настройки приложения при загрузке
        /// </summary>
        private void loadAppSettings()
        {
            // если окно открыто, соответствующий чекбокс должен быть выбран
            if (_windowsList[WIN_HSI_IDX].Visibility == System.Windows.Visibility.Visible)
            {
                HSIControlCb.IsChecked = true;
            }
            // управляем отображением телеметрической информацией
            string powerLabelVisible = AppSettings.Load("PowerLabel");
            if (powerLabelVisible != null)
            {
                switch (powerLabelVisible)
                {
                    case "Visible":
                        TMGrid.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "Hidden":
                        TMGrid.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    default:
                        TMGrid.Visibility = System.Windows.Visibility.Visible;
                        break;
                }
            }
        }

        /// <summary>
        /// Сохраняем специфические настройки приложения
        /// В данном случае - видимость панели телеметрии
        /// </summary>
        private void saveAppSettings()
        {
            AppSettings.Save("PowerLabel", Convert.ToString(TMGrid.Visibility));
        }

        /// <summary>
        /// Панель управления, окно "Управление ВСИ"
        /// При установке флажка, показываем окно, при снятии влажка - просто скрываем
        /// TODO: надо бы сделать автоматическую привязку
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HSIControlCb_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)HSIControlCb.IsChecked)
            {
                _windowsList[WIN_HSI_IDX].Owner = _windowsList[0];
                _windowsList[WIN_HSI_IDX].Show();
            }
            else
            {
                _windowsList[WIN_HSI_IDX].Hide();
            }
        }

        /// <summary>
        /// Кнопка управления питанием
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _controlValuesList[XsanConst.POWER_CTRL_IDX].SetProperty(XsanConst.CTRL_POWER_IDX, Convert.ToInt32(!EGSE.Tm.IsPowerOn));
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
    }
}
