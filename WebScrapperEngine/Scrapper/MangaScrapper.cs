using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using WebScrapperEngine.Entity;

namespace WebScrapperEngine.Scrapper
{
    class MangaScrapper
    {
        private MainWindow mainWindow;
        private Context context;

        private BackgroundWorker mangaCreationWorker = new BackgroundWorker();
        private BackgroundWorker mangaEpisodeWorker = new BackgroundWorker();

        public MangaScrapper(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            context = new Context();

            mangaEpisodeWorker.DoWork += MangaEpisodeWork;
            mangaEpisodeWorker.RunWorkerCompleted += MangaEpisodeWorkCompleted;

            mangaCreationWorker.DoWork += MangaCreationWork;
            mangaCreationWorker.RunWorkerCompleted += MangaCreationWorkCompleted;
        }

        public void BookmarkEpisode(Creation creation, Bookmark bookmark)
        {
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load(creation.Link);

            string nodeText = doc.DocumentNode.SelectSingleNode(Mangasee.episodeList).InnerHtml;
            int firstStringPosition = nodeText.IndexOf("vm.Chapters") + 14;
            int secondStringPosition = nodeText.IndexOf("vm.NumSubs") - 6;

            string siteJson = nodeText.Substring(firstStringPosition, secondStringPosition - firstStringPosition);
            var siteResponse = JsonConvert.DeserializeObject<List<EpisodeData>>(siteJson);

            try 
            { 
                foreach (var data in siteResponse)
                {
                    double timesOf = Math.Floor(Convert.ToDouble(data.Chapter) / 100000);
                    double episodeNumberFonLinQ = (Convert.ToDouble(data.Chapter) - 100000 * timesOf) / 10;

                    if (!context.Episodes.Any(n => n.Bookmark.Creation.SiteName == creation.SiteName
                    && n.Bookmark.Creation.Title == creation.Title && n.EpisodeNumber == episodeNumberFonLinQ))
                    {
                        context.Episodes.Add(new Episode()
                        {
                            BookmarkId = bookmark.BookmarkId,
                            EpisodeNumber = episodeNumberFonLinQ,
                            Link = creation.Link.Replace("/manga/", "/read-online/") + "-chapter-" + episodeNumberFonLinQ.ToString(new CultureInfo("en-US")) +
                            (timesOf > 1 ? "-index-" + timesOf : "") + ".html",
                            WatchStatus = episodeNumberFonLinQ <= 1 ? (int)WatchStatus.NextWatch : (int)WatchStatus.NeedToWatch
                        });
                    }
                }
                context.SaveChanges();
            }
            catch (Exception e)
            {
                mainWindow.exceptionListBox.Items.Add("Bookmark of manga failed! Exception: " + e.GetType().Name);
            }
        }

        public void SearchEpisode()
        {
            List<Bookmark> bookmarks = context.Bookmarks.Where(n => n.Creation.CreationType == (int)CreationType.Manga).ToList();

            foreach (var bookmark in bookmarks)
            {
                HtmlWeb web = new HtmlWeb();
                var doc = web.Load(bookmark.Creation.Link);

                string nodeText = doc.DocumentNode.SelectSingleNode(Mangasee.episodeList).InnerHtml;
                int firstStringPosition = nodeText.IndexOf("vm.Chapters") + 14;
                int secondStringPosition = nodeText.IndexOf("vm.NumSubs") - 6;

                string siteJson = nodeText.Substring(firstStringPosition, secondStringPosition - firstStringPosition);
                var siteResponse = JsonConvert.DeserializeObject<List<EpisodeData>>(siteJson);

                try 
                {
                    foreach (var data in siteResponse)
                    {
                        double timesOf = Math.Floor(Convert.ToDouble(data.Chapter) / 100000);
                        double episodeNumberFonLinQ = (Convert.ToDouble(data.Chapter) - 100000 * timesOf) / 10;

                        if (!context.Episodes.Any(n => n.Bookmark.Creation.SiteName == bookmark.Creation.SiteName
                        && n.Bookmark.Creation.Title == bookmark.Creation.Title && n.EpisodeNumber == episodeNumberFonLinQ))
                        {
                            context.Episodes.Add(new Episode()
                            {
                                BookmarkId = bookmark.BookmarkId,
                                EpisodeNumber = episodeNumberFonLinQ,
                                Link = bookmark.Creation.Link.Replace("/manga/", "/read-online/") + "-chapter-" + episodeNumberFonLinQ.ToString(new CultureInfo("en-US")) +
                                (timesOf > 1 ? "-index-" + timesOf : "") + ".html",
                                WatchStatus = episodeNumberFonLinQ <= 1 ? (int)WatchStatus.NextWatch : (int)WatchStatus.NeedToWatch
                            });
                        }
                    }
                    context.SaveChanges();
                    mainWindow.CorrectWatchStatus(bookmark);
                }
                catch (Exception e)
                {
                    mainWindow.exceptionListBox.Items.Add("Episode search of manga failed! Exception: " + e.GetType().Name);
                }
            }
        }


        public void SearchMangaseeSite()
        {
            List<Creation> creations = new List<Creation>();

            try 
            {
                string siteJson = mainWindow.MakeRequest(Mangasee.websiteLink + Mangasee.apiPath, Mangasee.cuttenWebsiteLink);
                var siteResponse = JsonConvert.DeserializeObject<List<Data>>(siteJson);

                foreach (var data in siteResponse)
                {
                    var mangaCreation = new Creation()
                    {
                        CreationType = (int)CreationType.Manga,
                        SiteName = (int)SiteName.Mangasee,
                        Title = data.S != null ? Regex.Replace(data.S, @"[^0-9a-zA-Z]+", "") : "No name",
                        Link = data.I != null ? Mangasee.websiteLink + Mangasee.linkPath + data.I : "No link",
                        Image = Mangasee.imagePath + data.I + ".jpg",
                        NewStatus = (int)NewStatus.New
                    };

                    if (!context.Creations.Any(n => n.SiteName == mangaCreation.SiteName && n.Title == mangaCreation.Title))
                    {
                        creations.Add(mangaCreation);
                    }
                }

                context.BulkInsert(creations);
            }
            catch (Exception e)
            {
                mainWindow.exceptionListBox.Items.Add("Creation search of manga failed! Exception: " + e.GetType().Name);
            }
        }

        public void RunWorker()
        {
            mangaEpisodeWorker.RunWorkerAsync();
        }

        private void MangaEpisodeWork(object sender, DoWorkEventArgs e)
        {
            SearchEpisode();
        }

        private void MangaEpisodeWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

            mainWindow.mangaEpisodeFilterDotImage.Visibility = Visibility.Hidden;

            mangaCreationWorker.RunWorkerAsync();
        }

        private void MangaCreationWork(object sender, DoWorkEventArgs e)
        {
            SearchMangaseeSite();
        }

        private void MangaCreationWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

            mainWindow.mangaCreationFilterDotImage.Visibility = Visibility.Hidden;
        }

        public class Data
        {
            public string I { get; set; }
            public string S { get; set; }
            public List<string> A { get; set; }
        }
        public class EpisodeData
        {
            public string Chapter { get; set; }
            public string Type { get; set; }
            public string Date { get; set; }
            public string ChapterName { get; set; }
        }
        public static class Mangasee
        {
            public const string cuttenWebsiteLink = "mangasee123.com";
            public const string websiteLink = "https://mangasee123.com";
            public const string apiPath = "/_search.php";
            public const string linkPath = "/manga/";
            public const string imagePath = "https://temp.compsci88.com/cover/";

            public const string episodeList = "/html/body/script[10]/text()";
        }
    }
}