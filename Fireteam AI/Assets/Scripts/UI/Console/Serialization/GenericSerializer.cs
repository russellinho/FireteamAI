using Koobando.UI.Console.Utilities;
using System;

namespace Koobando.UI.Console
{
    /// <summary>
    /// Serializer for all types that are generic constructions of a single type.
    /// </summary>
    public abstract class GenericSerializer : Serializer
    {
        /// <summary>
        /// The incomplete generic type of this serializer.
        /// </summary>
        protected abstract Type GenericType { get; }

        private Func<object, Theme, string> _recursiveSerializer;

        protected GenericSerializer()
        {
            if (!GenericType.IsGenericType)
            {
                throw new ArgumentException($"Generic Serializers must use a generic type as their base");
            }

            if (GenericType.IsConstructedGenericType)
            {
                throw new ArgumentException($"Generic Serializers must use an incomplete generic type as their base");
            }
        }

        public virtual int Priority => -500;

        public bool CanSerialize(Type type)
        {
            return type.IsGenericTypeOf(GenericType);
        }

        string Serializer.SerializeFormatted(object value, Theme theme, Func<object, Theme, string> recursiveSerializer)
        {
            _recursiveSerializer = recursiveSerializer;
            return SerializeFormatted(value, theme);
        }

        protected string SerializeRecursive(object value, Theme theme)
        {
            return _recursiveSerializer(value, theme);
        }

        public abstract string SerializeFormatted(object value, Theme theme);
    }
}