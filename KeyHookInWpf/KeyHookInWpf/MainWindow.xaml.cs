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

        private GlobalHotkeyService hotkeyService;
        //private GlobalHotkeyService hotkeyService2;

        public MainWindow()
        {
            InitializeComponent();
            SetModeText();
            logger = NLog.LogManager.GetCurrentClassLogger();

            hotkeyService = new GlobalHotkeyService();
            hotkeyService.KeyEvent += Mahook_CustomEvent;

            //hotkeyService2 = new HotkeyService();

            hotkeyService.AddOrUpdateOnReleaseHotkey(
                "Pause",
                () =>
                {
                    logger.Info("this was on release!");

                    var stringBuilder = new StringBuilder(Eventlines.Text);
                    stringBuilder.Insert(0, $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.fff} [Pause] hotkey triggered on release\n");

                    Eventlines.Text = stringBuilder.ToString();
                });

            hotkeyService.AddOrUpdateQuickCastHotkey(
                "F12",
                () =>
                {
                    logger.Info("this was with quickcast!");

                    var stringBuilder = new StringBuilder(Eventlines.Text);
                    stringBuilder.Insert(0, $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.fff} [F12] hotkey triggered on quickcast (key down - no repeat) \n");

                    Eventlines.Text = stringBuilder.ToString();
                });

            this.Closing += MainWindow_Closing;
        }

        private void Mahook_CustomEvent(object sender, GlobalHotkeyServiceEventArgs e)
        {
            this.ShownKeys.Content = e.AsSettingString;
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            hotkeyService.Dispose();
            //hotkeyService2.Dispose();
        }

        private void SetModeText()
        {
            this.ToggleModeButton.Content = "Toggle Mode";
            this.CurrentMode.Content = hotkeyService.ProcessingHotkeys ? "Capturing hotkeys" : "Not capturing hotkeys";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var previousMode = hotkeyService.ProcessingHotkeys;

            if (hotkeyService.Running)
            {
                hotkeyService.Stop();
            }

            hotkeyService.Start(!previousMode);
            SetModeText();
        }
    }
}
