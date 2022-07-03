using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapperEngine.Entity;
using static WebScrapperEngine.Scrapper.AnimeScrapper;
using static WebScrapperEngine.Scrapper.DonghuaScrapper;
using static WebScrapperEngine.Scrapper.MangaScrapper;

namespace WebScrapperEngine.Action
{
    class Bookmarker
    {
        private HtmlWeb web;
        private Context context;

        public Bookmarker()
        {
            web = new HtmlWeb();
            context = new Context();
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

                context.Bookmarks.Add(bookmark);
                context.SaveChanges();

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
                    case SiteName.Kickassanime:
                        nodes = doc.DocumentNode.SelectNodes(Kickassanime.episodeList);
                        break;
                    case SiteName.Mangasee:
                        nodes = doc.DocumentNode.SelectNodes(Mangasee.episodeList);
                        break;
                    default:
                        nodes = null;
                        break;
                }

                foreach (var node in nodes)
                {
                    string episodeNumber;
                    string episodeLink;
                    switch ((SiteName)creation.SiteName)
                    {
                        case SiteName.Naruldonghua:
                            episodeNumber = node.SelectSingleNode(Naruldonghua.episodeNumber) != null ? node.SelectSingleNode(Naruldonghua.episodeNumber).InnerText : null;
                            episodeLink = node.SelectSingleNode(Naruldonghua.episodeLink).GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Naruldonghua.episodeLink).GetAttributeValue<string>("href", null) : null;
                            break;
                        case SiteName.Animexin:
                            episodeNumber = node.SelectSingleNode(Animexin.episodeNumber) != null ? node.SelectSingleNode(Animexin.episodeNumber).InnerText : null;
                            episodeLink = node.SelectSingleNode(Animexin.episodeLink).GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Animexin.episodeLink).GetAttributeValue<string>("href", null) : null;
                            break;
                        case SiteName.Kickassanime:
                            episodeNumber = node.SelectSingleNode(Kickassanime.episodeNumber) != null ? node.SelectSingleNode(Kickassanime.episodeNumber).InnerText : null;
                            episodeLink = node.SelectSingleNode(Kickassanime.episodeLink).GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Kickassanime.episodeLink).GetAttributeValue<string>("href", null) : null;
                            break;
                        case SiteName.Mangasee:
                            episodeNumber = node.SelectSingleNode(Mangasee.episodeNumber) != null ? node.SelectSingleNode(Mangasee.episodeNumber).InnerText : null;
                            episodeLink = node.SelectSingleNode(Mangasee.episodeLink).GetAttributeValue<string>("href", null) != null ? node.SelectSingleNode(Mangasee.episodeLink).GetAttributeValue<string>("href", null) : null;
                            break;
                        default:
                            episodeNumber = null;
                            episodeLink = null;
                            break;
                    }

                    context.Episodes.Add(new Episode()
                    {
                        BookmarkId = bookmark.BookmarkId,
                        EpisodeNumber = Int32.Parse(episodeNumber),
                        Link = episodeLink,
                        WatchStatus = Int32.Parse(episodeNumber) <= 1 ? (int)WatchStatus.NextWatch : (int)WatchStatus.NeedToWatch
                    });
                }
                context.SaveChanges();
            }
        }

        public void DeleteBookmarkCreation(Bookmark bookmark)
        {
            if (context.Bookmarks.Any(n => n.CreationId == bookmark.CreationId))
            {
                context.Bookmarks.Remove(context.Bookmarks.FirstOrDefault(n => n.CreationId == bookmark.CreationId));

                foreach (var episode in context.Episodes.Where(n => n.BookmarkId == bookmark.BookmarkId))
                {
                    context.Episodes.Remove(episode);
                }

                context.SaveChanges();
            }
        }
    }
}
