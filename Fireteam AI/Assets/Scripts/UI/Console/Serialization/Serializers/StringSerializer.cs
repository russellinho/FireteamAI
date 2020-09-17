namespace Koobando.UI.Console.Serializers
{
    public class StringSerializer : BasicSerializer<string>
    {
        public override int Priority => int.MaxValue;

        public override string SerializeFormatted(string value, Theme theme)
        {
            return value;
        }
    }
}
