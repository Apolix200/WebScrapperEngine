using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using WebScrapperEngine.Entity;
using static WebScrapperEngine.Scrapper.DonghuaScrapper;

namespace WebScrapperEngine.Scrapper
{
    class AnimeScrapper
    {
        private MainWindow mainWindow;
        private Context context;

        private BackgroundWorker animeCreationWorker = new BackgroundWorker();
        private BackgroundWorker animeEpisodeWorker = new BackgroundWorker();
        private BackgroundWorker animeImageRefreshWorker = new BackgroundWorker();

        private bool refreshImageNeeded = false;

        public bool StopWorker { get; set; }

        public AnimeScrapper(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            context = new Context();

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
                string link = creation.Link.Replace(Kickassanime.websiteLink + "/", "");
                string requestString = Kickassanime.websiteLink + Kickassanime.apiEpisodeLink + link;
                string siteJson = mainWindow.MakeRequest(requestString, Kickassanime.cuttenWebsiteLink);
                SeriesResponse seriesResponse = JsonConvert.DeserializeObject<SeriesResponse>(siteJson);
                string nextEpisode = seriesResponse.Watch_uri.Replace("/" + link + "/", "");

                string request = Kickassanime.websiteLink + Kickassanime.apiEpisodeLink + link +  Kickassanime.episodePath + Kickassanime.languageJP;
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

                    if (i != pages) {
                        request = Kickassanime.websiteLink + Kickassanime.apiEpisodeLink + link + Kickassanime.episodePath + Kickassanime.pagePath + (i + 1) + "&" + Kickassanime.languageJP;
                        siteJson = mainWindow.MakeRequest(request, Kickassanime.cuttenWebsiteLink);
                        episodeResponse = JsonConvert.DeserializeObject<EpisodeResponse>(siteJson);
                    }
                }

                context.SaveChanges();

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
                    string link = bookmark.Creation.Link.Replace(Kickassanime.websiteLink + "/", "");
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
                            if (!context.Episodes.Any(n => n.Bookmark.Creation.SiteName == bookmark.Creation.SiteName
                            && n.Bookmark.Creation.Title == bookmark.Creation.Title && n.EpisodeNumber == data.Episode_number))
                            {
                                context.Episodes.Add(new Episode()
                                {
                                    BookmarkId = bookmark.BookmarkId,
                                    EpisodeNumber = data.Episode_number,
                                    Link = bookmark.Creation.Link + "/ep-" + data.Episode_string + "-" + data.Slug,
                                    WatchStatus = data.Episode_number <= 1 ? (int)WatchStatus.NextWatch : (int)WatchStatus.NeedToWatch
                                });

                                context.Bookmarks.FirstOrDefault(n => n.BookmarkId == bookmark.BookmarkId).UpdatedAt = DateTime.Now;
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
                catch (Exception e)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.exceptionListBox.Items.Add("Episode search of anime failed! Exception: " + e.Message);
                    });
                }
            }
        }

        public void SearchKickassSite()
        {
            SiteResponse siteResponse = null;
            string siteJson = "";
            int index = 1;

            do
            {
                if (StopWorker) { break; }

                try 
                {
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
                }
                catch (Exception e)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.exceptionListBox.Items.Add("Creation search of anime failed! Exception: " + e.Message);
                    });
                }

                index++;

            } while (siteResponse.Result.Count() > 0);
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

        public static class Kickassanime
        {
            public const string cuttenWebsiteLink = "kickassanime.am";
            public const string websiteLink = "https://kickassanime.am";
            public const string apiPath = "/api/anime?page=";
            public const string imagePath = "/image/poster/";
            public const string apiEpisodeLink = "/api/show/";
            public const string episodePath = "/episodes?";
            public const string pagePath = "ep=1?&page=";
            public const string languageJP = "lang=ja-JP";
            public const string languageEN = "lang=en-US";
        }
    }
}