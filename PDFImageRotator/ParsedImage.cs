using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PDFImageRotator
{
    public sealed class ParsedImage
    {
        public Image Image
        {
            get;
            set;
        }

        public Stream ImageStream
        {
            get;
            set;
        }

        public ImageFormat Format
        {
            get;
            set;
        }

        public float Width
        {
            get;
            set;
        }

        public float Height
        {
            get;
            set;
        }

        public RotateFlipType PerformedRotation
        {
            get;
            set;
        }

        public void Close()
        {
            if (this.ImageStream != null)
            {
                this.ImageStream.Close();
            }
        }
    }
}