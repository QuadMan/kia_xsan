using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kia_xan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {      
        public XSAN Xsan;
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        HSIWindow hWin;

        public MainWindow()
        {
            InitializeComponent();
            Xsan = new XSAN();

            hWin = new HSIWindow(Xsan);

            LogsClass.Instance.Files[(int)LogsClass.Idx.logOperator].LogText = "Мы загрузились";

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(timerWork);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        public void timerWork(object sender, EventArgs e)
        {
            TimeLabel.Content = Xsan.eTime.ToString();
            Xsan.HSIInt.BUKStat[0].UpdateTimeEventData();
            Xsan.HSIInt.BUKStat[1].UpdateTimeEventData();
           
            if (Xsan.Connected)
            {
                ConnectionLabel.Background = Brushes.LightGreen;
                ConnectionLabel.Content = "Прибор подсоединен";
            }
            else
            {
                ConnectionLabel.Background = Brushes.Red;
                ConnectionLabel.Content = "Прибор отключен";
            }
            SpeedLabel.Content = Xsan.Device.speed.ToString();
            //GlobalBufLabel.Content = _dev.globalBufSize.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            hWin.Close();
            Xsan.Device.finishAll();
            LogsClass.Instance.Files[(int)LogsClass.Idx.logOperator].LogText = "Мы выходим...";
            LogsClass.Instance.Files.FlushAll();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Xsan.Device.CmdHSIControl(1);
            //Xsan.Device.CmdHSIImitControl(0x10);

            hWin.Show();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void HSIControlCb_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)HSIControlCb.IsChecked)
            {
                hWin.Show();
            }
            else
            {
                hWin.Hide();
            }
        }
    }
}
