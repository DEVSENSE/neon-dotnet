using System;
using System.Collections.Generic;
using System.Text;

namespace Devsense.Neon.Parser
{
    internal ref struct Tokenizer
    {
        Lexer.TokenEnumerator tokens;

        Token fetch;

        public ReadOnlySpan<char> indent;

        public int line => tokens.Line;

        public Tokenizer(Lexer lexer)
        {
            this.tokens = lexer.GetEnumerator();
        }

        Token CreateEOF() => new Token(ReadOnlySpan<char>.Empty, NeonTokens.EOF, tokens.Line);

        public Token Fetch()
        {
            if (fetch.Type == NeonTokens.EOF)
            {
                for (; ; )
                {
                    var prev = tokens.Current.Type;

                    if (!tokens.MoveNext())
                    {
                        fetch = CreateEOF();
                        break;
                    }

                    fetch = tokens.Current;

                    if (fetch.Type == NeonTokens.Newline)
                    {
                        indent = ReadOnlySpan<char>.Empty;
                    }
                    else if (fetch.Type == NeonTokens.Whitespace)
                    {
                        if (prev == NeonTokens.Newline)
                        {
                            indent = fetch.Value;
                        }
                        continue;
                    }
                    else if (fetch.Type == NeonTokens.Comment)
                    {
                        continue;
                    }

                    //
                    break;
                }
            }

            //
            return fetch;
        }

        public bool Consume(char ch, out Token token)
        {
            token = Fetch();

            if (token.IsChar(out var actual) && actual == ch)
            {
                fetch = default;
                return true;
            }

            token = default;
            return false;
        }

        public bool Consume(NeonTokens type, out Token token)
        {
            token = Fetch();

            if (token.Type == type)
            {
                fetch = default;
                return true;
            }

            token = default;
            return false;
        }
    }
}
