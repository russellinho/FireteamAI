using Koobando.UI.Console.Utilities;
using System;

namespace Koobando.UI.Console.Serializers
{
    public class TypeSerialiazer : PolymorphicSerializer<Type>
    {
        public override string SerializeFormatted(Type value, Theme theme)
        {
            return value.GetDisplayName();
        }
    }
}
