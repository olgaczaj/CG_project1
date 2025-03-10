using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Drawing;
using Microsoft.Win32;
using System.CodeDom;
namespace CG_project1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Grid convMatrixGrid = new Grid();
            for (int i = 0; i < 9; i++)
            {
                convMatrixGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < 9; i++)
            {
                convMatrixGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < 81; i++)
            {
                System.Windows.Controls.TextBox t = new System.Windows.Controls.TextBox();
                t.Name = $"text{i}";
                t.Height = 15; t.Width = 15;
                t.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 100, 100, 100));
                t.IsEnabled = true;
                t.Text = "0";
                t.FontSize = 10;
                if (i == 40)
                {
                    t.BorderBrush = System.Windows.Media.Brushes.Red;
                    t.BorderThickness = new Thickness(2.0);
                    t.Text = "1";
                }
                Grid.SetColumn(t, i / 9);
                Grid.SetRow(t, i % 9);
                convMatrixGrid.Children.Add(t);
            }
            convFilters.Content = convMatrixGrid;
            kernel = new Kernel(40, 9, 9);
            conFiltersDictionary = new Dictionary<string, Kernel>
            {
                { "none", new Kernel() },
                {"Blur", new Kernel(4, 3, 3, 1, 0, "blur") },
                {"Gaussian Blur", new Kernel(4, 3, 3, 1, 0, "gauss") },
                {"Sharpen", new Kernel(4, 3, 3, 1, 0, "sharpen") },
                {"Edge Detection", new Kernel(4, 3, 3, 1, 0, "edge") },
                {"Emboss", new Kernel(4, 3, 3, 1, 0, "emboss") }
            };
            convFilterChooser.ItemsSource = conFiltersDictionary.Keys;
            convFilterChooser.SelectionChanged += ChooseKernel;

            dither = new Dither();
            cq = new ColorQuantization();

            ditherNr.DataContext = dither;
            colorQNr.DataContext = cq;
        }

        public WriteableBitmap wBit { get; set; }
        private BitmapImage iBit { get; set; }

        public Kernel kernel { get; set; }

        public Dither dither { get; set; }

        public ColorQuantization cq { get; set; }

        private Dictionary<string, Kernel> conFiltersDictionary {  get; set; }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFileName = dlg.FileName;
                iBit = new BitmapImage();
                iBit.BeginInit();
                iBit.UriSource = new Uri(selectedFileName);
                iBit.EndInit();
                originalPicture.Source = iBit;
                wBit = new WriteableBitmap(iBit);
                modifiedPicture.Source = wBit;
            }
        }

        private void SavePicture(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;
            BitmapEncoder en = new JpegBitmapEncoder();
            en.Frames.Add(BitmapFrame.Create(wBit));
            dlg.ShowDialog();
            string filePath = dlg.FileName;
            en.Save(new FileStream(filePath, FileMode.Create));
        }

        private void Inversion(object sender, RoutedEventArgs e)
        {
            Filter.Apply(0, wBit, this);
        }

        private void GammaCorrection(object sender, RoutedEventArgs e)
        {
            Filter.Apply(2, wBit, this);
        }

        private void ContrastEnhancement(object sender, RoutedEventArgs e)
        {
            Filter.Apply(3, wBit, this);
        }

        private void ApplyCFilters(object sender, RoutedEventArgs e)
        {
            Filter.Apply(4, wBit, this);
        }

        private void Median(object sender, RoutedEventArgs e)
        {
            Filter.Apply(5, wBit, this);
        }

        private void Greyscale(object sender, RoutedEventArgs e)
        {
            Filter.Apply(6, wBit, this);
        }

        private void Button_Restore(object sender, RoutedEventArgs e)
        {
            modifiedPicture.Source = iBit;
            wBit = new WriteableBitmap(iBit);
        }

        private void Brightness(object sender, RoutedEventArgs e)
        {
            Filter.Apply(1, wBit, this);
        }

        private void Dithering(object sender, RoutedEventArgs e)
        {
            Filter.Apply(7, wBit, this);
        }

        private void ColorQuant(object sender, RoutedEventArgs e)
        {
            Filter.Apply(8, wBit, this);
        }

        private void ChooseKernel (object sender, RoutedEventArgs e)
        {
            kernel = conFiltersDictionary[convFilterChooser.SelectedItem as string];
            Grid convFilterMatrix = convFilters.Content as Grid;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    System.Windows.Controls.TextBox t = convFilterMatrix.Children[i*9+j] as System.Windows.Controls.TextBox;
                    if (i >= kernel.height || j >= kernel.width)
                    {
                        t.Text = "";
                        t.IsEnabled = false;
                        t.Background = System.Windows.Media.Brushes.White;
                    }
                    else
                    {
                        t.Text = kernel.data[i*kernel.width + j].ToString();
                        t.IsEnabled = true;
                        t.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 100, 100, 100));
                    }
                    if(kernel.anchor == i*kernel.width + j)
                    {
                        t.BorderBrush = System.Windows.Media.Brushes.Red;
                        t.BorderThickness = new Thickness(2.0);
                    }
                    else
                    {
                        t.BorderBrush = System.Windows.Media.Brushes.Black;
                        t.BorderThickness = new Thickness(1.0);
                    }
                }
            }
        }

        private void SaveKernel (object sender, RoutedEventArgs e)
        {
            Grid convFilterMatrix = convFilters.Content as Grid;
            int[] input = new int[kernel.width*kernel.height];
            for(int i = 0; i < kernel.height; i++)
            {
                for(int j = 0; j < kernel.width; j++)
                {
                    input[i * kernel.width + j] = int.Parse((convFilterMatrix.Children[j * 9 + i] as System.Windows.Controls.TextBox).Text);
                }
            }
            KernelName name = new KernelName();
            name.ShowDialog();
            conFiltersDictionary.Add(name.name, new Kernel(input, kernel.width, kernel.height, kernel.width, kernel.height, kernel.anchor));
            convFilterChooser.ItemsSource = conFiltersDictionary.Keys;
        }

        private void ChangeKernel(object sender, RoutedEventArgs e)
        {
            KernelChange win = new KernelChange();
            win.ShowDialog();
            if (win.column != kernel.width || win.row != kernel.height)
            {
                kernel = new Kernel(win.anchor, win.column, win.row);
                Grid convFilterMatrix = convFilters.Content as Grid;
                int ind  = 0;
                for (int i = 0; i < 81; i++)
                {
                    System.Windows.Controls.TextBox t = convFilterMatrix.Children[i] as System.Windows.Controls.TextBox;
                    
                        t.BorderBrush = System.Windows.Media.Brushes.Black;
                        t.BorderThickness = new Thickness(1.0);
                    if (i % 9 >= kernel.height || (int)(i / 9) >= kernel.width)
                    {
                        t.IsEnabled = false;
                        t.Background = System.Windows.Media.Brushes.White;
                        t.Text = "";
                    }
                    else
                    {
                        t.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 100, 100, 100));
                        t.IsEnabled = true;
                        if (t.Text == "")
                            t.Text = "0";
                        if (ind == kernel.anchor)
                        {
                            t.BorderBrush = System.Windows.Media.Brushes.Red;
                            t.BorderThickness = new Thickness(2.0);
                        }
                        ind++;
                    }
                }
            }
            kernel.offset = win.offset;
            kernel.divisor = win.divisor;
        }
    }
}