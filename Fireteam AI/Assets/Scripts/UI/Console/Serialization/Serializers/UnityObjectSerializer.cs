using UnityEngine;

namespace Koobando.UI.Console.Serializers
{
    public class UnityObjectSerializer : PolymorphicSerializer<Object>
    {
        public override string SerializeFormatted(Object value, Theme theme)
        {
            return value.name;
        }
    }
}
