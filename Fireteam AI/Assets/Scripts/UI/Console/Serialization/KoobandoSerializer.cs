using Koobando.UI.Console.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Koobando.UI.Console
{
    /// <summary>
    /// Handles formatted serialization for console returns.
    /// </summary>
    public class KoobandoSerializer
    {
        private readonly Serializer[] _serializers;
        private readonly Dictionary<Type, Serializer> _serializerLookup = new Dictionary<Type, Serializer>();
        private readonly HashSet<Type> _unserializableLookup = new HashSet<Type>();

        private readonly Func<object, Theme, string> _recursiveSerializer;

        /// <summary>
        /// Creates a Serializer with a custom set of serializers.
        /// </summary>
        /// <param name="serializers">The Serializers to use in this Koobando Serializer.</param>
        public KoobandoSerializer(IEnumerable<Serializer> serializers)
        {
            _recursiveSerializer = SerializeFormatted;
            _serializers = serializers.OrderByDescending(x => x.Priority)
                                      .ToArray();
        }

        /// <summary>
        /// Creates a Serializer with the default injected serializers
        /// </summary>
        public KoobandoSerializer() : this(new InjectionLoader<Serializer>().GetInjectedInstances())
        {

        }

        /// <summary>
        /// Serializes the object with formatting for displaying in the console.
        /// </summary>
        /// <param name="value">The value to format and serialize.</param>
        /// <param name="theme">(Optional) KoobandoTheme to use for formatting the results.</param>
        /// <returns>The formatted serialization.</returns>
        public string SerializeFormatted(object value, Theme theme = null)
        {
            if (value is null)
            {
                return string.Empty;
            }

            Type type = value.GetType();
            string result = string.Empty;

            string SerializeInternal(Serializer serializer)
            {
                try
                {
                    return serializer.SerializeFormatted(value, theme, _recursiveSerializer);
                }
                catch (Exception e)
                {
                    throw new Exception($"Serialization of {type.GetDisplayName()} via {serializer} failed:\n{e.Message}", e);
                }
            }

            if (_serializerLookup.ContainsKey(type))
            {
                result = SerializeInternal(_serializerLookup[type]);
            }
            else if (_unserializableLookup.Contains(type))
            {
                result = value.ToString();
            }
            else
            {
                bool converted = false;

                foreach (Serializer serializer in _serializers)
                {
                    if (serializer.CanSerialize(type))
                    {
                        result = SerializeInternal(serializer);

                        _serializerLookup[type] = serializer;
                        converted = true;
                        break;
                    }
                }

                if (!converted)
                {
                    result = value.ToString();
                    _unserializableLookup.Add(type);
                }
            }

            if (theme && !string.IsNullOrWhiteSpace(result))
            {
                result = theme.ColorizeReturn(result, type);
            }

            return result;
        }
    }
}
