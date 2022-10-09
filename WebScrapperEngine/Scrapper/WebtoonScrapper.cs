using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WebScrapperEngine.Entity;

namespace WebScrapperEngine.Scrapper
{
    class WebtoonScrapper
    {
        private MainWindow mainWindow;
        private HtmlWeb web;
        private Context context;

        private BackgroundWorker webtoonCreationWorker = new BackgroundWorker();
        private BackgroundWorker webtoonEpisodeWorker = new BackgroundWorker();

        public WebtoonScrapper(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            web = new HtmlWeb();
            context = new Context();

            webtoonEpisodeWorker.DoWork += WebtoonEpisodeWork;
            webtoonEpisodeWorker.RunWorkerCompleted += WebtoonEpisodeWorkCompleted;

            webtoonCreationWorker.DoWork += WebtoonCreationWork;
            webtoonCreationWorker.RunWorkerCompleted += WebtoonCreationWorkCompleted;
        }
        public void BookmarkEpisode(Creation creation, Bookmark bookmark)
        {
        }

        public void SearchEpisode()
        {
        }

        public void SearchWebtoonSite()
        {
            string websiteLink = Webtoonxyz.websiteLink;
            bool nextButtonExist = true;

            do
            {
                var doc = web.Load(websiteLink);
                var nodes = doc.DocumentNode.SelectNodes(Webtoonxyz.contentPath);

                try
                {
                    if (nodes != null)
                    {
                        foreach (var node in nodes)
                        {
                            MessageBox.Show(node.ToString());

                            //var donghuaCreation = new Creation()
                            //{
                            //    CreationType = (int)CreationType.Webtoon,
                            //    SiteName = (int)SiteName.Webtoonxyz,
                            //    Title = node.SelectSingleNode(Animexin.titlePath).InnerText != null ? Regex.Replace(node.SelectSingleNode(Animexin.titlePath).InnerText, @"[^0-9a-zA-Z]+", "") : null,
                            //    Link = node.SelectSingleNode(Animexin.linkPath).GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Animexin.linkPath).GetAttributeValue<string>("href", null) : null,
                            //    Image = node.SelectSingleNode(Animexin.imagePath).Attributes[Animexin.imageSrc].Value != null ? node.SelectSingleNode(Animexin.imagePath).Attributes[Animexin.imageSrc].Value : null,
                            //    NewStatus = (int)NewStatus.New,
                            //    UpdatedAt = DateTime.Now
                            //};

                            //if (!context.Creations.Any(n => n.SiteName == donghuaCreation.SiteName && n.Title == donghuaCreation.Title))
                            //{
                            //    var seriesEpisodeDoc = web.Load(donghuaCreation.Link);
                            //    donghuaCreation.Link = seriesEpisodeDoc.DocumentNode.SelectSingleNode(Animexin.linkToSeriesPath) != null ? seriesEpisodeDoc.DocumentNode.SelectSingleNode(Animexin.linkToSeriesPath).GetAttributeValue<string>("href", null) : donghuaCreation.Link;

                            //    context.Creations.Add(donghuaCreation);
                            //    context.SaveChanges();
                            //}
                        }
                    }
                }
                catch (Exception e)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.exceptionListBox.Items.Add("Creation search of webtoonxyz failed! Exception: " + e.Message);
                    });
                }

                //nextButtonExist = doc.DocumentNode.SelectSingleNode(Animexin.nextButtonPath) != null;

                //websiteLink = nextButtonExist ? doc.DocumentNode.SelectSingleNode(Animexin.nextButtonPath).GetAttributeValue<string>("href", null) : null;

            } while (!nextButtonExist);
        }

        public bool IsWorkerRunning()
        {
            return webtoonCreationWorker.IsBusy || webtoonEpisodeWorker.IsBusy;
        }

        public void RunWorker()
        {
            webtoonEpisodeWorker.RunWorkerAsync();

            mainWindow.webtoonEpisodeFilterDotImage.Visibility = Visibility.Visible;
            mainWindow.webtoonCreationFilterDotImage.Visibility = Visibility.Visible;
        }

        private void WebtoonEpisodeWork(object sender, DoWorkEventArgs e)
        {
            SearchEpisode();
        }

        private void WebtoonEpisodeWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

            mainWindow.webtoonEpisodeFilterDotImage.Visibility = Visibility.Hidden;

            webtoonCreationWorker.RunWorkerAsync();
        }

        private void WebtoonCreationWork(object sender, DoWorkEventArgs e)
        {
            SearchWebtoonSite();
        }

        private void WebtoonCreationWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.LoadCreationsAndEpisodes();

            mainWindow.webtoonCreationFilterDotImage.Visibility = Visibility.Hidden;
        }
        public static class Webtoonxyz
        {
            public const string websiteLink = "https://www.webtoon.xyz";
            public const string contentPath = "/html/body/div[1]/div/div[2]/div[2]/div/div/div/div[1]/div[2]/div/div[2]/div[2]/div/div[1]/div[position()>0]";

            public const string nextButtonPath = "/html/body/div[3]/div/div[3]/div[3]/div[2]/div[@class='hpage']/a[@class='r']";
            public const string titlePath = "div/a/div[@class='limit']/div[@class='egghead']/div[@class='eggtitle']/text()";
            public const string linkToSeriesPath = "/html/body/div[3]/div/div[4]/article/div[2]/div/div[1]/div[2]/span[2]/a";
            public const string linkPath = "div/a";
            public const string imagePath = "div/a/div[@class='limit']/img";
            public const string imageSrc = "src";

            public const string episodeList = "/html/body/div[@id='content']/div/div[@class='postbody']/article/div[@class='bixbox bxcl epcheck']/div[@class='eplister']/ul/li[position()>0]";
            public const string episodeNumber = "a/div[@class='epl-num']";
            public const string episodeLink = "a";
        }
    }
}
