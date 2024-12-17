﻿using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

class Program
{
    private const int HttpClientTimeoutSeconds = 1800;
    private const string ConvertApiBaseUrl = "https://v2.convertapi.com/convert";
    private const string UserAgentFormat = "convertapi-cli/{0}";

    static async Task Main(string[] args)
    {
        if (args.Length < 2 && Console.IsInputRedirected == false)
        {
            DisplayHelp();
            return;
        }

        string apiToken = args.Length > 0 ? args[0] : null;
        string outputFile = args.Length > 1 ? args[1] : null;
        string[] inputFiles;

        if (Console.IsInputRedirected)
        {
            Console.WriteLine("Reading input files from PIPE...");
            var pipeInput = new List<string>();
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                pipeInput.Add(line);
            }
            inputFiles = pipeInput.ToArray();
        }
        else
        {
            inputFiles = args.Skip(2).TakeWhile(arg => !arg.Contains('=')).ToArray();
        }

        // Validate input files
        foreach (var file in inputFiles)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine($"Error: Input file not found: {file}");
                return;
            }
        }

        string fromFormat = Directory.Exists(outputFile) ? (args.Length > 3 ? args[^2] : throw new ArgumentException("[from-format] is required when output path is a folder.")) : Path.GetExtension(inputFiles[0]).Trim('.').ToLower();
        string toFormat = Directory.Exists(outputFile) ? (args.Length > 4 ? args[^1] : throw new ArgumentException("[to-format] is required when output path is a folder.")) : Path.GetExtension(outputFile).Trim('.').ToLower();

        var dynamicProperties = args.Skip(2 + inputFiles.Length).Where(arg => arg.Contains('=')).ToDictionary(
            arg => arg.Split('=')[0],
            arg => arg.Split('=')[1]
        );

        await ConvertFiles(apiToken, fromFormat, toFormat, inputFiles, outputFile, dynamicProperties);
    }

    static void DisplayHelp()
    {
        Console.WriteLine("ConvertAPI CLI Tool");
        Console.WriteLine("====================");
        Console.WriteLine("ConvertAPI provides a simple way to convert files between different formats, merge, and apply transformations using its powerful API.");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  convertapi-cli.exe <api-token> <output-file> <input-files...> [from-format] [to-format] [key1=value1 key2=value2 ...]");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  Convert a single PDF to DOCX:");
        Console.WriteLine("    convertapi-cli.exe YOUR_API_TOKEN output.docx input.pdf");
        Console.WriteLine();
        Console.WriteLine("  Merge multiple PDF files into one:");
        Console.WriteLine("    convertapi-cli.exe YOUR_API_TOKEN merged_output.pdf file1.pdf file2.pdf file3.pdf pdf merge");
        Console.WriteLine();
        Console.WriteLine("  Protect a PDF with a password:");
        Console.WriteLine("    convertapi-cli.exe YOUR_API_TOKEN protected_output.pdf input.pdf UserPassword=1234 OwnerPassword=abcd");
        Console.WriteLine();
        Console.WriteLine("  Add a watermark to a PDF:");
        Console.WriteLine("    convertapi-cli.exe YOUR_API_TOKEN watermarked_output.pdf input.pdf WatermarkText=Confidential");
        Console.WriteLine();
        Console.WriteLine("Dynamic Properties:");
        Console.WriteLine("  Specify additional parameters using key=value pairs, such as UserPassword, OwnerPassword, or WatermarkText, depending on the API being used.");
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

            if (inputFiles.Length == 1)
            {
                form.Add(new StreamContent(File.OpenRead(inputFiles[0]))
                {
                    Headers = { ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "file", FileName = Path.GetFileName(inputFiles[0]) } }
                });
            }
            else
            {
                foreach (var inputFile in inputFiles)
                {
                    form.Add(new StreamContent(File.OpenRead(inputFile))
                    {
                        Headers = { ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "files", FileName = Path.GetFileName(inputFile) } }
                    });
                }
            }

            foreach (var property in properties)
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
                    if (Microsoft.Net.Http.Headers.ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition) && (contentDisposition.FileName != null || contentDisposition.FileNameStar.HasValue))
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
            }
        } 
    } 
  
  static string GetVersion()
    {
        var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        return version ?? "unknown";
    }
}