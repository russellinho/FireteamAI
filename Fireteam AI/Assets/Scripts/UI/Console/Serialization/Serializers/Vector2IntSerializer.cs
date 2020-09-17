using UnityEngine;

namespace Koobando.UI.Console.Serializers
{
    public class Vector2IntSerializer : BasicSerializer<Vector2Int>
    {
        public override string SerializeFormatted(Vector2Int value, Theme theme)
        {
            return $"({value.x}, {value.y})";
        }
    }
}