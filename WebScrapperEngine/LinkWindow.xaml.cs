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
using WebScrapperEngine.Entity;

namespace WebScrapperEngine
{
    /// <summary>
    /// Interaction logic for linkWindow.xaml
    /// </summary>
    public partial class LinkWindow : Window
    {
        private Context context;

        public LinkWindow()
        {
            context = new Context();

            InitializeComponent();
        }

        private void LinkBookmark_Loaded(object sender, RoutedEventArgs e)
        {
            List<Creation> creations = context.Creations.ToList();

            allCreationsDataGrid.ItemsSource = creations;
        }

        private void confirmLink_Click(object sender, RoutedEventArgs e)
        {
            LinkWindow linkWindow = new LinkWindow();

            linkWindow.Show();
        }
    }
}
