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

            while (source.ConsumeNewLine()) ;

            var value = ParseBlock(ref source, source.indent);

            if (source.Consume(NeonTokens.EOF, out _))
            {
                return value;
            }
            else
            {
                throw new NeonParseException("Unexpected", source.line);
            }
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
                else
                {
                    break;
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
                value = LiteralFactory.Create(literal.Value.Trim());
            }
            else if (source.Fetch().Type == NeonTokens.Newline)
            {
                // no value is null
                return LiteralFactory.Null();
            }
            else if (source.Consume('[', out var c) || source.Consume('{', out c) || source.Consume('(', out c))
            {
                return new Block(ParseBraces(ref source, StringUtils.ClosingBrace(c.Value[0])));
            }
            else
            {
                throw new NeonParseException("Unexpected", source.line);
            }

            // ( entity arguments )
            if (source.Consume('('))
            {
                value = new Entity(value, ParseBraces(ref source, ')'));

                // TODO: chained entity
            }

            //
            return value;
        }

        static KeyValuePair<INeonValue, INeonValue>[] ParseBraces(ref Tokenizer source, char closing)
        {
            var items = new List<KeyValuePair<INeonValue, INeonValue>>();

            for (; ; )
            {
                while (source.ConsumeNewLine()) ;

                // )
                if (source.Consume(closing))
                {
                    break;
                }

                // key: value?
                var value = ParseValue(ref source);

                if (source.Consume(':') || source.Consume('='))
                {
                    if (source.Consume(',') || source.Fetch().Is(closing))
                    {
                        // key: <null>,
                        items.Add(new(value, LiteralFactory.Null()));
                        continue;
                    }

                    // key: value
                    items.Add(new(value, ParseValue(ref source)));
                }

                // ,
                if (source.Consume(','))
                {
                    continue;
                }

                // )
                if (source.Consume(closing))
                {
                    break;
                }

                //
                throw new NeonParseException("unexpected", source.line);
            }

            return items.ToArray();
        }
    }
}
