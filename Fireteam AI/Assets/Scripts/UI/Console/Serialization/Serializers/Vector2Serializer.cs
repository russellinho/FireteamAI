using UnityEngine;

namespace Koobando.UI.Console.Serializers
{
    public class Vector2Serializer : BasicSerializer<Vector2>
    {
        public override string SerializeFormatted(Vector2 value, Theme theme)
        {
            return $"({value.x}, {value.y})";
        }
    }
}