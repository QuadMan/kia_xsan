using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public ObservableCollection<kia_xan.HSIInterface.BUKChannel> BUKCollection = new ObservableCollection<kia_xan.HSIInterface.BUKChannel>();
        public ObservableCollection<kia_xan.HSIInterface.KVVChannel> KVVCollection = new ObservableCollection<kia_xan.HSIInterface.KVVChannel>();
        public ObservableCollection<string> uksList = new ObservableCollection<string>();

        public void newUKSFrame(byte[] buf, byte[] timeBuf)
        {
            string uksString = BitConverter.ToString(buf);
            //Dispatcher.Invoke(new Action((
            //                   delegate { uksList.Add(uksString); }
            //                   ), DispatcherPriority.Background));
            //uksList.Add(uksString);
        }

        public HSIWindow(XSAN xxsan)
        {
            InitializeComponent();
            //
            BUKCollection.Add(new HSIInterface.BUKChannel("Основной"));
            BUKCollection.Add(new HSIInterface.BUKChannel("Резервный"));
            KVVCollection.Add(new HSIInterface.KVVChannel("Основной"));
            KVVCollection.Add(new HSIInterface.KVVChannel("Резервный"));
            //
            _xsan = xxsan;
            _xsan.HSIInt.BUKStat.onUKSFrameReceived = newUKSFrame;
            BUKGrid.DataContext = BUKCollection;
            KVVGrid.DataContext = KVVCollection;
            UKSListBox.DataContext = uksList;

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(timerWork);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void UpdateBUKControl()
        {
            BUKOnCb.IsChecked = (bool)((_xsan.BUKControl.GetValue & 1) == 1);
            BUKDataChannelCbb.SelectedIndex = (_xsan.BUKControl.GetValue >> 2) & 3;
            BUKCmdChannelCbb.SelectedIndex = (_xsan.BUKControl.GetValue >> 1) & 1;
            BUKErrorRegisterCb.IsChecked = (bool)(((_xsan.BUKControl.GetValue >>  4) & 1) == 1);
        }

        private void UpdateKVVControl()
        {
            KVVCmdChannelCbb.SelectedIndex = (_xsan.KVVControl.GetValue & 3);
            KVVDatChannelCbb.SelectedIndex = ((_xsan.KVVControl.GetValue >> 2) & 3);
            KVVReadyCb.IsChecked = ((_xsan.KVVControl.GetValue >> 4) & 1) == 1;
            KVVBusyCb.IsChecked = ((_xsan.KVVControl.GetValue >> 5) & 1) == 1;
            KVVMECb.IsChecked = ((_xsan.KVVControl.GetValue >> 6) & 1) == 1;
        }

        public void timerWork(object sender, EventArgs e)
        {
            for (int i = 0; i < 2; i++)
            {
                BUKCollection[i].SRCnt = _xsan.HSIInt.BUKStat.Channels[i].SRCnt;
                BUKCollection[i].ObtCnt = _xsan.HSIInt.BUKStat.Channels[i].OBTCnt;
                BUKCollection[i].DRCnt = _xsan.HSIInt.BUKStat.Channels[i].DRCnt;
                BUKCollection[i].TimeStampCnt = _xsan.HSIInt.BUKStat.Channels[i].TimeStampCnt;
                BUKCollection[i].UksCnt = _xsan.HSIInt.BUKStat.Channels[i].UKSCnt;

                KVVCollection[i].ErrInCRCCnt = _xsan.HSIInt.KVVStat.Channels[i].ErrInCRCCnt;
                KVVCollection[i].ErrInMarkerCnt = _xsan.HSIInt.KVVStat.Channels[i].ErrInMarkerCnt;
                KVVCollection[i].ErrInParityCnt = _xsan.HSIInt.KVVStat.Channels[i].ErrInParityCnt;
                KVVCollection[i].ErrInStopBitCnt = _xsan.HSIInt.KVVStat.Channels[i].ErrInStopBitCnt;
                KVVCollection[i].FramesCnt = _xsan.HSIInt.KVVStat.Channels[i].FramesCnt;
                KVVCollection[i].StatusBUSYCnt = _xsan.HSIInt.KVVStat.Channels[i].StatusBUSYCnt;
                KVVCollection[i].StatusDataFramesCnt = _xsan.HSIInt.KVVStat.Channels[i].StatusDataFramesCnt;
                KVVCollection[i].StatusMECnt = _xsan.HSIInt.KVVStat.Channels[i].StatusMECnt;
                KVVCollection[i].StatusSRCnt = _xsan.HSIInt.KVVStat.Channels[i].StatusSRCnt;
            }

            // проверим, были ли изменены элементы управления, нужно обновить
            if (_xsan.BUKControl.TimerTick() == EGSE.Utilites.ControlValue.ValueState.vsChanged)
            {
                UpdateBUKControl();
            }
            if (_xsan.KVVControl.TimerTick() == EGSE.Utilites.ControlValue.ValueState.vsChanged)
            {
                UpdateKVVControl();
            }
        }

        private void GetKVVControl()
        {
            if (!this.IsInitialized) return;

            int tmpKVVControl = 0;
            tmpKVVControl |= KVVCmdChannelCbb.SelectedIndex;
            tmpKVVControl |= (KVVDatChannelCbb.SelectedIndex << 2);
            if ((bool)KVVReadyCb.IsChecked) { tmpKVVControl |= (1 << 4); }
            if ((bool)KVVBusyCb.IsChecked) { tmpKVVControl |= (1 << 5); }
            if ((bool)KVVMECb.IsChecked) { tmpKVVControl |= (1 << 6); }

            _xsan.KVVControl.SetValue = tmpKVVControl;
            _xsan.Device.CmdHSIKVVControl((byte)_xsan.KVVControl.SetValue, 100);
        }

        private void GetBUKControl()
        {
            if (!this.IsInitialized) return;

            int tmpBUKControl = 0;

            if ((bool)BUKOnCb.IsChecked) { tmpBUKControl = 1; }
            tmpBUKControl |= BUKCmdChannelCbb.SelectedIndex << 1;
            tmpBUKControl |= BUKDataChannelCbb.SelectedIndex << 2;
            if ((bool)BUKErrorRegisterCb.IsChecked) { tmpBUKControl |= (1 << 4); }

            _xsan.BUKControl.SetValue = tmpBUKControl;
            _xsan.Device.CmdHSIBUKControl((byte)_xsan.BUKControl.SetValue);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (UksStrTextBox.Text == "") return;

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
    //        byte[] UKSData = new byte[5]{0x01,0x03,0x05,0x06,0xAA};

        }

        private void KVVReadyCb_Click(object sender, RoutedEventArgs e)
        {
            GetKVVControl();
        }

        private void BUKErrorRegisterCb_Click(object sender, RoutedEventArgs e)
        {
            GetBUKControl();
        }

        private void BUKDataChannelCbb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetBUKControl();
        }

        private void GetBUKControl(object sender, SelectionChangedEventArgs e)
        {
            GetBUKControl();
        }

        private void KVVCmdChannelCbb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetKVVControl();
        }

        private void KVVDatChannelCbb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetKVVControl();
        }

        private void BUKCmdChannelCbb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetBUKControl();
        }
    }
}
