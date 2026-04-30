namespace ImageOptimizer.Models.Enums
{
    /// <summary>
    /// Specifies the type of input source provided to the optimizer.
    /// </summary>
    public enum ModeEnum
    {
        /// <summary>
        /// V1: JSON file containing remote image URLs.
        /// </summary>
        FILE,

        /// <summary>
        /// MVP: local folder containing image files.
        /// </summary>
        FOLDER
    }
}
