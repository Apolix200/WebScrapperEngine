using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private HtmlWeb web;
        private Context context;

        public DonghuaScrapper()
        {
            web = new HtmlWeb();
            context = new Context();
        }

        public void SearchNaruldonghuaSite()
        {
            List<Creation> creations = new List<Creation>();
            string websiteLink = Naruldonghua.websiteLink;
            bool nextButtonExist = true;

            do
            {
                var doc = web.Load(websiteLink);
                var nodes = doc.DocumentNode.SelectNodes(Naruldonghua.contentPath);

                if (nodes != null)
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

                            creations.Add(donghuaCreation);
                        }
                    }
                }

                nextButtonExist = doc.DocumentNode.SelectSingleNode(Naruldonghua.nextButtonPath) != null;

                websiteLink = nextButtonExist ? doc.DocumentNode.SelectSingleNode(Naruldonghua.nextButtonPath).GetAttributeValue<string>("href", null) : Naruldonghua.websiteLink;

            } while (nextButtonExist);

            context.BulkInsert(creations);
        }

        public void SearchAnimexinSite()
        {
            List<Creation> creations = new List<Creation>();
            string websiteLink = Animexin.websiteLink;
            bool nextButtonExist = true;

            do
            {
                var doc = web.Load(websiteLink);
                var nodes = doc.DocumentNode.SelectNodes(Animexin.contentPath);

                if (nodes != null)
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

                            creations.Add(donghuaCreation);
                        }
                    }
                }

                nextButtonExist = doc.DocumentNode.SelectSingleNode(Animexin.nextButtonPath) != null;

                websiteLink = nextButtonExist ? doc.DocumentNode.SelectSingleNode(Animexin.nextButtonPath).GetAttributeValue<string>("href", null) : Animexin.websiteLink;

            } while (nextButtonExist);

            context.BulkInsert(creations);
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
            public const string contentPath = "/html/body/div[3]/div/div[3]/div[3]/div[2]/div[1]/article[position()>0]";
            public const string nextButtonPath = "/html/body/div[3]/div/div[3]/div[3]/div[2]/div[@class='hpage']/a[@class='r']";
            public const string titlePath = "div/a/div[@class='limit']/div[@class='egghead']/div[@class='eggtitle']/text()";
            public const string linkToSeriesPath = "/html/body/div[3]/div/div[4]/article/div[2]/div/div[1]/div[2]/span[2]/a";
            public const string linkPath = "div/a";
            public const string imagePath = "div/a/div[@class='limit']/img";
            public const string imageSrc = "src";

            public const string episodeList = "/html/body/div[3]/div/div[3]/article/div[@class='bixbox bxcl epcheck']/div[@class='eplister'/ul/li[position()>0]";
            public const string episodeNumber = "a/div[@class='epl-num']";
            public const string episodeLink = "a";
        }
    }
}