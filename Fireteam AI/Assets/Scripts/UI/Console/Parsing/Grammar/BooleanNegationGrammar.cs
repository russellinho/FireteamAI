﻿using System;
using System.Text.RegularExpressions;

namespace Koobando.UI.Console.Grammar
{
    public class BooleanNegationGrammar : IGrammarConstruct
    {
        private readonly Regex _negationRegex = new Regex(@"^!\S+$");

        public int Precedence => 0;

        public bool Match(string value, Type type)
        {
            return type == typeof(bool) && _negationRegex.IsMatch(value);
        }

        public object Parse(string value, Type type, Func<string, Type, object> recursiveParser)
        {
            value = value.Substring(1);
            return !(bool)recursiveParser(value, type);
        }
    }
}
