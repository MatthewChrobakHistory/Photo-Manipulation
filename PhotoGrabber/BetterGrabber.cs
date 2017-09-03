using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace PhotoGrabber
{
    public class BetterGrabber
    {
        private WebClient _client;
        private Stack<string> _pages;
        private List<string> _visited;
        public const string RegexURLMatch = @"(https?:\/\/)?(www\.)?([A-z\-]+\.)+\w+\/?";
        private string[] _allowedExtensions;
        private bool _local = false;
        private string _first;

        private string[] invalids = new string[] {
            "\"",
            "\'",
            "src=",
            "href="
        };

        public BetterGrabber() {
            this._client = new WebClient();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            this._pages = new Stack<string>();
            this._visited = new List<string>();

            Console.Write("Enter the website where you wish to start:");
            this._first = Console.ReadLine();
            this._pages.Push(this._first);

            Console.Write("Enter the searching extension:");
            this._allowedExtensions = Console.ReadLine().Split(' ');

            if (this._allowedExtensions.Length == 1) {
                if (this._allowedExtensions[0] == "") {
                    this._allowedExtensions = new string[]
                    {
                        ".png",
                        ".gif",
                        ".jpg",
                        ".jpeg",
                        ".bmp"
                    };
                }
            }

            Console.Write("Local only? ");
            this._local = bool.TryParse(Console.ReadLine(), out this._local);
        }

        public void Start() {

            // Keep looping while there are pages to test.
            while (this._pages.Count > 0) {
                // Add the current page to the visited.
                string page = this._pages.Pop();
                this._visited.Add(page);

                string source = string.Empty;
                try {
                    source = this._client.DownloadString(page);
                } catch (Exception e) {
                    continue;
                }

                // Get all the photo links and references.
                var photos = source.GetRegexMatches("<img(.+?)>");
                var datasrc = source.GetRegexMatches("data\\-src=\"[^\"]*\"");
                var links = source.GetRegexMatches("<a(.+?)>");

                // Go through every data-src
                for (int i = 0; i < datasrc.Count; i++) {
                    datasrc[i] = datasrc[i].Remove(0, "data-src=\"".Length);
                    datasrc[i] = datasrc[i].Substring(0, datasrc[i].Length - 1);
                }

                DownloadPhotos(datasrc);

                // Go through every photo.
                for (int i = 0; i < photos.Count; i++) {
                    // Get the exact source url.
                    string photo = Regex.Match(photos[i], "src=(\"|')(.+?)(\"|')").Value;

                    // Purify it.
                    foreach (string pattern in invalids) {
                        photo = photo.Replace(pattern, string.Empty);
                    }

                    photo = photo.ToLower();

                    if (photo.Contains("?")) {
                        photo = photo.Remove(photo.IndexOf('?'));
                    }

                    bool valid = false;

                    foreach (string extension in this._allowedExtensions) {
                        if (photo.EndsWith(extension)) {
                            valid = true;
                            photos[i] = photo.VerifyURL(page);
                            break;
                        }
                    }

                    if (!valid) {
                        photos.RemoveAt(i--);
                    }
                }

                DownloadPhotos(photos);

                for (int i = 0; i < links.Count; i++) {
                    string link = Regex.Match(links[i], "href=(\"|')(.+?)(\"|')").Value;

                    foreach (string pattern in invalids) {
                        link = link.Replace(pattern, string.Empty);
                    }

                    if (link != string.Empty) {
                        link = link.VerifyURL(page);

                        if (link != string.Empty) {
                            if (this._local && link.StartsWith(this._first)) {
                                if (!_visited.Contains(link)) {
                                    Console.WriteLine("Adding link: {0}", link);
                                    this._pages.Push(link);
                                }
                            }
                        }

                        
                    }
                }
            }
        }

        private void DownloadPhotos(List<string> photos) {
            foreach (string image in photos) {
                string filename = image;

                filename = filename.Replace(Regex.Match(filename, BetterGrabber.RegexURLMatch).Value, string.Empty);
                filename = filename.Replace("/", "\\");
                string path = "photos\\";

                string fullpath = filename;

                try {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + path + fullpath)) {
                        continue;
                    }

                    while (fullpath.Contains("\\")) {
                        path += fullpath.Substring(0, fullpath.IndexOf("\\") + 1);
                        fullpath = fullpath.Remove(0, fullpath.IndexOf("\\") + 1);

                        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + path)) {
                            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + path);
                        }
                    }
                } catch (Exception e) {
                    continue;
                }
               

               
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "photos\\" + fullpath)) {
                    Console.WriteLine("Downloading {0}", image);
                    try {
                        this._client.DownloadFile(image, AppDomain.CurrentDomain.BaseDirectory + path + fullpath);
                    } catch (Exception e) {
                        continue;
                    }
                }
                
            }
        }
    }

    public static class Extensions
    {
        public static string VerifyURL(this string path, string host) {

            // See if there's a base-url for the given string.
            var match = Regex.Match(path, BetterGrabber.RegexURLMatch).Value;
            if (match == string.Empty) {
                // Append the host to the path.
                path = Regex.Match(host, BetterGrabber.RegexURLMatch).Value + path;
            }

            if (path.StartsWith("//")) {
                path = "http:" + path;
            }

            if (!path.StartsWith("http://") && !path.StartsWith("https://")) {
                path = "http://" + path;
            }
            return path;
        }

        public static List<string> GetRegexMatches(this string source, string pattern) {
            List<string> results = new List<string>();

            // Get the match.
            var match = Regex.Match(source, pattern);

            while (match != Match.Empty) {
                results.Add(match.Value);
                match = match.NextMatch();
            }

            return results;
        }
    }
}
