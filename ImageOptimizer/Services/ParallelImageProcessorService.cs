using ImageOptimizer.Models;

namespace ImageOptimizer.Services;

/// <summary>
/// Processes images concurrently using async/await and Parallel.ForEachAsync.
/// </summary>
public class ParallelImageProcessor : IImageProcessorService
{
    private readonly ImageDownloaderService? _downloader;
    private readonly int _maxParallelism;

    /// <summary>
    /// Creates a processor for FOLDER / MVP mode (no downloader needed).
    /// </summary>
    /// <param name="maxParallelism">Maximum number of images processed concurrently.</param>
    public ParallelImageProcessor(int maxParallelism = 8)
    {
        _downloader = null;
        _maxParallelism = maxParallelism;
    }

    /// <summary>
    /// Creates a processor for FILE / V1 mode (downloader required).
    /// </summary>
    /// <param name="downloader">Service used to download images from URLs.</param>
    /// <param name="maxParallelism">Maximum number of images processed concurrently.</param>
    public ParallelImageProcessor(ImageDownloaderService downloader, int maxParallelism = 8)
    {
        _downloader = downloader;
        _maxParallelism = maxParallelism;
    }

    /// <inheritdoc/>
    public async Task ProcessLocalFolderAsync(string inputDir, string outputDir)
    {
        var files = Directory.GetFiles(inputDir, "*.*")
                .Where(f => new[] { ".jpg", ".jpeg", ".png" }
                .Contains(Path.GetExtension(f).ToLowerInvariant()));

        await Parallel.ForEachAsync(files,
            new ParallelOptions { MaxDegreeOfParallelism = _maxParallelism },
            async (file, ct) =>
            {
                await ImageResizerService.ResizeFromFileAsync(file, outputDir, ct);
            });
    }

    /// <inheritdoc/>
    public async Task ProcessUrlsAsync(IEnumerable<ImageModel> sources, string outputDir)
    {
        if (_downloader is null)
        {
            throw new InvalidOperationException("Cannot process URLs without a downloader.");
        }

        await Parallel.ForEachAsync(sources,
            new ParallelOptions { MaxDegreeOfParallelism = _maxParallelism },
            async (src, ct) =>
            {
                await using var stream = await _downloader.DownloadAsync(src.Url, ct);
                await ImageResizerService.ResizeToWebpAsync(stream, outputDir, src.Name, ct);
            });
    }
}
