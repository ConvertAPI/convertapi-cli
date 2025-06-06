using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

public class Program
{
    private const int HttpClientTimeoutSeconds = 1800;
    private const string ConvertApiBaseUrl = "https://v2.convertapi.com/convert";
    private const string UserAgentFormat = "convertapi-cli/{0}";

    public static async Task Main(string[] args)
    {
        if (args.Length < 3)
        {
            DisplayHelp();
            return;
        }

        // Replacing null values with empty string to avoid unexpected behaviour.
        args = args.Select(s => s ?? "").ToArray();

        string apiToken = args[0];
        string outputDirectoryOrFile = args[1];

        // Extract input files, formats, and dynamic properties
        var parametersCount = args.Count(x => x.Contains('='));
        int inputFilesEndIndex = args.Length > 4
            ? args.Length - parametersCount - 2 // Exclude [from-format] and [to-format]
            : args.Length - parametersCount;

        string[] inputFiles = args.Skip(2).Take(inputFilesEndIndex - 2).ToArray();

        string fromFormat = args.Length > inputFilesEndIndex
            ? args[^(2 + parametersCount)] // Second-to-last argument - parameters with '='
            : Path.GetExtension(inputFiles[0]).Trim('.').ToLower(); // Infer from first input file

        string toFormat = args.Length > inputFilesEndIndex + 1
            ? args[^(1 + parametersCount)] // Last argument - parameters with '='
            : Path.GetExtension(outputDirectoryOrFile).Trim('.').ToLower(); // Infer from output file

        ValidateInputFiles(inputFiles, fromFormat);

        // Extract dynamic properties
        var dynamicProperties = args.Skip(2 + inputFiles.Length).Where(arg => arg.Contains('=')).ToDictionary(
            arg => arg.Split('=')[0],
            arg => arg.Split('=')[1]
        );

        if (dynamicProperties.Any(x => x.Key.ToLower() == "storefile" && x.Value.ToLower() == "true"))
        {
            Console.WriteLine("Error: StoreFile parameter is not allowed in cli tool. Please use our API directly if you need this parameter.");
            Environment.Exit(1);
            return;
        }

        // Setting file name for simplified (3 parameters) conversion.
        if (args.Length == 3 && dynamicProperties.Keys.All(x => !x.Equals("FileName", StringComparison.CurrentCultureIgnoreCase)))
            dynamicProperties.Add("FileName", Path.GetFileNameWithoutExtension(outputDirectoryOrFile));

        await ConvertFiles(apiToken, fromFormat, toFormat, inputFiles, outputDirectoryOrFile, dynamicProperties);
    }

    private static void ValidateInputFiles(string[] inputFiles, string fromFormat)
    {
        if (fromFormat == "web")
            return;

        if (inputFiles.Length == 0)
        {
            Console.WriteLine("Error: At least one input file is required.");
            Environment.Exit(1);
            return;
        }

        foreach (var file in inputFiles)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine($"Error: Input file not found: {file}");
                Environment.Exit(1);
                return;
            }
        }
    }

    static void DisplayHelp()
    {
        Console.WriteLine("ConvertAPI CLI Tool");
        Console.WriteLine("====================");
        Console.WriteLine("ConvertAPI provides a simple way to convert files between different formats, merge, and apply transformations using its powerful API.");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  convertapi-cli.exe <api-token> <output-directory> <input-files...> [from-format] [to-format] [key1=value1 key2=value2 ...]");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  Convert a single PDF to DOCX:");
        Console.WriteLine("    convertapi-cli.exe YOUR_API_TOKEN output.docx input.pdf");
        Console.WriteLine();
        Console.WriteLine("  Merge multiple PDF files into one:");
        Console.WriteLine("    convertapi-cli.exe YOUR_API_TOKEN merged_output.pdf file1.pdf file2.pdf file3.pdf pdf merge");
        Console.WriteLine();
        Console.WriteLine("  Protect a PDF with a password:");
        Console.WriteLine("    convertapi-cli.exe YOUR_API_TOKEN protected_output.pdf input.pdf pdf protect UserPassword=1234 OwnerPassword=abcd FileName=protected");
        Console.WriteLine();
        Console.WriteLine("  Add a watermark to a PDF:");
        Console.WriteLine("    convertapi-cli.exe YOUR_API_TOKEN watermarked_output.pdf input.pdf pdf watermark Text=Confidential FileName=watermark");
        Console.WriteLine();
        Console.WriteLine("Dynamic Properties:");
        Console.WriteLine("  Specify additional parameters using key=value pairs, such as UserPassword, OwnerPassword, or WatermarkText, depending on the API being used.");
        Console.WriteLine();
        Console.WriteLine("Exit Codes:");
        Console.WriteLine("  0 - Success");
        Console.WriteLine("  1 - Error in arguments/parameters validation");
        Console.WriteLine("  2 - API response error");
        Console.WriteLine();
        Console.WriteLine("For more information, visit: https://www.convertapi.com/");
    }

    static async Task ConvertFiles(string apiToken, string fromFormat, string toFormat, string[] inputFiles, string outputPath, Dictionary<string, string> properties)
    {
        string url = $"{ConvertApiBaseUrl}/{fromFormat}/to/{toFormat}";

        using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(HttpClientTimeoutSeconds) })
        using (var form = new MultipartFormDataContent())
        {
            string version = GetVersion();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(string.Format(UserAgentFormat, version));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/mixed"));

            if (inputFiles.Any(x => !string.IsNullOrEmpty(x)))
            {
                if (inputFiles.Length == 1)
                {
                    AddFilesToFormParameters(form, "file", inputFiles[0]);
                }
                else
                {
                    int fileIndex = 0;
                    foreach (var inputFile in inputFiles)
                    {
                        AddFilesToFormParameters(form, $"files[{fileIndex}]", inputFile);
                        fileIndex++;
                    }
                }
            }
            
            foreach (var (filePropertyName, filePath) in properties.Where(x => x.Key.ToLower().EndsWith("file") && x.Key.ToLower() != ("storefile")))
            {
                AddFilesToFormParameters(form, filePropertyName, filePath);
            }

            foreach (var property in properties.Where(x => !x.Key.ToLower().EndsWith("file")))
            {
                form.Add(new StringContent(property.Value), property.Key);
            }

            Console.WriteLine($"Sending request to ConvertAPI with dynamic properties...");

            HttpResponseMessage response = await httpClient.PostAsync(url, form);

            if (response.IsSuccessStatusCode && response.Content.Headers.ContentType?.MediaType == "multipart/mixed")
            {
                Console.WriteLine("Processing multipart/mixed response...");

                var boundary = HeaderUtilities.RemoveQuotes(response.Content.Headers.ContentType.Parameters.First(p => p.Name == "boundary").Value).Value;
                var multipartReader = new MultipartReader(boundary, await response.Content.ReadAsStreamAsync());
                MultipartSection section;
                int fileIndex = 1;

                while ((section = await multipartReader.ReadNextSectionAsync()) != null)
                {
                    if (Microsoft.Net.Http.Headers.ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition) &&
                        (contentDisposition.FileName != null || contentDisposition.FileNameStar.HasValue))
                    {
                        var fileName = contentDisposition.FileNameStar.HasValue ? contentDisposition.FileNameStar.Value : contentDisposition.FileName.Value.Trim('"');
                        var filePath = Directory.Exists(outputPath) ? Path.Combine(outputPath, fileName) : Path.Combine(Path.GetDirectoryName(outputPath), fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await section.Body.CopyToAsync(fileStream);
                        }

                        Console.WriteLine($"File saved: {filePath}");
                        fileIndex++;
                    }
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}. Response message: {errorContent}");
                Environment.Exit(2);
            }
        }
    }

    private static void AddFilesToFormParameters(MultipartFormDataContent form, string parameterName,  string filePath)
    {
        form.Add(new StreamContent(File.OpenRead(filePath))
        {
            Headers = { ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = parameterName, FileName = Path.GetFileName(filePath) } }
        });
    }

    static string GetVersion()
    {
        var version = Assembly.GetExecutingAssembly()
            .GetName()
            .Version?
            .ToString() ?? "unknown";

        return version;
    }
}