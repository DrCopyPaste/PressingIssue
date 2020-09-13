using NLog;
using Services.Contracts;
using Services.Win32;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace KeyHookInWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Logger logger { get; private set; }

        private GlobalHotkeyService mahook;
        private GlobalHotkeyService mahook2;

        public MainWindow()
        {
            InitializeComponent();

            logger = NLog.LogManager.GetCurrentClassLogger();

            mahook = new GlobalHotkeyService();

            mahook.KeyEvent += Mahook_CustomEvent;
            SetModeText();

            //mahook2 = new HotkeyService();

            mahook.AddOrUpdateOnReleaseHotkey(
                "Pause",
                () =>
                {
                    logger.Info("this was on release!");

                    var stringBuilder = new StringBuilder(Eventlines.Text);
                    stringBuilder.Insert(0, $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.fff} [Pause] hotkey triggered on release\n");

                    Eventlines.Text = stringBuilder.ToString();
                });

            /*
            mahook.AddOrUpdateOnReleaseHotkey(Guid.NewGuid().ToString(), new Services.Contracts.HotkeyAction() { Action = () => logger.Info("this was on release!") });
            */

            mahook.AddOrUpdateQuickCastHotkey(
                "F12",
                () =>
                {
                    logger.Info("this was with quickcast!");

                    var stringBuilder = new StringBuilder(Eventlines.Text);
                    stringBuilder.Insert(0, $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.fff} [F12] hotkey triggered on quickcast (key down - no repeat) \n");

                    Eventlines.Text = stringBuilder.ToString();
                });

            /*
            mahook.AddOrUpdateQuickCastHotkey(Guid.NewGuid().ToString(), new Services.Contracts.HotkeyAction() { Action = () => logger.Info("this was with quickcast!") });
            */






            this.Closing += MainWindow_Closing;

            //var logger = NLog.LogManager.GetCurrentClassLogger();
            //logger.Info("this is a test from main");

            /*
            
             */
        }

        private void Mahook_CustomEvent(object sender, GlobalHotkeyServiceEventArgs e)
        {
            this.ShownKeys.Content = e.AsSettingString;
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            mahook.Dispose();
            //mahook2.Dispose();
        }

        private void SetModeText()
        {
            this.ToggleModeButton.Content = "Toggle Mode";
            this.CurrentMode.Content = mahook.ProcessingHotkeys ? "Capturing hotkeys" : "Not capturing hotkeys";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var previousMode = mahook.ProcessingHotkeys;

            if (mahook.Running)
            {
                mahook.Stop();
            }

            mahook.Start(!previousMode);
            SetModeText();
        }
    }
}
