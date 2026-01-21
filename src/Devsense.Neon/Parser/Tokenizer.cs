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

        public Exception Unexpected() => new NeonParseException(
            $"Unexpected token {Fetch().Type} \"{Fetch().Value.ToString()}\" at line {this.line}.",
            this.line
        );

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

        public bool ConsumeNewLine() => Consume(NeonTokens.Newline, out _);

        public bool Consume(char ch) => Consume(ch, out _);

        public bool Consume(char ch, out Token token)
        {
            token = Fetch();

            if (token.Is(ch))
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
                Fetch(); // fetch new indent
                return true;
            }

            token = default;
            return false;
        }
    }
}
