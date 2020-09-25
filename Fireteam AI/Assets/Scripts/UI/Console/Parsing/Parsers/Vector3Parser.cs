using UnityEngine;

namespace Koobando.UI.Console.Parsers
{
    public class Vector3Parser : BasicCachedParser<Vector3>
    {
        public override Vector3 Parse(string value)
        {
            return ParseRecursive<Vector4>(value);
        }
    }
}
