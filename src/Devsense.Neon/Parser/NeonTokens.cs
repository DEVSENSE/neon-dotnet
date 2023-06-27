using System;
using System.Collections.Generic;
using System.Text;

namespace Devsense.Neon.Parser
{
    //static class Tokens
    //{
    //    public const char LeftBrace = '{';
    //    public const char RightBrace = '}';

    //    public const char LeftBracket = '[';
    //    public const char RightBracket = ']';

    //    public const char LeftParen = '(';
    //    public const char RightParen = ')';

    //    public const char Dash = '-';
    //    public const char Colon = ':';
    //    public const char Comma = ',';
    //    public const char Hash = '#';

    //    public static bool IsSpaceOrTab(char c) => c == ' ' || c == '\t';

    //    public static bool IsNewLine(char c) => c == '\n' || c == '\r';
    //}

    enum NeonTokens
    {
        EOF = 0,

        @Char,
        @String,
        Literal,
        Comment,
        Newline,
        Whitespace,
    }
}
