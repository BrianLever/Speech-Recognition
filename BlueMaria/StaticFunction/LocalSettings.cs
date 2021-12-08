using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BlueMaria.StaticFunction
{
    class LocalSettings
    {  
        public static Action<object, EventArgs> LoadMainPage { get; set; }
        public static Action<object, EventArgs> LoadShrikedPage { get; set; }
        public static Action<object, EventArgs> LoadSettings { get; set; }
        public static Action<object, EventArgs> ToastNotification { get; set; }
        public static Action<object, EventArgs> ToastRetry { get; set; }
        public static Action<object, EventArgs> ToastErrorNotification { get; set; }
        public static Action<object, EventArgs> Closingtowindowstray { get; set; }
        public static Action<object, EventArgs> Kill { get; set; }
        public static Action<object, EventArgs> StopRecordApi { get; set; }
        public static Action<object, EventArgs> PasswrdLogout { get; set; }
        public static Action<object, EventArgs> Showwindow { get; set; }
        public static Action<object, EventArgs> ButtionCheck { get; set; }
        public static Action<object, EventArgs> LoadDataContext { get; set; }
        public static Action<object, EventArgs> LoadMainPageforshrink { get; set; }
        public static Action<object, EventArgs> LoadShrikedPageDatacontext { get; set; }
        public static string CurrentLaguage { get;  set; }
        private static IList<string> _supportedLanguages;
        public static IList<string> SupportedLanguages { get { return _supportedLanguages ?? (_supportedLanguages = new[] { "en", "de", "es","fr","it","pt" }); } }
        public static IList<string> _laguages;
        public static IList<string> Laguages { get { return _laguages ?? (_laguages = new[] { "English", "Deutsch", "Español", "Français", "Italiano", "Português" }); } }
        public static bool AutoLaunch { get;  set; }
        public static Action<object, EventArgs> CloseError { get; set; }
        public static bool LoginCheck { get; set; }
        public static bool _isLoggedIn { get; set; }
        public static bool canRecord { get; set; }
        public static bool StopRecording { get; set; }
        public static bool isListening { get; set; }
        public static string PassWordd { get;  set; }
        public static bool IsMainpageLoaded { get; set; }
        public static bool WindowShrinked { get; set; }
        public static bool Netcheck { get; set; }
        public static string UserName { get;  set; }
        public static bool IsNetAvailable { get; set; }
        public static bool IsCreditZero { get; set; }
        public static bool StartUp { get; set; }
        public static string CurrentPage { get;  set; }

        public static string ApiRecordingStatus = "Stop";

        public static string currentfavicon;
        #region Favicon
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        static System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        private const int SW_HIDE = 0;
        private const int SW_NORMAL = 1;

        public static void ChangeIcon(string IcoStatus)
        {

            currentfavicon = IcoStatus;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {

                if (_isLoggedIn)
                {
                    if (IcoStatus == "Red")
                    {
                        ni.Icon = new System.Drawing.Icon(@"Favicon\Red.ico");
                    }
                    else if (IcoStatus == "Grey")
                    {
                        ni.Icon = new System.Drawing.Icon(@"Favicon\HalfGreen.ico");
                    }
                    else if (IcoStatus == "HalfGreen")
                    {
                        ni.Icon = new System.Drawing.Icon(@"Favicon\HalfGreen.ico");
                    }
                    else if (IcoStatus == "Green")
                    {
                        ni.Icon = new System.Drawing.Icon(@"Favicon\Green.ico");
                    }
                }
                else
                {
                    if (IcoStatus == "Red")
                    {
                        ni.Icon = new System.Drawing.Icon(@"Favicon\Red.ico");
                    }
                    else if (IcoStatus == "Grey")
                    {
                        ni.Icon = new System.Drawing.Icon(@"Favicon\Grey.ico");
                    }
                    else if (IcoStatus == "HalfGreen")
                    {
                        ni.Icon = new System.Drawing.Icon(@"Favicon\Grey.ico");
                    }
                    else if (IcoStatus == "Green")
                    {
                        ni.Icon = new System.Drawing.Icon(@"Favicon\Green.ico");
                    }
                }

                ni.Visible = true;

                ni.Text = "Blue-Maria";
                ni.DoubleClick +=
                    delegate (object sender, EventArgs args)
                    {
                        ShowMainWindow();
                    };
                ni.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();              
                ni.ContextMenuStrip.Items.Add(BlueMariaLocalization.Properties.Resources.OpenBlueMaria).Click += (s, e) => ShowMainWindow();
                ni.ContextMenuStrip.Items.Add(BlueMariaLocalization.Properties.Resources.Close).Click += (s, e) => ExitApplication();

            }));




        }


        private static void ShowMainWindow()
        {
            Process current = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id == current.Id)
                {
                    Showwindow?.Invoke("", EventArgs.Empty);
                    IntPtr pointer = FindWindow(null, "Blue-Maria");
                    ShowWindow(pointer, SW_NORMAL);
                    break;
                }
            }
        }
        public static void MinimizeWindow()
        {
            Process current = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id == current.Id)
                {
                    IntPtr pointer = FindWindow(null, "Blue-Maria");
                    ShowWindow(pointer, SW_HIDE);
                    break;
                }
            }
        }

        public static void ExitApplication()
        {
            ni.Dispose();
            ni = null;
            Application.Current.Shutdown();
           
        }
        #endregion

    }
}
