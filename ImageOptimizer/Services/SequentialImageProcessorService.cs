using ImageOptimizer.Models;

namespace ImageOptimizer.Services;

/// <summary>
/// Processes images one at a time, in a single thread (baseline for benchmarking).
/// </summary>
public class SequentialImageProcessor : IImageProcessorService
{
    private readonly ImageDownloaderService? _downloader;

    /// <summary>
    /// Creates a processor for FOLDER / MVP mode (no downloader needed).
    /// </summary>
    public SequentialImageProcessor() => _downloader = null;

    /// <summary>
    /// Creates a processor for FILE / V1 mode (downloader required).
    /// </summary>
    /// <param name="downloader">Service used to download images from URLs.</param>
    public SequentialImageProcessor(ImageDownloaderService downloader) => _downloader = downloader;

    /// <inheritdoc/>
    public async Task ProcessLocalFolderAsync(string inputDir, string outputDir)
    {
        var files = Directory.GetFiles(inputDir, "*.*")
                .Where(f => new[] { ".jpg", ".jpeg", ".png" }
                .Contains(Path.GetExtension(f).ToLowerInvariant()));

        foreach (var file in files)
        {
            await ImageResizerService.ResizeFromFileAsync(file, outputDir);
        }
    }

    /// <inheritdoc/>
    public async Task ProcessUrlsAsync(IEnumerable<ImageModel> sources, string outputDir)
    {
        if (_downloader is null)
        {
            throw new InvalidOperationException("Cannot process URLs without a downloader.");
        }

        foreach (var src in sources)
        {
            await using var stream = await _downloader.DownloadAsync(src.Url);
            await ImageResizerService.ResizeToWebpAsync(stream, outputDir, src.Name);
        }
    }
}
