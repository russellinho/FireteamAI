using UnityEngine;

namespace Koobando.UI.Console.Parsers
{
    public class Vector2Parser : BasicCachedParser<Vector2>
    {
        public override Vector2 Parse(string value)
        {
            return ParseRecursive<Vector4>(value);
        }
    }
}
