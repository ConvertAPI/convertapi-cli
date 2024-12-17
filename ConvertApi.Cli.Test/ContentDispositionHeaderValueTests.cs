using Microsoft.Net.Http.Headers;

namespace ConvertApi.Cli.Test;

[TestFixture]
public class ContentDispositionHeaderValueTests
{
    [Test]
    public void TryParse_ValidContentDisposition_ReturnsTrueAndParsesContentDisposition()
    {
        // Arrange
        var validContentDisposition = "form-data; name=\"file\"; filename=\"example.txt\"";

        // Act
        var parseResult = ContentDispositionHeaderValue.TryParse(validContentDisposition, out var contentDisposition);

        // Assert
        Assert.IsTrue(parseResult, "Expected TryParse to return true for a valid content disposition string.");
        Assert.IsNotNull(contentDisposition, "Expected contentDisposition to be parsed successfully.");
        Assert.AreEqual("example.txt", contentDisposition.FileName.Value, "FileName should match the provided filename.");
        Assert.AreEqual("file", contentDisposition.Name.Value, "Name should match the provided name.");
    }
    
    [Test]
    public void TryParse_ValidContentDisposition_ReturnsTrueAndParsesContentDispositionSize()
    {
        // Arrange
        var validContentDisposition = "attachment; filename*=utf-8''demo.pdf; size=180658";

        // Act
        var parseResult = ContentDispositionHeaderValue.TryParse(validContentDisposition, out var contentDisposition);

        // Assert
        Assert.IsTrue(parseResult, "Expected TryParse to return true for a valid content disposition string.");
        Assert.IsNotNull(contentDisposition, "Expected contentDisposition to be parsed successfully.");
        Assert.AreEqual("demo.pdf", contentDisposition.FileNameStar.Value, "FileName should match the provided filename.");
    }
}