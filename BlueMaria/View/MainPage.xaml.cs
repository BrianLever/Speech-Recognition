using BlueMaria.StaticFunction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Shapes;


using System.Windows.Media.Animation;
using BlueMaria.View;
using BlueMaria.ViewModels;
using System.Runtime.InteropServices;
using System.DirectoryServices.AccountManagement;
using System.Windows.Forms;

namespace BlueMaria.View
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
       
        public event EventHandler Switched;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        private const int SW_HIDE = 0;
        private const int SW_NORMAL = 1;

        private static readonly log4net.ILog Log =
           log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        protected void OnSwitched([CallerMemberName] string propertyName = null) =>
            Switched?.Invoke(this, EventArgs.Empty);
        MainWindow _main;
        public MainPage(MainWindow Main)
        {
            InitializeComponent();
            _main = Main;
            // this is to emulate WndProc and seems keeping the other window foreground even if click on this
            this.PreviewMouseLeftButtonDown += MainWindow_PreviewMouseDown;
            this.PreviewMouseLeftButtonUp += MainWindow_PreviewMouseUp;
            LocalSettings.PasswrdLogout += (s, e) => 
            {
                if (Properties.Settings.Default.IsRememberMeOn&& LocalSettings.PassWordd!=null&&LocalSettings.UserName!=null)
                {
                    Pbox.Password = LocalSettings.PassWordd;
                    txtusername.Text = LocalSettings.UserName;

                }
                else if(!Properties.Settings.Default.IsRememberMeOn)
                {
                    txtusername.Text = "";
                    Pbox.Password = "";
                }
            };
        }


        IntPtr _previousForegroundWindow;
        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Get the current foreground window.
            var foregroundWindow = UnsafeWindowMethods.GetForegroundWindow();

            var handle = Process.GetCurrentProcess().MainWindowHandle;
            //var handle = new WindowInteropHelper(this).Handle;

            // If this window is not the current foreground window, then activate itself.
            if (foregroundWindow != handle)
            {
                UnsafeWindowMethods.SetForegroundWindow(handle);

                // Store the handle of previous foreground window.
                if (foregroundWindow != IntPtr.Zero)
                {
                    _previousForegroundWindow = foregroundWindow;
                }
            }
        }

        private void MainWindow_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ViewModels.MainViewModel vm = (ViewModels.MainViewModel)this.DataContext;
            if (vm.NeedsLogIn)
            {
                // disable deactivating for when we need to type in user/pass
                _previousForegroundWindow = IntPtr.Zero;
            }

            // If previous window still exist, then activate it.
            if (UnsafeWindowMethods.IsWindow(_previousForegroundWindow))
            {
                UnsafeWindowMethods.SetForegroundWindow(_previousForegroundWindow);
                _previousForegroundWindow = IntPtr.Zero;
            }
        }


        private void _closeButton_Click(object sender, RoutedEventArgs e)
        {
            LocalSettings.
                Closingtowindowstray?.Invoke(this, EventArgs.Empty);
        }
        private void ButtonShrink_Click(object sender, RoutedEventArgs e)
        {
            
            ViewModels.MainViewModel vm = (ViewModels.MainViewModel)this.DataContext;
            var screen = WpfScreen.GetScreenFrom(_main);
            var minLeft = (int)(screen.DeviceBounds.Width - 75.0671140939597);
            var minTop = (int)(screen.DeviceBounds.Height - 113.422818791946308724832214765101);
            LocalSettings.LoadShrikedPage?.Invoke(this, EventArgs.Empty);
            vm.IsMinimized = !vm.IsMinimized;
            if (vm.Left.ToString().Contains("-"))
            {
                vm.Left = 0;
            }
            if (vm.Top.ToString().Contains("-"))
            {
                vm.Top = 0;
            }
            if (vm.Left > minLeft)
                vm.Left = minLeft;
            if (vm.Top > minTop)
                vm.Top = minTop;
           
        }
        private async void ButtonHomepage_Click(object sender, RoutedEventArgs e)
        {
            if (!await InternetCheck.Internet())
            {
                System.Windows.MessageBox.Show("This action is not possible without internet connection" + " Please connect to the internet first",
                    "Blue-Maria", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            else
            {
                System.Diagnostics.Process.Start("https://blue-maria.com/");
            }
        }

        private async void ButtonFeedback_Click(object sender, RoutedEventArgs e)
        {
          
            if (!await InternetCheck.Internet())
            {
                System.Windows.MessageBox.Show("This action is not possible without internet connection" + " Please connect to the internet first",
                    "Blue-Maria", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            else
            {
                System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=37");
            }
        }

        private async void ButtonBuy_Click(object sender, RoutedEventArgs e)
        {
            if (!await InternetCheck.Internet())
            {
                System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + " Please connect to the internet first",
                    "Blue-Maria",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            else
            {

                System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=31");
            }
        }

        private async void ButtonSignUp_Click(object sender, RoutedEventArgs e)
        {
          


            if (!await InternetCheck.Internet())
            {
                System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + " Please connect to the internet first", 
                    "Blue-Maria", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            else
            {
                System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=388");
            }
        }

        private async void ButtonPWReset_Click(object sender, RoutedEventArgs e)
        {
            if (!await InternetCheck.Internet())
            {
                System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + " Please connect to the internet first",
                    "Blue-Maria", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            else
            {
                System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=3660");
            }

        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModels.MainViewModel vm = (ViewModels.MainViewModel)this.DataContext;
            vm.SecurePassword = ((PasswordBox)sender).SecurePassword;
        }        
        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            var screen = WpfScreen.GetScreenFrom(_main);
            var minLeft = (int)(screen.DeviceBounds.Width - 600);
            var minTop = (int)(screen.DeviceBounds.Height - 600);

            MainViewModel vm = (MainViewModel)this.DataContext;
            //vm.IsMinimized = !vm.IsMinimized;
            if (vm.Left.ToString().Contains("-"))
            {
                vm.Left = 0;
            }
            if (vm.Top.ToString().Contains("-"))
            {
                vm.Top = 0;
            }
            if (vm.Left > minLeft)
                vm.Left = minLeft;
            if (vm.Top > minTop)
                vm.Top = minTop;
            LocalSettings.LoadSettings?.Invoke(this, EventArgs.Empty);
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void error_Initialized(object sender, EventArgs e)
        {

            //MessageBoxResult result = System.Windows.MessageBox.Show("Something went wrong  " + "\n" + "Please Click 'Yes' if you want report this issue.", "Blue-Maria", MessageBoxButton.YesNo, MessageBoxImage.Error);
            //if (result == MessageBoxResult.Yes)
            //{
            //    System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=37");
            //}

        }     
        private async void ButtonMic_Click(object sender, RoutedEventArgs e)
        {
            if (!await InternetCheck.Internet())
            {
                System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + "\n" + "Please connect to the internet first..",
                    "Blue-Maria", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            else if(!LocalSettings.canRecord)
            {
                System.Windows.MessageBox.Show("This action is not possible without a microphone as part of your computer" + "\n" + "\n" + "Please repair or attach your microphone to the computer and try again. You can also check your microphone audio settings and hardware configuration (device driver) in the Windows settings.",
                    "Blue-Maria", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            else if (!LocalSettings._isLoggedIn)
            {
                System.Windows.MessageBox.Show("You need to be logged in to use the dictation functionality" + "\n\n" + " To make use of the trail minutes or your credits you need to \n be logged in. If you do not have an account yet, then please \n sign-up on our homepage",
                    "Blue-Maria",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            var path = System.Windows.Forms.Application.StartupPath;
            path = System.IO.Path.Combine(path,
                "help",
                "help.html");

            System.Diagnostics.Process.Start(path);

        }
    }
}
