using System;
using System.Collections.Generic;
using System.IO;
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

namespace PrintingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string path;
        List<ItemModel> Items = new List<ItemModel>();
        int Paid, Gross;

        public MainWindow()
        {
            InitializeComponent();

            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/ItemList.txt";

            if (File.Exists(path))
            {
                List<string> lines = File.ReadLines(path).ToList();

                ItemName.ItemsSource = lines;
            }
            else
            {
                MessageBox.Show("Please Create Item File 'ItemList.txt' In My Documents");
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Items.Add(new ItemModel
            {
                Sr = Items.Count + 1,
                ItemName = ItemName.SelectedItem.ToString(),
                Qty = Convert.ToInt32(qty.Text),
                Rate = Convert.ToInt32(rate.Text)
            });

            List.ItemsSource = Items;
            List.Items.Refresh();
            Gross = Items.Sum(x => x.Total);


            units.Text = Items.Count.ToString();
            gross.Text = Gross.ToString();
            payable.Text = Gross.ToString();

            ItemName.SelectedItem = null;
            qty.Text = null;
            rate.Text = null;

            ItemName.Focus();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {

        }

        private void print_Click(object sender, RoutedEventArgs e)
        {
            if (Items.Count == 0) MessageBox.Show("Please add some Items");
            else
            {
                GetPrint(Items, Paid);
            }
        }
        private void paidtb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (paidtb.Text != null && paidtb.Text != "")
            {
                Paid = Convert.ToInt32(paidtb.Text);

                if (Paid > 0)
                {
                    paid.Text = Paid.ToString();
                    balance.Text = (Paid - Gross).ToString();
                }
            }
        }

        void GetPrint(List<ItemModel> items, int paidBill)
        {
            PrintDialog printDlg = new PrintDialog();

            FlowDocument doc = new FlowDocument();
            doc.PageWidth = printDlg.PrintableAreaWidth;
            doc.PagePadding = new Thickness(20, 70, 0, 70);
            doc.ColumnGap = 0;
            doc.ColumnWidth = doc.PageWidth - doc.ColumnGap - doc.PagePadding.Left - doc.PagePadding.Right;

            BlockUIContainer uIContainer = new BlockUIContainer();

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(@"pack://application:,,,/Assests/logo.png");
            bitmap.EndInit();

            Image image = new Image();
            image.Source = bitmap;
            image.Height = 100;
            image.Width = 100;
            image.HorizontalAlignment = HorizontalAlignment.Left;
            image.Margin = new Thickness(70, 0, 0, 0);

            uIContainer.Child = image;

            doc.Blocks.Add(uIContainer);

            Paragraph p1 = new Paragraph();

            StringBuilder builder = new StringBuilder();

            builder.Append(String.Format("{0,-11} {1,-9} {2,-8} {3,-10} \n\n", "Invoice # ", DateTime.Now.ToString("ddMMyyyyHHmm"), "", ""));
            builder.Append(String.Format("{0,-7} {1,-13} {2,-7} {3,-11} \n", "Date : ", DateTime.Now.ToShortDateString(), "Time : ", DateTime.Now.ToShortTimeString()));

            builder.Append("\n----------------------------------------\n");
            builder.Append(String.Format("{0,-3} {1,-17} {2,-4} {3,-6} {4,-6} \n", "Sr#", "ItemName", "Qty", "Rate", "Total"));
            builder.Append("----------------------------------------\n");

            foreach (var s in items)
            {
                builder.Append(String.Format("{0,-3} {1,-17} {2,-4} {3,-6} {4,-6} \n", s.Sr, s.ItemName, s.Qty, s.Rate, s.Total));
            }

            builder.Append("----------------------------------------\n");
            builder.Append(String.Format("Units {0,-12} Gross Total :  {1,-7}  \n", items.Count, items.Sum(x => x.Total)));
            builder.Append("----------------------------------------\n");
            builder.Append(String.Format("{0,33} {1,-7}  \n", "Amount Payable : ", items.Sum(x => x.Total)));
            builder.Append(String.Format("{0,33} {1,-7}  \n", "Amount Paid : ", paidBill));
            builder.Append(String.Format("{0,33} {1,-7}  \n", "Balance : ", paidBill - items.Sum(x => x.Total)));
            builder.Append("----------------------------------------\n");

            p1.FontSize = 11;
            p1.FontWeight = FontWeights.Black;
            p1.FontFamily = new FontFamily("Courier New");

            p1.Inlines.Add(builder.ToString());

            doc.Blocks.Add(p1);

            IDocumentPaginatorSource idpSource;

            idpSource = doc;

            if (printDlg.ShowDialog() == true)
                printDlg.PrintDocument(idpSource.DocumentPaginator, "Bill");
        }

    }

    public class ItemModel
    {
        public int Sr { get; set; }
        public string ItemName { get; set; }
        public int Qty { get; set; }
        public int Rate { get; set; }
        public int Total { get { return Qty * Rate; } }
    }
}
