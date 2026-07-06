namespace AlpineGearHub.Listings.Application.Commands.UploadListingImage;

// The client-supplied Content-Type header is just a claim, not a fact - a request can label
// arbitrary bytes as "image/png". This checks the actual file signature (magic bytes) instead,
// so storage only ever holds files that are genuinely one of the allowed image formats.
internal static class ImageSignature
{
    // WebP's marker starts at byte 8, so 12 bytes covers every signature checked below.
    private const int HeaderLength = 12;

    public static async Task<string?> DetectContentTypeAsync(Stream content, CancellationToken ct)
    {
        if (!content.CanSeek)
            throw new InvalidOperationException("Cannot validate an unseekable upload stream.");

        var startPosition = content.Position;
        var header = new byte[HeaderLength];
        var bytesRead = await content.ReadAsync(header.AsMemory(0, HeaderLength), ct);
        content.Position = startPosition;

        if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return "image/jpeg";

        if (bytesRead >= 8
            && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
            && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            return "image/png";

        if (bytesRead >= 12
            && header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F'
            && header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P')
            return "image/webp";

        return null;
    }
}
