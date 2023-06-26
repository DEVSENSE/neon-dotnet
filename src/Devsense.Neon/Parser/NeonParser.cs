using System;
using System.Collections.Generic;
using System.Text;
using Devsense.Neon.Model;

namespace Devsense.Neon.Parser
{
    public static class NeonParser
    {
        public static INeonValue Parse(ReadOnlySpan<char> content)
        {
            var source = new Tokenizer(new Lexer(content));

            while (source.Consume(NeonTokens.Newline, out _)) ;

            return ParseBlock(ref source, source.indent);
        }

        // parse content on new line after ':'
        static INeonValue ParseBlock(ref Tokenizer source, ReadOnlySpan<char> baseindent)
        {
            var index = 0L;
            var items = new List<KeyValuePair<INeonValue, INeonValue>>();

            for (; ; )
            {
                INeonValue key, value;

                if (source.Consume(NeonTokens.EOF, out _))
                {
                    break;
                }

                if (source.indent.Equals(baseindent, StringComparison.Ordinal) == false)
                {
                    break;
                }

                if (source.Consume('-'))
                {
                    // list
                    key = LiteralFactory.Create(index++);
                    value = ParseValue(ref source);
                }
                else
                {
                    key = ParseValue(ref source);

                    if (source.Consume(':'))
                    {
                        if (source.ConsumeNewLine())
                        {
                            var newindent = source.indent;
                            if (newindent.StartsWith(baseindent))
                            {
                                value = ParseBlock(ref source, newindent);
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            value = ParseValue(ref source);
                        }
                    }
                    else if (source.Consume('='))
                    {
                        value = ParseValue(ref source);
                    }
                    else if (items.Count == 0)
                    {
                        // value only
                        return key;
                    }
                    else
                    {
                        value = LiteralFactory.Null();
                    }
                }

                //
                items.Add(new(key, value));

                // \n
                if (source.ConsumeNewLine())
                {
                    continue;
                }
            }

            //
            return new Block(items.ToArray());
        }

        static INeonValue ParseValue(ref Tokenizer source)
        {
            INeonValue value;

            if (source.Consume(NeonTokens.String, out var str))
            {
                value = LiteralFactory.CreateString(str.Value);
            }
            else if (source.Consume(NeonTokens.Literal, out var literal))
            {
                value = LiteralFactory.Create(literal.Value);
            }
            else if (source.Consume('[', out var c) || source.Consume('{', out c) || source.Consume('(', out c))
            {
                // TODO
                throw new NotImplementedException();
            }
            else
            {
                throw new NeonParseException("Unexpected", source.line);
            }

            // ( entity arguments )
            if (source.Consume('('))
            {
                // TODO
            }

            //
            return value;
        }
    }
}
