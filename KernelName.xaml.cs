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
using System.Windows.Shapes;

namespace CG_project1
{
    /// <summary>
    /// Interaction logic for KernelName.xaml
    /// </summary>
    public partial class KernelName : Window
    {
        public string name;
        public KernelName()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            name = kernelName.Text;
            Close();
        }
    }
}
