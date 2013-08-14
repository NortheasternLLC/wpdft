using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

using iTextSharp.text.pdf;

namespace PDFImageRotator
{
    public static class ImageParser
    {
        /// <summary>Parses images from pdf document.</summary>
        /// <param name="filePath">The pdf-file full path.</param>
        /// <returns>Collection of images and streams that are associated with them.</returns>
        public static List<ParsedImage> ParseImages(string filePath)
        {
            var imgList = new List<ParsedImage>();
            var raf = new RandomAccessFileOrArray(filePath);
            var reader = new PdfReader(raf, null);

            try
            {
                for (var pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
                {
                    var pg = reader.GetPageN(pageNumber);
                    var size = reader.GetPageSize(pageNumber);
                    var res = (PdfDictionary)PdfReader.GetPdfObject(pg.Get(PdfName.RESOURCES));

                    var xobj = (PdfDictionary)PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT));
                    if (xobj == null)
                    {
                        continue;
                    }

                    foreach (var name in xobj.Keys)
                    {
                        var obj = xobj.Get(name);
                        if (!obj.IsIndirect())
                        {
                            continue;
                        }

                        var tg = (PdfDictionary)PdfReader.GetPdfObject(obj);

                        var type = (PdfName)PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE));

                        if (!PdfName.IMAGE.Equals(type))
                        {
                            continue;
                        }

                        var refIndex = Convert.ToInt32(((PRIndirectReference)obj).Number.ToString(CultureInfo.InvariantCulture));
                        var pdfObj = reader.GetPdfObject(refIndex);
                        var pdfStrem = (PdfStream)pdfObj;
                        var bytes = PdfReader.GetStreamBytesRaw((PRStream)pdfStrem);
                        if (bytes == null)
                        {
                            continue;
                        }

                        var memStream = new MemoryStream(bytes) { Position = 0 };
                        var img = Image.FromStream(memStream);
                        imgList.Add(new ParsedImage
                        {
                            Image = img,
                            ImageStream = memStream,
                            Format = img.RawFormat,
                            Width = size.Width,
                            Height = size.Height,
                            PerformedRotation = RotateFlipType.RotateNoneFlipNone
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                reader.Close();
                raf.Close();
            }

            return imgList;
        }

        public static void SaveImages(List<ParsedImage> images, string outputFile)
        {
            var doc = new iTextSharp.text.Document();
            try
            {
                var writer = PdfWriter.GetInstance(doc, new FileStream(outputFile, FileMode.Create));
                writer.SetPdfVersion(PdfWriter.PDF_VERSION_1_5);
                writer.CompressionLevel = PdfStream.BEST_COMPRESSION;
                doc.Open();

                foreach (var parsedImage in images)
                {
                    var image = iTextSharp.text.Image.GetInstance(parsedImage.Image, parsedImage.Format);
                    var width = parsedImage.Width;
                    var height = parsedImage.Height;
                    if ((parsedImage.PerformedRotation == RotateFlipType.Rotate90FlipNone) ||
                        (parsedImage.PerformedRotation == RotateFlipType.Rotate270FlipNone))
                    {
                        var temp = width;
                        width = height;
                        height = temp;
                    }

                    var size = new iTextSharp.text.Rectangle(width, height);
                    image.ScaleAbsolute(width, height);
                    image.CompressionLevel = PdfStream.BEST_COMPRESSION;
                    doc.SetPageSize(size);
                    doc.NewPage();
                    image.SetAbsolutePosition(0, 0);
                    doc.Add(image);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                doc.Close();
            }
        }
    }
}