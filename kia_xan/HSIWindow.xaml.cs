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
    public class HSISTat : INotifyPropertyChanged
    {
        private string _chName;
        public HSISTat()
        {
            _chName = "Основной";
            _framesCnt = 10;
        }

        public string ChName
        {
            get { return _chName; }
            set
            {
                _chName = value;
                FirePropertyChangedEvent("ChName");
            }
        }
        private int _framesCnt;
        public int FramesCnt
        {
            get { return _framesCnt; }
            set
            {
                _framesCnt = value;
                FirePropertyChangedEvent("FramesCnt");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HSIStatList : ObservableCollection<HSISTat>
    {
        public HSIStatList()
            : base()
            {
                Add(new HSISTat());
                Add(new HSISTat());
            }
    }

    /// <summary>
    /// Interaction logic for HSIWindow.xaml
    /// </summary>
    public partial class HSIWindow : Window
    {
        HSIStatList hList = new HSIStatList();
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        XSAN _xsan;

        public HSIWindow(XSAN xxsan)
        {
            InitializeComponent();
            _xsan = xxsan;
            KVVGrid.DataContext = _xsan.HSIInt.BUKStat;

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(timerWork);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        public void timerWork(object sender, EventArgs e)
        {
            _xsan.BUKControl.TimerTick();
            _xsan.KVVControl.TimerTick();

            //Random random = new Random();
            //hList[0].FramesCnt += random.Next(5);
            //hList[1].FramesCnt += random.Next(10);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
        }
    }
}
