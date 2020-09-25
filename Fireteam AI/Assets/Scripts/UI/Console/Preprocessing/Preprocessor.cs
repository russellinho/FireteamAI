namespace Koobando.UI.Console
{
    /// <summary>
    /// Creates a Preprocessor that is loaded and used by the Processor.
    /// </summary>
    public interface Preprocessor
    {
        /// <summary>
        /// The priority of this preprocessor to resolve processing order.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Processes the provided text.
        /// </summary>
        /// <param name="text">The text to process.</param>
        /// <returns>The processed text.</returns>
        string Process(string text);
    }
}