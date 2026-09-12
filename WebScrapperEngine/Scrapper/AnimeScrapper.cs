using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using WebScrapperEngine.Entity;
namespace WebScrapperEngine.Scrapper
{
    class AnimeScrapper
    {
        private MainWindow mainWindow;
        private Context context;
        private HtmlWeb web;

        private BackgroundWorker animeCreationWorker = new BackgroundWorker();
        private BackgroundWorker animeEpisodeWorker = new BackgroundWorker();
        private BackgroundWorker animeImageRefreshWorker = new BackgroundWorker();

        private bool refreshImageNeeded = false;

        public bool StopWorker { get; set; }

        public AnimeScrapper(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            context = new Context();
            web = new HtmlWeb();

            animeEpisodeWorker.DoWork += AnimeEpisodeWork;
            animeEpisodeWorker.RunWorkerCompleted += AnimeEpisodeWorkCompleted;

            animeCreationWorker.DoWork += AnimeCreationWork;
            animeCreationWorker.RunWorkerCompleted += AnimeCreationWorkCompleted;

            animeImageRefreshWorker.DoWork += AnimeImageRefreshWork;
            animeImageRefreshWorker.RunWorkerCompleted += AnimeImageRefreshWorkCompleted;
        }

        public void BookmarkEpisode(Creation creation, Bookmark bookmark)
        {
            try
            {
                switch ((SiteName)creation.SiteName)
                {
                    case SiteName.Kickassanime:
                        SearchKickassEpisode(creation, bookmark);
                        break;
                    case SiteName.Aniwave:
                        SearchAniwaveEpisode(creation, bookmark);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception e)
            {
                mainWindow.Dispatcher.Invoke(() =>
                {
                    mainWindow.exceptionListBox.Items.Add("Bookmark of anime failed! Exception: " + e.Message);
                });
            }
        }

        public void SearchEpisode()
        {
            List<Bookmark> bookmarks = context.Bookmarks.Where(n => n.Creation.CreationType == (int)CreationType.Anime && n.Completed == 0).ToList();

            foreach (var bookmark in bookmarks)
            {
                if (StopWorker) { break; }

                try
                {
                    switch ((SiteName)bookmark.Creation.SiteName)
                    {
                        case SiteName.Kickassanime:
                            SearchKickassEpisode(bookmark.Creation, bookmark);
                            break;
                        case SiteName.Aniwave:
                            SearchAniwaveEpisode(bookmark.Creation, bookmark);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.exceptionListBox.Items.Add("Episode search of anime failed! Exception: " + e.Message);
                    });
                }
            }
        }

        public void SearchKickassEpisode(Creation creation, Bookmark bookmark)
        {
            string link = creation.Link.Replace(Kickassanime.websiteLink + "/", "");
            string requestString = Kickassanime.websiteLink + Kickassanime.apiEpisodeLink + link;
            string siteJson = mainWindow.MakeRequest(requestString, Kickassanime.cuttenWebsiteLink);
            SeriesResponse seriesResponse = JsonConvert.DeserializeObject<SeriesResponse>(siteJson);

            string nextEpisode = seriesResponse.Watch_uri.Replace("/" + link + "/", "");
            string request = Kickassanime.websiteLink + Kickassanime.apiEpisodeLink + link + Kickassanime.episodePath + Kickassanime.languageJP;
            siteJson = mainWindow.MakeRequest(request, Kickassanime.cuttenWebsiteLink);
            EpisodeResponse episodeResponse = JsonConvert.DeserializeObject<EpisodeResponse>(siteJson);

            var pages = episodeResponse.Pages != null ? episodeResponse.Pages.Count : 1;

            for (int i = 1; i <= pages; i++)
            {
                foreach (var data in episodeResponse.Result)
                {
                    if (!context.Episodes.Any(n => n.Bookmark.Creation.SiteName == creation.SiteName
                    && n.Bookmark.Creation.Title == creation.Title && n.EpisodeNumber == data.Episode_number))
                    {
                        context.Episodes.Add(new Episode()
                        {
                            BookmarkId = bookmark.BookmarkId,
                            EpisodeNumber = data.Episode_number,
                            Link = creation.Link + "/ep-" + data.Episode_number + "-" + data.Slug,
                            WatchStatus = data.Episode_number <= 1 ? (int)WatchStatus.NextWatch : (int)WatchStatus.NeedToWatch
                        });
                    }
                }
                if (i != pages)
                {
                    request = Kickassanime.websiteLink + Kickassanime.apiEpisodeLink + link + Kickassanime.episodePath + Kickassanime.pagePath + (i + 1) + "&" + Kickassanime.languageJP;
                    siteJson = mainWindow.MakeRequest(request, Kickassanime.cuttenWebsiteLink);
                    episodeResponse = JsonConvert.DeserializeObject<EpisodeResponse>(siteJson);
                }
            }

            context.SaveChanges();
            mainWindow.CorrectWatchStatus(bookmark);
        }

        public void SearchAniwaveEpisode(Creation creation, Bookmark bookmark)
        {
            var match = Regex.Match(creation.Link, @"/watch/(.+)-(\d+)$");

            string slug = match.Groups[1].Value;
            string id = match.Groups[2].Value;

            string requestString = Aniwave.websiteLink + Aniwave.apiEpisodeLink + id + Aniwave.apiEpisodeLinkPart2;
            string siteJson = mainWindow.MakeRequest(requestString, Aniwave.cuttenWebsiteLink);
            AnimewaveEpisodeResponse response = JsonConvert.DeserializeObject<AnimewaveEpisodeResponse>(siteJson);

            var doc = new HtmlDocument();
            doc.LoadHtml(response.Result);

            var nodes = doc.DocumentNode.SelectNodes(Aniwave.episodeList);

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    string episodeNumber = node.GetAttributeValue("data-num", null) ?? null;
                    string episodeLink = Aniwave.websiteLink + Regex.Replace(node.GetAttributeValue("href", null) ?? null, $@"/watch/{id}/", $"/watch/{slug}-{id}/");

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
            }

            context.SaveChanges();
            mainWindow.CorrectWatchStatus(bookmark);
        }

        public void SearchKickassSite()
        {
            SiteResponse siteResponse = null;
            string siteJson = "";
            int index = 1;

            try
            {
                do {

                if (StopWorker) { break; }

                    siteJson = mainWindow.MakeRequest(Kickassanime.websiteLink + Kickassanime.apiPath + index, Kickassanime.cuttenWebsiteLink);
                    siteResponse = JsonConvert.DeserializeObject<SiteResponse>(siteJson);

                    foreach (var data in siteResponse.Result)
                    {
                        var animeCreation = new Creation()
                        {
                            CreationType = (int)CreationType.Anime,
                            SiteName = (int)SiteName.Kickassanime,
                            Title = data.Title != null ? Regex.Replace(data.Title, @"[^0-9a-zA-Z]+", "") : "No name",
                            Link = data.Slug != null ? Kickassanime.websiteLink + "/" + data.Slug : "No link",
                            Image = data.Poster.Hq != null ? Kickassanime.websiteLink + Kickassanime.imagePath + data.Poster.Hq + ".webp" : "No image",
                            NewStatus = (int)NewStatus.New,
                            UpdatedAt = DateTime.Now
                        };

                        if (!context.Creations.Any(n => n.SiteName == animeCreation.SiteName && n.Title == animeCreation.Title))
                        {
                            context.Creations.Add(animeCreation);
                            context.SaveChanges();
                        }
                    }

                    index++;

                } while (siteResponse.Result.Count() > 0);
            }
            catch (Exception e)
            {
                mainWindow.Dispatcher.Invoke(() =>
                {
                    mainWindow.exceptionListBox.Items.Add("Creation search of anime failed! Exception: " + e.Message);
                });
            }
        }

        public void SearchAniwaveSite()
        {
            string websiteListLink = Aniwave.websiteListLink;
            bool nextButtonExist = true;

            HtmlNodeCollection nodes;
            string Title;
            string Link;
            string Image;

            do
            {
                if (StopWorker) { break; }

                var doc = web.Load(websiteListLink);
                nodes = doc.DocumentNode.SelectNodes(Aniwave.contentPath);

                try
                {
                    if (nodes != null)
                    {
                        foreach (var node in nodes)
                        {
                            Title = node.SelectSingleNode(Aniwave.titlePath)?.InnerText != null ? Regex.Replace(node.SelectSingleNode(Aniwave.titlePath).InnerText, @"[^0-9a-zA-Z]+", "") : null;
                            Link = node.SelectSingleNode(Aniwave.linkPath)?.GetAttributeValue<string>("href", null) != null ? 
                                Aniwave.websiteLink + node.SelectSingleNode(Aniwave.linkPath).GetAttributeValue<string>("href", null) : null;
                            Image = node.SelectSingleNode(Aniwave.imagePath)?.GetAttributeValue<string>(Aniwave.imageSrc, null) ?? 
                                (node.SelectSingleNode(Aniwave.imagePath)?.GetAttributeValue<string>(Aniwave.imageFallbackSrc, null) ?? null);        
                            
                            var donghuaCreation = new Creation()
                            {
                                CreationType = (int)CreationType.Anime,
                                SiteName = (int)SiteName.Aniwave,
                                Title = Title,
                                Link = Link,
                                Image = Image,
                                NewStatus = (int)NewStatus.New,
                                UpdatedAt = DateTime.Now
                            };

                            if (!context.Creations.Any(n => n.SiteName == donghuaCreation.SiteName && n.Title == donghuaCreation.Title))
                            {
                                context.Creations.Add(donghuaCreation);
                                context.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentNullException("Creation search of Aniwave was null");
                    }
                }
                catch (Exception e)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.exceptionListBox.Items.Add("Creation search of Aniwave failed! Exception: " + e.Message);
                    });
                }

                var navigations = doc.DocumentNode.SelectNodes(Aniwave.nextButtonPath);
                var activeNavigation = navigations?.FirstOrDefault(x => x.GetAttributeValue("class", "").Contains("active"));

                HtmlNode nextActiveNavigation = null;
                if (activeNavigation != null)
                {
                    var activeIndex = navigations.IndexOf(activeNavigation);
                    if (activeIndex >= 0 && activeIndex < navigations.Count - 1)
                    {
                        nextActiveNavigation = navigations[activeIndex + 1];
                    }
                }

                nextButtonExist = nextActiveNavigation != null;
                websiteListLink = nextButtonExist ? Aniwave.websiteLink + nextActiveNavigation.SelectSingleNode("a")?.GetAttributeValue<string>("href", null) : null;

            } while (nextButtonExist);

            var test = "";
        }

        public void SearchImage()
        {
            HtmlWeb web = new HtmlWeb();
            List<Creation> creations = context.Creations.Where(n => n.CreationType == (int)CreationType.Anime).ToList();
            foreach (var creation in creations)
            {
                try
                {
                    string link = creation.Link.Replace(Kickassanime.websiteLink + "/", "");
                    string requestString = Kickassanime.websiteLink + Kickassanime.apiEpisodeLink + link;
                    string siteJson = mainWindow.MakeRequest(requestString, Kickassanime.cuttenWebsiteLink);
                    SeriesResponse seriesResponse = JsonConvert.DeserializeObject<SeriesResponse>(siteJson);

                    string image = seriesResponse.Poster.Hq != null ? Kickassanime.websiteLink + Kickassanime.imagePath + seriesResponse.Poster.Hq + ".webp" : "No image";

                    if (creation.Image != image)
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
            return animeCreationWorker.IsBusy || animeEpisodeWorker.IsBusy || animeImageRefreshWorker.IsBusy;
        }

        public void RunWorker()
        {
            StopWorker = false;

            ReplaceDatabaseLinks();

            animeEpisodeWorker.RunWorkerAsync();

            mainWindow.animeEpisodeFilterDotImage.Visibility = Visibility.Visible;
            mainWindow.animeCreationFilterDotImage.Visibility = Visibility.Visible;
        }

        private void AnimeEpisodeWork(object sender, DoWorkEventArgs e)
        {
            SearchEpisode();
        }

        private void AnimeEpisodeWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

            mainWindow.animeEpisodeFilterDotImage.Visibility = Visibility.Hidden;

            animeCreationWorker.RunWorkerAsync();
        }

        private void AnimeCreationWork(object sender, DoWorkEventArgs e)
        {
            SearchAniwaveSite();
            SearchKickassSite();
        }

        private void AnimeCreationWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

            mainWindow.animeCreationFilterDotImage.Visibility = Visibility.Hidden;

            if (refreshImageNeeded)
            {
                animeImageRefreshWorker.RunWorkerAsync();
            }
        }

        public void RunRefreshImageWorker()
        {
            if (!IsWorkerRunning())
            {
                animeImageRefreshWorker.RunWorkerAsync();
            }
            else
            {
                refreshImageNeeded = true;
            }

            mainWindow.animeImageFilterDotImage.Visibility = Visibility.Visible;
        }

        private void AnimeImageRefreshWork(object sender, DoWorkEventArgs e)
        {
            SearchImage();
        }

        private void AnimeImageRefreshWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

            mainWindow.animeImageFilterDotImage.Visibility = Visibility.Hidden;
            refreshImageNeeded = false;
        }

        private void ReplaceDatabaseLinks()
        {
            try
            {
                var creations = context.Creations
                    .Where(c => c.SiteName == (int)SiteName.Kickassanime).ToList();

                foreach (var creation in creations)
                {
                    if (!creation.Link.StartsWith(Kickassanime.websiteLink, StringComparison.OrdinalIgnoreCase))
                    {
                        var uri = new Uri(creation.Link);
                        string slug = uri.AbsolutePath.TrimStart('/');

                        creation.Link = $"{Kickassanime.websiteLink}/{slug}";

                        if (!string.IsNullOrEmpty(creation.Image) && !creation.Image.StartsWith("data:image") &&
                            !creation.Image.StartsWith(Kickassanime.websiteLink, StringComparison.OrdinalIgnoreCase))
                        {
                            var imgFileName = creation.Image.Split('/').Last();
                            creation.Image = $"{Kickassanime.websiteLink}{Kickassanime.imagePath}{imgFileName}";
                        }
                    }

                    var episodes = creation.Bookmark.SelectMany(s => s.Episode).ToList();

                    foreach (var episode in episodes)
                    {
                        if (!episode.Link.StartsWith(Kickassanime.websiteLink, StringComparison.OrdinalIgnoreCase))
                        {
                            var uri = new Uri(creation.Link);
                            string slug = uri.AbsolutePath.TrimStart('/');

                            episode.Link = $"{Kickassanime.websiteLink}/{slug}";
                        }
                    }
                }

                context.SaveChanges();
            }
            catch (Exception e)
            {
                mainWindow.Dispatcher.Invoke(() =>
                {
                    mainWindow.exceptionListBox.Items.Add("ReplaceDatabaseLinks failed! Exception: " + e.Message);
                });
            }
        }

        public class SiteResponse
        {
            public List<Data> Result { get; set; }
        }

        public class Data
        {
            public string Slug { get; set; }
            public string Title { get; set; }
            public Poster Poster { get; set; }
        }

        public class Poster
        {
            public string Hq { get; set; }
        }

        public class SeriesResponse
        {
            public Poster Poster { get; set; }
            public string Watch_uri { get; set; }
        }

        public class EpisodeResponse
        {
            public List<PageData> Pages { get; set; }
            public List<EpisodeData> Result { get; set; }
        }

        public class PageData
        {
            public double Number { get; set; }
            public string From { get; set; }
            public string To { get; set; }
        }

        public class EpisodeData
        {
            public string Slug { get; set; }
            public double Episode_number { get; set; }
            public string Episode_string { get; set; }
        }

        public class AnimewaveEpisodeResponse
        {
            public int Status { get; set; }
            public string Result { get; set; }
        }

        public static class Kickassanime
        {
            public const string cuttenWebsiteLink = "kaa.lt";
            public const string websiteLink = "https://kaa.lt";
            public const string apiPath = "/api/anime?page=";
            public const string imagePath = "/image/poster/";

            public const string apiEpisodeLink = "/api/show/";
            public const string episodePath = "/episodes?";
            public const string pagePath = "ep=1?&page=";
            public const string languageJP = "lang=ja-JP";
            public const string languageEN = "lang=en-US";
        }

        public static class Aniwave
        {
            public const string cuttenWebsiteLink = "aniwaves.ru";
            public const string websiteLink = "https://aniwaves.ru";
            public const string websiteListLink = "https://aniwaves.ru/az-list";
            public const string contentPath = "/html/body/div[@id='wrapper']/div[@id='body']/div/div/aside[@class='main']/section/div[@class='body']/div[@id='list-items']/div[position()>0]";
            public const string nextButtonPath = "/html/body/div[@id='wrapper']/div[@id='body']/div/div/aside[@class='main']/section/div[@class='body']/nav/ul/li[position()>0]";
            public const string titlePath = "div/div[@class='info']/div[@class='b1']/a/text()";
            public const string linkPath = "div/div[@class='info']/div[@class='b1']/a";
            public const string imagePath = "div/div[@class='ani poster tip tooltipstered']/a/img";
            public const string imageSrc = "data-src";
            public const string imageFallbackSrc = "src";

            public const string episodeList = "//div[contains(@class, 'episodes')]//li/a";
            public const string episodeNumber = "a/b/text()";
            public const string episodeLink = "a";

            public const string apiEpisodeLink = "/ajax/episode/list/";
            public const string apiEpisodeLinkPart2 = "?style=&vrf=";
        }
    }
}