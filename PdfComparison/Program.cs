using System;
using System.IO;

namespace PdfComparison
{
    public class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine($"Usage incorrect => must pass in 3 parameters");
                Console.WriteLine($"   TestNamePrefix");
                Console.WriteLine($"   OutputPath");
                Console.WriteLine($"   pathToReferencePdf");
                Console.WriteLine($"   pathToTestPdf");
                return -1;
            }

            var testNamePrefix = args[0];
            var outputBasePath = args[1];
            var referencePdf = args[2];
            var testFilePdf = args[3];
            if (!Directory.Exists(outputBasePath))
            {
                Console.WriteLine($"OutputPath does not exist - {outputBasePath}");
                return -1;
            }
            if (!File.Exists(referencePdf))
            {
                Console.WriteLine($"Reference file does not exist - {referencePdf}");
                return -1;
            }
            if (!File.Exists(testFilePdf))
            {
                Console.WriteLine($"Test file does not exist - {testFilePdf}");
                return -1;
            }

            try
            {
                var resultPath = PDFComparer.DoComparison(testNamePrefix, outputBasePath, referencePdf, testFilePdf);
                Console.WriteLine(resultPath);
                return 0;
            }
            catch (Exception pokemon)
            {
                Console.WriteLine("Error");
                Console.WriteLine(pokemon.GetType().Name);
                Console.WriteLine(pokemon.Message);
                Console.WriteLine(pokemon.StackTrace);
                return -1;
            }
        }
    }
}
