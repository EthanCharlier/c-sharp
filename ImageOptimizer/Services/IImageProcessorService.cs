using ImageOptimizer.Models;

namespace ImageOptimizer.Services;

/// <summary>
/// Processes images into multiple WebP resolutions.
/// Implementations differ in their execution model (sequential vs parallel).
/// </summary>
public interface IImageProcessorService
{
    /// <summary>
    /// Processes all images from a local folder (FOLDER / MVP mode).
    /// </summary>
    /// <param name="inputDir">Folder containing source images (.jpg, .jpeg, .png).</param>
    /// <param name="outputDir">Destination folder for the generated WebP files.</param>
    Task ProcessLocalFolderAsync(string inputDir, string outputDir);

    /// <summary>
    /// Downloads images from URLs and processes them (FILE / V1 mode).
    /// </summary>
    /// <param name="sources">Image sources to download and process.</param>
    /// <param name="outputDir">Destination folder for the generated WebP files.</param>
    Task ProcessUrlsAsync(IEnumerable<ImageModel> sources, string outputDir);
}
