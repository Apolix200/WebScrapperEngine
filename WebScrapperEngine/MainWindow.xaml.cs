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

        private Filter filter = Filter.All;
        private DatasourceFilter datasourceFilter = DatasourceFilter.Episodes;

        private DonghuaScrapper donghuaScrapper;
        private AnimeScrapper animeScrapper;
        private MangaScrapper mangaScrapper;
        //private GameScrapper gameScrapper;
        //private ErogeScrapper erogeScrapper;

        private Bookmarker bookmarker;

        private BackgroundWorker donghuaWorker = new BackgroundWorker();
        private BackgroundWorker animeWorker = new BackgroundWorker();
        private BackgroundWorker mangaWorker = new BackgroundWorker();
        private BackgroundWorker gameWorker = new BackgroundWorker();
        private BackgroundWorker erogeWorker = new BackgroundWorker();

        private BrushConverter bc = new BrushConverter();

        public MainWindow()
        {
            InitializeComponent();

            context = new Context();

            donghuaScrapper = new DonghuaScrapper();
            animeScrapper = new AnimeScrapper();
            mangaScrapper = new MangaScrapper();

            bookmarker = new Bookmarker();

            LoadCreationsAndEpisodes();

            donghuaWorker.DoWork += DonghuaWork;
            donghuaWorker.RunWorkerCompleted += DonghuaWorkCompleted;

            animeWorker.DoWork += AnimeWork;
            animeWorker.RunWorkerCompleted += AnimeWorkCompleted;

            mangaWorker.DoWork += MangaWork;
            mangaWorker.RunWorkerCompleted += MangaWorkCompleted;

            gameWorker.DoWork += GameWork;
            gameWorker.RunWorkerCompleted += GameWorkCompleted;

            erogeWorker.DoWork += ErogeWork;
            erogeWorker.RunWorkerCompleted += ErogeWorkCompleted;
        }

        #region BackgroundWorkers
        private void WebScrapper_Loaded(object sender, RoutedEventArgs e)
        {
            donghuaWorker.RunWorkerAsync();
            animeWorker.RunWorkerAsync();
            mangaWorker.RunWorkerAsync();
            gameWorker.RunWorkerAsync();
            erogeWorker.RunWorkerAsync();
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
        private void ErogeWork(object sender, DoWorkEventArgs e)
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
        private void ErogeWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            erogeFilterDotImage.Visibility = Visibility.Hidden;
        }
        #endregion
        #region GridLoadFilters        
        private void FilterButtonFocus()
        {
            allFilterButton.Background = (Brush)bc.ConvertFrom("#274B69");
            donghuaFilterButton.Background = (Brush)bc.ConvertFrom("#274B69");
            animeFilterButton.Background = (Brush)bc.ConvertFrom("#274B69");
            mangaFilterButton.Background = (Brush)bc.ConvertFrom("#274B69");
            gameFilterButton.Background = (Brush)bc.ConvertFrom("#274B69");
            erogeFilterButton.Background = (Brush)bc.ConvertFrom("#274B69");

            switch (filter)
            {
                case Filter.All:
                    allFilterButton.Background = (Brush)bc.ConvertFrom("#85A1C1");
                    break;
                case Filter.Donghua:
                    donghuaFilterButton.Background = (Brush)bc.ConvertFrom("#85A1C1");
                    break;
                case Filter.Anime:
                    animeFilterButton.Background = (Brush)bc.ConvertFrom("#85A1C1");
                    break;
                case Filter.Manga:
                    mangaFilterButton.Background = (Brush)bc.ConvertFrom("#85A1C1");
                    break;
                case Filter.Game:
                    gameFilterButton.Background = (Brush)bc.ConvertFrom("#85A1C1");
                    break;
                case Filter.Eroge:
                    erogeFilterButton.Background = (Brush)bc.ConvertFrom("#85A1C1");
                    break;
                default:
                    break;
            }
        }
        private void DatasourceFilterButtonFocus()
        {
            creationsFilterButton.Background = (Brush)bc.ConvertFrom("#274B69");
            episodesFilterButton.Background = (Brush)bc.ConvertFrom("#274B69");

            switch (datasourceFilter)
            {
                case DatasourceFilter.Creations:
                    creationsFilterButton.Background = (Brush)bc.ConvertFrom("#85A1C1");
                    break;
                case DatasourceFilter.Episodes:
                    episodesFilterButton.Background = (Brush)bc.ConvertFrom("#85A1C1");
                    break;
                default:
                    break;
            }
        }
        private void LoadCreationsAndEpisodes()
        {
            List<Creation> creations = new List<Creation>();
            List<Bookmark> bookmarks = new List<Bookmark>();
            List<Episode> episodes = new List<Episode>();

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
                case Filter.Eroge:
                    creations = context.Creations.Where(c => c.CreationType == (int)CreationType.Eroge).ToList();
                    bookmarks = context.Bookmarks.Where(b => b.Creation.CreationType == (int)CreationType.Eroge).ToList();
                    episodes = context.Episodes.Where(e => e.Bookmark.Creation.CreationType == (int)CreationType.Eroge).ToList();
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
        private void LoadErogeCreation_Click(object sender, RoutedEventArgs e)
        {
            filter = Filter.Eroge;
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
        private void LoadingIcon_MediaEnded(object sender, RoutedEventArgs e)
        {
            sender.GetType().GetProperty("Position").SetValue(sender, new TimeSpan(0, 0, 1));
            sender.GetType().GetMethod("Play");
        }
        private void FilterButton_MouseEnter(object sender, MouseEventArgs e)
        {
            sender.GetType().GetProperty("Background").SetValue(sender, (Brush)bc.ConvertFrom("#85A1C1"));
        }
        private void FilterButton_MouseLeave(object sender, MouseEventArgs e)
        {
            FilterButtonFocus();
        }
        private void DatasourceFilterButton_MouseEnter(object sender, MouseEventArgs e)
        {
            sender.GetType().GetProperty("Background").SetValue(sender, (Brush)bc.ConvertFrom("#85A1C1"));
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
            var filtered = context.Creations.Where(creation => creation.Title.ToLower()
            .Contains(Regex.Replace(searchDataGridTextBox.Text.ToLower(), @"[^0-9a-zA-Z]+", "")));

            creationsDataGrid.ItemsSource = filtered;
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
        Eroge
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
        Eroge
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
