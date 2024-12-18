namespace ConvertApi.Cli.Test;

[TestFixture]
public class DirectCallTests
{
    private const string ApiToken = "token_ST4z2qhE"; // Provide your API token
    private static readonly string TestOutputDir = Path.Combine(Directory.GetCurrentDirectory(), "test_output");

    [SetUp]
    public void Setup()
    {
        if (Directory.Exists(TestOutputDir))
        {
            Directory.Delete(TestOutputDir, true);
        }
        Directory.CreateDirectory(TestOutputDir);
    }

    [Test]
    public void TestConvertPdfToDocx()
    {
        var outputFile = Path.Combine(TestOutputDir, "simple.docx");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");

        Run($"{ApiToken} {outputFile} {inputFile}", inputFile);
        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    public void TestMergePdfs()
    {
        var outputFile = Path.Combine(TestOutputDir, "simple.pdf");
        var inputFile1 = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");
        var inputFile2 = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "invoice.pdf");
        Run($"{ApiToken} {outputFile} {inputFile1} {inputFile2} pdf merge", $"{inputFile1}{Environment.NewLine}{inputFile2}{Environment.NewLine}");

        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    public void TestAddWatermarkToPdf()
    {
        var outputFile = Path.Combine(TestOutputDir, "watermark.pdf");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");
        Run($"{ApiToken} {outputFile} {inputFile} pdf watermark Text=Confidential FileName=watermark", inputFile);

        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    public void TestProtectPdfWithPassword()
    {
        var outputFile = Path.Combine(TestOutputDir, "protected.pdf");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");
        Run($"{ApiToken} {outputFile} {inputFile} pdf protect UserPassword=1234 OwnerPassword=abcd FileName=protected", inputFile);
        
        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    private void Run(string arguments, string inputFiles = "")
    {
        var originalIn = Console.In;
        try
        {
            Console.SetIn(new StringReader(inputFiles));
            Program.Main(arguments.Split()).GetAwaiter().GetResult();
        }
        finally
        {
            // Restore the original input
            Console.SetIn(originalIn);
        }
    }
}