using System;

namespace Koobando.UI.Console
{
    /// <summary>
    /// Serializer for a single type.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    public abstract class BasicSerializer<T> : Serializer
    {
        private Func<object, Theme, string> _recursiveSerializer;

        public virtual int Priority => 0;

        public bool CanSerialize(Type type)
        {
            return type == typeof(T);
        }

        string Serializer.SerializeFormatted(object value, Theme theme, Func<object, Theme, string> recursiveSerializer)
        {
            _recursiveSerializer = recursiveSerializer;
            return SerializeFormatted((T)value, theme);
        }

        protected string SerializeRecursive(object value, Theme theme)
        {
            return _recursiveSerializer(value, theme);
        }

        public abstract string SerializeFormatted(T value, Theme theme);
    }
}
