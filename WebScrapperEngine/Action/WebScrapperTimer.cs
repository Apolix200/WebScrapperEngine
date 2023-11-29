﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WebScrapperEngine.Scrapper;

namespace WebScrapperEngine.Action
{
    class WebScrapperTimer
    {
        private MainWindow mainWindow;
        private DispatcherTimer timer;

        private DonghuaScrapper donghuaScrapper;
        private AnimeScrapper animeScrapper;
        private MangaScrapper mangaScrapper;
        private WebtoonScrapper webtoonScrapper;

        public WebScrapperTimer(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            donghuaScrapper = new DonghuaScrapper(mainWindow);
            animeScrapper = new AnimeScrapper(mainWindow);
            mangaScrapper = new MangaScrapper(mainWindow);
            webtoonScrapper = new WebtoonScrapper(mainWindow);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += WebScrapperTimer_Tick;
        }

        public void StopTimer()
        {
            donghuaScrapper.StopWorker();

            mainWindow.restartButton.Background = (System.Windows.Media.Brush)mainWindow.Resources["LightGreenBrush"];
            timer.Stop();
        }


        public void StartTimer() 
        {
            if (RestartIsEnabled())
            {
                timer.Start();

                mainWindow.restartButton.Background = (System.Windows.Media.Brush)mainWindow.Resources["RedBrush"];
            }
        }

        private void WebScrapperTimer_Tick(object sender, EventArgs e)
        {
            if (CheckForInternetConnection())
            {
                donghuaScrapper.RunWorker();
                animeScrapper.RunWorker();
                mangaScrapper.RunWorker();

                //webtoonScrapper.RunWorker();

                mainWindow.restartButton.Background = (System.Windows.Media.Brush)mainWindow.Resources["LightGreenBrush"];
                timer.Stop();
            }
        }

        public void RefreshImageStart()
        {
            if (CheckForInternetConnection())
            {
                donghuaScrapper.RunRefreshImageWorker();
                //animeScrapper.RunRefreshImageWorker();
                //mangaScrapper.RunRefreshImageWorker();
            }

        }

        public bool RestartIsEnabled ()
        {
            return !timer.IsEnabled && !donghuaScrapper.IsWorkerRunning() && !animeScrapper.IsWorkerRunning() 
                && !mangaScrapper.IsWorkerRunning() && !webtoonScrapper.IsWorkerRunning();
        }

        private static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
