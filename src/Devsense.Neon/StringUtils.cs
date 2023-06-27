using System;
using System.Collections.Generic;
using System.Text;

namespace Devsense.Neon
{
    static class StringUtils
    {
        public static bool IsValidKeyword(ReadOnlySpan<char> value) => AllLetters(value) && (AllLC(value) || AllUC(value) || FirstUC(value));

        public static bool AllLetters(ReadOnlySpan<char> value)
        {
            foreach (char c in value)
            {
                if (c >= 'a' && c <= 'z') continue;
                if (c >= 'A' && c <= 'Z') continue;
                return false;
            }

            return true;
        }

        public static bool AllLC(ReadOnlySpan<char> value)
        {
            foreach (var c in value)
            {
                if (!char.IsLower(c)) return false;
            }

            return true;
        }

        public static bool FirstUC(ReadOnlySpan<char> value)
        {
            return !value.IsEmpty && char.IsUpper(value[0]) && AllLC(value.Slice(1));
        }

        public static bool AllUC(ReadOnlySpan<char> value)
        {
            foreach (var c in value)
            {
                if (!char.IsUpper(c)) return false;
            }

            return true;
        }

        public static char ClosingBrace(char opening) => opening switch
        {
            '(' => ')',
            '[' => ']',
            '<' => '>',
            '{' => '}',
            _ => throw new ArgumentException()
        };
    }
}
