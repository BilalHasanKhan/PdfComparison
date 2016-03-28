using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;
using Newtonsoft.Json;

namespace PdfComparison
{
    public class PDFComparer
    { 
        private const int ThumbnailWidthAndHeight = 200;
        private static readonly MagickGeometry ThumbnailGeometry = new MagickGeometry(ThumbnailWidthAndHeight) { FillArea = false, IgnoreAspectRatio = false };

        public static string DoComparison(string testNamePrefix, string outputBasePath, string referencePdf, string testFilePdf)
        { 
            var outputPath = Path.Combine(outputBasePath, $"{testNamePrefix}_{DateTime.Now:yy_MM_dd_HH_mm_ss}");
            var referencePath = Path.Combine(outputPath, PageComparison.ReferenceRelativePath);
            var testPath = Path.Combine(outputPath, PageComparison.TestRelativePath);
            var resultPath = Path.Combine(outputPath, PageComparison.ResultRelativePath);

            Directory.CreateDirectory(outputPath);
            Directory.CreateDirectory(referencePath);
            Directory.CreateDirectory(testPath);
            Directory.CreateDirectory(resultPath);

            var referencePages = Split(referencePdf, referencePath);
            var testPages = Split(testFilePdf, testPath);

            var compareResults = Compare(Math.Max(referencePages, testPages), outputPath, referencePath, testPath, resultPath);

            var docComparison = new DocumentComparison()
            {
                TestDocumentPath = testFilePdf,
                ReferenceDocumentPath = referencePdf,
                PageComparisons = compareResults.ToList()
            };

            var resultFilePath = Path.Combine(outputPath, "results.json");
            var json = JsonConvert.SerializeObject(docComparison, Formatting.Indented);
            File.WriteAllText(resultFilePath, json);

            return resultFilePath;
        }

        private static IEnumerable<PageComparison> Compare(int numPages, string rootPath, string referencePath, string toTestPath, string comparePath)
        {
            for (var currentPage = 1; currentPage <= numPages; currentPage++)
            {
                var currentComparison = new PageComparison()
                {
                    PageNumber = currentPage,
                    RootPath = rootPath
                };

                using (var referenceImage = new MagickImage())
                using (var testImage = new MagickImage())
                using (var compareImage = new MagickImage())
                {
                    var referenceImagePath = currentComparison.ReferencePagePath;
                    if (File.Exists(referenceImagePath))
                        referenceImage.Read(referenceImagePath);

                    var testImagePath = currentComparison.TestPagePath;
                    if (File.Exists(testImagePath))
                        testImage.Read(testImagePath);

                    InsertEmptyImageIfNecessary(referenceImage, testImage);
                    InsertEmptyImageIfNecessary(testImage, referenceImage);

                    EnsureImageIsColored(referenceImage);
                    EnsureImageIsColored(testImage);

                    var result = referenceImage.Compare(testImage, ErrorMetric.Absolute, compareImage, Channels.All);

                    compareImage.Write(currentComparison.ComparePagePath);
                    compareImage.Resize(ThumbnailGeometry);
                    compareImage.Write(currentComparison.ComparePageThumbnailPath);

                    currentComparison.ComparisonScore = result;
                    currentComparison.PixelCount = referenceImage.Height * referenceImage.Width;
                }

                yield return currentComparison;
            }
        }

        private static void InsertEmptyImageIfNecessary(MagickImage fillThisImage, MagickImage withThisSize)
        {
            if (fillThisImage.Height == 0)
                if (withThisSize.Height > 0)
                    fillThisImage.Resize(withThisSize.Width, withThisSize.Height);
        }
      
        private static void EnsureImageIsColored(MagickImage referenceImage)
        {
            referenceImage.ColorSpace = ColorSpace.sRGB;
            referenceImage.ColorType = ColorType.TrueColorAlpha;
            referenceImage.Depth = 8;
        }

        static int Split(string whichPdf, string outputPath)
        {
            var settings = new MagickReadSettings();

            // Settings the density to 300 dpi will create an image with a better quality
            settings.Density = new PointD(300, 300);

            using (var images = new MagickImageCollection())
            {
                // Add all the pages of the pdf file to the collection
                images.Read(whichPdf, settings);

                int pageNumber = 0;
                foreach (MagickImage image in images)
                {
                    pageNumber++;

                    // Write page to file that contains the page number
                    image.Write(Path.Combine(outputPath, PageComparison.CreatePageFileName(pageNumber)));

                    // Write a thumbnail for the page
                    image.Resize(ThumbnailGeometry);
                    image.Write(Path.Combine(outputPath, PageComparison.CreateThumbnailFileName(pageNumber)));
                }

                return pageNumber;
            }
        }
    }
}