using System.Collections;

namespace Koobando.UI.Console.Serializers
{
    public class DictionaryEntrySerializer : BasicSerializer<DictionaryEntry>
    {
        public override string SerializeFormatted(DictionaryEntry value, Theme theme)
        {
            string innerKey = SerializeRecursive(value.Key, theme);
            string innerValue = SerializeRecursive(value.Value, theme);

            return $"{innerKey}: {innerValue}";
        }
    }
}
