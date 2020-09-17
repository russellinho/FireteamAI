using System;

namespace Koobando.UI.Console
{
    /// <summary>
    /// Serializer for all types inheriting from a single type.
    /// </summary>
    /// <typeparam name="T">Base type of the types to serialize.</typeparam>
    public abstract class PolymorphicSerializer<T> : Serializer where T : class
    {
        private Func<object, Theme, string> _recursiveSerializer;

        public virtual int Priority => -1000;

        public bool CanSerialize(Type type)
        {
            return typeof(T).IsAssignableFrom(type);
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
