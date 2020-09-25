namespace Koobando.UI.Console.Parsers
{
    public class StringParser : BasicCachedParser<string>
    {
        public override int Priority => int.MaxValue;

        public override string Parse(string value)
        {
            return value.UnescapeText('"');
        }
    }
}
