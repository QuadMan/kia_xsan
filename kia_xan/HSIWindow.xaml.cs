using System.Diagnostics;
using EGSE.Utilites;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace kia_xan
{
    /// <summary>
    /// Interaction logic for HSIWindow.xaml
    /// </summary>
    public partial class HSIWindow : Window
    {
        /// <summary>
        /// Список для вывода в таблицу данных от XSAN
        /// </summary>
        public ObservableCollection<kia_xan.HSIInterface.XSANChannel> XSANCollection = new ObservableCollection<kia_xan.HSIInterface.XSANChannel>();
        
        /// <summary>
        /// Список для вывода в таблицу данных от БУНИ
        /// </summary>
        public ObservableCollection<kia_xan.HSIInterface.BUNIChannel> BUNICollection = new ObservableCollection<kia_xan.HSIInterface.BUNIChannel>();
        
        // таймер 1 раз в секунду
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        // ссылка на устройство
        private XSAN _xsan;

        public HSIWindow()
        {
            InitializeComponent();
            //
            // создаем коллекции для отображения статиcтики в таблицах по XSAN и БУНИ
            XSANCollection.Add(new HSIInterface.XSANChannel("Основной"));
            XSANCollection.Add(new HSIInterface.XSANChannel("Резервный"));
            BUNICollection.Add(new HSIInterface.BUNIChannel("Основной"));
            BUNICollection.Add(new HSIInterface.BUNIChannel("Резервный"));
            XSANGrid.DataContext = XSANCollection;
            BUNIGrid.DataContext = BUNICollection;
            //
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(timerWork);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            //
            DataContext = _xsan;
        }

        public void Init(XSAN xxsan)
        {
            _xsan = xxsan;
            _xsan.HSIInt.XSANStat.onUKSFrameReceived = newUKSFrame;            
        }

        /// <summary>
        /// Обработчк пришедшего УКС
        /// </summary>
        /// <param name="buf">Данные УКС</param>
        /// <param name="timeBuf">Время получения</param>
        public void newUKSFrame(byte[] buf, byte[] timeBuf)
        {
            EgseTime time = new EgseTime();
            time.Data = timeBuf;

            string uksString = time.ToString() + ": " + Converter.ByteArrayToHexStr(buf);
            UKSListBox.Dispatcher.Invoke(new Action(delegate { UKSListBox.Items.Add(uksString); UKSListBox.ScrollIntoView(uksString); }));
            //
            LogsClass.Instance.Files[LogsClass.HsiIdx].LogText = "УКС: " + uksString;
        }

        /// <summary>
        /// Раз в секунду переписываем из статистики реального времени (_xsan.HSIInt.XSANStat и т.д.) данные в наши коллекции для отображения
        /// в таблицах
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timerWork(object sender, EventArgs e)
        {
            if ((bool) NoScreenUpdateCb.IsChecked) return;

            for (int i = 0; i < 2; i++)
            {
                XSANCollection[i].SRCnt = _xsan.HSIInt.XSANStat.Channels[i].SRCnt;
                XSANCollection[i].ObtCnt = _xsan.HSIInt.XSANStat.Channels[i].OBTCnt;
                XSANCollection[i].DRCnt = _xsan.HSIInt.XSANStat.Channels[i].DRCnt;
                XSANCollection[i].TimeStampCnt = _xsan.HSIInt.XSANStat.Channels[i].TimeStampCnt;
                XSANCollection[i].UksCnt = _xsan.HSIInt.XSANStat.Channels[i].UKSCnt;
                XSANCollection[i].OBTStr = kia_xan.HSIInterface.BUNITime.ConvertToStr(_xsan.HSIInt.XSANStat.Channels[i].OBTVal);

                BUNICollection[i].ErrInCRCCnt = _xsan.HSIInt.BUNIStat.Channels[i].ErrInCRCCnt;
                BUNICollection[i].ErrInMarkerCnt = _xsan.HSIInt.BUNIStat.Channels[i].ErrInMarkerCnt;
                BUNICollection[i].ErrInParityCnt = _xsan.HSIInt.BUNIStat.Channels[i].ErrInParityCnt;
                BUNICollection[i].ErrInStopBitCnt = _xsan.HSIInt.BUNIStat.Channels[i].ErrInStopBitCnt;
                BUNICollection[i].FramesCnt = _xsan.HSIInt.BUNIStat.Channels[i].FramesCnt;
                BUNICollection[i].StatusBUSYCnt = _xsan.HSIInt.BUNIStat.Channels[i].StatusBUSYCnt;
                BUNICollection[i].StatusDataFramesCnt = _xsan.HSIInt.BUNIStat.Channels[i].StatusDataFramesCnt;
                BUNICollection[i].StatusMECnt = _xsan.HSIInt.BUNIStat.Channels[i].StatusMECnt;
                BUNICollection[i].StatusSRCnt = _xsan.HSIInt.BUNIStat.Channels[i].StatusSRCnt;
            }

            // покажем статистику по записанным данным, если запись производится
            if ((bool)WriteDataCheckBox.IsChecked)
            {
                LogFileNameLabel.Content = System.IO.Path.GetFileName(_xsan.XsanFileName);
                LogFileSizeLabel.Content = Converter.FileSizeToStr((ulong)_xsan.XsanFileSize);/// (ulong)_hsiFramesStream.Length);
            }
        }

        /// <summary>
        /// Кнопка "Выдать УКС"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (UksStrTextBox.Text == String.Empty) return;

            // проверяем количество байт
            if (UksStrTextBox.Text.Split(' ').Length > HSIInterface.HSI_MAX_UKS_BYTES_COUNT)
            {
                MessageBox.Show("Длина УКС не должна быть больше " + HSIInterface.HSI_MAX_UKS_BYTES_COUNT.ToString());
                return;
            }
            
            byte[] UKSData;
            try
            {
                 UKSData = EGSE.Utilites.Converter.HexStrToByteArray(UksStrTextBox.Text);
                _xsan.Device.CmdSendUKS(UKSData);
            }
            catch {
                MessageBox.Show("Ошибка задания строки УКС - нужно перечислить байты через пробел в шестнадцатеричном виде.");
            }
            finally {
                UKSData = null;
            }
        }

        /// <summary>
        /// Кнопка "Очистка статистики"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _xsan.HSIInt.BUNIStat.Clear();
            _xsan.HSIInt.XSANStat.Clear();
        }

        /// <summary>
        /// При закрытии окна (скрываем)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        /// <summary>
        /// Логгируем все нажания кнопок, чекбоксов и т.д.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseLoggerEvent(object sender, MouseButtonEventArgs e)
        {
            string logEvent = EventClickToString.ElementClicked(e);
            if (logEvent != null) {
                LogsClass.Instance.Files[LogsClass.OperatorIdx].LogText = logEvent; 
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
