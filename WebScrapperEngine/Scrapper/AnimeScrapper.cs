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

namespace WebScrapperEngine.Scrapper
{
    class AnimeScrapper
    {
        private MainWindow mainWindow;
        private Context context;

        private BackgroundWorker animeCreationWorker = new BackgroundWorker();
        private BackgroundWorker animeEpisodeWorker = new BackgroundWorker();

        public AnimeScrapper(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            context = new Context();

            animeEpisodeWorker.DoWork += AnimeEpisodeWork;
            animeEpisodeWorker.RunWorkerCompleted += AnimeEpisodeWorkCompleted;

            animeCreationWorker.DoWork += AnimeCreationWork;
            animeCreationWorker.RunWorkerCompleted += AnimeCreationWorkCompleted;
        }

        public void BookmarkEpisode(Creation creation, Bookmark bookmark)
        {
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load(creation.Link);

            string nodeText = doc.DocumentNode.SelectSingleNode(Kickassanime.episodeList).InnerHtml;
            int firstStringPosition = nodeText.IndexOf("\"episodes\"") + 11;
            int secondStringPosition = nodeText.IndexOf("types") - 2;

            string siteJson = nodeText.Substring(firstStringPosition, secondStringPosition - firstStringPosition);
            var siteResponse = JsonConvert.DeserializeObject<List<EpisodeData>>(siteJson);

            try
            {
                foreach (var data in siteResponse)
                {
                    double episodeNumberFonLinQ = Convert.ToDouble(data.Epnum.Split(' ')[1]);
                    if (!context.Episodes.Any(n => n.Bookmark.Creation.SiteName == creation.SiteName
                    && n.Bookmark.Creation.Title == creation.Title && n.EpisodeNumber == episodeNumberFonLinQ))
                    {
                        context.Episodes.Add(new Episode()
                        {
                            BookmarkId = bookmark.BookmarkId,
                            EpisodeNumber = episodeNumberFonLinQ,
                            Link = Kickassanime.websiteLink + data.Slug,
                            WatchStatus = episodeNumberFonLinQ <= 1 ? (int)WatchStatus.NextWatch : (int)WatchStatus.NeedToWatch
                        });
                    }
                }
                context.SaveChanges();
            }
            catch (Exception e)
            {
                mainWindow.exceptionListBox.Items.Add("Bookmark of anime failed! Exception: " + e.GetType().Name);
            }
        }

        public void SearchEpisode()
        {
            List<Bookmark> bookmarks = context.Bookmarks.Where(n => n.Creation.CreationType == (int)CreationType.Anime).ToList();

            foreach (var bookmark in bookmarks)
            {
                HtmlWeb web = new HtmlWeb();
                var doc = web.Load(bookmark.Creation.Link);

                string nodeText = doc.DocumentNode.SelectSingleNode(Kickassanime.episodeList).InnerHtml;
                int firstStringPosition = nodeText.IndexOf("\"episodes\"") + 11;
                int secondStringPosition = nodeText.IndexOf("types") - 2;

                string siteJson = nodeText.Substring(firstStringPosition, secondStringPosition - firstStringPosition);
                var siteResponse = JsonConvert.DeserializeObject<List<EpisodeData>>(siteJson);

                try
                {

                    foreach (var data in siteResponse)
                    {
                        double episodeNumberFonLinQ = Convert.ToDouble(data.Epnum.Split(' ')[1]);
                        if (!context.Episodes.Any(n => n.Bookmark.Creation.SiteName == bookmark.Creation.SiteName
                        && n.Bookmark.Creation.Title == bookmark.Creation.Title && n.EpisodeNumber == episodeNumberFonLinQ))
                        {
                            context.Episodes.Add(new Episode()
                            {
                                BookmarkId = bookmark.BookmarkId,
                                EpisodeNumber = episodeNumberFonLinQ,
                                Link = Kickassanime.websiteLink + data.Slug,
                                WatchStatus = episodeNumberFonLinQ <= 1 ? (int)WatchStatus.NextWatch : (int)WatchStatus.NeedToWatch
                            });
                        }
                    }
                    context.SaveChanges();
                    mainWindow.CorrectWatchStatus(bookmark);
                }
                catch (Exception e)
                {
                    mainWindow.exceptionListBox.Items.Add("Episode search of anime failed! Exception: " + e.GetType().Name);
                }
            }
        }

        public void SearchKickassSite()
        {
            //List<Creation> creations = new List<Creation>();
            SiteResponse siteResponse;
            string siteJson;
            int index = 1;

            do
            {
                siteJson = mainWindow.MakeRequest(Kickassanime.websiteLink + Kickassanime.apiPath + index, Kickassanime.cuttenWebsiteLink);
                siteResponse = JsonConvert.DeserializeObject<SiteResponse>(siteJson);

                try 
                { 
                    foreach (var data in siteResponse.Data)
                    {
                        var animeCreation = new Creation()
                        {
                            CreationType = (int)CreationType.Anime,
                            SiteName = (int)SiteName.Kickassanime,
                            Title = data.Name != null ? Regex.Replace(data.Name, @"[^0-9a-zA-Z]+", "") : "No name",
                            Link = data.Slug != null ? Regex.Replace(Kickassanime.websiteLink + data.Slug, @"([^\/]+$)", "") : "No link",
                            Image = data.Poster != null ? Kickassanime.websiteLink + Kickassanime.imagePath + data.Poster : "No image",
                            NewStatus = (int)NewStatus.New
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
                    mainWindow.exceptionListBox.Items.Add("Creation search of anime failed! Exception: " + e.GetType().Name);
                }

                index++;

            } while (siteResponse.Data.Count() > 0);
        }

        public void RunWorker()
        {
            animeEpisodeWorker.RunWorkerAsync();
        }
        private void AnimeEpisodeWork(object sender, DoWorkEventArgs e)
        {
            SearchEpisode();
        }

        private void AnimeEpisodeWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

            animeCreationWorker.RunWorkerAsync();
        }

        private void AnimeCreationWork(object sender, DoWorkEventArgs e)
        {
            SearchKickassSite();
        }

        private void AnimeCreationWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

            mainWindow.animeFilterDotImage.Visibility = Visibility.Hidden;
        }

        public class SiteResponse
        {
            public List<Data> Data { get; set; }
            public string Page { get; set; }
        }

        public class Data
        {
            public string Episode { get; set; }
            public string Slug { get; set; }
            public string Type { get; set; }
            public string Episode_date { get; set; }
            public string Name { get; set; }
            public string Poster { get; set; }
        }

        public class EpisodeData
        {
            public string Epnum { get; set; }
            public string Name { get; set; }
            public string Slug { get; set; }
            public string Createddate { get; set; }
            public string Num { get; set; }
        }

        public static class Kickassanime
        {
            public const string cuttenWebsiteLink = "www2.kickassanime.ro";
            public const string websiteLink = "https://www2.kickassanime.ro";
            public const string apiPath = "/api/get_anime_list/all/";
            public const string imagePath = "/uploads/";
            public const string linkToSeriesPath = "/html/body/div[1]/div[1]/div[1]/div/aside/div/div[1]/div/a";

            public const string episodeList = "/html/body/script[4]/text()";
        }
    }
}