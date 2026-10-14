using PortfolioPro.Application.Common.Helpers;

namespace PortfolioPro.UnitTests;

public class FileSignatureTests
{
    [Fact]
    public void IsValid_ShouldAcceptRealPng()
    {
        using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00]);

        Assert.True(FileSignature.IsValid(stream, ".png"));
    }

    [Fact]
    public void IsValid_ShouldRejectExeRenamedAsJpg()
    {
        // "MZ" Windows exe dosyalarının başlangıcı
        using var stream = new MemoryStream([0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00]);

        Assert.False(FileSignature.IsValid(stream, ".jpg"));
    }

    [Fact]
    public void IsValid_ShouldNotChangeStreamPosition()
    {
        using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46, 0x2D, 0x31]);

        FileSignature.IsValid(stream, ".pdf");

        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public void IsValid_ShouldRejectUnknownExtension()
    {
        using var stream = new MemoryStream([0x3C, 0x73, 0x76, 0x67]); // "<svg"

        Assert.False(FileSignature.IsValid(stream, ".svg"));
    }
}
