using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

using ImageRecognizerInterface;

using tessnet2;

namespace RecognizeImage
{
    public sealed class ImageOCR
    {
        /// <summary>The recognizer.</summary>
        private readonly Tesseract recognizer;

        /// <summary>Initializes a new instance of the <see cref="ImageOCR"/> class.</summary>
        public ImageOCR()
        {
            this.recognizer = new Tesseract();
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            var path = Path.Combine(folder, "tessdata");
            path = path.Substring(6);
            this.recognizer.Init(path, "eng", false);
        }

        public List<RecognizedWord> ParseData(Image image)
        {
            var bitmap = new Bitmap(image);
            var words = this.recognizer.DoOCR(bitmap, new Rectangle(0, 0, bitmap.Size.Width, bitmap.Size.Height));

            var result = new List<RecognizedWord>();
            foreach (var word in words)
            {
                result.Add(new RecognizedWord
                {
                    Confidence = word.Confidence,
                    Text = word.Text,
                    Left = word.Left,
                    Right = word.Right,
                    Bottom = word.Bottom,
                    Top = word.Top
                });
            }

            return result;
        }
    }
}