using UnityEngine;

namespace Koobando.UI.Console.Parsers
{
    public class Vector2IntParser : BasicCachedParser<Vector2Int>
    {
        public override Vector2Int Parse(string value)
        {
            return (Vector2Int)ParseRecursive<Vector3Int>(value);
        }
    }
}
