namespace ImageOptimizer.Models;

/// <summary>
/// Represents an image source loaded from the JSON sources file (FILE mode).
/// </summary>
/// <param name="Name">Logical name used as the base filename for generated outputs.</param>
/// <param name="Url">Absolute HTTP(S) URL of the source image.</param>
public record ImageModel(string Name, string Url);
