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
using System.Windows.Threading;
using System.IO;

namespace PrimesTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Loaded += MainWindow_Loaded;

            InitializeComponent();
        }



        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Start();

            UIControl.Init(LstHeader, LstPrimes, LstBinary, LstStats, txtStatus, prgBar, txtUsage);
        }



        private void OpenFile_Click(object sender, RoutedEventArgs e) => UIControl.OpenFile();
        private void CloseFile_Click(object sender, RoutedEventArgs e) => UIControl.CloseFile();
        private void SaveFile_Click(object sender, RoutedEventArgs e) => UIControl.SaveFile();
        private void LstPrimes_Selected(object sender, RoutedEventArgs e) => UIControl.SelectedPrime(LstPrimes.SelectedIndex);
        private void ROCheck_Click(object sender, RoutedEventArgs e) => UIControl.ROCheck();
        private void RDCheck_Click(object sender, RoutedEventArgs e) => UIControl.RDCheck();
        private void JumpToPrime_Click(object sender, RoutedEventArgs e) => UIControl.JumpToPrime();
        private void JumpToBinary_Click(object sender, RoutedEventArgs e) => UIControl.JumpToBinary();
        private void FindPrime_Click(object sedner, RoutedEventArgs e) => UIControl.FindPrime();
        private void FindNCCBigJump_Click(object sender, RoutedEventArgs e) => UIControl.FindNCCBigJump();
        private void IsPrime_Click(object sender, RoutedEventArgs e) => UIControl.IsPrime();



        private void Timer_Tick(object sender, EventArgs e)
        {
            UIControl.Update();
        }
    }
}
