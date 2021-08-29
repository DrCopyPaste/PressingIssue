using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PressingIssue.Services.Contracts;
using PressingIssue.Services.Contracts.Events;
using PressingIssue.Services.Win32;

namespace TestClient_SimpleGlobalHotkeyService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly SimpleGlobalHotkeyService hotkeyService;

        private string textContents;

        public string TextContents
        {
            get { return textContents; }
            set { textContents = value; OnPropertyChanged(); }
        }


        private void UpdateText(string contents, bool isQuickCast = true)
        {
            var stringBuilder = new StringBuilder(TextContents);
            var keyType = isQuickCast ? "as quickcast" : "on release";
            stringBuilder.Insert(0, $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.fff} {contents} hotkey triggered {keyType}\n");

            TextContents = stringBuilder.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
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
                    // carful what action you put in here
                    // this logger call is pretty cheap for example

                    var logger = NLog.LogManager.GetCurrentClassLogger();
                    logger.Info($"triggering Shift + Pause on release");

                    // but using objects that hold references to ui thread things tend to clog up memory
                    // try spamming hotkeys while using UpdateText in this action and watch memory usage with task manager

                    //var workTask = Task.Run(() => UpdateText("Pause", false));
                    //workTask.Wait();
                    //workTask.Dispose();

                    //using (var workTask = Task.Run(() => UpdateText("Pause", true)))
                    //{
                    //    workTask.Wait();
                    //}
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
                    // carful what action you put in here
                    // this logger call is pretty cheap for example

                    var logger = NLog.LogManager.GetCurrentClassLogger();
                    logger.Info($"triggering Shift + F12 quick cast");

                    // but using objects that hold references to ui thread things tend to clog up memory
                    // try spamming hotkeys while using UpdateText in this action and watch memory usage with task manager

                    //var workTask = Task.Run(() => UpdateText("F12", true));
                    //workTask.Wait();
                    //workTask.Dispose();

                    //using (var workTask = Task.Run(() => UpdateText("F12", true)))
                    //{
                    //    workTask.Wait();
                    //}
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
