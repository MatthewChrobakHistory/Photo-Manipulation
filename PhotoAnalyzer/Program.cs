using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace PhotoAnalyzer
{
    public class BigColor
    {
        public BigInteger A;
        public BigInteger R;
        public BigInteger G;
        public BigInteger B;
    }

    public static class Program
    {
        static BigColor[][] avgPhoto;

        private static void Main(string[] args)
        {
            Console.Write("Please enter the folder to analyze: ");
            string folder = Console.ReadLine();

            // If the folder exists, analyze it.
            if (Directory.Exists(folder)) {
                Program.analyzeFolder(folder);
            }
        }

        private static void analyzeFolder(string baseFolder)
        {
            Program.initializeAvgPhotoSize(baseFolder);
            long total = 0;

            for (int x = 0; x < avgPhoto.Length; x++) {
                for (int y = 0; y < avgPhoto[x].Length; y++) {
                    Program.avgPhoto[x][y] = new BigColor();
                }
            }
            
            foreach (string file in Directory.GetFiles(baseFolder, "*.jpg", SearchOption.AllDirectories)) {
                try {
                    using (var bmp = (Bitmap)Image.FromFile(file)) {
                        for (int x = 0; x < bmp.Width; x++) {
                            for (int y = 0; y < bmp.Height; y++) {
                                var pixel = bmp.GetPixel(x, y);

                                avgPhoto[x][y].A += pixel.A;
                                avgPhoto[x][y].R += pixel.R;
                                avgPhoto[x][y].G += pixel.G;
                                avgPhoto[x][y].B += pixel.B;
                            }
                        }

                        Console.WriteLine("Finished {0}", total);
                        total += 1;
                    }
                } catch (Exception e) {

                }
               
            }
            var newBmp = new Bitmap(avgPhoto.Length, avgPhoto[0].Length);

            for (int x = 0; x < avgPhoto.Length; x++) {
                for (int y = 0; y < avgPhoto[x].Length; y++) {

                    var pixel = avgPhoto[x][y];

                    newBmp.SetPixel(x, y, Color.FromArgb(
                        (int)(pixel.A / total),
                        (int)(pixel.R / total),
                        (int)(pixel.G / total),
                        (int)(pixel.B / total)
                        ));
                }
            }

            newBmp.Save(AppDomain.CurrentDomain.BaseDirectory + "average.png");
        }

        private static void initializeAvgPhotoSize(string baseFolder)
        {
            int maxX = 0, maxY = 0;
            int total = 0;


            foreach (string file in Directory.GetFiles(baseFolder, "*.jpg", SearchOption.AllDirectories)) {
                try {
                    using (var bmp = Image.FromFile(file)) {
                        maxX = bmp.Width >= maxX ? bmp.Width : maxX;
                        maxY = bmp.Height >= maxY ? bmp.Height : maxY;

                        total++;
                    }
                } catch (Exception e) {

                }
                
            }

            Program.avgPhoto = new BigColor[maxX][];

            for (int i = 0; i < maxX; i++) {
                Program.avgPhoto[i] = new BigColor[maxY];
            }

            Console.WriteLine("Found biggest file: {0} by {1} for a total of {2}", maxX, maxY, total);
        }
    }
}
