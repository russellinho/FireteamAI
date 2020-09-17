using Koobando.UI.Console.Utilities;
using System;

namespace Koobando.UI.Console.Parsers
{
    public class EnumParser : PolymorphicCachedParser<Enum>
    {
        public override Enum Parse(string value, Type type)
        {
            try
            {
                return (Enum)Enum.Parse(type, value);
            }
            catch (Exception e)
            {
                throw new ParserInputException($"Cannot parse '{value}' to the type '{type.GetDisplayName()}'. To see the supported values, use the command `enum-info {type}`", e);
            }
        }
    }
}