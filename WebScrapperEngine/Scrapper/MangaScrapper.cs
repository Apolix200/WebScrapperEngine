using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private Context context;

        public MangaScrapper()
        {

            context = new Context();
        }

        public string MakeRequest()
        {
            CookieContainer cookieJar = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Mangasee.websiteLink + Mangasee.apiPath);
            request.CookieContainer = cookieJar;
            request.Accept = @"text/html, application/xhtml+xml, */*";
            request.Referer = @"https://mangasee123.com";
            request.Headers.Add("Accept-Language", "en-GB");
            request.UserAgent = @"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)";
            request.Host = @"mangasee123.com";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string htmlString;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                htmlString = reader.ReadToEnd();
            }
            return htmlString;
        }

        public void SearchMangaseeSite()
        {
            List<Creation> creations = new List<Creation>();
            string siteJson = MakeRequest();
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
        public class Data
        {
            public string I { get; set; }
            public string S { get; set; }
            public List<string> A { get; set; }
        }
        public static class Mangasee
        {
            public const string websiteLink = "https://mangasee123.com";
            public const string apiPath = "/_search.php";
            public const string linkPath = "/manga/";
            public const string imagePath = "https://cover.nep.li/cover/";

            public const string episodeList = "/html/body/div[1]/div/div/div/div/div[2]/a[position()>0]";
            public const string episodeNumber = "a/span[@class='ng-binding']";
            public const string episodeLink = "a";
        }
    }
}