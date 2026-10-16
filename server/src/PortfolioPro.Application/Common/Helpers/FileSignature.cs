namespace PortfolioPro.Application.Common.Helpers;

// Dosya uzantısına güvenmiyoruz, dosyanın ilk byte'larına (magic number) bakıyoruz.
// NOT: Birisi virus.exe'nin adını foto.jpg yapıp yükleyebilir. Uzantı aynı ama içerik farklı.
// Her dosya tipinin başında sabit byte'lar var, onları kontrol ediyoruz.
public static class FileSignature
{
    private static readonly Dictionary<string, byte[][]> Signatures = new()
    {
        [".jpg"] = [[0xFF, 0xD8, 0xFF]],
        [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
        [".png"] = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],
        [".webp"] = [[0x52, 0x49, 0x46, 0x46]], // "RIFF", devamında "WEBP" kontrolü aşağıda
        [".pdf"] = [[0x25, 0x50, 0x44, 0x46]]   // "%PDF"
    };

    public static bool IsValid(Stream stream, string extension)
    {
        if (!Signatures.TryGetValue(extension, out var signatures))
            return false;

        // NOT: Burada Span yerine normal dizi kullandım. Span lambda içinde kullanılamıyor, derleme hatası veriyordu.
        var header = new byte[12];
        var originalPosition = stream.Position;
        var read = stream.Read(header, 0, header.Length);
        stream.Position = originalPosition; // okuduktan sonra başa sarmayı unutma, yoksa kaydederken eksik yazar

        var ok = signatures.Any(sig => read >= sig.Length && header.Take(sig.Length).SequenceEqual(sig));

        if (ok && extension == ".webp")
            ok = read >= 12 && header.Skip(8).Take(4).SequenceEqual("WEBP"u8.ToArray());

        return ok;
    }
}
