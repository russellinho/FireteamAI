using System;

namespace Koobando.UI.Console.Parsers
{
    public class TypeParser : BasicCachedParser<Type>
    {
        public override Type Parse(string value)
        {
            return KoobandoParser.ParseType(value);
        }
    }
}
