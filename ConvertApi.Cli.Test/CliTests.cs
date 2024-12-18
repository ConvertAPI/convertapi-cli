using System.Diagnostics;

namespace ConvertApi.Cli.Test;

[TestFixture]
public class CliTests
{
    private static readonly string CliExecutablePath = Path.Combine(Directory.GetCurrentDirectory(), "convertapi-cli.exe");
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
    [TestCase(true)]
    [TestCase(false)]
    public void TestConvertPdfToDocx(bool cli)
    {
        var outputFile = Path.Combine(TestOutputDir, "simple.docx");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");

        var process = Run($"{ApiToken} {outputFile} {inputFile}", cli, inputFile);

        if (cli)
            Assert.AreEqual(0, process.ExitCode, "CLI did not exit cleanly.");
        
        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TestMergePdfs(bool cli)
    {
        var outputFile = Path.Combine(TestOutputDir, "simple.pdf");
        var inputFile1 = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");
        var inputFile2 = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "invoice.pdf");
        var process = Run($"{ApiToken} {outputFile} {inputFile1} {inputFile2} pdf merge", cli, $"{inputFile1}{Environment.NewLine}{inputFile2}{Environment.NewLine}");

        if (cli)
            Assert.AreEqual(0, process.ExitCode, "CLI did not exit cleanly.");
        
        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TestAddWatermarkToPdf(bool cli)
    {
        var outputFile = Path.Combine(TestOutputDir, "watermark.pdf");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");
        var process = Run($"{ApiToken} {outputFile} {inputFile} pdf watermark Text=Confidential FileName=watermark", cli, inputFile);

        if (cli)
            Assert.AreEqual(0, process.ExitCode, "CLI did not exit cleanly.");
        
        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TestProtectPdfWithPassword(bool cli)
    {
        var outputFile = Path.Combine(TestOutputDir, "protected.pdf");
        var inputFile = Path.Combine(Directory.GetCurrentDirectory(), "../../../../", "test_files", "simple.pdf");
        var process = Run($"{ApiToken} {outputFile} {inputFile} pdf protect UserPassword=1234 OwnerPassword=abcd FileName=protected", cli, inputFile);

        if (cli)
            Assert.AreEqual(0, process.ExitCode, "CLI did not exit cleanly.");
        
        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    private Process? Run(string arguments, bool cli = true, string inputFiles = "")
    {
        if (cli)
            return RunCli(arguments);
        
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

        return null;
    }
    
    private Process RunCli(string arguments)
    {
        if (!File.Exists(CliExecutablePath))
            throw new FileNotFoundException($"CLI executable not found at {CliExecutablePath}");
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = CliExecutablePath,
                Arguments = arguments,
                UseShellExecute = true, // Run in a normal console environment, because otherwise process hangs because of readline in program.cs
                CreateNoWindow = true 
            }
        };
        
        if (!process.Start())
            throw new InvalidOperationException("Failed to start the CLI process.");
        
        // 20s to prevent infinite hangs
        if (!process.WaitForExit(20000))
        {
            process.Kill();
            throw new TimeoutException("The CLI process did not exit within 2 minutes.");
        }
        
        if (process.ExitCode != 0)
            throw new Exception($"CLI process exited with code {process.ExitCode}");
        
        return process;
    }
}