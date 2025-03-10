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
    /// Interaction logic for KernelChange.xaml
    /// </summary>
    public partial class KernelChange : Window
    {
        public int row {  get; set; }
        public int column { get; set; }
        public double offset {  get; set; }
        public double divisor {  get; set; }

        public int anchor { get; set; }

        public KernelChange()
        {
            InitializeComponent();
            rowNrInput.Text = row.ToString();
            columnNrInput.Text = column.ToString();
            divisorInput.Text = divisor.ToString();
            offsetInput.Text = offset.ToString();
        }

        public void SaveChanges(object sender, RoutedEventArgs e)
        {
            int tmp;
            double tmpd;
            tmp = int.Parse(rowNrInput.Text);
            if (tmp >= 1 && tmp <= 9)
                row = tmp;
            tmp = int.Parse(columnNrInput.Text);
            if (tmp >= 1 && tmp <= 9)
                column = tmp;
            tmpd = double.Parse(divisorInput.Text);
            if(tmpd != 0)
                divisor = tmpd;
            offset = double.Parse(offsetInput.Text);
            anchor = int.Parse(anchorInput.Text);
            Close();
        }
    }
}
