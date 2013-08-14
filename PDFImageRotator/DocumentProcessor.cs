using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;

using ImageRecognizerInterface;

namespace PDFImageRotator
{
    public sealed class DocumentProcessor : MarshalByRefObject, IImageRecognizer
    {
        public static int OperationsRemain = 4;
        public static Dictionary<RotateFlipType, long> metrics = new Dictionary<RotateFlipType, long>();
        public static Dictionary<RotateFlipType, List<RecognizedWord>> imagesWords = new Dictionary<RotateFlipType, List<RecognizedWord>>();
        public static AutoResetEvent isImagesParsingDone = new AutoResetEvent(false);

        public static byte[] CurrentImage;

        /// <summary>Parsing the document.</summary>
        /// <param name="inputFile">The input file.</param>
        /// <param name="outputFile">The output file.</param>
        /// <param name="shouldRotate">If should rotate an image.</param>
        /// <param name="shouldClip">If should clip an image.</param>
        public void ProcessDocument(string inputFile, string outputFile, bool shouldRotate, bool shouldClip)
        {
            // load images
            var images = ImageParser.ParseImages(inputFile);

            // process images
            foreach (var parsedImage in images)
            {
                OperationsRemain = 4;
                metrics.Clear();
                imagesWords.Clear();
                isImagesParsingDone.Reset();

                var stream = new MemoryStream();
                parsedImage.Image.Save(stream, parsedImage.Image.RawFormat);
                CurrentImage = stream.ToArray();

                if (shouldClip)
                {
                    parsedImage.Image = this.PerformClipping(parsedImage.Image);
                }

                parsedImage.PerformedRotation = RotateFlipType.RotateNoneFlipNone;

                if (shouldRotate)
                {
                    parsedImage.PerformedRotation = this.PerformRotation(parsedImage.Image);
                }
            }

            // save images
            ImageParser.SaveImages(images, outputFile);
        }

        private Image PerformClipping(Image image)
        {
            var bitmap = new Bitmap(image);
            var edged = Magora.Image.Image.EdgeDetection(bitmap, 10f, Color.Red);

            var sumLeft = -1;
            var sumRight = -1;
            var sumBottom = -1;
            var sumTop = -1;
            var limitLeft = edged.Width / 10;
            var limitTop = edged.Height/10;
            for (var index = 1; index < 10; index++)
            {
                var height = edged.Height * index / 10;
                var location = 0;
                while (location < limitLeft)
                {
                    var color = edged.GetPixel(location, height);
                    if ((200 < color.R) && (color.G < 20) && (color.B < 20))
                    {
                        var nodeIndex = 1;
                        while (nodeIndex < 5)
                        {
                            color = edged.GetPixel(location + nodeIndex, height);
                            if (!((200 < color.R) && (color.G < 20) && (color.B < 20)))
                            {
                                break;
                            }

                            nodeIndex++;
                        }

                        if (nodeIndex == 5 && 15 < location)
                        {
                            if (sumLeft < 0)
                            {
                                sumLeft = location;
                            }
                            else if (location < sumLeft)
                            {
                                sumLeft = location;
                            }

                            break;
                        }
                    }

                    location++;
                }

                location = 1;
                while (location < limitLeft)
                {
                    var color = edged.GetPixel(edged.Width - location, height);
                    if ((200 < color.R) && (color.G < 20) && (color.B < 20))
                    {
                        var nodeIndex = 1;
                        while (nodeIndex < 5)
                        {
                            color = edged.GetPixel(edged.Width - location - nodeIndex, height);
                            if (!((200 < color.R) && (color.G < 20) && (color.B < 20)))
                            {
                                break;
                            }

                            nodeIndex++;
                        }

                        if (nodeIndex == 5 && 15 < location)
                        {
                            if (sumRight < 0)
                            {
                                sumRight = location;
                            }
                            else if (location < sumRight)
                            {
                                sumRight = location;
                            }

                            break;
                        }

                        location++;
                    }

                    location++;
                }

                var width = edged.Width*index/10;
                location = 0;
                while (location < limitTop)
                {
                    var color = edged.GetPixel(width, location);
                    if ((200 < color.R) && (color.G < 20) && (color.B < 20))
                    {
                        var nodeIndex = 1;
                        while (nodeIndex < 5)
                        {
                            color = edged.GetPixel(width, location + nodeIndex);
                            if (!((200 < color.R) && (color.G < 20) && (color.B < 20)))
                            {
                                break;
                            }

                            nodeIndex++;
                        }

                        if (nodeIndex == 5 && 15 < location)
                        {
                            if (sumTop < 0)
                            {
                                sumTop = location;
                            }
                            else if (location < sumTop)
                            {
                                sumTop = location;
                            }

                            break;
                        }
                    }

                    location++;
                }

                location = 1;
                while (location < limitTop)
                {
                    var color = edged.GetPixel(width, edged.Height - location);
                    if ((200 < color.R) && (color.G < 20) && (color.B < 20))
                    {
                        var nodeIndex = 1;
                        while (nodeIndex < 5)
                        {
                            color = edged.GetPixel(width, edged.Height - location - nodeIndex);
                            if (!((200 < color.R) && (color.G < 20) && (color.B < 20)))
                            {
                                break;
                            }

                            nodeIndex++;
                        }

                        if (nodeIndex == 5 && 15 < location)
                        {
                            if (sumBottom < 0)
                            {
                                sumBottom = location;
                            }
                            else if (location < sumBottom)
                            {
                                sumBottom = location;
                            }

                            break;
                        }
                    }

                    location++;
                }
            }

            if (sumLeft < 0)
            {
                sumLeft = 0;
            }

            if (sumRight < 0)
            {
                sumRight = 0;
            }

            if (sumBottom < 0)
            {
                sumBottom = 0;
            }

            if (sumTop < 0)
            {
                sumTop = 0;
            }

            return bitmap.Clone(new Rectangle(sumLeft, sumTop, edged.Width - sumRight - sumLeft, edged.Height - sumBottom - sumTop), bitmap.PixelFormat);
        }

        private RotateFlipType PerformRotation(Image image)
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            var path = Path.Combine(folder, "RecognizeImage.exe");
            path = path.Substring(6);

            // create for process with different tasks
            var processStartInfo = new ProcessStartInfo(path);

            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;

            processStartInfo.Arguments = "0";
            Process.Start(processStartInfo);

            processStartInfo.Arguments = "90";
            Process.Start(processStartInfo);

            processStartInfo.Arguments = "180";
            Process.Start(processStartInfo);

            processStartInfo.Arguments = "270";
            Process.Start(processStartInfo);

            isImagesParsingDone.WaitOne();

            var realDirection = RotateFlipType.RotateNoneFlipNone;
            var result = metrics[realDirection];
            var words = imagesWords[realDirection];
            if (result < metrics[RotateFlipType.Rotate90FlipNone])
            {
                realDirection = RotateFlipType.Rotate90FlipNone;
                result = metrics[realDirection];
                words = imagesWords[realDirection];
            }

            if (result < metrics[RotateFlipType.Rotate180FlipNone])
            {
                realDirection = RotateFlipType.Rotate180FlipNone;
                result = metrics[realDirection];
                words = imagesWords[realDirection];
            }

            if (result < metrics[RotateFlipType.Rotate270FlipNone])
            {
                realDirection = RotateFlipType.Rotate270FlipNone;
                result = metrics[realDirection];
                words = imagesWords[realDirection];
            }

            if (realDirection == RotateFlipType.Rotate180FlipNone && ((result - metrics[RotateFlipType.RotateNoneFlipNone]) / (double)result * 100 < 7.5))
            {
                realDirection = RotateFlipType.RotateNoneFlipNone;
            }

            if (words.Count < 10)
            {
                realDirection = RotateFlipType.RotateNoneFlipNone;
            }

            if (realDirection != RotateFlipType.RotateNoneFlipNone)
            {
                image.RotateFlip(realDirection);
            }

            return realDirection;
        }

        public byte[] GetImage()
        {
            return CurrentImage;
        }

        public void ImageRecognized(long metric, RotateFlipType rotate, List<RecognizedWord> words)
        {
            lock (metrics)
            {
                metrics[rotate] = metric;
            }

            lock (imagesWords)
            {
                imagesWords[rotate] = words;
            }

            if (Interlocked.Decrement(ref OperationsRemain) == 0)
            {
                isImagesParsingDone.Set();
            }
        }
    }
}