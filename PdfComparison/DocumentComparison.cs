using System.Collections.Generic;
using System.Linq;

namespace PdfComparison
{
    public class DocumentComparison
    {
        public string TestDocumentPath { get; set; }
        public string ReferenceDocumentPath { get; set; }
        public List<PageComparison> PageComparisons { get; set; }
        public double ComparisonScore => PageComparisons.Sum(p => p.ComparisonScore);
        public int PixelCount => PageComparisons.Sum(p => p.PixelCount);
        public int CountPagesWithDifferences => PageComparisons.Count(p => p.ComparisonScore > 0);
        public int CountPages => PageComparisons.Count;
    }
}