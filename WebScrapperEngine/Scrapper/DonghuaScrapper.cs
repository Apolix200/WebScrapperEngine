using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WebScrapperEngine.Entity;

namespace WebScrapperEngine.Scrapper
{
    class DonghuaScrapper
    {
        private MainWindow mainWindow;
        private HtmlWeb web;
        private Context context;

        private BackgroundWorker donghuaCreationWorker = new BackgroundWorker();
        private BackgroundWorker donghuaEpisodeWorker = new BackgroundWorker();

        public DonghuaScrapper(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            web = new HtmlWeb();
            context = new Context();

            donghuaEpisodeWorker.DoWork += DonghuaEpisodeWork;
            donghuaEpisodeWorker.RunWorkerCompleted += DonghuaEpisodeWorkCompleted;

            donghuaCreationWorker.DoWork += DonghuaCreationWork;
            donghuaCreationWorker.RunWorkerCompleted += DonghuaCreationWorkCompleted;
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
            catch (Exception e)
            {
                mainWindow.exceptionListBox.Items.Add("Bookmark of donghua failed! Exception: " + e.GetType().Name);
            }
            
        }

        public void SearchEpisode()
        {
            List<Bookmark> bookmarks = context.Bookmarks.Where(n => n.Creation.CreationType == (int)CreationType.Donghua).ToList();

            foreach (var bookmark in bookmarks)
            {
                HtmlWeb web = new HtmlWeb();
                var doc = web.Load(bookmark.Creation.Link);

                HtmlNodeCollection nodes;
                switch ((SiteName)bookmark.Creation.SiteName)
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
                    foreach (var node in nodes)
                    {
                        string episodeNumber;
                        string episodeLink;

                        switch ((SiteName)bookmark.Creation.SiteName)
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
                        }
                    }
                    context.SaveChanges();
                    mainWindow.CorrectWatchStatus(bookmark);
                }
                catch (Exception e)
                {
                    mainWindow.exceptionListBox.Items.Add("Episode search of donghua failed! Exception: " + e.GetType().Name);
                }

            }
        }

        public void SearchNaruldonghuaSite()
        {
            string websiteLink = Naruldonghua.websiteLink;
            bool nextButtonExist = true;

            do
            {
                var doc = web.Load(websiteLink);
                var nodes = doc.DocumentNode.SelectNodes(Naruldonghua.contentPath);

                try
                {
                    foreach (var node in nodes)
                    {
                        var donghuaCreation = new Creation()
                        {
                            CreationType = (int)CreationType.Donghua,
                            SiteName = (int)SiteName.Naruldonghua,
                            Title = node.SelectSingleNode(Naruldonghua.titlePath) != null ? Regex.Replace(node.SelectSingleNode(Naruldonghua.titlePath).InnerText, @"[^0-9a-zA-Z]+", "") : null,
                            Link = node.SelectSingleNode(Naruldonghua.linkPath).GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Naruldonghua.linkPath).GetAttributeValue<string>("href", null) : null,
                            Image = node.SelectSingleNode(Naruldonghua.imagePath).Attributes[Naruldonghua.imageSrc].Value != null ? node.SelectSingleNode(Naruldonghua.imagePath).Attributes[Naruldonghua.imageSrc].Value : null,
                            NewStatus = (int)NewStatus.New
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
                catch (Exception e)
                {
                    mainWindow.exceptionListBox.Items.Add("Creation search of naruldonghua failed! Exception: " + e.GetType().Name);
                }

                nextButtonExist = doc.DocumentNode.SelectSingleNode(Naruldonghua.nextButtonPath) != null;

                websiteLink = nextButtonExist ? doc.DocumentNode.SelectSingleNode(Naruldonghua.nextButtonPath).GetAttributeValue<string>("href", null) : null;

            } while (nextButtonExist);

        }

        public void SearchAnimexinSite()
        {
            string websiteLink = Animexin.websiteLink;
            bool nextButtonExist = true;

            do
            {
                var doc = web.Load(websiteLink);
                var nodes = doc.DocumentNode.SelectNodes(Animexin.contentPath);

                try
                {
                    foreach (var node in nodes)
                    {
                        var donghuaCreation = new Creation()
                        {
                            CreationType = (int)CreationType.Donghua,
                            SiteName = (int)SiteName.Animexin,
                            Title = node.SelectSingleNode(Animexin.titlePath) != null ? Regex.Replace(node.SelectSingleNode(Animexin.titlePath).InnerText, @"[^0-9a-zA-Z]+", "") : null,
                            Link = node.SelectSingleNode(Animexin.linkPath).GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Animexin.linkPath).GetAttributeValue<string>("href", null) : null,
                            Image = node.SelectSingleNode(Animexin.imagePath).Attributes[Animexin.imageSrc].Value != null ? node.SelectSingleNode(Animexin.imagePath).Attributes[Animexin.imageSrc].Value : null,
                            NewStatus = (int)NewStatus.New
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
                catch (Exception e)
                {
                    mainWindow.exceptionListBox.Items.Add("Creation search of animexin failed! Exception: " + e.GetType().Name);
                }

                nextButtonExist = doc.DocumentNode.SelectSingleNode(Animexin.nextButtonPath) != null;

                websiteLink = nextButtonExist ? doc.DocumentNode.SelectSingleNode(Animexin.nextButtonPath).GetAttributeValue<string>("href", null) : null;

            } while (nextButtonExist);
        }

        public void RunWorker()
        {
            donghuaEpisodeWorker.RunWorkerAsync();
        }
        private void DonghuaEpisodeWork(object sender, DoWorkEventArgs e)
        {
            SearchEpisode();
        }

        private void DonghuaEpisodeWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

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

            mainWindow.donghuaFilterDotImage.Visibility = Visibility.Hidden;
        }

        public static class Naruldonghua
        {
            public const string websiteLink = "https://naruldonghua.com";
            public const string contentPath = "/html/body/div[3]/div/div[3]/div[3]/div[2]/div[1]/article[position()>0]";
            public const string nextButtonPath = "/html/body/div[3]/div/div[3]/div[3]/div[2]/div[@class='hpage']/a[@class='r']";
            public const string titlePath = "div/a/div[@class='tt']/text()";
            public const string linkPath = "div/a";
            public const string linkToSeriesPath = "/html/body/div[3]/div/div[4]/article/div[2]/div/div[1]/div[2]/span[2]/a";
            public const string imagePath = "div/a/div[@class='limit']/img";
            public const string imageSrc = "data-src";

            public const string episodeList = "/html/body/div[3]/div/div[3]/article/div[@class='bixbox bxcl epcheck']/div[@class='eplister']/ul/li[position()>0]";
            public const string episodeNumber = "a/div[@class='epl-num']";
            public const string episodeLink = "a";
        }

        public static class Animexin
        {
            public const string websiteLink = "https://animexin.xyz";
            public const string apiPath = "wp-content/cache/gov-cache/ajax/53293a78ade50ac049948a6705a6725e.json?time=1657288651";
            public const string contentPath = "/html/body/div[3]/div/div[3]/div[3]/div[2]/div[1]/article[position()>0]";
            public const string nextButtonPath = "/html/body/div[3]/div/div[3]/div[3]/div[2]/div[@class='hpage']/a[@class='r']";
            public const string titlePath = "div/a/div[@class='limit']/div[@class='egghead']/div[@class='eggtitle']/text()";
            public const string linkToSeriesPath = "/html/body/div[3]/div/div[4]/article/div[2]/div/div[1]/div[2]/span[2]/a";
            public const string linkPath = "div/a";
            public const string imagePath = "div/a/div[@class='limit']/img";
            public const string imageSrc = "src";

            public const string episodeList = "/html/body/div[3]/div/div[3]/article/div[@class='bixbox bxcl epcheck']/div[@class='eplister']/ul/li[position()>0]";
            public const string episodeNumber = "a/div[@class='epl-num']";
            public const string episodeLink = "a";
        }
    }
}