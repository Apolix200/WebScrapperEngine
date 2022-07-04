using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebScrapperEngine.Action;
using WebScrapperEngine.Entity;
using WebScrapperEngine.Scrapper;

namespace WebScrapperEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Context context;

        private DonghuaScrapper donghuaScrapper;
        private AnimeScrapper animeScrapper;
        private MangaScrapper mangaScrapper;
        //private GameScrapper gameScrapper;
        //private VnScrapper vnScrapper;

        private Bookmarker bookmarker;

        private Filter filter;
        private DatasourceFilter datasourceFilter;

        private List<Creation> creations = new List<Creation>();
        private List<Bookmark> bookmarks = new List<Bookmark>();
        private List<Episode> episodes = new List<Episode>();

        private BackgroundWorker donghuaWorker = new BackgroundWorker();
        private BackgroundWorker animeWorker = new BackgroundWorker();
        private BackgroundWorker mangaWorker = new BackgroundWorker();
        private BackgroundWorker gameWorker = new BackgroundWorker();
        private BackgroundWorker vnWorker = new BackgroundWorker();

        private BrushConverter bc = new BrushConverter();

        public MainWindow()
        {
            InitializeComponent();

            context = new Context();

            donghuaScrapper = new DonghuaScrapper();
            animeScrapper = new AnimeScrapper();
            mangaScrapper = new MangaScrapper();

            bookmarker = new Bookmarker();

            filter = Filter.All;
            datasourceFilter = DatasourceFilter.Creations;

            LoadCreationsAndEpisodes();

            donghuaWorker.DoWork += DonghuaWork;
            donghuaWorker.RunWorkerCompleted += DonghuaWorkCompleted;

            animeWorker.DoWork += AnimeWork;
            animeWorker.RunWorkerCompleted += AnimeWorkCompleted;

            mangaWorker.DoWork += MangaWork;
            mangaWorker.RunWorkerCompleted += MangaWorkCompleted;

            gameWorker.DoWork += GameWork;
            gameWorker.RunWorkerCompleted += GameWorkCompleted;

            vnWorker.DoWork += vnWork;
            vnWorker.RunWorkerCompleted += vnWorkCompleted;
        }

        #region BackgroundWorkers
        private void WebScrapper_Loaded(object sender, RoutedEventArgs e)
        {
            donghuaWorker.RunWorkerAsync();
            animeWorker.RunWorkerAsync();
            mangaWorker.RunWorkerAsync();
            gameWorker.RunWorkerAsync();
            vnWorker.RunWorkerAsync();
        }
        private void DonghuaWork(object sender, DoWorkEventArgs e)
        {
            donghuaScrapper.SearchNaruldonghuaSite();
            donghuaScrapper.SearchAnimexinSite();
        }
        private void AnimeWork(object sender, DoWorkEventArgs e)
        {
            animeScrapper.SearchKickassSite();
        }
        private void MangaWork(object sender, DoWorkEventArgs e)
        {
            mangaScrapper.SearchMangaseeSite();
        }
        private void GameWork(object sender, DoWorkEventArgs e)
        {
        }
        private void vnWork(object sender, DoWorkEventArgs e)
        {
        }
        private void DonghuaWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadCreationsAndEpisodes();

            donghuaFilterDotImage.Visibility = Visibility.Hidden;
        }
        private void AnimeWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadCreationsAndEpisodes();

            animeFilterDotImage.Visibility = Visibility.Hidden;
        }
        private void MangaWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadCreationsAndEpisodes();

            mangaFilterDotImage.Visibility = Visibility.Hidden;
        }
        private void GameWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            gameFilterDotImage.Visibility = Visibility.Hidden;
        }
        private void vnWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            vnFilterDotImage.Visibility = Visibility.Hidden;
        }
        #endregion
        #region GridLoadFilters        
        private void FilterButtonFocus()
        {
            foreach (Button btn in sideMenuFilter.Children)
            {
                btn.Background = this.Resources["DarkBrush"] as Brush;
            }

            switch (filter)
            {
                case Filter.All:
                    allFilterButton.Background = this.Resources["DarkPressedBrush"] as Brush;
                    break;
                case Filter.Donghua:
                    donghuaFilterButton.Background = this.Resources["DarkPressedBrush"] as Brush;
                    break;
                case Filter.Anime:
                    animeFilterButton.Background = this.Resources["DarkPressedBrush"] as Brush;
                    break;
                case Filter.Manga:
                    mangaFilterButton.Background = this.Resources["DarkPressedBrush"] as Brush;
                    break;
                case Filter.Game:
                    gameFilterButton.Background = this.Resources["DarkPressedBrush"] as Brush;
                    break;
                case Filter.Vn:
                    vnFilterButton.Background = this.Resources["DarkPressedBrush"] as Brush;
                    break;
                default:
                    break;
            }
        }
        private void DatasourceFilterButtonFocus()
        {
            creationsFilterButton.Background = this.Resources["DarkBrush"] as Brush;
            episodesFilterButton.Background = this.Resources["DarkBrush"] as Brush;

            switch (datasourceFilter)
            {
                case DatasourceFilter.Creations:
                    creationsFilterButton.Background = this.Resources["DarkPressedBrush"] as Brush;
                    break;
                case DatasourceFilter.Episodes:
                    episodesFilterButton.Background = this.Resources["DarkPressedBrush"] as Brush;
                    break;
                default:
                    break;
            }
        }
        private void LoadCreationsAndEpisodes()
        {
            creations = new List<Creation>();
            bookmarks = new List<Bookmark>();
            episodes = new List<Episode>();

            switch (filter)
            {
                case Filter.All:
                    creations = context.Creations.ToList();
                    episodes = context.Episodes.ToList();
                    bookmarks = context.Bookmarks.ToList();
                    break;
                case Filter.Donghua:
                    creations = context.Creations.Where(c => c.CreationType == (int)CreationType.Donghua).ToList();
                    bookmarks = context.Bookmarks.Where(b => b.Creation.CreationType == (int)CreationType.Donghua).ToList();
                    episodes = context.Episodes.Where(e => e.Bookmark.Creation.CreationType == (int)CreationType.Donghua).ToList();
                    break;
                case Filter.Anime:
                    creations = context.Creations.Where(c => c.CreationType == (int)CreationType.Anime).ToList();
                    bookmarks = context.Bookmarks.Where(b => b.Creation.CreationType == (int)CreationType.Anime).ToList();
                    episodes = context.Episodes.Where(e => e.Bookmark.Creation.CreationType == (int)CreationType.Anime).ToList();
                    break;
                case Filter.Manga:
                    creations = context.Creations.Where(c => c.CreationType == (int)CreationType.Anime).ToList();
                    bookmarks = context.Bookmarks.Where(b => b.Creation.CreationType == (int)CreationType.Anime).ToList();
                    episodes = context.Episodes.Where(e => e.Bookmark.Creation.CreationType == (int)CreationType.Anime).ToList();
                    break;
                case Filter.Game:
                    creations = context.Creations.Where(c => c.CreationType == (int)CreationType.Game).ToList();
                    bookmarks = context.Bookmarks.Where(b => b.Creation.CreationType == (int)CreationType.Game).ToList();
                    episodes = context.Episodes.Where(e => e.Bookmark.Creation.CreationType == (int)CreationType.Game).ToList();
                    break;
                case Filter.Vn:
                    creations = context.Creations.Where(c => c.CreationType == (int)CreationType.Vn).ToList();
                    bookmarks = context.Bookmarks.Where(b => b.Creation.CreationType == (int)CreationType.Vn).ToList();
                    episodes = context.Episodes.Where(e => e.Bookmark.Creation.CreationType == (int)CreationType.Vn).ToList();
                    break;
                default:
                    break;
            }

            switch (datasourceFilter)
            {
                case DatasourceFilter.Creations:
                    creationsDataGrid.Visibility = Visibility.Visible;
                    episodesDataGrid.Visibility = Visibility.Hidden;
                    break;
                case DatasourceFilter.Episodes:
                    creationsDataGrid.Visibility = Visibility.Hidden;
                    episodesDataGrid.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }

            creationsDataGrid.ItemsSource = creations;
            bookmarksDataGrid.ItemsSource = bookmarks;
            episodesDataGrid.ItemsSource = episodes;

            FilterButtonFocus();
            DatasourceFilterButtonFocus();
        }
        private void LoadAllCreation_Click(object sender, RoutedEventArgs e)
        {
            filter = Filter.All;
            LoadCreationsAndEpisodes();
        }
        private void LoadDonghuaCreation_Click(object sender, RoutedEventArgs e)
        {
            filter = Filter.Donghua;
            LoadCreationsAndEpisodes();
        }
        private void LoadAnimeCreation_Click(object sender, RoutedEventArgs e)
        {
            filter = Filter.Anime;
            LoadCreationsAndEpisodes();
        }
        private void LoadMangaCreation_Click(object sender, RoutedEventArgs e)
        {
            filter = Filter.Manga;
            LoadCreationsAndEpisodes();
        }
        private void LoadGameCreation_Click(object sender, RoutedEventArgs e)
        {
            filter = Filter.Game;
            LoadCreationsAndEpisodes();
        }
        private void LoadVnCreation_Click(object sender, RoutedEventArgs e)
        {
            filter = Filter.Vn;
            LoadCreationsAndEpisodes();
        }
        private void LoadDatasourceCreations_Click(object sender, RoutedEventArgs e)
        {
            datasourceFilter = DatasourceFilter.Creations;
            LoadCreationsAndEpisodes();
        }
        private void LoadDatasourceEpisodes_Click(object sender, RoutedEventArgs e)
        {
            datasourceFilter = DatasourceFilter.Episodes;
            LoadCreationsAndEpisodes();
        }
        private void FilterButton_MouseEnter(object sender, MouseEventArgs e)
        {
            sender.GetType().GetProperty("Background").SetValue(sender, this.Resources["HighlightBrush"] as Brush);
        }
        private void FilterButton_MouseLeave(object sender, MouseEventArgs e)
        {
            FilterButtonFocus();
        }
        private void DatasourceFilterButton_MouseEnter(object sender, MouseEventArgs e)
        {
            sender.GetType().GetProperty("Background").SetValue(sender, this.Resources["HighlightBrush"] as Brush);
        }
        private void DatasourceFilterButton_MouseLeave(object sender, MouseEventArgs e)
        {
            DatasourceFilterButtonFocus();
        }


        #endregion
        #region ToolbarButtons
        private void bookmarksDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void seenAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void changeStatus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddPresetButton_Click(object sender, RoutedEventArgs e)
        {
            var addButton = sender as FrameworkElement;
            if (addButton != null)
            {
                addButton.ContextMenu.IsOpen = true;
            }
        }

        private void addBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (creationsDataGrid != null && creationsDataGrid.SelectedItems != null)
            {
                foreach (Creation creation in creationsDataGrid.SelectedItems)
                {
                    bookmarker.BookmarkCreation(creation);
                }

                LoadCreationsAndEpisodes();
            }
        }

        private void deleteBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (bookmarksDataGrid != null && bookmarksDataGrid.SelectedItems != null)
            {
                foreach (Bookmark bookmark in bookmarksDataGrid.SelectedItems)
                {
                    bookmarker.DeleteBookmarkCreation(bookmark);
                }

                LoadCreationsAndEpisodes();
            }
        }

        private void searchFiltered_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (datasourceFilter)
            {
                case DatasourceFilter.Creations:
                    List<Creation> filteredCreations = creations.Where(creation => creation.Title.ToLower()
                    .Contains(Regex.Replace(searchDataGridTextBox.Text.ToLower(), @"[^0-9a-zA-Z]+", ""))).ToList();

                    creationsDataGrid.ItemsSource = filteredCreations;
                    break;
                case DatasourceFilter.Episodes:
                    List<Episode> filteredEpisodes = episodes.Where(episode => episode.Bookmark.Creation.Title.ToLower()
                    .Contains(Regex.Replace(searchDataGridTextBox.Text.ToLower(), @"[^0-9a-zA-Z]+", ""))).ToList();

                    episodesDataGrid.ItemsSource = filteredEpisodes;
                    break;
                default:
                    break;
            }
        }

        private void creationsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
    #endregion

    public enum Filter
    {
        All,
        Donghua,
        Anime,
        Manga,
        Game,
        Vn
    }

    public enum DatasourceFilter
    {
        Creations,
        Episodes
    }

    public enum NewStatus
    {
        New,
        Seen
    }

    public enum CreationType
    {
        Donghua,
        Anime,
        Manga,
        Game,
        Vn
    }

    public enum SiteName
    {
        Naruldonghua,
        Animexin,
        Kickassanime,
        Mangasee
    }
    public enum WatchStatus
    {
        NextWatch,
        LatestWatch,
        NeedToWatch,
        AlreadyWatch
    }
}
