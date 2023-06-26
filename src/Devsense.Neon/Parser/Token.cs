﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Devsense.Neon.Parser
{
    public ref struct Token
    {
        public ReadOnlySpan<char> Value { get; }

        public NeonTokens Type { get; }

        public int Line { get; }

        public bool IsChar(out char c)
        {
            if (Type == NeonTokens.Char && Value.Length == 1)
            {
                c = Value[0];
                return true;
            }

            c = '\0';
            return false;
        }

        public Token(ReadOnlySpan<char> value, NeonTokens type, int line)
        {
            Value = value;
            Type = type;
            Line = line;
        }
    }
}
