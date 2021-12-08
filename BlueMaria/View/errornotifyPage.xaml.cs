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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlueMaria.View
{
    /// <summary>
    /// Interaction logic for errornotifyPage.xaml
    /// </summary>
    public partial class errornotifyPage : UserControl
    {
        public errornotifyPage()
        {
            InitializeComponent();
        }

        private void Run_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //LocalSettings.CloseError?.Invoke(this, EventArgs.Empty);
            //System.Diagnostics.Process.Start("https://blue-maria.com/?page_id=37"); 
        }

        private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //LocalSettings.CloseError?.Invoke(this, EventArgs.Empty);
        }
    }
}
