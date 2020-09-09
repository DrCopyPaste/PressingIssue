﻿using NLog;
using Services.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KeyHookInWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Logger logger { get; private set; }

        private HotkeyService mahook;
        private HotkeyService mahook2;

        public MainWindow()
        {
            InitializeComponent();

            logger = NLog.LogManager.GetCurrentClassLogger();

            mahook = new HotkeyService();
            //mahook2 = new HotkeyService();

            

            this.Closing += MainWindow_Closing;

            //var logger = NLog.LogManager.GetCurrentClassLogger();
            //logger.Info("this is a test from main");

            /*
            
             */
        }

        

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            mahook.Dispose();
            //mahook2.Dispose();
        }
    }
}
