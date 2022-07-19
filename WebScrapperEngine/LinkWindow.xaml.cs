using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WebScrapperEngine.Action;
using WebScrapperEngine.Entity;

namespace WebScrapperEngine
{
    /// <summary>
    /// Interaction logic for linkWindow.xaml
    /// </summary>
    public partial class LinkWindow : Window
    {
        private Context context;
        private Bookmark bookmark;

        public LinkWindow(Bookmark bookmark)
        {
            context = new Context();

            this.bookmark = bookmark;

            InitializeComponent();
        }

        private void LinkBookmark_Loaded(object sender, RoutedEventArgs e)
        {
            List<Creation> allCreations = context.Creations.Where(creation =>
            creation.CreationType == (int)bookmark.Creation.CreationType
            && creation.SiteName != (int)bookmark.Creation.SiteName).ToList();

            //List<Creation> recommendCreations = context.Creations.Where(creation =>
            //StringSimilarity.compareStrings(creation.Title, bookmark.Creation.Title) >= 0.5).ToList();

            //double value = StringSimilarity.compareStrings(context.Creations.FirstOrDefault().Title, bookmark.Creation.Title);
            //bool bigger = StringSimilarity.compareStrings(context.Creations.FirstOrDefault().Title, bookmark.Creation.Title) >= 0.5;

            //MessageBox.Show("Higher: " + bigger + ", Value: " + value);

            allCreationsDataGrid.ItemsSource = allCreations;
            //recommendCreationsDataGrid.ItemsSource = recommendCreations;
        }

        private void confirmLink_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void searchFiltered_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            List<Creation> allCreations = context.Creations.Where(creation => creation.Title.ToLower()
            .Contains(Regex.Replace(searchDataGridTextBox.Text.ToLower(), @"[^0-9a-zA-Z]+", ""))).ToList();

            allCreationsDataGrid.ItemsSource = allCreations;
        }
    }
}
