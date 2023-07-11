using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Devsense.Neon.Parser
{
    ref struct Lexer
    {
        /// <summary>
        /// The source content.
        /// </summary>
        readonly ReadOnlySpan<char> content;

        public Lexer(ReadOnlySpan<char> content)
        {
            this.content = content;
        }

        public TokenEnumerator GetEnumerator() => new TokenEnumerator(content);

        //public static IList<Token> Tokenize(ReadOnlySpan<char> content)
        //{
        //    var result = new List<Token>();

        //    foreach (var token in new Lexer(content))
        //    {
        //        result.Add(token);
        //    }

        //    //
        //    return result;
        //}

        static bool IsSpaceOrTab(char c) => c == ' ' || c == '\t';

        static bool IsNewLine(char c) => c == '\n' || c == '\r';

        static char Hash => '#';

        // \t \n \r \f \b \" \\ \/ \_
        static readonly Dictionary<char, char> s_escapedChars = new Dictionary<char, char>()
        {
            { 't', '\t' },
            { 'n', '\n' },
            { 'r', '\r' },
            { 'f', '\f' },
            { 'b', '\b' },
            { '"', '"' },
            { '\\', '\\' },
            { '/', '/' },
            { '_', '\u00A0' },
        };

        static bool TryConsumeWhitespace(ReadOnlySpan<char> source, out int charsCount)
        {
            int n = 0;
            while (n < source.Length && IsSpaceOrTab(source[n]))
            {
                n++;
            }

            charsCount = n;

            return n != 0;
        }

        static bool TryConsumeNewLine(ReadOnlySpan<char> source, out int charsCount)
        {
            charsCount =
                source.StartsWith("\n".AsSpan(), StringComparison.Ordinal) ? 1 :
                source.StartsWith("\r\n".AsSpan(), StringComparison.Ordinal) ? 2 :
                source.StartsWith("\r".AsSpan(), StringComparison.Ordinal) ? 1 :
                0;

            return charsCount != 0;
        }

        static bool TryConsumeComment(ReadOnlySpan<char> source, out int charsCount)
        {
            if (source.Length != 0 && source[0] == '#')
            {
                int n = 1;
                while (n < source.Length && !IsNewLine(source[n]))
                {
                    n++;
                }

                charsCount = n;
                return true;
            }

            //
            charsCount = 0;
            return false;
        }

        static char ConsumeUtfCodePoint(ReadOnlySpan<char> codepoint)
        {
            int num = 0;
            foreach (var c in codepoint)
            {
                var digit =
                    (c >= '0' && c <= '9') ? c - '0' :
                    (c >= 'A' && c <= 'F') ? c - 'A' + 10 :
                    (c >= 'a' && c <= 'f') ? c - 'a' + 10 :
                    0;

                num = num * 16 + digit;
            }

            return (char)num;
        }

        static bool ConsumeEscapedSequence(ReadOnlySpan<char> source, StringBuilder result, out int charsCount)
        {
            Debug.Assert(source.Length != 0);

            if (source.Length >= 2 && source[0] == '\\')
            {
                // \t \n \r \f \b \" \\ \/ \_
                // \u00A9
                if (s_escapedChars.TryGetValue(source[1], out var unescaped))
                {
                    charsCount = 2;
                    result.Append(unescaped);
                    return true;
                }
                else if (source[1] == 'u' && source.Length >= 6)
                {
                    charsCount = 2 + 4; // \uFFFF
                    result.Append(ConsumeUtfCodePoint(source.Slice(2, 4)));
                    return true;
                }
            }

            //
            charsCount = 0;
            return false;
        }

        static bool TryConsumeQuotedString(ReadOnlySpan<char> source, out int charsCount, out string? value)
        {
            Debug.Assert(source.Length != 0);

            var quote = source[0];
            Debug.Assert(quote == '"' || quote == '\'');

            int n = 1;
            var sb = new StringBuilder();

            while (n < source.Length)
            {
                var c = source[n];

                if (IsNewLine(c))
                {
                    break;
                }
                else if (c == '\\' && quote == '\"' && n + 1 < source.Length)
                {
                    // consume escaped sequence:
                    if (ConsumeEscapedSequence(source.Slice(n), sb, out charsCount))
                    {
                        n += charsCount;
                        continue;
                    }
                }
                else if (c == quote)
                {
                    // done
                    charsCount = n + 1;
                    value = sb.ToString();
                    return true;
                }

                // continue
                n++;
                sb.Append(c);
            }

            //
            charsCount = -1;
            value = null;
            return false;
        }

        static bool TryConsumeTrippleQuotedString(ReadOnlySpan<char> source, out int charsCount, out string? value)
        {
            charsCount = -1;
            value = null;

            //
            Debug.Assert(source.Length >= 3);

            var quote = source[0];
            Debug.Assert(quote == '"' || quote == '\'');
            Debug.Assert(quote == source[1]);
            Debug.Assert(quote == source[2]);

            var closing = quote == '\"' ? "\"\"\"" : "'''";
            int n = closing.Length;


            if (TryConsumeNewLine(source.Slice(n), out var newline))
            {
                n += newline;
            }
            else
            {
                // unexpected
                return false;
            }

            var sb = new StringBuilder();
            var linestarts = new List<int>() { 0 }; // remember beginnings of line in {sb}

            while (n < source.Length)
            {
                var c = source[n];

                if (TryConsumeNewLine(source.Slice(n), out newline))
                {
                    // \n\s*'''
                    int end = n + newline;
                    TryConsumeWhitespace(source.Slice(end), out var ws); end += ws; // \s*
                    if (source.Slice(end).StartsWith(closing.AsSpan(), StringComparison.Ordinal)) // '''
                    {
                        end += closing.Length;
                        charsCount = end;

                        // trim indentation within {sb}
                        var indentation = ReadOnlySpan<char>.Empty;
                        for (int line = 0; line < linestarts.Count; line++)
                        {
                            var lineindent = sb.SubStringOfAny(linestarts[line], " \t".AsSpan()).AsSpan();
                            indentation = line > 0 ? StringUtils.CommonPrefix(indentation, lineindent) : lineindent;
                            if (indentation.IsEmpty) break;
                        }
                        for (int line = linestarts.Count - 1; line >= 0; line--)
                        {
                            sb.Remove(linestarts[line], indentation.Length);
                        }

                        //
                        value = sb.ToString();
                        return true;
                    }

                    // \n
                    sb.Append('\n');
                    n += newline;
                    linestarts.Add(sb.Length);
                    continue;
                }
                else if (c == '\\' && quote == '\"' && n + 1 < source.Length)
                {
                    // consume escaped sequence:
                    if (ConsumeEscapedSequence(source.Slice(n), sb, out charsCount))
                    {
                        n += charsCount;
                        continue;
                    }
                }

                // continue
                n++;
                sb.Append(c);
            }

            //
            return false;
        }

        public ref struct TokenEnumerator
        {
            /// <summary>
            /// The remaining content.
            /// </summary>
            ReadOnlySpan<char> source;

            public int Line { get; private set; }

            public TokenEnumerator(ReadOnlySpan<char> source)
            {
                this.source = source;
                this.Current = default(Token);
            }

            public Token Current { get; private set; }

            bool Consume(int nchars, NeonTokens type, string? value = null)
            {
                Debug.Assert(nchars >= 0);

                Current = new Token(value != null ? value.AsSpan() : source.Slice(0, nchars), type, Line);
                source = source.Slice(nchars);
                return nchars > 0;
            }

            public bool MoveNext()
            {
                // EOF
                if (source.IsEmpty)
                {
                    return Consume(0, NeonTokens.EOF); // false
                }

                // Whitespace
                if (TryConsumeWhitespace(source, out var n))
                {
                    return Consume(n, NeonTokens.Whitespace);
                }

                // NewLine
                if (TryConsumeNewLine(source, out n))
                {
                    Line++;
                    return Consume(n, NeonTokens.Newline);
                }

                // Comment
                if (TryConsumeComment(source, out n))
                {
                    return Consume(n, NeonTokens.Comment);
                }

                // Punctuation
                if (",=[]{}()".IndexOf(source[0]) >= 0)
                {
                    return Consume(1, NeonTokens.Char);
                }

                // :<ws>
                // -<ws>
                if ((source[0] == '-' || source[0] == ':') && source.Length >= 2 && char.IsWhiteSpace(source[1]))
                {
                    return Consume(1, NeonTokens.Char);
                }

                var c = source[0];
                if (c == '\'' || c == '\"')
                {
                    // string

                    if (source.Length >= 4 && source[1] == c && source[2] == c && IsNewLine(source[3])) // (""" | ''') \n
                    {
                        // tripple quotes
                        if (TryConsumeTrippleQuotedString(source, out n, out var value))
                        {
                            return Consume(n, NeonTokens.String, value: value);
                        }
                    }
                    else if (TryConsumeQuotedString(source, out n, out var value))
                    {
                        return Consume(n, NeonTokens.String, value: value);
                    }
                }
                else // anything else
                {
                    // literal
                    n = 1;
                    while (n < source.Length)
                    {
                        c = source[n];

                        if (c == Hash || IsNewLine(c) || c == '(' || c == ',' || c == '=' || c == ')' || c == '}' || c == ']')
                        {
                            break;
                        }

                        if (c == ':' && n + 1 < source.Length && char.IsWhiteSpace(source[n + 1]))  // :<space|newline>
                        {
                            break;
                        }

                        //
                        n++;
                    }

                    return Consume(n, NeonTokens.Literal);
                }

                //
                throw new NeonParseException("Unexpected", Line);
            }
        }
    }
}
