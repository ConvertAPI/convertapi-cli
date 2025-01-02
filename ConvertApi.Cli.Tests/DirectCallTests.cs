namespace ConvertApi.Cli.Tests;

[TestFixture]
public class DirectCallTests
{
    private const string ApiToken = "token_6yNouyuY"; // Provide your API token
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
        var outputFile = Path.Combine(TestOutputDir, "simple2.docx");
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
    public async Task TestAddWatermarkOverlayToPdf()
    {
        var outputFile = Path.Combine(TestOutputDir, "watermarkOverlay.pdf");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");
        var overlayFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");
    
        await Program.Main([ApiToken, TestOutputDir, inputFile, "pdf", "watermark-overlay", $"OverlayFile={overlayFile}", "FileName=watermarkOverlay"]);
    
        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }
    
    [Test]
    public async Task TestDocxCompare()
    {
        var outputFile = Path.Combine(TestOutputDir, "demo.docx");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "demo.docx");
        var compareFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "demo-changed.docx");
    
        await Program.Main([ApiToken, TestOutputDir, inputFile, "docx", "compare", $"CompareFile={compareFile}", "storeFile=false"]);
    
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
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "22pages.pdf");
    
        await Program.Main([ApiToken, TestOutputDir, inputFile, "pdf", "png", "Resolution=300"]);

        var outputFiles = Directory.GetFiles(TestOutputDir);
        Assert.That(outputFiles, Has.Length.EqualTo(22), "Output files were not created.");
    }
    
    [Test]
    public async Task TestPdfToExtractImages()
    {
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");
    
        await Program.Main([ApiToken, TestOutputDir, inputFile, "pdf", "extract-images"]);
    
        var outputFiles = Directory.GetFiles(TestOutputDir);
        Assert.That(outputFiles, Has.Length.EqualTo(1), "Output file was not created.");
    }
    
    [Test]
    public async Task TestPdfToExtractImagesError()
    {
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "22pages.pdf");
    
        await Program.Main([ApiToken, TestOutputDir, inputFile, "pdf", "extract-images"]);
        
        var outputFiles = Directory.GetFiles(TestOutputDir);
        Assert.That(outputFiles, Has.Length.EqualTo(0), "There should not be any converted files.");
    }
    
    [Test]
    public async Task TestPdfToDocxWithMixedUpInputs()
    {
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");
    
        // pdf and docx are mixed on purpose!
        await Program.Main([ApiToken, TestOutputDir, inputFile, "docx", "pdf"]);
        
        var outputFiles = Directory.GetFiles(TestOutputDir);
        Assert.That(outputFiles, Has.Length.EqualTo(0), "There should not be any converted files.");
    }

    [Test]
    public async Task TestWebToPdf()
    {
        await Program.Main([ApiToken, TestOutputDir, "", "web", "pdf", "url=https://www.google.com"]);
        
        var outputFiles = Directory.GetFiles(TestOutputDir);
        Assert.That(outputFiles, Has.Length.EqualTo(1), "Output file was not created.");
    }
    
    [Test]
    public async Task TestWebToPdfWithNullasInput()
    {
        await Program.Main([ApiToken, TestOutputDir, null, "web", "pdf", "url=https://www.google.com"]);
        
        var outputFiles = Directory.GetFiles(TestOutputDir);
        Assert.That(outputFiles, Has.Length.EqualTo(1), "Output file was not created.");
    }
}