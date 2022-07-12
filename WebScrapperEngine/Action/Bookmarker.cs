using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using WebScrapperEngine.Entity;
using WebScrapperEngine.Scrapper;
using static WebScrapperEngine.Scrapper.AnimeScrapper;
using static WebScrapperEngine.Scrapper.DonghuaScrapper;
using static WebScrapperEngine.Scrapper.MangaScrapper;

namespace WebScrapperEngine.Action
{
    class Bookmarker
    {
        private MainWindow mainWindow;
        private HtmlWeb web;
        private Context context;

        private DonghuaScrapper donghuaScrapper;
        private AnimeScrapper animeScrapper;
        private MangaScrapper mangaScrapper;

        public Bookmarker(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            web = new HtmlWeb();
            context = new Context();

            donghuaScrapper = new DonghuaScrapper(mainWindow);
            animeScrapper = new AnimeScrapper(mainWindow);
            mangaScrapper = new MangaScrapper(mainWindow);
        }

        public void BookmarkCreation(Creation creation)
        {
            if (!context.Bookmarks.Any(n => n.CreationId == creation.CreationId))
            {
                Bookmark bookmark = new Bookmark()
                {
                    CreationId = creation.CreationId,
                    ConnectedId = null
                };

                try
                {
                    context.Bookmarks.Add(bookmark);
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    mainWindow.exceptionListBox.Items.Add("Bookmark creation failed! Exception: " + e.GetType().Name);
                }

                switch ((SiteName)creation.SiteName)
                {
                    case SiteName.Naruldonghua:
                        donghuaScrapper.BookmarkEpisode(creation, bookmark);
                        break;
                    case SiteName.Animexin:
                        donghuaScrapper.BookmarkEpisode(creation, bookmark);
                        break;
                    case SiteName.Kickassanime:
                        animeScrapper.BookmarkEpisode(creation, bookmark);
                        break;
                    case SiteName.Mangasee:
                        mangaScrapper.BookmarkEpisode(creation, bookmark);
                        break;
                    default:
                        break;
                }
            }
        }

        public void DeleteBookmarkCreation(Bookmark bookmark)
        {
            if (context.Bookmarks.Any(n => n.CreationId == bookmark.CreationId))
            {
                try
                {
                    context.Bookmarks.Remove(context.Bookmarks.FirstOrDefault(n => n.CreationId == bookmark.CreationId));

                    foreach (var episode in context.Episodes.Where(n => n.BookmarkId == bookmark.BookmarkId))
                    {
                        context.Episodes.Remove(episode);
                    }

                    context.SaveChanges();
                }
                    catch (Exception e)
                {
                    mainWindow.exceptionListBox.Items.Add("Bookmark deletion failed! Exception: " + e.GetType().Name);
                }
        }
        }
    }
}
