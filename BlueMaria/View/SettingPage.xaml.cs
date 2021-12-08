using BlueMaria.StaticFunction;
using BlueMaria.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlueMaria.View
{
    /// <summary>
    /// Interaction logic for setting_Page.xaml
    /// </summary>
    public partial class SettingPage : Page
    {
        MainWindow _main;

        static string[] curretsetting;

        public SettingPage(MainWindow Main)
        {
            InitializeComponent();
            _main = Main;
            curretsetting = new string[1];
            GeneralPage objgeneralpge = new GeneralPage();
            frmesettigns.Navigate(objgeneralpge);
            b1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
            objgeneralpge = null;
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            b1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
            b2.Background = new SolidColorBrush(Colors.Transparent);
            b3.Background = new SolidColorBrush(Colors.Transparent);
            GeneralPage objgeneralpge = new GeneralPage();
            frmesettigns.Navigate(objgeneralpge);
            objgeneralpge = null;
            curretsetting[0] = "b1";
        }
        private void Border_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            b2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
            b1.Background = new SolidColorBrush(Colors.Transparent);
            b3.Background = new SolidColorBrush(Colors.Transparent);
            AudioPage objaudioPag = new AudioPage();
            frmesettigns.Navigate(objaudioPag);
            objaudioPag = null;
            curretsetting[0] = "b2";
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            var screen = WpfScreen.GetScreenFrom(_main);
            var minLeft = (int)(screen.DeviceBounds.Width - MainViewModel.DefaultWidth);
            var minTop = (int)(screen.DeviceBounds.Height - MainViewModel.DefaultHeight);

            SettingPageViewModel vm = (SettingPageViewModel)this.DataContext;
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
            LocalSettings.LoadMainPage?.Invoke(this, EventArgs.Empty);
            LocalSettings.PasswrdLogout?.Invoke(this, EventArgs.Empty);
        }

        private void error_Initialized(object sender, EventArgs e)
        {
            //MessageBoxResult result = System.Windows.MessageBox.Show("Something went wrong" + "\n" + "Please Click 'Yes' if you want report this issue.", "Blue-Maria", MessageBoxButton.YesNo, MessageBoxImage.Error);
            //if (result == MessageBoxResult.Yes)
            //{
            //    System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=37");
            //}
        }

        private void b3_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            b3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
            b1.Background = new SolidColorBrush(Colors.Transparent);
            b2.Background = new SolidColorBrush(Colors.Transparent);
            AboutPage objabtpge= new AboutPage();
            frmesettigns.Navigate(objabtpge);
            objabtpge = null;
            curretsetting[0] = "b3";
        }



        private void b1_IsMouseCaptureWithinChanged(object sender, System.Windows.Input.MouseEventArgs e)
        {
            b1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD2E5FF"));
            b2.Background = new SolidColorBrush(Colors.Transparent);
            b3.Background = new SolidColorBrush(Colors.Transparent);
            var r = curretsetting[0];   
                   
            switch(r)
            {
                case "b1": b1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
                    break;
                case "b2":
                    b2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
                    break;
                case "b3":
                    b3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));                   
                    break;
            }
           
        }

        private void b2_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {      
            b2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD2E5FF"));
            b1.Background = new SolidColorBrush(Colors.Transparent);
            b3.Background = new SolidColorBrush(Colors.Transparent);
            var r = curretsetting[0];
            if (r == null)
            {
                r = "b1";
            }
            switch (r)
            {
                case "b1":
                    b1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
                    break;
                case "b2":
                    b2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
                    break;
                case "b3":
                    b3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
                    break;
            }
        }

        private void b3_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            b3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD2E5FF"));
            b2.Background = new SolidColorBrush(Colors.Transparent);
            b1.Background = new SolidColorBrush(Colors.Transparent);
            var r = curretsetting[0];
            if (r == null)
            {
                r = "b1";
            }
            switch ( r)
            {
                case "b1":
                    b1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
                    break;
                case "b2":
                    b2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
                    break;
                case "b3":
                    b3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
                    break;
            }
        }


      

        private void Border_PreviewMouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            b1.Background = new SolidColorBrush(Colors.Transparent);
            b2.Background = new SolidColorBrush(Colors.Transparent);
            b3.Background = new SolidColorBrush(Colors.Transparent);
            var r = curretsetting[0];
            if (r == null)
            {
                r = "b1";
            }
            switch (r)
            {
                case "b1":
                    b1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
                    break;
                case "b2":
                    b2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
                    break;
                case "b3":
                    b3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA0C8FF"));
                    break;
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
