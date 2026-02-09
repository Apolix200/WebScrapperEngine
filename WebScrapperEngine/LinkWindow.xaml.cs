using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WebScrapperEngine.Action;
using WebScrapperEngine.Entity;
using static System.Net.Mime.MediaTypeNames;

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

        public ObservableCollection<Creation> AllCreations { get; set;  }
            = new ObservableCollection<Creation>();
        public ObservableCollection<Creation> RecommendCreations { get; set; }
            = new ObservableCollection<Creation>();

        public LinkWindow(Bookmark bookmark, MainWindow mainWindow)
        {
            context = new Context();
            this.mainWindow = mainWindow;
            DataContext = this;

            selectedBookmark = bookmark;

            InitializeComponent();
        }

        private void LinkBookmark_Loaded(object sender, RoutedEventArgs e)
        {
            var currentBookmark = context.Bookmarks.Include("BookmarkCreations").FirstOrDefault(b => b.BookmarkId == selectedBookmark.BookmarkId);

            AllCreations = new ObservableCollection<Creation>(context.Creations.Where(creation =>
                creation.CreationType == (int)currentBookmark.Creation.CreationType
                && creation.SiteName != (int)currentBookmark.Creation.SiteName).ToList());

            RecommendCreations = new ObservableCollection<Creation>();
            foreach (Creation creation in context.Creations.Where(creation =>
                 creation.CreationType == (int)currentBookmark.Creation.CreationType
                 && creation.SiteName != (int)currentBookmark.Creation.SiteName).ToList())
            {
                if (StringSimilarity.CompareStrings(creation.Title, currentBookmark.Creation.Title) >= 0.5
                    || currentBookmark.BookmarkCreations.Any(bc => bc.CreationId == creation.CreationId))
                {
                    RecommendCreations.Add(creation);
                }
            }

            allCreationsDataGrid.ItemsSource = AllCreations;
            recommendCreationsDataGrid.ItemsSource = RecommendCreations;
        }

        private void confirmLink_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCreation != null && selectedBookmark != null)
            {
                var bookmark = context.Bookmarks.Where(b => b.BookmarkId == selectedBookmark.BookmarkId).FirstOrDefault();

                if(!bookmark.BookmarkCreations.Any(a => a.CreationId == selectedCreation.CreationId))
                {
                    bookmark.BookmarkCreations.Add(new BookmarkCreation
                    {
                        BookmarkId = bookmark.BookmarkId,
                        CreationId = selectedCreation.CreationId
                    });
                    if (!RecommendCreations.Any(c => c.CreationId == selectedCreation.CreationId))
                    {
                        RecommendCreations.Add(selectedCreation);
                    }
                }

                context.SaveChanges();
                this.recommendCreationsDataGrid.Items.Refresh();
                mainWindow.bookmarksDataGrid.Items.Refresh();
            }
        }

        private void confirmUnlink_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCreation != null && selectedBookmark != null)
            {
                var bookmark = context.Bookmarks.Where(b => b.BookmarkId == selectedBookmark.BookmarkId).FirstOrDefault();

                if (bookmark != null)
                {
                    var linkToRemove = bookmark.BookmarkCreations
                        .FirstOrDefault(bc => bc.CreationId == selectedCreation.CreationId);

                    if (linkToRemove != null)
                    {
                        bookmark.BookmarkCreations.Remove(linkToRemove);
                        if (StringSimilarity.CompareStrings(selectedCreation.Title, selectedBookmark.Creation.Title) < 0.5)
                        {
                            RecommendCreations.Remove(selectedCreation);
                        }
                       
                    }
                }

                context.SaveChanges();
                this.recommendCreationsDataGrid.Items.Refresh();
                mainWindow.bookmarksDataGrid.Items.Refresh();
            }
        }

        private void searchFiltered_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string filterText = Regex.Replace(searchDataGridTextBox.Text.ToLower(), @"[^0-9a-zA-Z]+", "");

            List<Creation> filteredCretions = new List<Creation>();
            if (filterText != "")
            {
                filteredCretions = AllCreations.Where(creation =>
                StringSimilarity.CompareStrings(creation.Title.ToLower(), filterText) >= 0.8).ToList();
            }

            if(filteredCretions.Count <= 0)
            {
                filteredCretions = AllCreations.Where(creation => creation.Title.ToLower().Contains(filterText)).ToList();
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

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void selectAllCreation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGrid dataGrid = sender as DataGrid;
                if (dataGrid != null && dataGrid.SelectedItems != null)
                {
                    foreach (Creation creation in dataGrid.SelectedItems)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = creation.Link,
                            UseShellExecute = true
                        });
                    }
                }
            }
        }
        private void selectRecommendCreation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGrid dataGrid = sender as DataGrid;
                if (dataGrid != null && dataGrid.SelectedItems != null)
                {
                    foreach (Creation creation in dataGrid.SelectedItems)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = creation.Link,
                            UseShellExecute = true
                        });
                    }
                }
            }
        }

    }
}
