using System.Diagnostics;

namespace ConvertApi.Cli.Test;

[TestFixture]
public class CliTests
{
    private static readonly string SolutionDirectory = GetSolutionDirectory();
    private static readonly string CliExecutablePath = Path.Combine(SolutionDirectory, "ConvertApi.Cli", "bin", "Debug", "net8.0", "convertapi-cli.exe");
    private const string ApiToken = "your_api_token_here"; // Provide your API token
    private static readonly string TestOutputDir = Path.Combine(SolutionDirectory, "test_output");

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
        var outputFile = Path.Combine(TestOutputDir, "output.docx");
        var inputFile = "sample.pdf";
        var process = RunCli($"{ApiToken} {outputFile} {inputFile} pdf docx");

        Assert.AreEqual(0, process.ExitCode, "CLI did not exit cleanly.");
        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    public void TestMergePdfs()
    {
        var outputFile = Path.Combine(TestOutputDir, "merged.pdf");
        var process = RunCli($"{ApiToken} {outputFile} file1.pdf file2.pdf pdf merge");

        Assert.AreEqual(0, process.ExitCode, "CLI did not exit cleanly.");
        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    public void TestAddWatermarkToPdf()
    {
        var outputFile = Path.Combine(TestOutputDir, "watermarked.pdf");
        var process = RunCli($"{ApiToken} {outputFile} sample.pdf pdf pdf WatermarkText=Confidential");

        Assert.AreEqual(0, process.ExitCode, "CLI did not exit cleanly.");
        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    [Test]
    public void TestProtectPdfWithPassword()
    {
        var outputFile = Path.Combine(TestOutputDir, "protected.pdf");
        var process = RunCli($"{ApiToken} {outputFile} sample.pdf pdf pdf UserPassword=1234 OwnerPassword=abcd");

        Assert.AreEqual(0, process.ExitCode, "CLI did not exit cleanly.");
        Assert.IsTrue(File.Exists(outputFile), "Output file was not created.");
    }

    private Process RunCli(string arguments)
    {
        if (!File.Exists(CliExecutablePath))
        {
            throw new FileNotFoundException($"CLI executable not found at {CliExecutablePath}");
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = CliExecutablePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        Console.WriteLine(process.StandardOutput.ReadToEnd());
        Console.WriteLine(process.StandardError.ReadToEnd());

        return process;
    }

    private static string GetSolutionDirectory()
    {
        var directory = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(directory) && !File.Exists(Path.Combine(directory, "ConvertApi.Cli.sln")))
        {
            directory = Directory.GetParent(directory)?.FullName;
        }

        if (string.IsNullOrEmpty(directory))
        {
            throw new DirectoryNotFoundException("Solution directory not found.");
        }

        return directory;
    }
}