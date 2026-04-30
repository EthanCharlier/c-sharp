namespace ImageOptimizer.Services;

/// <summary>
/// Downloads remote images over HTTP into in-memory streams (used in FILE / V1 mode).
/// </summary>
public class ImageDownloaderService
{
    private readonly HttpClient _http;

    /// <summary>
    /// Creates a new downloader using the provided HTTP client.
    /// </summary>
    /// <param name="http">HTTP client to use for all download operations.</param>
    public ImageDownloaderService(HttpClient http) => _http = http;

    /// <summary>
    /// Downloads an image from a URL and returns its content as an in-memory stream.
    /// </summary>
    /// <param name="url">Absolute HTTP(S) URL of the image to download.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A seekable stream containing the image bytes. The caller must dispose it.</returns>
    public async Task<Stream> DownloadAsync(string url, CancellationToken ct = default)
    {
        // Fully buffer the response so the resulting stream can be re-read for each output resolution.
        var bytes = await _http.GetByteArrayAsync(url, ct);
        return new MemoryStream(bytes);
    }
}
