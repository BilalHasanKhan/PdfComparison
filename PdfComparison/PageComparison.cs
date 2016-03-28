using System.IO;

namespace PdfComparison
{
    public class PageComparison
    {
        public const string TestRelativePath = "test";
        public const string ReferenceRelativePath = "reference";
        public const string ResultRelativePath = "result";

        public static string CreatePageFileName(int pageNumber) => $"Page{pageNumber:0000}.png";
        public static string CreateThumbnailFileName(int pageNumber) => $"Thumb{pageNumber:0000}.png";

        public string RootPath { get; set; }
        public int PageNumber { get; set; }
        public int PixelCount { get; set; }
        public double ComparisonScore { get; set; }
        public string TestPagePath => Path.Combine(RootPath, TestRelativePath, CreatePageFileName(PageNumber));
        public string TestPageThumbnailPath => Path.Combine(RootPath, TestRelativePath, CreateThumbnailFileName(PageNumber));
        public string ReferencePagePath => Path.Combine(RootPath, ReferenceRelativePath, CreatePageFileName(PageNumber));
        public string ReferencePageThumbnailPath => Path.Combine(RootPath, ReferenceRelativePath, CreateThumbnailFileName(PageNumber));
        public string ComparePagePath => Path.Combine(RootPath, ResultRelativePath, CreatePageFileName(PageNumber));
        public string ComparePageThumbnailPath => Path.Combine(RootPath, ResultRelativePath, CreateThumbnailFileName(PageNumber));
    }
}