using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

        private BrushConverter bc = new BrushConverter();

        private string filterText = "";
        private bool bigImage = false;
        private int bigImageSize = 120;
        private int smallImageSize = 40;
        public int imageSize { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            context = new Context();

            donghuaScrapper = new DonghuaScrapper(this);
            animeScrapper = new AnimeScrapper(this);
            mangaScrapper = new MangaScrapper(this);

            bookmarker = new Bookmarker(this);

            filter = Filter.All;
            datasourceFilter = DatasourceFilter.Episodes;

            imageSize = bigImage ? bigImageSize : smallImageSize;

            LoadCreationsAndEpisodes();

            gameFilterDotImage.Visibility = Visibility.Hidden;
            vnFilterDotImage.Visibility = Visibility.Hidden;
        }
        public string MakeRequest(string requestString, string websiteLink)
        {
            CookieContainer cookieJar = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestString);
            request.CookieContainer = cookieJar;
            request.Accept = @"text/html, application/xhtml+xml, */*";
            request.Referer = @"https://" + websiteLink;
            request.Headers.Add("Accept-Language", "en-GB");
            request.UserAgent = @"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)";
            request.Host = websiteLink;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string htmlString;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                htmlString = reader.ReadToEnd();
            }
            return htmlString;
        }

        public void CorrectWatchStatus(Bookmark bookmark)
        {
            List<Episode> episodes = context.Episodes.Where(n => n.BookmarkId == bookmark.BookmarkId).ToList();

            double? nextEpisode = episodes.FirstOrDefault(n => n.WatchStatus == (int)WatchStatus.NextWatch)?.EpisodeNumber;

            if(nextEpisode == null)
            {
                nextEpisode = episodes.FirstOrDefault(n => n.WatchStatus == (int)WatchStatus.LatestWatch)?.EpisodeNumber + 1;

                if(nextEpisode == null)
                {
                    nextEpisode = episodes.Where(n => n.WatchStatus == (int)WatchStatus.AlreadyWatch).Max(n => n.EpisodeNumber);

                    if (nextEpisode == null)
                    {
                        nextEpisode = 1;
                    }

                }
            }

            foreach (var episode in episodes)
            {
                if (episode.EpisodeNumber > nextEpisode)
                {
                    if(episode.WatchStatus != (int)WatchStatus.NeedToWatch)
                    {
                        context.Episodes.FirstOrDefault(n => n.EpisodeId == episode.EpisodeId).WatchStatus = (int)WatchStatus.NeedToWatch;
                    }
                }
                else if (episode.EpisodeNumber == nextEpisode)
                {
                    if(episode.WatchStatus != (int)WatchStatus.NextWatch)
                    {
                        context.Episodes.FirstOrDefault(n => n.EpisodeId == episode.EpisodeId).WatchStatus = (int)WatchStatus.NextWatch;
                    }        
                }
                else if (episode.EpisodeNumber == nextEpisode - 1)
                {
                    if(episode.WatchStatus != (int)WatchStatus.LatestWatch)
                    {
                        context.Episodes.FirstOrDefault(n => n.EpisodeId == episode.EpisodeId).WatchStatus = (int)WatchStatus.LatestWatch;
                    }
                }
                else
                {
                    if(episode.WatchStatus != (int)WatchStatus.AlreadyWatch)
                    {
                        context.Episodes.FirstOrDefault(n => n.EpisodeId == episode.EpisodeId).WatchStatus = (int)WatchStatus.AlreadyWatch;
                    }    
                }
            }
            context.SaveChanges();
        }

        private void WebScrapper_Loaded(object sender, RoutedEventArgs e)
        {
            donghuaScrapper.RunWorker();
            animeScrapper.RunWorker();
            mangaScrapper.RunWorker();
        }
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
        public void LoadCreationsAndEpisodes()
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
                    creations = context.Creations.Where(c => c.CreationType == (int)CreationType.Manga).ToList();
                    bookmarks = context.Bookmarks.Where(b => b.Creation.CreationType == (int)CreationType.Manga).ToList();
                    episodes = context.Episodes.Where(e => e.Bookmark.Creation.CreationType == (int)CreationType.Manga).ToList();
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

            creationsDataGrid.ItemsSource = creations.OrderBy(o => o.NewStatus);
            bookmarksDataGrid.ItemsSource = bookmarks;
            episodesDataGrid.ItemsSource = episodes.OrderBy(o => o.WatchStatus);

            CancelDropdownList();
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
        private void imageResize_Click(object sender, RoutedEventArgs e)
        {
            bigImage = !bigImage;
            imageSize = bigImage ? bigImageSize : smallImageSize;

            creationsDataGrid.Items.Refresh();
            creationsDataGridImage.Width = bigImage ? bigImageSize : smallImageSize + 30;
            episodesDataGrid.Items.Refresh();
            episodesdataGridImage.Width = bigImage ? bigImageSize : smallImageSize + 30;
        }

        private void changeStatus_Click(object sender, RoutedEventArgs e)
        {
            Button btn;

            switch (datasourceFilter)
            {
                case DatasourceFilter.Creations:
                    foreach (NewStatus s in (NewStatus[])Enum.GetValues(typeof(NewStatus)))
                    {
                        btn = new Button();

                        btn.Content = s.ToString();
                        btn.Name = "Button" + s.ToString();
                        btn.Style = Resources["ActionButton"] as Style;
                        btn.Height = 40;
                        btn.Click += new RoutedEventHandler(creationChangeStatus_Click);

                        statusDropDownList.Children.Add(btn);
                    }
                    break;
                case DatasourceFilter.Episodes:
                    foreach (WatchStatus s in (WatchStatus[])Enum.GetValues(typeof(WatchStatus)))
                    {
                        btn = new Button();

                        btn.Content = s.ToString();
                        btn.Name = "Button" + s.ToString();
                        btn.Style = Resources["ActionButton"] as Style;
                        btn.Height = 40;
                        btn.Click += new RoutedEventHandler(episodeChangeStatus_Click);

                        statusDropDownList.Children.Add(btn);
                    }
                    break;
                default:
                    break;
            }

            btn = new Button();

            btn.Content = "Cancel";
            btn.Name = "Cancel";
            btn.Style = Resources["ActionButton"] as Style;
            btn.Height = 40;
            btn.Click += new RoutedEventHandler(cancelChangeStatus_Click);

            statusChangeButton.IsEnabled = false;
            statusDropDownList.Children.Add(btn);
        }

        private void creationChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            Button status = sender as Button;
            if (creationsDataGrid != null && creationsDataGrid.SelectedItems != null)
            {
                foreach (Creation creation in creationsDataGrid.SelectedItems)
                {
                    NewStatus newStatus = (NewStatus)Enum.Parse(typeof(NewStatus), status.Content.ToString());
                    context.Creations.FirstOrDefault(n => n.CreationId == creation.CreationId).NewStatus = (int)newStatus;
                    context.SaveChanges();
                }
            }
            creationsDataGrid.Items.Refresh();

            CancelDropdownList();
        }

        private void episodeChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            Button status = sender as Button;
            if (episodesDataGrid != null && episodesDataGrid.SelectedItems != null)
            {
                foreach (Episode episode in episodesDataGrid.SelectedItems)
                {
                    WatchStatus watchStatus = (WatchStatus)Enum.Parse(typeof(WatchStatus), status.Content.ToString());
                    context.Episodes.FirstOrDefault(n => n.EpisodeId == episode.EpisodeId).WatchStatus = (int)watchStatus;
                    context.SaveChanges();
                }
            }
            episodesDataGrid.Items.Refresh();

            CancelDropdownList();
        }

        private void cancelChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            CancelDropdownList();
        }

        private void CancelDropdownList()
        {
            statusDropDownList.Children.Clear();
            statusChangeButton.IsEnabled = true;
        }

        private void trashDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (episodesDataGrid != null && episodesDataGrid.SelectedItems != null)
            {
                foreach (Episode episode in episodesDataGrid.SelectedItems)
                {
                    context.Episodes.Remove(episode);
                }

                context.SaveChanges();
                LoadCreationsAndEpisodes();
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

        private void linkBookmark_Click(object sender, RoutedEventArgs e)
        {
            LinkWindow linkWindow = new LinkWindow();

            linkWindow.Show();
        }

        private void copyBookmark_Click(object sender, RoutedEventArgs e)
        {
            string clipboardText = "";
            foreach (Bookmark bookmark in context.Bookmarks)
            {
                clipboardText += bookmark.Creation.Title + "\n";
            }
            Clipboard.SetText(clipboardText);
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

        private void episodesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGrid dataGrid = sender as DataGrid;
                if (dataGrid != null && dataGrid.SelectedItems != null)
                {
                    foreach (Episode episode in dataGrid.SelectedItems)
                    {
                        List<Episode> sameCreationEpisodes = context.Episodes.Where(n => n.Bookmark.BookmarkId == episode.Bookmark.BookmarkId).ToList();
                        foreach (Episode creationEpisode in sameCreationEpisodes)
                        {
                            if (creationEpisode.EpisodeNumber < episode.EpisodeNumber)
                            {
                                context.Episodes.FirstOrDefault(n => n.EpisodeId == creationEpisode.EpisodeId).WatchStatus = (int)WatchStatus.AlreadyWatch;
                            }
                            else if (creationEpisode.EpisodeNumber == episode.EpisodeNumber)
                            {
                                context.Episodes.FirstOrDefault(n => n.EpisodeId == creationEpisode.EpisodeId).WatchStatus = (int)WatchStatus.LatestWatch;
                            }
                            else if (creationEpisode.EpisodeNumber == episode.EpisodeNumber + 1)
                            {
                                context.Episodes.FirstOrDefault(n => n.EpisodeId == creationEpisode.EpisodeId).WatchStatus = (int)WatchStatus.NextWatch;
                            }
                            else
                            {
                                context.Episodes.FirstOrDefault(n => n.EpisodeId == creationEpisode.EpisodeId).WatchStatus = (int)WatchStatus.NeedToWatch;
                            }
                        }

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = episode.Link,
                            UseShellExecute = true
                        });
                    }
                    context.SaveChanges();

                    LoadCreationsAndEpisodes();
                }
            }
        }

        private void bookmarksDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGrid dataGrid = sender as DataGrid;
                if (dataGrid != null && dataGrid.SelectedItems != null)
                {
                    foreach (Bookmark bookmark in dataGrid.SelectedItems)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = bookmark.Creation.Link,
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
