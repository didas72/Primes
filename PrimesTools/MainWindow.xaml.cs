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
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) => UIControl.Init(LstHeader, LstPrimes, LstBinary, LstStats, txtStatus, prgBar);



        private void BtnOpenFile_Click(object sender, RoutedEventArgs e) => UIControl.OpenFile();
        private void BtnCloseFile_Click(object sender, RoutedEventArgs e) => UIControl.CloseFile();
    }
}
