using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImageMagick;
using Newtonsoft.Json;
using TwoPS.Processes;

namespace PdfComparison
{
    public class PDFComparer
    { 
        private const int ThumbnailWidthAndHeight = 200;
        private static readonly MagickGeometry ThumbnailGeometry = new MagickGeometry(ThumbnailWidthAndHeight) { FillArea = false, IgnoreAspectRatio = false };

        public static string DoComparison(string testNamePrefix, string outputBasePath, string referencePdf, string testFilePdf)
        {
            var when = DateTime.UtcNow;

            var outputPath = Path.Combine(outputBasePath, $"{testNamePrefix}_{when:yy_MM_dd_HH_mm_ss}");
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
                WhenUtc = when,
                FolderName = outputPath,
                TestNamePrefix = testNamePrefix,
                TestDocumentPath = testFilePdf,
                ReferenceDocumentPath = referencePdf,
                PageComparisons = compareResults.ToList()
            };

            var resultFilePath = Path.Combine(outputPath, "results.json");
            var json = JsonConvert.SerializeObject(docComparison, Formatting.Indented);
            File.WriteAllText(resultFilePath, json);

            var md = Path.ChangeExtension(resultFilePath, "md");
            var html = Path.ChangeExtension(resultFilePath, "html");

            WriteMarkdown(md, docComparison);
            CreateHtmlFromMarkdown(md, html);
            return resultFilePath;
        }

        private static void CreateHtmlFromMarkdown(string md, string html)
        {
            try
            {
                var process = new Process("pandoc.exe", "--from", "markdown", "--to", "html", "--standalone", md);
                var result = process.Run();
                File.WriteAllText(html, result.AllOutput);
            }
            catch (Exception pokemon)
            {
                // ignored
            }
        }

        private static string ImageLink(string which, PageComparison page)
        {
            return $"[![{which}]({which}/Thumb{page.PageNumber:0000}.png \"{which}\")]({which}/Page{page.PageNumber:0000}.png)";
        }

        private static void WriteMarkdown(string output, DocumentComparison docComparison)
        {
            var build = new StringBuilder();
            build.AppendLine("#Comparison");
            build.AppendLine();
            build.AppendLine("Comparison of:");
            build.AppendLine();
            build.AppendLine($"* Reference: [{docComparison.ReferenceDocumentPath}]({docComparison.ReferenceDocumentPath})");
            build.AppendLine($"* Test File: [{docComparison.TestDocumentPath}]({docComparison.TestDocumentPath})");
            build.AppendLine();
            build.AppendLine($"There {(docComparison.CountPages > 1 ? "were" : "was")} {docComparison.CountPages} page{(docComparison.CountPages > 1 ? "s" : "")}.");
            build.AppendLine();
            build.AppendLine($"{docComparison.CountPagesWithDifferences} page{(docComparison.CountPagesWithDifferences > 1 ? "s" : "")} had differences.");
            build.AppendLine();
            build.AppendLine("##Page Details");
            build.AppendLine();
            build.AppendLine("| Reference | Difference | Test File | ");
            build.AppendLine("|---|---|---|");
            
            foreach (var page in docComparison.PageComparisons)
            {
                build.AppendLine($"| {(ImageLink("reference", page))} | {(ImageLink("result", page))} | {(ImageLink("test", page))} |");
            }

            File.WriteAllText(output, build.ToString());
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