using HtmlAgilityPack;
using Newtonsoft.Json;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using WebScrapperEngine.Action;
using WebScrapperEngine.Entity;
using static WebScrapperEngine.Scrapper.AnimeScrapper;
using static WebScrapperEngine.Scrapper.DonghuaScrapper;

namespace WebScrapperEngine.Scrapper
{
    class WebtoonScrapper
    {
        private MainWindow mainWindow;
        private HtmlWeb web;
        private Context context;

        private BackgroundWorker webtoonCreationWorker = new BackgroundWorker();
        private BackgroundWorker webtoonEpisodeWorker = new BackgroundWorker();

        public bool StopWorker { get; set; }

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
            public const string websiteLink = "http://www.webtoon.xyz";
            public const string websiteR18Path = "/?s&post_type=wp-manga&adult=1&m_orderby=latest";
            public const string contentPath2 = "/html/body/div[1]/div/div[3]/div[2]/div/div/div/div/div[2]/div/div[2]/div/div[1]";
            public const string contentPath = "/html/body/div[@class='wrap']/div/div[@class='site-content']/div[@class='c-page-content']/div/div/div/div/div[@class='main-col-inner']/div/div[@class='tab-content-wrap']/div/div[position()>0]";
        }
    }
}
