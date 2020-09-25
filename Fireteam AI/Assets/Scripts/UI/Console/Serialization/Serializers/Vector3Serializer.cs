using UnityEngine;

namespace Koobando.UI.Console.Serializers
{
    public class Vector3Serializer : BasicSerializer<Vector3>
    {
        public override string SerializeFormatted(Vector3 value, Theme theme)
        {
            return $"({value.x}, {value.y}, {value.z})";
        }
    }
}