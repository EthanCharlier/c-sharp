using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ImageOptimizer.Services;

/// <summary>
/// Resizes images into multiple WebP resolutions (1080p, 720p, 480p).
/// </summary>
public static class ImageResizerService
{
    private static readonly int[] Resolutions = { 1080, 720, 480 };

    /// <summary>
    /// Resizes an image from a stream and saves the WebP outputs.
    /// </summary>
    /// <param name="sourceStream">Stream containing the source image.</param>
    /// <param name="outputDir">Destination folder for the WebP files.</param>
    /// <param name="baseName">Base filename used to build output names.</param>
    /// <param name="ct">Optional cancellation token.</param>
    public static async Task ResizeToWebpAsync(
        Stream sourceStream,
        string outputDir,
        string baseName,
        CancellationToken ct = default)
    {
        // Load the image once; it will be cloned for each target resolution.
        using var image = await Image.LoadAsync(sourceStream, ct);

        Directory.CreateDirectory(outputDir);
        var encoder = new WebpEncoder { Quality = 80 };

        foreach (var height in Resolutions)
        {
            // Clone so each resize doesn't alter the source between passes.
            using var clone = image.Clone(ctx =>
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(0, height), // width auto, ratio preserved
                    Mode = ResizeMode.Max
                }));

            var outputPath = Path.Combine(outputDir, $"{baseName}_{height}p.webp");
            await clone.SaveAsync(outputPath, encoder, ct);
        }
    }

    /// <summary>
    /// Resizes an image from a local file and saves the WebP outputs.
    /// </summary>
    /// <param name="filePath">Path to the source image on disk.</param>
    /// <param name="outputDir">Destination folder for the WebP files.</param>
    /// <param name="ct">Optional cancellation token.</param>
    public static async Task ResizeFromFileAsync(string filePath, string outputDir, CancellationToken ct = default)
    {
        var baseName = Path.GetFileNameWithoutExtension(filePath);
        await using var fs = File.OpenRead(filePath);
        await ResizeToWebpAsync(fs, outputDir, baseName, ct);
    }
}
