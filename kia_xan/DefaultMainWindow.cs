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
        //private AboutWin aboutWin;
        // список элементов упрвления
        //private List<ControlValue> _controlValuesList = new List<ControlValue>(); // TODO: добавление по ключу с проверкой уникальности!
        // список всех окно, основное окно программы всегда под индексом 0
        //private List<Window> _windowsList = new List<Window>();

        public TestC test = new TestC();

        /*********************************************************************************************************
         * 
         * СТАНДАРТНЫЕ ОБРАБОТЧИКИ
         * 
         * *******************************************************************************************************/
        public MainWindow()
        {
            InitializeComponent();
            this.Title = SW_CAPTION;// + " " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();// +"  " + SW_VERSION;

            //!_windowsList.Add(Window.GetWindow(this));

            initDevice();
            initWindows();
            initControlValues();
            loadWindows();
            initModules();

            loadAppSettings();

            //LogsClass.Instance.Files[LogsClass.MainIdx].LogText = "Программа " + SW_VERSION + " загрузилась";
            
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(timerWork);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            //
            EGSE.Device.Start();
            //
            //DataContext = this.test;
            hsiWin.DataContext = EGSE;
        }

        /// <summary>
        /// Передает во все элементы управления тик времени для отсчета периода их обновления
        /// </summary>
        //private void testControlValuesOnTimeTick()
        //{
            //foreach (ControlValue cv in _controlValuesList)
            //{
                //cv.TimerTick();
            //}
        //}

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
            // проверяем элементы управления - изменились ли они
            //testControlValuesOnTimeTick();
            // индикация подключения, скорости
            TimeLabel.Content = EGSE.ETime.ToString();
            
            if (EGSE.Connected)
            {
                ConnectionLabel.Background = Brushes.LightGreen;
                ConnectionLabel.Content = DEV_NAME + " подключен";
            }
            else
            {
                ConnectionLabel.Background = Brushes.Red;
                ConnectionLabel.Content = DEV_NAME + " отключен";

                // инициализируем все экранные формы на значения по-умолчанию при отключении от устройства
                DefaultScreenInit();
                //hsiWin.Cle
            }
             
            SpeedLabel.Content = Converter.SpeedToStr(EGSE.Device.Speed) + " [" + EGSE.Device.GlobalBufferSize.ToString() + "]";
        }

        /// <summary>
        /// Вызывается при закрытии приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)л
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
            //Window mainWin = Window.GetWindow(this);
            
            foreach (Window w in Application.Current.Windows)
            {
                if (w != Application.Current.MainWindow)
                {
                    w.Close();
                }
            }
            
            EGSE.Device.FinishAll();
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
//            State = !State;
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

    public class TestC : INotifyPropertyChanged
    {
        private bool _isWinOpened;

        public bool IsWinOpened
        {
            get { return _isWinOpened; }
            set
            {
                _isWinOpened = value;
                FirePropertyChangedEvent("IsWinOpened");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
