using UnityEngine;

namespace Koobando.UI.Console.Serializers
{
    public class Vector4Serializer : BasicSerializer<Vector4>
    {
        public override string SerializeFormatted(Vector4 value, Theme theme)
        {
            return $"({value.x}, {value.y}, {value.z}, {value.w})";
        }
    }
}