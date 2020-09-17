﻿using System;

namespace Koobando.UI.Console
{
    /// <summary>
    /// Exception to be thrown by an IQcParser.
    /// </summary>
    public class ParserException : Exception
    {
        public ParserException(string message) : base(message) { }
        public ParserException(string message, Exception innerException) : base(message, innerException) { }
    }
}
