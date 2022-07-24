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
        private MainWindow mainWindow;

        private Bookmark selectedBookmark;
        private Creation selectedCreation;

        private List<Creation> allCreations;
        private List<Creation> recommendCreations;

        public int? ConnectedId { get; set; }

        public LinkWindow(Bookmark bookmark, MainWindow mainWindow)
        {
            context = new Context();
            this.mainWindow = mainWindow;
            DataContext = this;

            selectedBookmark = bookmark;
            ConnectedId = bookmark.ConnectedId;

            allCreations = context.Creations.Where(creation =>
            creation.CreationType == (int)bookmark.Creation.CreationType
            && creation.SiteName != (int)bookmark.Creation.SiteName).ToList();

            recommendCreations = new List<Creation>();

            foreach (Creation creation in context.Creations.Where(creation =>
            creation.CreationType == (int)bookmark.Creation.CreationType
            && creation.SiteName != (int)bookmark.Creation.SiteName).ToList())
            {
                if (StringSimilarity.compareStrings(creation.Title, bookmark.Creation.Title) >= 0.5)
                {
                    recommendCreations.Add(creation);
                }
            }

            InitializeComponent();
        }

        private void LinkBookmark_Loaded(object sender, RoutedEventArgs e)
        {
            allCreationsDataGrid.ItemsSource = allCreations;
            recommendCreationsDataGrid.ItemsSource = recommendCreations;
        }

        private void confirmLink_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCreation != null)
            {
                context.Bookmarks.Where(bookmark => bookmark.BookmarkId == selectedBookmark.BookmarkId).FirstOrDefault().ConnectedId =
                selectedCreation.CreationId;
            }
            context.SaveChanges();

            mainWindow.bookmarksDataGrid.Items.Refresh();
            this.Close();
        }

        private void searchFiltered_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string filterText = Regex.Replace(searchDataGridTextBox.Text.ToLower(), @"[^0-9a-zA-Z]+", "");

            List<Creation> filteredCretions = new List<Creation>();
            if (filterText != "")
            {
                filteredCretions = allCreations.Where(creation =>
                StringSimilarity.compareStrings(creation.Title.ToLower(), filterText) >= 0.8).ToList();
            }

            if(filteredCretions.Count <= 0)
            {
                filteredCretions = allCreations.Where(creation => creation.Title.ToLower().Contains(filterText)).ToList();
            }


            allCreationsDataGrid.ItemsSource = filteredCretions;
        }

        private void selectAllCreation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid != null && dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count > 0)
            {
                selectedCreation = (Creation)dataGrid.SelectedItems[0];
            }       
            recommendCreationsDataGrid.UnselectAll();
        }

        private void selectRecommendCreation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid != null && dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count > 0)
            {
                selectedCreation = (Creation)dataGrid.SelectedItems[0];
            }
            allCreationsDataGrid.UnselectAll();
        }
    }
}
