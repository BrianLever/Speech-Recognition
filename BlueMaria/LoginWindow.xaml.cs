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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static BlueMaria.UnsafeWindowMethods;

namespace BlueMaria
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            this.Loaded += LoginWindow_Loaded;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModels.MainViewModel vm = (ViewModels.MainViewModel)this.DataContext;
            vm.LoggedInChanged += LoggedInChanged;
        }

        private void _closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModels.MainViewModel vm = (ViewModels.MainViewModel)this.DataContext;
            vm.SecurePassword = ((PasswordBox)sender).SecurePassword;
        }

        //private void ButtonShrink_Click(object sender, RoutedEventArgs e)
        //{
        //    ViewModels.MainViewModel vm = (ViewModels.MainViewModel)this.DataContext;
        //    vm.IsMinimized = !vm.IsMinimized;
        //}

        private void ButtonLogIn_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
        }

        private void LoggedInChanged(object sender, EventArgs e)
        {
            ViewModels.MainViewModel vm = (ViewModels.MainViewModel)this.DataContext;
            if (vm.IsLoggedIn) this.Close();
        }

    }
}
