using System;
using System.Collections.Generic;
using System.Text;

namespace Devsense.Neon
{
    public class NeonParseException : FormatException
    {
        /// <summary>
        /// Error line.
        /// </summary>
        public int Line { get; set; }

        public NeonParseException(string message, int line)
        {
            this.Line = line;
        }
    }
}
