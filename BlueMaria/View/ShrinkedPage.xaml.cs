using BlueMaria.StaticFunction;
using BlueMaria.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace BlueMaria.View
{
    /// <summary>
    /// Interaction logic for ShrinkedPage.xaml
    /// </summary>
    public partial class ShrinkedPage : Page
    {
        MainWindow _main;
        public ShrinkedPage(MainWindow main)
        {
            InitializeComponent();
            this.PreviewMouseLeftButtonDown += MainWindowShrinked_PreviewMouseDown;
            this.PreviewMouseLeftButtonUp += MainWindowShrinked_PreviewMouseUp;
            _main = main;
        }
        IntPtr _previousForegroundWindow;
        private void MainWindowShrinked_PreviewMouseDown(object sender, MouseButtonEventArgs e)
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

        private void MainWindowShrinked_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // If previous window still exist, then activate it.
            if (UnsafeWindowMethods.IsWindow(_previousForegroundWindow))
            {
                UnsafeWindowMethods.SetForegroundWindow(_previousForegroundWindow);
                _previousForegroundWindow = IntPtr.Zero;
            }
        }

        private void _closeButton_Click(object sender, RoutedEventArgs e)
        {
            LocalSettings.Closingtowindowstray?.Invoke(this, EventArgs.Empty);
            // Application.Current.Shutdown();
        }

        private void ButtonShrink_Click(object sender, RoutedEventArgs e)
        {
            //OnSwitched();
            
            var screen = WpfScreen.GetScreenFrom(_main);
            var minLeft = (int)(screen.DeviceBounds.Width - MainViewModel.DefaultWidth);
            var minTop = (int)(screen.DeviceBounds.Height - MainViewModel.DefaultHeight);
            LocalSettings.LoadMainPageforshrink?.Invoke(this, EventArgs.Empty);
            MainViewModel vm = (MainViewModel)this.DataContext;
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

        private async void ButtonMic1_Click(object sender, RoutedEventArgs e)
        {
            if (!await InternetCheck.Internet())
            {
                System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + "\n" + "Please connect to the internet first..",
                    "Blue-Maria", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            else if (!LocalSettings.canRecord)
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

        private async void ButtonMic2_Click(object sender, RoutedEventArgs e)
        {
            if (!await InternetCheck.Internet())
            {
                System.Windows.MessageBox.Show("This action is not possible without internet connection" + "\n" + "\n" + "Please connect to the internet first.",
                    "Blue-Maria", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            else if (!LocalSettings.canRecord)
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
    }
}
