using UnityEngine;

namespace Koobando.UI.Console.Parsers
{
    public class QuaternionParser : BasicCachedParser<Quaternion>
    {
        public override Quaternion Parse(string value)
        {
            Vector4 vector = ParseRecursive<Vector4>(value);
            return new Quaternion(vector.x, vector.y, vector.z, vector.w);
        }
    }
}
