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
    //public class UksListClass : ObservableCollection
    //{
    //    public List<string> uksList { get; set; }

    //    public void AddB(string b)
    //    {
    //        uksList.Add(b);
    //        NotifyPropertyChanged("uksList");
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void NotifyPropertyChanged(string info)
    //    {
    //        if (PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new PropertyChangedEventArgs(info));
    //        }
    //    }
    //}

    /// <summary>
    /// Interaction logic for HSIWindow.xaml
    /// </summary>
    public partial class HSIWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        XSAN _xsan;

        public ObservableCollection<kia_xan.HSIInterface.XSANChannel> XSANCollection = new ObservableCollection<kia_xan.HSIInterface.XSANChannel>();
        public ObservableCollection<kia_xan.HSIInterface.BUNIChannel> BUNICollection = new ObservableCollection<kia_xan.HSIInterface.BUNIChannel>();
        //public ObservableCollection<string> uksList = new ObservableCollection<string>();

        private bool _canClose = false;
        FileStream _hsiFramesStream;

        public void CanClose()
        {
            _canClose = true;
        }

        public void newUKSFrame(byte[] buf, byte[] timeBuf)
        {
            EgseTime time = new EgseTime();
            time.data = timeBuf;

            string uksString = time.ToString()+": "+Converter.ByteArrayToHexStr(buf);
            UKSListBox.Dispatcher.Invoke(new Action(delegate { UKSListBox.Items.Add(uksString); UKSListBox.ScrollIntoView(uksString); }));            
        }

        public HSIWindow(XSAN xxsan)
        {
            InitializeComponent();
            //
            _hsiFramesStream = null;
            //
            XSANCollection.Add(new HSIInterface.XSANChannel("Основной"));
            XSANCollection.Add(new HSIInterface.XSANChannel("Резервный"));
            BUNICollection.Add(new HSIInterface.BUNIChannel("Основной"));
            BUNICollection.Add(new HSIInterface.BUNIChannel("Резервный"));
            //
            //UKSListBox.Items.Add("Test string");
            //
            _xsan = xxsan;
            _xsan.HSIInt.XSANStat.onUKSFrameReceived = newUKSFrame;
            XSANGrid.DataContext = XSANCollection;
            BUNIGrid.DataContext = BUNICollection;
            //UKSListBox.DataContext = uksList;

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(timerWork);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void UpdateBUKControl()
        {
            BUNIOnCb.IsChecked = (bool)((_xsan.BUNIControl.GetValue & 1) == 1);
            BUNIDataChannelCbb.SelectedIndex = (_xsan.BUNIControl.GetValue >> 2) & 3;
            BUNICmdChannelCbb.SelectedIndex = (_xsan.BUNIControl.GetValue >> 1) & 1;
            //BUKErrorRegisterCb.IsChecked = (bool)(((_xsan.BUKControl.GetValue >>  4) & 1) == 1);
        }

        private void UpdateKVVControl()
        {
            XSANCmdChannelCbb.SelectedIndex = (_xsan.XSANControl.GetValue & 3);
            XSANDatChannelCbb.SelectedIndex = ((_xsan.XSANControl.GetValue >> 2) & 3);
            XSANReadyCb.IsChecked = ((_xsan.XSANControl.GetValue >> 4) & 1) == 1;
            XSANBusyCb.IsChecked = ((_xsan.XSANControl.GetValue >> 5) & 1) == 1;
            XSANMECb.IsChecked = ((_xsan.XSANControl.GetValue >> 6) & 1) == 1;
        }

        public void timerWork(object sender, EventArgs e)
        {
            if ((bool)NoScreenUpdateCb.IsChecked == false)
            {
                for (int i = 0; i < 2; i++)
                {
                    XSANCollection[i].SRCnt = _xsan.HSIInt.XSANStat.Channels[i].SRCnt;
                    XSANCollection[i].ObtCnt = _xsan.HSIInt.XSANStat.Channels[i].OBTCnt;
                    XSANCollection[i].DRCnt = _xsan.HSIInt.XSANStat.Channels[i].DRCnt;
                    XSANCollection[i].TimeStampCnt = _xsan.HSIInt.XSANStat.Channels[i].TimeStampCnt;
                    XSANCollection[i].UksCnt = _xsan.HSIInt.XSANStat.Channels[i].UKSCnt;

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
            }

            // проверим, были ли изменены элементы управления, нужно обновить
            if (_xsan.BUNIControl.TimerTick() == EGSE.Utilites.ControlValue.ValueState.vsChanged)
            {
                UpdateBUKControl();
            }
            if (_xsan.XSANControl.TimerTick() == EGSE.Utilites.ControlValue.ValueState.vsChanged)
            {
                UpdateKVVControl();
            }
            //
            if ((bool)WriteDataCheckBox.IsChecked)
            {
                LogFileNameLabel.Content = System.IO.Path.GetFileName(_hsiFramesStream.Name);
                LogFileSizeLabel.Content = Converter.FileSizeToStr((ulong)_hsiFramesStream.Length);
            }
        }

        private void GetXSANControl()
        {
            if (!this.IsInitialized) return;

            int tmpXSANControl = 0;
            tmpXSANControl = XSANCmdChannelCbb.SelectedIndex;
            tmpXSANControl |= (XSANDatChannelCbb.SelectedIndex << 2);
            if ((bool)XSANReadyCb.IsChecked) { tmpXSANControl |= (1 << 4); }
            if ((bool)XSANBusyCb.IsChecked) { tmpXSANControl |= (1 << 5); }
            if ((bool)XSANMECb.IsChecked) { tmpXSANControl |= (1 << 6); }

            _xsan.XSANControl.SetValue = tmpXSANControl;
            _xsan.Device.CmdHSIXSANControl((byte)_xsan.XSANControl.SetValue, 100);
        }

        private void GetBUNIControl()
        {
            if (!this.IsInitialized) return;

            int tmpBUKControl = 0;

            if ((bool)BUNIOnCb.IsChecked) { tmpBUKControl = 1; }
            tmpBUKControl |= BUNICmdChannelCbb.SelectedIndex << 1;
            tmpBUKControl |= BUNIDataChannelCbb.SelectedIndex << 2;
            //if ((bool)BUKErrorRegisterCb.IsChecked) { tmpBUKControl |= (1 << 4); }

            tmpBUKControl |= (1 << 4); // бит включения герцовой метки
            _xsan.BUNIControl.SetValue = tmpBUKControl;
            _xsan.Device.CmdHSIBUNIControl((byte)_xsan.BUNIControl.SetValue);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (UksStrTextBox.Text == "") return;
            // проверяем количество байт
            if (UksStrTextBox.Text.Split(' ').Length > HSIInterface.HSI_MAX_UKS_BYTES_COUNT)
            {
                MessageBox.Show("Длина УКС не должна быть больше " + HSIInterface.HSI_MAX_UKS_BYTES_COUNT.ToString());
                return;
            }
            //
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

        private void KVVReadyCb_Click(object sender, RoutedEventArgs e)
        {
            GetXSANControl();
        }

        private void BUKErrorRegisterCb_Click(object sender, RoutedEventArgs e)
        {
            GetBUNIControl();
        }

        private void BUKDataChannelCbb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetBUNIControl();
        }

        private void GetBUKControl(object sender, SelectionChangedEventArgs e)
        {
            GetBUNIControl();
        }

        private void XSANCmdChannelCbb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetXSANControl();
        }

        private void KVVDatChannelCbb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetXSANControl();
        }

        private void BUKCmdChannelCbb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetBUNIControl();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _xsan.HSIInt.BUNIStat.Clear();
            _xsan.HSIInt.XSANStat.Clear();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_canClose)
            {
                e.Cancel = false;
            }
            else {
                e.Cancel = true;
                Hide();
            }
        }

        private void WriteDataCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)WriteDataCheckBox.IsChecked)
            {
                _hsiFramesStream = new FileStream("test.dat", System.IO.FileMode.Create);
                LogFileNameLabel.Visibility = Visibility.Visible;
                LogFileSizeLabel.Visibility = Visibility.Visible;
            }
            else
            {
                if (_hsiFramesStream != null)
                {
                    _hsiFramesStream.Close();
                    _hsiFramesStream = null;
                }
                LogFileNameLabel.Visibility = Visibility.Hidden;
                LogFileSizeLabel.Visibility = Visibility.Hidden;
            }
            // выбираем, по какому каналу записываем данные (по комбобоксу выбора приема данных)
            uint selectedChannel = 0;
            switch (BUNIDataChannelCbb.SelectedIndex) {
                case 1:selectedChannel = 0;
                    break;
                case 2: selectedChannel = 1;
                    break;
                case 3 :
                    selectedChannel = 0;
                    break;
                default :
                    selectedChannel = 0;
                    break;
            }
            _xsan.SetFileAndChannelForLogXSANData(_hsiFramesStream, selectedChannel);
        }

        private void mouseLoggerEvent(object sender, MouseButtonEventArgs e)
        {
            string logEvent = Converter.ElementClicked(e);
            if (logEvent != null) {
                LogsClass.Instance.Files[(int)LogsClass.Idx.logOperator].LogText = logEvent; 
            }
        }
    }
}
