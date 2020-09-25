﻿using System.Collections;

namespace Koobando.UI.Console.Serializers
{
    public class IDictionarySerializer : IEnumerableSerializer<IDictionary>
    {
        protected override IEnumerable GetObjectStream(IDictionary value)
        {
            foreach (DictionaryEntry item in value)
            {
                yield return item;
            }
        }
    }
}
