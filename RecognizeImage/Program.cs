using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

using ImageRecognizerInterface;

namespace RecognizeImage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rotate = RotateFlipType.RotateNoneFlipNone;
            switch (args[0])
            {
                case "0":
                    rotate = RotateFlipType.RotateNoneFlipNone;
                    break;

                case "90":
                    rotate = RotateFlipType.Rotate90FlipNone;
                    break;

                case "180":
                    rotate = RotateFlipType.Rotate180FlipNone;
                    break;

                case "270":
                    rotate = RotateFlipType.Rotate270FlipNone;
                    break;
            }

            var port = ConfigurationManager.AppSettings["tcpPort"];
            var parser = (IImageRecognizer)Activator.GetObject(
                typeof(IImageRecognizer),
                "tcp://localhost:"+ port + "/ImageRecognizer");

            var imageBytes = parser.GetImage();
            var stream = new MemoryStream(imageBytes);
            var image = Image.FromStream(stream);
            if (rotate != RotateFlipType.RotateNoneFlipNone)
            {
                image.RotateFlip(rotate);
            }

            var ocr = new ImageOCR();
            var words = ocr.ParseData(image);
            var metric = GetRecognitionMetric(words);
            parser.ImageRecognized(metric, rotate, words);
        }

        private static long GetRecognitionMetric(IEnumerable<RecognizedWord> words)
        {
            var regex = new Regex(@"([a-zA-Z0-9]+)");

            var count = 0L;
            foreach (var word in words)
            {
                var matches = regex.Matches(word.Text);
                if (matches.Count == 0)
                {
                    continue;
                }

                foreach (Match match in matches)
                {
                    count += 1 < match.Value.Length ? match.Value.Length * 3 : match.Value.Length;
                }
            }

            return count;
        }
    }
}
