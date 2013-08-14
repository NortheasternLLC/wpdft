using System.Collections.Generic;
using System.Drawing;

namespace ImageRecognizerInterface
{
    public interface IImageRecognizer
    {
        /// <summary>Gets the image content.</summary>
        /// <returns>The image content.</returns>
        byte[] GetImage();

        /// <summary>Send the results of recognition to main process.</summary>
        /// <param name="metric">The metric.</param>
        /// <param name="rotate">The rotation direction.</param>
        /// <param name="words">The recognized words.</param>
        void ImageRecognized(long metric, RotateFlipType rotate, List<RecognizedWord> words);
    }
}
