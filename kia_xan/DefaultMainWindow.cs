using System.ComponentModel;
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
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        /*********************************************************************************************************
         * 
         * СТАНДАРТНЫЕ ОБРАБОТЧИКИ
         * 
         * *******************************************************************************************************/
        public MainWindow()
        {
            InitializeComponent();
            this.Title = SW_CAPTION;// + " " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();// +"  " + SW_VERSION;

            init();
            /*
            initDevice();
            initWindows();
            initControlValues();
            loadWindows();
            initModules();
             */
            loadWindows();
            loadAppSettings();

            //LogsClass.Instance.Files[LogsClass.MainIdx].LogText = "Программа " + SW_VERSION + " загрузилась";
            
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(timerWork);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            //
            XsanVM.Start();
        }

        /// <summary>
        /// Загружаем параметры окон из конфигурационного файла
        /// </summary>
        private void loadWindows()
        {
            foreach (Window w in Application.Current.Windows)
            {
                AppSettings.LoadWindow(w);
            }
        }

        /// <summary>
        /// Сохраняем параметры окно в конфигурационном файле
        /// </summary>
        private void saveAllWindows()
        {
            foreach (Window w in Application.Current.Windows)
            {
                AppSettings.SaveWindow(w);
            }
        }

        /// <summary>
        /// Обработка таймера 1 раз в секунду
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerWork(object sender, EventArgs e)
        {
            OnTimerWork();
            // TODO: доделать
//                DefaultScreenInit();
            XsanVM.GetTimeAndSpeed();
        }

        /// <summary>
        /// Вызывается при закрытии приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // сохраняем все настройки приложения
            saveAllWindows();
            saveAppSettings();
            // закрываем окна и устройства
            closeAll();
            // закрываем лог-файлы
            LogsClass.Instance.Files[LogsClass.MainIdx].LogText = "Программа завершена";
            LogsClass.Instance.Files.FlushAll();

            Application.Current.Shutdown();
        }

        /// <summary>
        /// Закрываем все окна, кроме основного, так как оно само закрывается
        /// И отключаемся от устройства
        /// </summary>
        private void closeAll()
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w != Application.Current.MainWindow)
                {
                    w.Close();
                }
            }
            
            _xsanModel.FinishAll();
        }

        /// <summary>
        /// При нажатии на кнопку "Выйти"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Кнопка "О программе"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            Window aboutWin = new AboutWin();
            aboutWin.Owner = Window.GetWindow(this);
            aboutWin.ShowDialog();
        }

        /// <summary>
        /// Для отлова нажатия на кнопки-чекбоксы и т.д.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseLoggerEvent(object sender, MouseButtonEventArgs e)
        {
            string logEvent = EventClickToString.ElementClicked(e);
            if (logEvent != null)
            {
                LogsClass.Instance.Files[LogsClass.OperatorIdx].LogText = logEvent;
            }
        }

        /// <summary>
        /// При активации окна проверяем, чтобы дочерние окна были видимы, если установлены чекбоксы соответствущие 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Activated(object sender, EventArgs e)
        {
            //checkWindowsActivation();
        }

    }
}
