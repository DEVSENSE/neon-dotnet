using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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

        static bool TryConsumeQuotedString(ReadOnlySpan<char> source, out int charsCount)
        {
            Debug.Assert(source.Length != 0);

            var quote = source[0];
            Debug.Assert(quote == '"' || quote == '\'');

            int n = 1;
            bool escaped = false;

            while (n < source.Length)
            {
                var c = source[n];

                if (IsNewLine(c))
                {
                    break;
                }
                else if (escaped)
                {
                    escaped = false;
                }
                else if (c == '\\' && quote == '\"')
                {
                    escaped = true;
                }
                else if (c == quote)
                {
                    // done
                    charsCount = n + 1;
                    return true;
                }

                // continue
                n++;
            }

            //
            charsCount = -1;
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

            bool Consume(int nchars, NeonTokens type)
            {
                Debug.Assert(nchars >= 0);

                Current = new Token(source.Slice(0, nchars), type, Line);
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

                    if (source.Length >= 3 && source[1] == c && source[2] == c) // """, '''
                    {
                        // tripple quotes
                        // TODO
                        throw new NotImplementedException();
                    }
                    else if (TryConsumeQuotedString(source, out n))
                    {
                        return Consume(n, NeonTokens.String);
                    }
                }
                else // anything else
                {
                    // literal
                    n = 1;
                    while (n < source.Length)
                    {
                        c = source[n];

                        if (c == Hash || IsNewLine(c) || c == '(' || c == ',' || c == '=')
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
