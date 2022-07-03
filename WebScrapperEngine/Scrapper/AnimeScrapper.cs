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
    class AnimeScrapper
    {
        private Context context;

        public AnimeScrapper()
        {
            context = new Context();
        }

        public string MakeRequest(int index)
        {
            CookieContainer cookieJar = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Kickassanime.websiteLink + Kickassanime.apiPath + index);
            request.CookieContainer = cookieJar;
            request.Accept = @"text/html, application/xhtml+xml, */*";
            request.Referer = @"https://www2.kickassanime.ro";
            request.Headers.Add("Accept-Language", "en-GB");
            request.UserAgent = @"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)";
            request.Host = @"www2.kickassanime.ro";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string htmlString;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                htmlString = reader.ReadToEnd();
            }
            return htmlString;
        }

        public void SearchKickassSite()
        {
            List<Creation> creations = new List<Creation>();
            SiteResponse siteResponse;
            string siteJson;
            int index = 1;

            do
            {
                siteJson = MakeRequest(index);
                siteResponse = JsonConvert.DeserializeObject<SiteResponse>(siteJson);

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
                        creations.Add(animeCreation);
                    }
                }

                index++;

            } while (siteResponse.Data.Count() > 0);

            context.BulkInsert(creations);
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

        public static class Kickassanime
        {
            public const string websiteLink = "https://www2.kickassanime.ro";
            public const string apiPath = "/api/get_anime_list/all/";
            public const string imagePath = "/uploads/";
            public const string linkToSeriesPath = "/html/body/div[1]/div[1]/div[1]/div/aside/div/div[1]/div/a";

            public const string episodeList = "/html/body/div[1]/div[1]/div[1]/div/div/div[2]/div[2]/table/tbody/tr[position()>0]";
            public const string episodeNumber = "td/a";
            public const string episodeLink = "td/a";
        }
    }
}