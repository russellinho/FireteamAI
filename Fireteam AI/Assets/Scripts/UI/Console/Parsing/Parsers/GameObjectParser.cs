using Koobando.UI.Console.Utilities;
using UnityEngine;

namespace Koobando.UI.Console.Parsers
{
    public class GameObjectParser : BasicParser<GameObject>
    {
        public override GameObject Parse(string value)
        {
            GameObject obj = GameObjectExtensions.Find(value, true);

            if (!obj)
            {
                throw new ParserInputException($"Could not find GameObject of name {value}.");
            }

            return obj;
        }
    }
}
