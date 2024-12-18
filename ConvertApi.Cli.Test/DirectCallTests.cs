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
    public async Task TestConvertPdfToDocx()
    {
        var outputFile = Path.Combine(TestOutputDir, "simple.docx");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");

        await Program.Main([ApiToken, outputFile, inputFile]);

        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    public async Task TestMergePdfs()
    {
        var outputFile = Path.Combine(TestOutputDir, "simple.pdf");
        var inputFile1 = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");
        var inputFile2 = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "invoice.pdf");

        await Program.Main([ApiToken, TestOutputDir, inputFile1, inputFile2, "pdf", "merge"]);

        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    public async Task TestMergeMutiplePdfs()
    {
        var outputFile = Path.Combine(TestOutputDir, "simple.pdf");
        var inputFile1 = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");
        var inputFile2 = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "invoice.pdf");
        var inputFile3 = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "invoice.pdf");

        await Program.Main([ApiToken, TestOutputDir, inputFile1, inputFile2, inputFile3, "pdf", "merge"]);

        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    public async Task TestAddWatermarkToPdf()
    {
        var outputFile = Path.Combine(TestOutputDir, "watermark.pdf");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");

        await Program.Main([ApiToken, TestOutputDir, inputFile, "pdf", "watermark", "Text=Confidential", "FileName=watermark"]);


        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    public async Task TestProtectPdfWithPassword()
    {
        var outputFile = Path.Combine(TestOutputDir, "protected.pdf");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");

        await Program.Main([ApiToken, TestOutputDir, inputFile, "pdf", "protect", "UserPassword=1234", "OwnerPassword=abcd", "FileName=protected"]);

        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    public async Task TestPdfToPngWithResolution()
    {
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");

        await Program.Main([ApiToken, TestOutputDir, inputFile, "pdf", "png", "Resolution=300"]);

        Assert.IsTrue(File.Exists(outputFolder), "Output file was not created.");
    }

    [Test]
    public async Task TestPdfToExtractImages()
    {
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");

        await Program.Main([ApiToken, TestOutputDir, inputFile, "pdf", "extract-images"]);

        Assert.IsTrue(File.Exists(outputFolder), "Output file was not created.");
    }

    [Test]
    public async Task TestPdfToExtractImagesError()
    {
        var outputFolder = Path.Combine(TestOutputDir, "watermark.pdf");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");

        await Program.Main([ApiToken, TestOutputDir, inputFile, "pdf", "extract-images"]);

        Should fail
        Assert.IsTrue(File.Exists(outputFolder), "Output file was not created.");
    }

    [Test]
    public async Task TestWebToPdf()
    {
        var outputFolder = Path.Combine(TestOutputDir, "watermark.pdf");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");

        await Program.Main([ApiToken, TestOutputDir, "", "web", "pdf", "url=https://www.google.com"]);

        
        Assert.IsTrue(File.Exists(outputFolder), "Output file was not created.");
    }
}