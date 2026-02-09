using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using WebScrapperEngine.Entity;
using static WebScrapperEngine.Scrapper.AnimeScrapper;

namespace WebScrapperEngine.Scrapper
{
    class DonghuaScrapper
    {
        private MainWindow mainWindow;
        private HtmlWeb web;
        private Context context;

        private BackgroundWorker donghuaCreationWorker = new BackgroundWorker();
        private BackgroundWorker donghuaEpisodeWorker = new BackgroundWorker();
        private BackgroundWorker donghuaImageRefreshWorker = new BackgroundWorker();

        private bool refreshImageNeeded = false;

        public bool StopWorker { get; set; }

        public DonghuaScrapper(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            web = new HtmlWeb();
            context = new Context();

            donghuaEpisodeWorker.DoWork += DonghuaEpisodeWork;
            donghuaEpisodeWorker.RunWorkerCompleted += DonghuaEpisodeWorkCompleted;

            donghuaCreationWorker.DoWork += DonghuaCreationWork;
            donghuaCreationWorker.RunWorkerCompleted += DonghuaCreationWorkCompleted;

            donghuaImageRefreshWorker.DoWork += DonghuaImageRefreshWork;
            donghuaImageRefreshWorker.RunWorkerCompleted += DonghuaImageRefreshWorkCompleted;
        }

        public void BookmarkEpisode(Creation creation, Bookmark bookmark)
        {
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load(creation.Link);

            HtmlNodeCollection nodes;
            switch ((SiteName)creation.SiteName)
            {
                case SiteName.Naruldonghua:
                    nodes = doc.DocumentNode.SelectNodes(Naruldonghua.episodeList);
                    break;
                case SiteName.Animexin:
                    nodes = doc.DocumentNode.SelectNodes(Animexin.episodeList);
                    break;
                default:
                    nodes = null;
                    break;
            }

            try
            {
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        string episodeNumber;
                        string episodeLink;

                        switch ((SiteName)creation.SiteName)
                        {
                            case SiteName.Naruldonghua:
                                episodeNumber = node.SelectSingleNode(Naruldonghua.episodeNumber) != null ? Regex.Replace(node.SelectSingleNode(Naruldonghua.episodeNumber).InnerText, @"[^0-9a-zA-Z]+", " ").Split(' ')[0] : null;
                                episodeLink = node.SelectSingleNode(Naruldonghua.episodeLink).GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Naruldonghua.episodeLink).GetAttributeValue<string>("href", null) : null;
                                break;
                            case SiteName.Animexin:
                                episodeNumber = node.SelectSingleNode(Animexin.episodeNumber) != null ? Regex.Replace(node.SelectSingleNode(Animexin.episodeNumber).InnerText, @"[^0-9a-zA-Z]+", " ").Split(' ')[0] : null;
                                episodeLink = node.SelectSingleNode(Animexin.episodeLink).GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Animexin.episodeLink).GetAttributeValue<string>("href", null) : null;
                                break;
                            default:
                                episodeNumber = null;
                                episodeLink = null;
                                break;
                        }

                        double episodeNumberFonLinQ = Convert.ToDouble(episodeNumber);
                        if (!context.Episodes.Any(n => n.Bookmark.Creation.SiteName == creation.SiteName
                        && n.Bookmark.Creation.Title == creation.Title && n.EpisodeNumber == episodeNumberFonLinQ))
                        {
                            context.Episodes.Add(new Episode()
                            {
                                BookmarkId = bookmark.BookmarkId,
                                EpisodeNumber = episodeNumberFonLinQ,
                                Link = episodeLink,
                                WatchStatus = episodeNumberFonLinQ <= 1 ? (int)WatchStatus.NextWatch : (int)WatchStatus.NeedToWatch
                            });
                        }
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                mainWindow.Dispatcher.Invoke(() =>
                {
                    mainWindow.exceptionListBox.Items.Add("Bookmark of donghua failed! Exception: " + e.Message);
                });
            }

        }

        public void SearchEpisode()
        {
            List<Bookmark> bookmarks = context.Bookmarks.Where(n => n.Creation.CreationType == (int)CreationType.Donghua && n.Completed == 0).ToList();

            foreach (var bookmark in bookmarks)
            {
                if (StopWorker) { break; }
                try {

                    var creations = bookmark.BookmarkCreations.Select(bc => bc.Creation)
                        .Concat(new[] { bookmark.Creation }).Where(c => c != null).Distinct().ToList();
                    var episodeNodeLists = new List<HtmlNodeCollection>();

                    foreach (var creation in creations)
                    {
                        var doc = web.Load(creation.Link);
                        HtmlNodeCollection nodes = null;

                        switch ((SiteName)creation.SiteName)
                        {
                            case SiteName.Naruldonghua:
                                nodes = doc.DocumentNode.SelectNodes(Naruldonghua.episodeList);
                                break;

                            case SiteName.Animexin:
                                nodes = doc.DocumentNode.SelectNodes(Animexin.episodeList);
                                break;
                        }

                        if (nodes != null)
                        {
                            episodeNodeLists.Add(nodes);
                        }                       
                    }

                    var nodesToProcess = episodeNodeLists
                        .OrderByDescending(n => n.Count)
                        .FirstOrDefault();

                    foreach (var node in nodesToProcess)
                    {
                        string episodeNumber = null;
                        string episodeLink = null;

                        switch ((SiteName)bookmark.Creation.SiteName)
                        {
                            case SiteName.Naruldonghua:
                                episodeNumber = node.SelectSingleNode(Naruldonghua.episodeNumber) != null ? Regex.Replace(node.SelectSingleNode(Naruldonghua.episodeNumber).InnerText, @"[^0-9a-zA-Z]+", " ").Split(' ')[0] : null;
                                episodeLink = node.SelectSingleNode(Naruldonghua.episodeLink)?.GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Naruldonghua.episodeLink).GetAttributeValue<string>("href", null) : null;
                                break;
                            case SiteName.Animexin:
                                episodeNumber = node.SelectSingleNode(Animexin.episodeNumber) != null ? Regex.Replace(node.SelectSingleNode(Animexin.episodeNumber).InnerText, @"[^0-9a-zA-Z]+", " ").Split(' ')[0] : null;
                                episodeLink = node.SelectSingleNode(Animexin.episodeLink)?.GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Animexin.episodeLink).GetAttributeValue<string>("href", null) : null;
                                break;
                            default:
                                break;
                        }
                        if (Double.TryParse(episodeNumber, out double episodeNumberFonLinQ))
                        {
                            if (!context.Episodes.Any(n => n.Bookmark.Creation.SiteName == bookmark.Creation.SiteName
                            && n.Bookmark.Creation.Title == bookmark.Creation.Title && n.EpisodeNumber == episodeNumberFonLinQ))
                            {
                                context.Episodes.Add(new Episode()
                                {
                                    BookmarkId = bookmark.BookmarkId,
                                    EpisodeNumber = episodeNumberFonLinQ,
                                    Link = episodeLink,
                                    WatchStatus = episodeNumberFonLinQ <= 1 ? (int)WatchStatus.NextWatch : (int)WatchStatus.NeedToWatch
                                });

                                context.Bookmarks.FirstOrDefault(n => n.BookmarkId == bookmark.BookmarkId).UpdatedAt = DateTime.Now;
                            }
                        }
                    }

                    context.SaveChanges();
                    mainWindow.CorrectWatchStatus(bookmark);
                }
                catch (Exception e)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.exceptionListBox.Items.Add("Episode search of donghua failed! Exception: " + e.Message);
                    });
                }

            }
        }

        public void SearchNaruldonghuaSite()
        {
            string websiteLink = Naruldonghua.websiteLink;
            bool nextButtonExist = true;

            HtmlNodeCollection nodes;
            string Title;
            string Link;
            string Image;

            do
            {
                if (StopWorker) { break; }

                var doc = web.Load(websiteLink);
                nodes = doc.DocumentNode.SelectNodes(Naruldonghua.contentPath);
                try
                {
                    if (nodes != null)
                    {
                        foreach (var node in nodes)
                        {
                            Title = node.SelectSingleNode(Naruldonghua.titlePath)?.InnerText != null ? Regex.Replace(node.SelectSingleNode(Naruldonghua.titlePath).InnerText, @"[^0-9a-zA-Z]+", "") : null;
                            Link = node.SelectSingleNode(Naruldonghua.linkPath)?.GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Naruldonghua.linkPath).GetAttributeValue<string>("href", null) : null;
                            Image = node.SelectSingleNode(Naruldonghua.imagePath)?.Attributes[Naruldonghua.imageSrc].Value != null ? node.SelectSingleNode(Naruldonghua.imagePath).Attributes[Naruldonghua.imageSrc].Value : null;
                            var donghuaCreation = new Creation()
                            {
                                CreationType = (int)CreationType.Donghua,
                                SiteName = (int)SiteName.Naruldonghua,
                                Title = Title,
                                Link = Link,
                                Image = Image,
                                NewStatus = (int)NewStatus.New,
                                UpdatedAt = DateTime.Now
                            };

                            if (!context.Creations.Any(n => n.SiteName == donghuaCreation.SiteName && n.Title == donghuaCreation.Title))
                            {
                                var seriesEpisodeDoc = web.Load(donghuaCreation.Link);
                                donghuaCreation.Link = seriesEpisodeDoc.DocumentNode.SelectSingleNode(Naruldonghua.linkToSeriesPath) != null ? seriesEpisodeDoc.DocumentNode.SelectSingleNode(Naruldonghua.linkToSeriesPath).GetAttributeValue<string>("href", null) : donghuaCreation.Link;

                                context.Creations.Add(donghuaCreation);
                                context.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentNullException("Creation search of naruldonghua was null");
                    }
                }
                catch (Exception e)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.exceptionListBox.Items.Add("Creation search of naruldonghua failed! Exception: " + e.Message);
                    });
                }

                nextButtonExist = doc.DocumentNode.SelectSingleNode(Naruldonghua.nextButtonPath) != null;

                websiteLink = nextButtonExist ? doc.DocumentNode.SelectSingleNode(Naruldonghua.nextButtonPath).GetAttributeValue<string>("href", null) : null;

            } while (nextButtonExist);

        }

        public void SearchAnimexinSite()
        {
            string websiteLink = Animexin.websiteLink;
            bool nextButtonExist = true;

            HtmlNodeCollection nodes;
            string Title;
            string Link;
            string Image;

            do
            {
                if (StopWorker) { break; }

                var doc = web.Load(websiteLink);
                nodes = doc.DocumentNode.SelectNodes(Animexin.contentPath);
                var error = doc.DocumentNode.SelectSingleNode(Animexin.contentErrorPath);

                try
                {
                    if (error == null)
                    {
                        if (nodes != null)
                        {
                            foreach (var node in nodes)
                            {
                                Title = node.SelectSingleNode(Animexin.titlePath)?.InnerText != null ? Regex.Replace(node.SelectSingleNode(Animexin.titlePath).InnerText, @"[^0-9a-zA-Z]+", "") : null;
                                Link = node.SelectSingleNode(Animexin.linkPath)?.GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Animexin.linkPath).GetAttributeValue<string>("href", null) : null;
                                Image = node.SelectSingleNode(Animexin.imagePath)?.Attributes[Animexin.imageSrc].Value != null ? node.SelectSingleNode(Animexin.imagePath).Attributes[Animexin.imageSrc].Value : null;
                                var donghuaCreation = new Creation()
                                {
                                    CreationType = (int)CreationType.Donghua,
                                    SiteName = (int)SiteName.Animexin,
                                    Title = Title,
                                    Link = Link,
                                    Image = Image,
                                    NewStatus = (int)NewStatus.New,
                                    UpdatedAt = DateTime.Now
                                };

                                if (!context.Creations.Any(n => n.SiteName == donghuaCreation.SiteName && n.Title == donghuaCreation.Title))
                                {
                                    var seriesEpisodeDoc = web.Load(donghuaCreation.Link);
                                    donghuaCreation.Link = seriesEpisodeDoc.DocumentNode.SelectSingleNode(Animexin.linkToSeriesPath) != null ? seriesEpisodeDoc.DocumentNode.SelectSingleNode(Animexin.linkToSeriesPath).GetAttributeValue<string>("href", null) : donghuaCreation.Link;

                                    context.Creations.Add(donghuaCreation);
                                    context.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            throw new ArgumentNullException("Creation search of animexin was null");
                        }
                    }
                }
                catch (Exception e)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.exceptionListBox.Items.Add("Creation search of animexin failed! Exception: " + e.Message);
                    });
                }

                nextButtonExist = doc.DocumentNode.SelectSingleNode(Animexin.nextButtonPath) != null;

                websiteLink = nextButtonExist ? doc.DocumentNode.SelectSingleNode(Animexin.nextButtonPath).GetAttributeValue<string>("href", null) : null;

            } while (nextButtonExist);
        }

        public void SearchImage()
        {
            List<Creation> creations = context.Creations.Where(n => n.CreationType == (int)CreationType.Donghua).ToList();
            HtmlDocument doc;
            HtmlNode node;
            string image;

            foreach (var creation in creations)
            {
                try
                {
                    doc = null;
                    node = null;
                    image = null;

                    switch ((SiteName)creation.SiteName)
                    {
                        case SiteName.Naruldonghua:
                            doc = web.Load(creation.Link);
                            node = doc.DocumentNode.Descendants(0)?.FirstOrDefault(n => n.HasClass(Naruldonghua.imageRefreshClass));
                            if (node != null)
                            {
                                image = node.SelectSingleNode(Naruldonghua.imageRefreshPath)?.Attributes[Naruldonghua.imageRefreshSrc]?.Value;
                            }
                            break;
                        case SiteName.Animexin:
                            doc = web.Load(creation.Link);
                            node = doc.DocumentNode.Descendants(0)?.FirstOrDefault(n => n.HasClass(Animexin.imageRefreshClass));
                            if (node != null)
                            {
                                image = node.SelectSingleNode(Animexin.imageRefreshPath)?.Attributes[Animexin.imageRefreshSrc]?.Value;
                            }
                            break;
                        default:
                            node = null;
                            break;
                    }

                    if (image != null)
                    {
                        creation.Image = image;
                        context.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.exceptionListBox.Items.Add("ImageSearch of " + creation.Link + " failed! Exception: " + e.Message);
                    });
                }
            }
        }

        public bool IsWorkerRunning()
        {
            return donghuaCreationWorker.IsBusy || donghuaEpisodeWorker.IsBusy || donghuaImageRefreshWorker.IsBusy;
        }

        public void RunWorker()
        {
            StopWorker = false;

            // ReplaceDatabaseLinks();

            donghuaEpisodeWorker.RunWorkerAsync();

            mainWindow.donghuaEpisodeFilterDotImage.Visibility = Visibility.Visible;
            mainWindow.donghuaCreationFilterDotImage.Visibility = Visibility.Visible;
        }
        private void DonghuaEpisodeWork(object sender, DoWorkEventArgs e)
        {
            SearchEpisode();
        }

        private void DonghuaEpisodeWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

            mainWindow.donghuaEpisodeFilterDotImage.Visibility = Visibility.Hidden;

            donghuaCreationWorker.RunWorkerAsync();
        }

        private void DonghuaCreationWork(object sender, DoWorkEventArgs e)
        {
            SearchNaruldonghuaSite();
            SearchAnimexinSite();
        }

        private void DonghuaCreationWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

            mainWindow.donghuaCreationFilterDotImage.Visibility = Visibility.Hidden;

            if (refreshImageNeeded)
            {
                donghuaImageRefreshWorker.RunWorkerAsync();
            }
        }

        public void RunRefreshImageWorker()
        {
            if (!IsWorkerRunning())
            {
                donghuaImageRefreshWorker.RunWorkerAsync();
            }
            else
            {
                refreshImageNeeded = true;
            }

            mainWindow.donghuaImageFilterDotImage.Visibility = Visibility.Visible;
        }

        private void DonghuaImageRefreshWork(object sender, DoWorkEventArgs e)
        {
            SearchImage();
        }

        private void DonghuaImageRefreshWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

            mainWindow.donghuaImageFilterDotImage.Visibility = Visibility.Hidden;
            refreshImageNeeded = false;
        }

        private void ReplaceDatabaseLinks()
        {
            try
            {
                var creations = context.Creations
                    .Where(c => c.SiteName == (int)SiteName.Naruldonghua || c.SiteName == (int)SiteName.Animexin).ToList();

                foreach (var creation in creations)
                {
                    string correctBaseUrl = null;

                    switch ((SiteName)creation.SiteName)
                    {
                        case SiteName.Naruldonghua:
                            correctBaseUrl = Naruldonghua.websiteLink;
                            break;

                        case SiteName.Animexin:
                            correctBaseUrl = Animexin.websiteLink;
                            break;
                    }

                    if (!creation.Link.StartsWith(correctBaseUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        if (Uri.TryCreate(creation.Link, UriKind.Absolute, out var oldUri))
                        {
                            creation.Link = correctBaseUrl + oldUri.AbsolutePath;
                        }
                    }
                    if (!string.IsNullOrEmpty(creation.Image) &&
                        !creation.Image.StartsWith("data:image", StringComparison.OrdinalIgnoreCase) &&
                        !creation.Image.StartsWith(correctBaseUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        if (Uri.TryCreate(creation.Image, UriKind.Absolute, out var oldImageUri))
                        {
                            creation.Image = correctBaseUrl + oldImageUri.AbsolutePath;
                        }
                    }
                }

                //context.SaveChanges();
            }
            catch (Exception e)
            {
                mainWindow.Dispatcher.Invoke(() =>
                {
                    mainWindow.exceptionListBox.Items.Add(
                        "ReplaceDatabaseLinks (Donghua) failed! Exception: " + e.Message);
                });
            }
        }

        public static class Naruldonghua
        {
            public const string websiteLink = "https://naruldonghua.com";
            public const string contentPath = "/html/body/div[@id='content']/div/div[@class='postbody']/div[@class='bixbox bbnofrm'][2]/div[@class='listupd normal']/div[@class='excstf']/article[position()>0]";
            public const string nextButtonPath = "/html/body/div[@id='content']/div/div[@class='postbody']/div[@class='bixbox bbnofrm'][2]/div[@class='listupd normal']/div[@class='hpage']/a[@class='r']";
            public const string titlePath = "div/a/div[@class='tt']/text()";
            public const string linkPath = "div/a";
            public const string linkToSeriesPath = "/html/body/div[@id='content']/div/div[@class='postbody']/article/div[2]/div/div[1]/div[2]/span[2]/a";
            public const string imagePath = "div/a/div[@class='limit']/img";
            public const string imageSrc = "src";

            public const string episodeList = "/html/body/div[@id='content']/div/div[@class='postbody']/article/div[@class='bixbox bxcl epcheck']/div[@class='eplister']/ul/li[position()>0]";
            public const string episodeNumber = "a/div[@class='epl-num']";
            public const string episodeLink = "a";

            public const string imageRefreshClass = "thumb";
            public const string imageRefreshPath = "img";
            public const string imageRefreshSrc = "src";
        }

        public static class Animexin
        {
            public const string websiteLink = "https://animexin.net";
            public const string contentPath = "/html/body/div[@id='content']/div/div[@class='postbody']/div[2]/div[@class='listupd normal']/div[@class='excstf']/article[position()>0]";
            public const string contentErrorPath = "/html/body/div[@id='content']/div/div[@class='notf']";
            public const string nextButtonPath = "/html/body/div[@id='content']/div/div[@class='postbody']/div[2]/div[@class='listupd normal']/div[@class='hpage']/a[@class='r']";
            public const string titlePath = "div/a/div[@class='tt']/text()";
            public const string linkPath = "div/a";
            public const string linkToSeriesPath = "/html/body/div[@id='content']/div/div[4]/article/div[2]/div/div[1]/div[2]/span[2]/a";
            public const string imagePath = "div/a/div[@class='limit']/img";
            public const string imageSrc = "src";

            public const string episodeList = "/html/body/div[@id='content']/div/div[@class='postbody']/article/div[@class='bixbox bxcl epcheck']/div[@class='eplister']/ul/li[position()>0]";
            public const string episodeNumber = "a/div[@class='epl-num']";
            public const string episodeLink = "a";

            public const string imageRefreshClass = "thumb";
            public const string imageRefreshPath = "img";
            public const string imageRefreshSrc = "src";

        }
    }
}