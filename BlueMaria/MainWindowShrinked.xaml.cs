using BlueMaria.ViewModels;
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
    /// Interaction logic for MainWindowShrinked.xaml
    /// </summary>
    public partial class MainWindowShrinked : Window
    {
        public event EventHandler Switched;

        protected void OnSwitched([CallerMemberName] string propertyName = null) =>
            Switched?.Invoke(this, EventArgs.Empty);

        public MainWindowShrinked()
        {
            InitializeComponent();

            // setting the WS_EX_NOACTIVATE
            this.Loaded += MainWindowShrinked_Loaded;

            // drag the whole win as titlebar
            this.MouseDown += MainWindowShrinked_MouseDown;

            // this is to emulate WndProc and seems keeping the other window foreground even if click on this
            this.PreviewMouseLeftButtonDown += MainWindowShrinked_PreviewMouseDown;
            this.PreviewMouseLeftButtonUp += MainWindowShrinked_PreviewMouseUp;
            //this.PreviewMouseDown += MainWindowShrinked_PreviewMouseDown;
            //this.PreviewMouseUp += MainWindowShrinked_PreviewMouseUp;
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

        private void MainWindowShrinked_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // this is to be able to drag the window (any place except witin controls that are 'eating' mouse)
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void MainWindowShrinked_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_NOACTIVATE; //.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        private void _closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonShrink_Click(object sender, RoutedEventArgs e)
        {
            OnSwitched();

            var screen = WpfScreen.GetScreenFrom(this);
            var minLeft = (int)(screen.DeviceBounds.Width - MainViewModel.DefaultWidth);
            var minTop = (int)(screen.DeviceBounds.Height - MainViewModel.DefaultHeight);

            MainViewModel vm = (MainViewModel)this.DataContext;
            vm.IsMinimized = !vm.IsMinimized;

            if (vm.Left > minLeft)
                vm.Left = minLeft;
            if (vm.Top > minTop)
                vm.Top = minTop;
        }
    }
}
