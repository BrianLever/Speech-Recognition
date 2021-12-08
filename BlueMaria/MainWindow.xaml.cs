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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static BlueMaria.UnsafeWindowMethods;

namespace BlueMaria
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event EventHandler Switched;

        protected void OnSwitched([CallerMemberName] string propertyName = null) =>
            Switched?.Invoke(this, EventArgs.Empty);

        public MainWindow()
        {
            InitializeComponent();

            // setting the WS_EX_NOACTIVATE
            this.Loaded += MainWindow_Loaded;

            // drag the whole win as titlebar
            this.MouseDown += MainWindow_MouseDown;

            // this is to emulate WndProc and seems keeping the other window foreground even if click on this
            this.PreviewMouseLeftButtonDown += MainWindow_PreviewMouseDown;
            this.PreviewMouseLeftButtonUp += MainWindow_PreviewMouseUp;
            //this.PreviewMouseDown += MainWindow_PreviewMouseDown;
            //this.PreviewMouseUp += MainWindow_PreviewMouseUp;
        }

        IntPtr _previousForegroundWindow;
        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //ViewModels.MainViewModel vm = (ViewModels.MainViewModel)this.DataContext;
            //if (vm.AllowLogIn)
            //{
            //}

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

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // this is to be able to drag the window (any place except witin controls that are 'eating' mouse)
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_NOACTIVATE; //.WS_EX_TOOLWINDOW;
            //SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        private void _closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonShrink_Click(object sender, RoutedEventArgs e)
        {
            OnSwitched();
            ViewModels.MainViewModel vm = (ViewModels.MainViewModel)this.DataContext;
            vm.IsMinimized = !vm.IsMinimized;
        }

        private void ButtonHomepage_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.blue-maria.com/");
        }

        private void ButtonFeedback_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.blue-maria.com/feedback");
        }

        private void ButtonBuy_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.blue-maria.com/buy");
        }

        private void ButtonSignUp_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.blue-maria.com/signup");
        }

        private void ButtonPWReset_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.blue-maria.com/passreset");
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModels.MainViewModel vm = (ViewModels.MainViewModel)this.DataContext;
            vm.SecurePassword = ((PasswordBox)sender).SecurePassword;
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            ViewModels.MainViewModel vm = (ViewModels.MainViewModel)this.DataContext;
            if (vm.AllowLogIn)
            {
                //await vm.LogInCommand.ExecuteAsync(null);
                return;
            }

            var window = new LoginWindow
            {
                DataContext = this.DataContext
            };
            window.Closed += LogInWindow_Closed;
            window.ShowDialog();
            //this.Hide();
            //window.Show();
        }

        private void LogInWindow_Closed(object sender, EventArgs e)
        {
            //this.Show();
        }
    }
}
