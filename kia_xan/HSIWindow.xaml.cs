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

namespace kia_xan
{
    /// <summary>
    /// Interaction logic for HSIWindow.xaml
    /// </summary>
    public partial class HSIWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        XSAN _xsan;

        public ObservableCollection<kia_xan.HSIInterface.BUKChannel> BUKCollection = new ObservableCollection<kia_xan.HSIInterface.BUKChannel>();

        public void newUKSFrame(byte[] buf)
        {

        }

        public HSIWindow(ref XSAN xxsan)
        {
            //
            BUKCollection.Add(new HSIInterface.BUKChannel("AAA"));
            BUKCollection.Add(new HSIInterface.BUKChannel("BBB"));
            //
            InitializeComponent();
            _xsan = xxsan;
            _xsan.HSIInt.BUKStat.onUKSFrameReceived = newUKSFrame;
            //KVVGrid.DataContext = _xsan.HSIInt.BUKStat;
            //BUKGrid.DataContext = _xsan.HSIInt.KVVStat;
            BUKGrid.DataContext = BUKCollection;

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
            BUKCollection[0].SRCnt++;
            BUKCollection[1].SRCnt++;
            BUKCollection[0].Name = "ff";
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _xsan.BUKControl.SetValue = 1;
        }

        private void BUKErrorRegisterCb_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void KVVReadyCb_Checked(object sender, RoutedEventArgs e)
        {
            _xsan.KVVControl.SetValue = 0x10;
            _xsan.Device.CmdHSIKVVControl((byte)_xsan.KVVControl.SetValue, 100);
        }
    }
}
