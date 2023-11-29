using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using System.IO;
using System.Net;
using System.Web;
using CloudProxySharp;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Headers;
using static WebScrapperEngine.Scrapper.WebtoonScrapper;

namespace WebScrapperEngine.Action
{
    public static class CloudflareEvader
    { 

        public static string FlareSolverrUrl = "http://localhost:8191/";
        public static string ProtectedUrl = Webtoonxyz.websiteLink;

        public static async Task<string> SampleGet()
        {
            var handler = new ClearanceHandler(FlareSolverrUrl)
            {
                MaxTimeout = 60000
            };

            var client = new HttpClient(handler);
            var content = await client.GetStringAsync(ProtectedUrl);

            return content;
        }

        public static async Task<HttpResponseMessage> SamplePostUrlEncoded()
        {
            var handler = new ClearanceHandler(FlareSolverrUrl)
            {
                MaxTimeout = 60000
            };

            var request = new HttpRequestMessage();
            request.Headers.ExpectContinue = false;
            request.RequestUri = new Uri(ProtectedUrl);
            var postData = new Dictionary<string, string> { { "story", "test" } };
            request.Content = FormUrlEncodedContentWithEncoding(postData, Encoding.UTF8);
            request.Method = HttpMethod.Post;

            var client = new HttpClient(handler);
            var content = await client.SendAsync(request);
            return content;
        }

        static ByteArrayContent FormUrlEncodedContentWithEncoding(
            IEnumerable<KeyValuePair<string, string>> nameValueCollection, Encoding encoding)
        {
            // utf-8 / default
            if (Encoding.UTF8.Equals(encoding) || encoding == null)
                return new FormUrlEncodedContent(nameValueCollection);

            // other encodings
            var builder = new StringBuilder();
            foreach (var pair in nameValueCollection)
            {
                if (builder.Length > 0)
                    builder.Append('&');
                builder.Append(HttpUtility.UrlEncode(pair.Key, encoding));
                builder.Append('=');
                builder.Append(HttpUtility.UrlEncode(pair.Value, encoding));
            }
            // HttpRuleParser.DefaultHttpEncoding == "latin1"
            var data = Encoding.GetEncoding("latin1").GetBytes(builder.ToString());
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return content;
        }

    }
}
