using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// Interaction logic for audioPage.xaml
    /// </summary>
    public partial class AudioPage : Page
    {

        public AudioPage()
        {
            InitializeComponent();
        }

        private void Run_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(@"C:\Windows\System32\mmsys.cpl");

        }
    }
}
