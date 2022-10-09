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

        private Bookmarker bookmarker;
        private WebScrapperTimer webScrapperTimer;
        private PictureSizeSetter pictureSizeSetter;

        private Filter filter;
        private DatasourceFilter datasourceFilter;

        private List<Creation> creations = new List<Creation>();
        private List<Bookmark> bookmarks = new List<Bookmark>();
        private List<Episode> episodes = new List<Episode>();

        private BrushConverter bc = new BrushConverter();

        private string filterText;

        public MainWindow()
        {
            InitializeComponent();

            context = new Context();

            bookmarker = new Bookmarker(this);
            webScrapperTimer = new WebScrapperTimer(this);
            pictureSizeSetter = new PictureSizeSetter(this, context);

            filterText = "";
            filter = Filter.All;
            datasourceFilter = DatasourceFilter.Episodes;

            LoadCreationsAndEpisodes();
        }

        private void WebScrapper_Loaded(object sender, RoutedEventArgs e)
        {
            webScrapperTimer.StartTimer();
        }

        private void WebScrapperWindow_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Space)
            {
                switch (datasourceFilter)
                {
                    case DatasourceFilter.Creations:
                        if (creationsDataGrid != null && creationsDataGrid.SelectedItems.Count > 0)
                        {
                            Creation creation = creationsDataGrid.SelectedItems[creationsDataGrid.SelectedItems.Count-1] as Creation;
                            searchDataGridTextBox.Text = creation.Title;
                        }
                        break;
                    case DatasourceFilter.Episodes:
                        if (episodesDataGrid != null && episodesDataGrid.SelectedItems.Count > 0)
                        {
                            Episode episode = episodesDataGrid.SelectedItems[episodesDataGrid.SelectedItems.Count-1] as Episode;
                            searchDataGridTextBox.Text = episode.Bookmark.Creation.Title;
                        }
                        break;
                    default:
                        break;
                }

                filterText = Regex.Replace(searchDataGridTextBox.Text.ToLower(), @"[^0-9a-zA-Z]+", "");

                LoadCreationsAndEpisodes();
            }

            if (e.Key == Key.System)
            {
                searchDataGridTextBox.Text = "";
                filterText = "";

                LoadCreationsAndEpisodes();
            }

            if (e.Key == Key.Tab)
            {
                filter = (int)filter < Enum.GetNames(typeof(Filter)).Length - 1 ? (Filter)((int)filter + 1) : Filter.All;

                LoadCreationsAndEpisodes();
            }
        }

        //------------------------------------------------GridLoadFilters------------------------------------------------------

        #region GridLoadFilters        
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
                case Filter.Webtoon:
                    creations = context.Creations.Where(c => c.CreationType == (int)CreationType.Webtoon).ToList();
                    bookmarks = context.Bookmarks.Where(b => b.Creation.CreationType == (int)CreationType.Webtoon).ToList();
                    episodes = context.Episodes.Where(e => e.Bookmark.Creation.CreationType == (int)CreationType.Webtoon).ToList();
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

            creationsDataGrid.ItemsSource = creations.Where(creation => creation.Title.ToLower().Contains(filterText)).OrderBy(o => o.NewStatus).ThenByDescending(t => t.UpdatedAt);
            bookmarksDataGrid.ItemsSource = bookmarks.OrderBy(o => o.Completed).ThenByDescending(t => t.UpdatedAt);
            episodesDataGrid.ItemsSource = episodes.Where(episode => episode.Bookmark.Creation.Title.ToLower().Contains(filterText)).OrderBy(o => o.WatchStatus).ThenBy(t => t.Bookmark.Creation.Title).ThenByDescending(t => t.EpisodeNumber);

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
        private void LoadWebtoonCreation_Click(object sender, RoutedEventArgs e)
        {
            filter = Filter.Webtoon;
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
        private void RestartTimer_Click(object sender, RoutedEventArgs e)
        {
            webScrapperTimer.StartTimer();
        }

        #endregion

        //---------------------------------------------MouseLeaveAndEnter----------------------------------------------------

        #region MouseLeaveAndEnter

        private void FilterButtonFocus()
        {
            foreach (Button btn in sideMenuFilter.Children)
            {
                if (btn.Name == "restartButton")
                {
                    btn.Background = webScrapperTimer.RestartIsEnabled() ? this.Resources["LightGreenBrush"] as Brush : this.Resources["RedBrush"] as Brush;
                }
                else
                {
                    btn.Background = this.Resources["DarkBrush"] as Brush;
                }
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
                //case Filter.Game:
                //    break;
                //case Filter.Vn:
                    break;
                default:
                    break;
            }
        }
        private void FilterButton_MouseEnter(object sender, MouseEventArgs e)
        {
            sender.GetType().GetProperty("Background").SetValue(sender, this.Resources["HighlightBrush"] as Brush);
        }
        private void FilterButton_MouseLeave(object sender, MouseEventArgs e)
        {
            FilterButtonFocus();
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
        private void DatasourceFilterButton_MouseEnter(object sender, MouseEventArgs e)
        {
            sender.GetType().GetProperty("Background").SetValue(sender, this.Resources["HighlightBrush"] as Brush);
        }
        private void DatasourceFilterButton_MouseLeave(object sender, MouseEventArgs e)
        {
            DatasourceFilterButtonFocus();
        }

        #endregion

        //------------------------------------------------ToolbarButtons------------------------------------------------------

        #region ToolbarButtons
        private void imageResize_Click(object sender, RoutedEventArgs e)
        {
            pictureSizeSetter.ChangeSize(datasourceFilter);
        }

        private void selectAll_Click(object sender, RoutedEventArgs e)
        {
            creationsDataGrid.UnselectAll();
            if (creationsDataGrid != null)
            {
                for (int i = 0; i < creationsDataGrid.Items.Count; i++)
                {
                    creationsDataGrid.SelectedItems.Add(creationsDataGrid.Items[i]);
                }
            }
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
            ConfirmWindow confirmWindow = new ConfirmWindow(this);

            confirmWindow.ShowDialog();

            if (confirmWindow.Confirmation)
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
                //creationsDataGrid.Items.Refresh();
                LoadCreationsAndEpisodes();
            }

            CancelDropdownList();
        }

        private void episodeChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            ConfirmWindow confirmWindow = new ConfirmWindow(this);

            confirmWindow.ShowDialog();

            if (confirmWindow.Confirmation)
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
                //episodesDataGrid.Items.Refresh();
                LoadCreationsAndEpisodes();
            }

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
            ConfirmWindow confirmWindow = new ConfirmWindow(this);

            confirmWindow.ShowDialog();

            if(confirmWindow.Confirmation)
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
        }

        private void linkBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (bookmarksDataGrid != null && bookmarksDataGrid.SelectedItems != null)
            {
                foreach (Bookmark bookmark in bookmarksDataGrid.SelectedItems)
                {
                    LinkWindow linkWindow = new LinkWindow(bookmark, this);

                    linkWindow.Show();
                }
            }
        }

        private void completeBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (bookmarksDataGrid != null && bookmarksDataGrid.SelectedItems != null)
            {
                foreach (Bookmark bookmark in bookmarksDataGrid.SelectedItems)
                {
                    context.Bookmarks.FirstOrDefault(b => b.BookmarkId == bookmark.BookmarkId).Completed = (bookmark.Completed == 0 ? 1 : 0);
                }
                context.SaveChanges();

                LoadCreationsAndEpisodes();
            }
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

        private void deleteDuplicates_Click(object sender, RoutedEventArgs e)
        {
            var duplicateCreations = context.Creations.GroupBy(a => new { a.CreationType, a.SiteName, a.Title, a.Link })
             .Where(a => a.Count() > 1)
             .SelectMany(a => a.ToList());

            var duplicateEpisodes = context.Episodes.GroupBy(a => new { a.BookmarkId, a.EpisodeNumber, a.Link })
             .Where(a => a.Count() > 1)
             .SelectMany(a => a.ToList());

            foreach (var d in duplicateCreations)
            {
                context.Creations.Remove(d);
            }

            foreach (var d in duplicateEpisodes)
            {
                context.Episodes.Remove(d);
            }

            context.SaveChanges();
        }

        private void searchFiltered_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            filterText = Regex.Replace(searchDataGridTextBox.Text.ToLower(), @"[^0-9a-zA-Z]+", "");

            LoadCreationsAndEpisodes();
        }

        #endregion

        //------------------------------------------------DataGridEvents------------------------------------------------------

        #region DataGridEvents

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

                        context.Creations.FirstOrDefault(n => n.CreationId == creation.CreationId).NewStatus = (int)NewStatus.Seen;
                        context.SaveChanges();

                        LoadCreationsAndEpisodes();
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

                        List<Episode> neededToWatchEpisodes = sameCreationEpisodes.Where(n => n.EpisodeNumber > episode.EpisodeNumber).ToList();
                        List<Episode> alreadyWatchEpisodes = sameCreationEpisodes.Where(n => n.EpisodeNumber <= episode.EpisodeNumber).ToList();

                        double? nextWatchEpisode = neededToWatchEpisodes.Min(m => m.EpisodeNumber);
                        double? latestWatchEpisode = alreadyWatchEpisodes.Max(m => m.EpisodeNumber);

                        foreach (Episode creationEpisode in sameCreationEpisodes)
                        {
                            if (creationEpisode.EpisodeNumber == nextWatchEpisode)
                            {
                                context.Episodes.FirstOrDefault(n => n.EpisodeId == creationEpisode.EpisodeId).WatchStatus = (int)WatchStatus.NextWatch;
                            }
                            else if (creationEpisode.EpisodeNumber == latestWatchEpisode)
                            {
                                context.Episodes.FirstOrDefault(n => n.EpisodeId == creationEpisode.EpisodeId).WatchStatus = (int)WatchStatus.LatestWatch;
                            }
                            else if (neededToWatchEpisodes.Any(a => a.EpisodeNumber == creationEpisode.EpisodeNumber))
                            {
                                context.Episodes.FirstOrDefault(n => n.EpisodeId == creationEpisode.EpisodeId).WatchStatus = (int)WatchStatus.NeedToWatch;
                            }
                            else if (alreadyWatchEpisodes.Any(a => a.EpisodeNumber == creationEpisode.EpisodeNumber))
                            {
                                context.Episodes.FirstOrDefault(n => n.EpisodeId == creationEpisode.EpisodeId).WatchStatus = (int)WatchStatus.AlreadyWatch;
                            }

                        }

                        context.SaveChanges();

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = episode.Link,
                            UseShellExecute = true
                        });
                    }          

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

        private void creationsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void bookmarksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void episodesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        #endregion

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

            if (nextEpisode == null)
            {
                nextEpisode = episodes.FirstOrDefault(n => n.WatchStatus == (int)WatchStatus.LatestWatch)?.EpisodeNumber + 0.0001;

                if (nextEpisode == null)
                {
                    nextEpisode = episodes.Where(n => n.WatchStatus == (int)WatchStatus.AlreadyWatch).Max(n => n.EpisodeNumber);

                    if (nextEpisode == null)
                    {
                        nextEpisode = 1;
                    }

                }
            }

            List<Episode> neededToWatchEpisodes = episodes.Where(n => n.EpisodeNumber >= nextEpisode).ToList();
            List<Episode> alreadyWatchEpisodes = episodes.Where(n => n.EpisodeNumber < nextEpisode).ToList();

            double? nextWatchEpisode = neededToWatchEpisodes.Min(m => m.EpisodeNumber);
            double? latestWatchEpisode = alreadyWatchEpisodes.Max(m => m.EpisodeNumber);

            foreach (var episode in episodes)
            {
                if (episode.EpisodeNumber == nextWatchEpisode)
                {
                    if (episode.WatchStatus != (int)WatchStatus.NextWatch)
                    {
                        context.Episodes.FirstOrDefault(n => n.EpisodeId == episode.EpisodeId).WatchStatus = (int)WatchStatus.NextWatch;
                    }
                }
                else if (episode.EpisodeNumber == latestWatchEpisode)
                {
                    if (episode.WatchStatus != (int)WatchStatus.LatestWatch)
                    {
                        context.Episodes.FirstOrDefault(n => n.EpisodeId == episode.EpisodeId).WatchStatus = (int)WatchStatus.LatestWatch;
                    }
                }
                else if (neededToWatchEpisodes.Any(a => a.EpisodeNumber == episode.EpisodeNumber))
                {
                    if (episode.WatchStatus != (int)WatchStatus.NeedToWatch)
                    {
                        context.Episodes.FirstOrDefault(n => n.EpisodeId == episode.EpisodeId).WatchStatus = (int)WatchStatus.NeedToWatch;
                    }
                }
                else if (alreadyWatchEpisodes.Any(a => a.EpisodeNumber == episode.EpisodeNumber))
                {
                    if (episode.WatchStatus != (int)WatchStatus.AlreadyWatch)
                    {
                        context.Episodes.FirstOrDefault(n => n.EpisodeId == episode.EpisodeId).WatchStatus = (int)WatchStatus.AlreadyWatch;
                    }
                }
            }


            context.SaveChanges();
        }
    }
    public enum Filter
    {
        All,
        Donghua,
        Anime,
        Manga,
        Webtoon
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
        Vn,
        Webtoon
    }

    public enum SiteName
    {
        Naruldonghua,
        Animexin,
        Kickassanime,
        Mangasee,
        Webtoonxyz
    }

    public enum WatchStatus
    {
        NextWatch,
        LatestWatch,
        NeedToWatch,
        AlreadyWatch
    }
}
