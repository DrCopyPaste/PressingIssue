using System;
using System.ComponentModel;
using System.Text;
using System.Windows;
using PressingIssue.Services.Contracts.Events;
using PressingIssue.Services.Win32;

namespace TestClient_SimpleGlobalHotkeyService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SimpleGlobalHotkeyService hotkeyService;

        public MainWindow()
        {
            InitializeComponent();
            hotkeyService = new SimpleGlobalHotkeyService();
            hotkeyService.KeyEvent += HotkeyServiceKeyEvent;

            SetModeText();

            hotkeyService.AddOrUpdateOnReleaseHotkey(
                new PressedKeysInfo()
                {
                    Keys = PressingIssue.Services.Contracts.Keys.Pause,
                    IsWinPressed = false,
                    IsAltPressed = false,
                    IsShiftPressed = true
                }
                ,
                () =>
                {
                    var stringBuilder = new StringBuilder(Eventlines.Text);
                    stringBuilder.Insert(0, $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.fff} [Pause] hotkey triggered on release\n");

                    Eventlines.Text = stringBuilder.ToString();
                });

            hotkeyService.AddOrUpdateQuickCastHotkey(
                new PressedKeysInfo()
                {
                    Keys = PressingIssue.Services.Contracts.Keys.F12,
                    IsWinPressed = false,
                    IsAltPressed = false,
                    IsShiftPressed = true
                },
                () =>
                {
                    var stringBuilder = new StringBuilder(Eventlines.Text);
                    stringBuilder.Insert(0, $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.fff} [F12] hotkey triggered on quickcast (key down - no repeat) \n");

                    Eventlines.Text = stringBuilder.ToString();
                });

            this.Closing += MainWindow_Closing;
        }

        private void HotkeyServiceKeyEvent(object sender, SimpleGlobalHotkeyServiceEventArgs e)
        {
            string result = $" {(e.KeyDown ? "Down" : "Up")} Alt:{e.IsAltPressed} - Ctrl:{e.IsCtrlPressed} - Shift:{e.IsShiftPressed} - Win:{e.IsWinPressed} - Key:{e.Key}";
            this.ShownKeys.Content = result;
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            hotkeyService.Dispose();
        }

        private void SetModeText()
        {
            this.ToggleModeButton.Content = "Toggle Mode";
            this.CurrentMode.Content = hotkeyService.ProcessingHotkeys ? "Capturing hotkeys" : "Not capturing hotkeys";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            hotkeyService.ProcessingHotkeys = !hotkeyService.ProcessingHotkeys;

            SetModeText();
        }
    }
}
