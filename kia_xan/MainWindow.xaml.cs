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
        private const string DEV_NAME = XsanConst.DEV_NAME;

        /// <summary>
        /// Допустимые команды циклограммы
        /// </summary>
        private XsanCyclogramCommands _xsanCycCommands = new XsanCyclogramCommands();
        /// <summary>
        /// Окно с Управлением ВСИ
        /// </summary>
        private HSIWindow hsiWin;
        /// <summary>
        /// КИА XSAN
        /// </summary>
        private XSAN EGSE;
        /*
        public Boolean State
        {
            get { return (Boolean)this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
          "State", typeof(Boolean), typeof(MainWindow), new PropertyMetadata(false));
        */
        private void initDevice()
        {
            EGSE = new XSAN();
        }

        private void initControlValues()
        {
            _xsanCycCommands.Xsan = EGSE;
            _xsanCycCommands.HsiWin = hsiWin;
        }

        /// <summary>
        /// Метод инициализируеющий дополнительные модули (если это необходимо)
        /// </summary>
        private void initModules()
        {
            CycloGrid.AddCycCommands(_xsanCycCommands.CycCommandsAvailable);
            hsiWin.Init(EGSE);

            DataContext = this;
        }

        /// <summary>
        /// Создаем все окна, какие нам нужны
        /// </summary>
        private void initWindows()
        {
            hsiWin = new HSIWindow();
            //hsiWin.Owner = Application.Current.MainWindow;
            hsiWin.UpdateLayout();
        }

        /// <summary>
        /// Раз в секунду по таймеру
        /// </summary>
        private void OnTimerWork()
        {
            EGSE.TickAllControlsValues();
            // выведем значения АЦП
            if (EGSE.Tm.IsPowerOn)
            {
                try
                {
                    float tmpUValue = EGSE.Tm.Adc.GetValue(XsanTm.ADC_CH_U);
                    if (tmpUValue > 15) {
                        U27VLabel.Content = Math.Round(tmpUValue).ToString();
                    }
                    else {
                        U27VLabel.Content = "---";
                    }
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
        /// Метод проверяет, если чекбокс управляющий окном управления выбран, чтобы было активно окно, ему соответсвующее
        /// </summary>
        private void checkWindowsActivation()
        {
            if ((bool)HSIControlCb.IsChecked)
            {
                hsiWin.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// Загружаем специфичные настройки приложения при загрузке
        /// </summary>
        private void loadAppSettings()
        {
            // если окно открыто, соответствующий чекбокс должен быть выбран
            if (hsiWin.Visibility == System.Windows.Visibility.Visible)
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

            AppSettings.LoadList(hsiWin.UksSendedList, "UksItems");
            hsiWin.UksStrText.ItemsSource = hsiWin.UksSendedList.ToArray<string>();
        }

        /// <summary>
        /// Сохраняем специфические настройки приложения
        /// В данном случае - видимость панели телеметрии
        /// </summary>
        private void saveAppSettings()
        {
            AppSettings.Save("PowerLabel", Convert.ToString(TMGrid.Visibility));
            AppSettings.SaveList(hsiWin.UksSendedList, "UksItems");
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
            EGSE.ControlValuesList[XsanConst.POWER_CTRL_IDX].SetProperty(XsanConst.PROPERTY_POWER_IDX, Convert.ToInt32(!EGSE.Tm.IsPowerOn));
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

        private void HSIControlCb_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
