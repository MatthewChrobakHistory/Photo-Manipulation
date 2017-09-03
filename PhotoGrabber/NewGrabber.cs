using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System;

namespace PhotoGrabber
{
    public class NewGrabber
    {
        static WebClient client = new WebClient();
        static List<string> visitedPages = new List<string>();
        static List<string> downloadedPhotos = new List<string>();
        static List<string> pages = new List<string>();

        public static void Main() {
            new BetterGrabber().Start();
        }

        public NewGrabber() { 

            string basePage = Console.ReadLine();
            string startPage = basePage;
            string[] invalids = new string[] {
                        "\"",
                        "\'",
                        "src=",
                        "href="
                    };

            pages.Add(startPage);

            while (pages.Count > 0) {
                visitedPages.Add(pages[0]);
                string page = pages[0];

                string source = string.Empty;

                try {
                    source = client.DownloadString(page);
                } catch (Exception e) {
                    pages.RemoveAt(0);
                    continue;
                }

                var photos = GetRegexMatches(source, "<img(.+?)>");
                var links = GetRegexMatches(source, "<a(.+?)>");
                

                for (int i = 0; i < photos.Count; i++) {
                    string photo = Regex.Match(photos[i], "src=(\"|')(.+?)(\"|')").Value;

                    foreach (string pattern in invalids) {
                        photo = photo.Replace(pattern, string.Empty);
                    }

                    if (!photo.StartsWith(basePage) && !photo.ToLower().Contains("www") && !photo.ToLower().StartsWith("http")) {
                        photo = basePage + photo;
                    }

                    photos[i] = photo;
                }

                for (int i = 0; i < links.Count; i++) {
                    string link = Regex.Match(links[i], "href=(\"|')(.+?)(\"|')").Value;

                    foreach (string pattern in invalids) {
                        link = link.Replace(pattern, string.Empty);
                    }

                    if (!link.StartsWith(basePage) && !link.ToLower().Contains("www") && !link.ToLower().StartsWith("http")) {
                        link = basePage + link;
                    }

                    links[i] = link;
                }

                DownloadPhotos(photos);
                AddLinks(links);
                pages.RemoveAt(0);
            }
        }

        private static void DownloadPhotos(List<string> photos) {
            foreach (string image in photos) {
                if (downloadedPhotos.Contains(image)) {
                    continue;
                }

                string filename = image;

                while (filename.Contains("\\")) {
                    if (filename.StartsWith("\\")) {
                        filename = filename.Remove(0, 1);
                        continue;
                    }
                    filename = filename.Remove(0, filename.LastIndexOf("\\"));
                }

                while (filename.Contains("/")) {
                    if (filename.StartsWith("/")) {
                        filename = filename.Remove(0, 1);
                        continue;
                    }
                    filename = filename.Remove(0, filename.LastIndexOf("/"));
                }

                downloadedPhotos.Add(image);

                System.Console.WriteLine("Downloading " + image);
                try {
                    client.DownloadFile(image, System.AppDomain.CurrentDomain.BaseDirectory + "photos\\" + filename);
                } catch (System.Exception e) {
                    continue;
                }
            }
        }

        private static void AddLinks(List<string> links) {
            foreach (string link in links) {
                if (link != "") {
                    if (!visitedPages.Contains(link)) {
                        System.Console.WriteLine("Adding link: " + link);
                        pages.Add(link);
                    }
                }
            }
        }

        private static List<string> GetRegexMatches(string source, string pattern) {

            List<string> result = new List<string>();

            // Get the photos.
            var match = Regex.Match(source, pattern);

            while (match != Match.Empty) {
                result.Add(match.Value);
                match = match.NextMatch();
            }

            return result;
        }
    }
}
