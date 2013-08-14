wpdft was developed as a work for hire by Northeastern, LLC in September and October of 2011 to support a project that required fixing hundreds of thousands of existing PDFs.

It is based on Google Tesseract and a number of other components, and is written in c#.

We don't have much in the way of documentation yet, please excuse this.

After building, use at the command line is basically: 

C:\PDFImageRotator.exe -i inputFileName.pdf -o outputFileName.pdf -r -c

the switch -r turns on rotation
the switch -c turns on cropping
the switch -i specifies the file file
the switch -o specifies the output file

The basic use case is to allow for batch processing of PDFs whose pages are raster images with no text data, to orient the pages so that text is facing the correct direction

This will also recrop pages somewhat, you might want to test this before using

I am also including binaries in the file parser.zip

We are releasing this code under the apache license.  Please feel free to work with it and maybe make it better.

If you have any questions or comments, feel free to email me: cmac@northeasternllc.com

Regards,
Christopher MacTaggart
Northeastern, LLC