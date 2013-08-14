using System;
using System.Configuration;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Security;

namespace PDFImageRotator
{
    public static class Program
    {
        /// <summary>Writes the error on console and closes application.</summary>
        /// <param name="errorMessage">The error message.</param>
        private static void WriteError(string errorMessage)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ForegroundColor = oldColor;
        }

        private static void WriteInformationMessage(string message)
        {
            Console.Write(message);
        }

        /// <summary>Determines whether the specified path is valid path.</summary>
        /// <param name="path">The path.</param>
        /// <param name="shouldExist">The valie indicating whether the specified file should exist.</param>
        /// <returns><see langword="true"/> if the specified path is valid path; otherwise, <see langword="false"/>.</returns>
        private static bool IsValidPath(string path, bool shouldExist)
        {
            try
            {
              var file = new FileInfo(path);
              if (shouldExist && !file.Exists)
              {
                  WriteError(string.Format("File {0} doesn't exist.", path));
                  return false;
              }
            }
            catch(ArgumentNullException)
            {
                WriteError("File name is null.");
                return false;
            }
            catch (ArgumentException)
            {
                WriteError(string.Format("The specified path contains invalid characters. Value: {0}", path));
                return false;
            }
            catch (PathTooLongException)
            {
                WriteError(string.Format("The specified file path is too long. Value: {0}", path));
                return false;
            }
            catch (NotSupportedException)
            {
                WriteError(string.Format("The specified path contains invalid characters. Value: {0}", path));
                return false;
            }
            catch (SecurityException)
            {
                WriteError(string.Format("File {0} is not accessible.", path));
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                WriteError(string.Format("File {0} is not accessible.", path));
                return false;
            }

            return true;
        }

        public static void Main(string[] args)
        {
            string inputFile = null;
            string outputFile = null;
            var shouldClip = false;
            var shouldRotate = false;
            var index = 0;
            while (index < args.Length)
            {
                switch (args[index].ToLower())
                {
                    case "-i":
                    {
                        if (index + 1 < args.Length)
                        {
                            inputFile = args[index + 1];
                            index++;
                        }

                        break;
                    }

                    case "-o":
                    {
                        if (index + 1 < args.Length)
                        {
                            outputFile = args[index + 1];
                            index++;
                        }

                        break;
                    }

                    case "-r":
                    {
                        shouldRotate = true;
                        break;
                    }

                    case "-c":
                    {
                        shouldClip = true;
                        break;
                    }
                }

                index++;
            }

            if (!IsValidPath(inputFile, true))
            {
                return;
            }

            if (!IsValidPath(outputFile, false))
            {
                return;
            }

            var port = int.Parse(ConfigurationManager.AppSettings["tcpPort"]);
            var channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            WriteInformationMessage("Processing...");
            try
            {
                RemotingConfiguration.RegisterWellKnownServiceType(
              typeof(DocumentProcessor),
              "ImageRecognizer",
              WellKnownObjectMode.Singleton);

                var processor = new DocumentProcessor();
                processor.ProcessDocument(inputFile, outputFile, shouldRotate, shouldClip);
            }
            finally
            {
                WriteInformationMessage("Done\n");
                ChannelServices.UnregisterChannel(channel);
            }
        }
    }
}
