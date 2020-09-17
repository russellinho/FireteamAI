using UnityEngine;

namespace Koobando.UI.Console.Serializers
{
    public class Vector3IntSerializer : BasicSerializer<Vector3Int>
    {
        public override string SerializeFormatted(Vector3Int value, Theme theme)
        {
            return $"({value.x}, {value.y}, {value.z})";
        }
    }
}