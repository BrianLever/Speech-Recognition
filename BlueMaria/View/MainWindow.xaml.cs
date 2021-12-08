using BlueMaria.StaticFunction;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static BlueMaria.UnsafeWindowMethods;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Threading;
using BlueMaria.ViewModels;

namespace BlueMaria.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        private const int SW_HIDE = 0;
        private const int SW_NORMAL = 1;


        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded; 
            //Closing to windows tray          
            LocalSettings.Closingtowindowstray += (s, e1) =>
            {
              Process current = Process.GetCurrentProcess();
              foreach (Process process in Process.GetProcessesByName(current.ProcessName))
              {
                if (process.Id == current.Id)
                {
                    IntPtr pointer = FindWindow(null, "Blue-Maria");
                    //ni.ShowBalloonTip(1000, "Blue-Maria", "Running in Background", System.Windows.Forms.ToolTipIcon.Info);
                    ShowWindow(pointer, SW_HIDE);
                    break;
                }
              }
            };
            // drag the whole win as titlebar
            this.MouseDown += MainWindow_MouseDown;
            //ToastNotification 
            LocalSettings.ToastNotification += (s, e1) => 
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    var message = (List<string>)s;
                    if (message.Count == 1)
                    {
                        myToast.TCornerRadius = "6";
                        myToast.THeight = "50";
                        myToast.TWidth = "200";
                        myToast.TBackground = new SolidColorBrush(Colors.Green);
                        myToast.Content = message[0];
                        myToast.Show();
                    }
                }
                    ));
              
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => notifier.ShowCustomMessage("",message[0],null)));
                //notifier.ShowInformation(message[0]);
            };
            LocalSettings.ToastErrorNotification += (s, e1) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    var message = (List<string>)s;
                    if (message.Count == 1)
                    {
                        myToast.TCornerRadius = "6";
                        myToast.THeight = "50";
                        myToast.TWidth = "250";
                        myToast.TBackground = new SolidColorBrush(Colors.Red);
                        myToast.Content = message[0];
                        myToast.Show();
                    }
                }
                ));
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => notifier.ShowCustomMessage("",message[0],null)));
                //notifier.ShowInformation(message[0]);
            };
            LocalSettings.ToastRetry += (s, e1) =>
            {
                var message = (List<string>)s;
                if (message.Count == 1)
                {
                    Toast.Toast.ToastDuration duration = Toast.Toast.ToastDuration.Long;
                    
                    myToast.TCornerRadius = "0";
                    myToast.THeight = "60";
                    myToast.TWidth = "2000";
                    myToast.TBackground = new SolidColorBrush(Colors.OrangeRed);
                    myToast.Content = message[0];
                    myToast.Show();
                }
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => notifier.ShowCustomMessage("",message[0],null)));
                //notifier.ShowInformation(message[0]);
            };
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_NOACTIVATE; //.WS_EX_TOOLWINDOW;
            //SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // this is to be able to drag the window (any place except witin controls that are 'eating' mouse)
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }        
    }
}
