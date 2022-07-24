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
    /// Interaction logic for ConfirmWindow.xaml
    /// </summary>
    public partial class ConfirmWindow : Window
    {
        private Context context;
        private MainWindow mainWindow;

        public bool Confirmation { get; set; }

        public ConfirmWindow(MainWindow mainWindow)
        {
            context = new Context();
            this.mainWindow = mainWindow;

            InitializeComponent();
        }

        private void ConfirmAction_Loaded(object sender, RoutedEventArgs e)
        {
            Confirmation = false;
        }

        private void accept_Click(object sender, RoutedEventArgs e)
        {
            Confirmation = true;
            this.Close();
        }

        private void refuse_Click(object sender, RoutedEventArgs e)
        {
            Confirmation = false;
            this.Close();
        }
    }
}
