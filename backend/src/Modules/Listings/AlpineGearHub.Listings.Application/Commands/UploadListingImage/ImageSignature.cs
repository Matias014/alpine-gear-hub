namespace AlpineGearHub.Listings.Application.Commands.UploadListingImage;

// The client-supplied Content-Type header (and filename) are just claims, not facts - a request
// can label arbitrary bytes as "image/png" or name the file whatever it likes. This checks the
// actual file signature (magic bytes) instead and hands back a matching, hardcoded-safe extension,
// so storage only ever holds files that are genuinely one of the allowed image formats, under a
// key built entirely from trusted values (never from client-supplied strings).
internal static class ImageSignature
{
    public readonly record struct Detected(string ContentType, string Extension);

    // WebP's marker starts at byte 8, so 12 bytes covers every signature checked below.
    private const int HeaderLength = 12;

    public static async Task<Detected?> DetectAsync(Stream content, CancellationToken ct)
    {
        if (!content.CanSeek)
            throw new InvalidOperationException("Cannot validate an unseekable upload stream.");

        var startPosition = content.Position;
        var header = new byte[HeaderLength];
        var bytesRead = await content.ReadAsync(header.AsMemory(0, HeaderLength), ct);
        content.Position = startPosition;

        if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return new Detected("image/jpeg", ".jpg");

        if (bytesRead >= 8
            && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
            && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            return new Detected("image/png", ".png");

        if (bytesRead >= 12
            && header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F'
            && header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P')
            return new Detected("image/webp", ".webp");

        return null;
    }
}
