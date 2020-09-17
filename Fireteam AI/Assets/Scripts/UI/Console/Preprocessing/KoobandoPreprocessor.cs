using System;
using System.Collections.Generic;
using System.Linq;

namespace Koobando.UI.Console
{
    /// <summary>
    /// Handles preprocessing of console input.
    /// </summary>
    public class KoobandoPreprocessor
    {
        private readonly Preprocessor[] _preprocessors;

        /// <summary>
        /// Creates a Koobando Preprocessor with a custom set of preprocessors.
        /// </summary>
        /// <param name="preprocessors">The Preprocessors to use in this Koobando Preprocessor.</param>
        public KoobandoPreprocessor(IEnumerable<Preprocessor> preprocessors)
        {
            _preprocessors = preprocessors.OrderByDescending(x => x.Priority)
                                          .ToArray();
        }

        /// <summary>
        /// Creates a Quantum Preprocessor with the default injected preprocessors
        /// </summary>
        public KoobandoPreprocessor() : this(new InjectionLoader<Preprocessor>().GetInjectedInstances())
        {

        }

        /// <summary>
        /// Processes the provided text.
        /// </summary>
        /// <param name="text">The text to process.</param>
        /// <returns>The processed text.</returns>
        public string Process(string text)
        {
            foreach (Preprocessor preprocessor in _preprocessors)
            {
                try
                {
                    text = preprocessor.Process(text);
                }
                catch (Exception e)
                {
                    throw new Exception($"Preprocessor {preprocessor} failed:\n{e.Message}", e);
                }
            }

            return text;
        }
    }
}
