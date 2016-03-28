#Pdf Image-Based Comparison

This is a simple app which:

- Takes 2 PDF inputs 
- Splits each of these into individual pages, creating a PNG image for each input page
- Compares these page images between the documents
- Outputs images, thumbnails and a json file with "scores" in to disk
- Writes the location of the json results file to stdout (to `Console.WriteLine`)

This experiment leans heavily on the fabulous Magick.Net Nuget package (which itself relies heavily on the fabulous ImageMagick, along with GhostScript to provide PDF rendering).

To use:

    PdfComparison.exe [prefix_for_output_folder] [path_for_output] [path_to_pdf_a] [path_to_pdf_b]

Example json output for 2 single file PDFs compared:

```
{
  "TestDocumentPath": "C:\\Users\\Stuart\\Downloads\\BritishAthletics-Groupon-8B123B4.pdf",
  "ReferenceDocumentPath": "C:\\Users\\Stuart\\Downloads\\BritishAthletics-Groupon-8A123C4.pdf",
  "PageComparisons": [
    {
      "RootPath": "\\\\STU7\\Temp\\BritAth_16_03_27_19_04_20",
      "PageNumber": 1,
      "PixelCount": 8699840,
      "ComparisonScore": 80607.0,
      "TestPagePath": "\\\\STU7\\Temp\\BritAth_16_03_27_19_04_20\\test\\Page0001.png",
      "TestPageThumbnailPath": "\\\\STU7\\Temp\\BritAth_16_03_27_19_04_20\\test\\Thumb0001.png",
      "ReferencePagePath": "\\\\STU7\\Temp\\BritAth_16_03_27_19_04_20\\reference\\Page0001.png",
      "ReferencePageThumbnailPath": "\\\\STU7\\Temp\\BritAth_16_03_27_19_04_20\\reference\\Thumb0001.png",
      "ComparePagePath": "\\\\STU7\\Temp\\BritAth_16_03_27_19_04_20\\result\\Page0001.png",
      "ComparePageThumbnailPath": "\\\\STU7\\Temp\\BritAth_16_03_27_19_04_20\\result\\Thumb0001.png"
    }
  ],
  "ComparisonScore": 80607.0,
  "PixelCount": 8699840,
  "CountPagesWithDifferences": 1,
  "CountPages": 1
}
```

Further experiments may follow - e.g. considering the use of Apache.PdfBox for text comparison.


#License

This code is licensed openly - please consider it as MsPL

The libraries used - including Magick.Net, ImageMagick and GhostScript all have their own licensing - please be especially aware of GhostScript's AGPL license.