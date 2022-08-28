using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
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


        public WebScrapperTimer(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            donghuaScrapper = new DonghuaScrapper(mainWindow);
            animeScrapper = new AnimeScrapper(mainWindow);
            mangaScrapper = new MangaScrapper(mainWindow);

            timer = new DispatcherTimer();
        }

        public void StartTimer() 
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += WebScrapperTimer_Tick;
            timer.Start();

            mainWindow.restartButton.GetType().GetProperty("Background").SetValue(mainWindow.restartButton, mainWindow.Resources["OrangeBrush"] as Brush);
        }

        private void WebScrapperTimer_Tick(object sender, EventArgs e)
        {

            if (CheckForInternetConnection() && !donghuaScrapper.IsWorkerRunning() && !animeScrapper.IsWorkerRunning() && !mangaScrapper.IsWorkerRunning())
            {
                donghuaScrapper.RunWorker();
                animeScrapper.RunWorker();
                mangaScrapper.RunWorker();

                mainWindow.restartButton.GetType().GetProperty("Background").SetValue(mainWindow.restartButton, mainWindow.Resources["LightGreenBrush"] as Brush);
                timer.Stop();
            }
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
