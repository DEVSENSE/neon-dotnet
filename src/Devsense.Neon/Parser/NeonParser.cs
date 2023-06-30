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
                throw source.Unexpected();
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

                if (source.ConsumeNewLine())
                {
                    continue;
                }

                var nlconsumed = false;

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
                            if (newindent.StartsWith(baseindent) && newindent.Length > baseindent.Length)
                            {
                                value = ParseBlock(ref source, newindent);
                                nlconsumed = true;
                            }
                            else
                            {
                                items.Add(new(key, LiteralFactory.Null()));
                                continue;
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
                if (source.ConsumeNewLine() || nlconsumed)
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
                throw source.Unexpected();
            }

            if (source.Fetch().Is('('))
            {
                value = ParseEntityArguments(ref source, value);
            }

            //
            return value;
        }

        static INeonEntity ParseEntityArguments(ref Tokenizer source, INeonValue value)
        {
            var entity = source.Consume('(')
                ? new Entity(value, ParseBraces(ref source, ')'))
                : new Entity(value);
              
            // chained entity?
            if (source.Consume(NeonTokens.Literal, out var entityLit))
            {
                entity.Next = ParseEntityArguments(ref source, LiteralFactory.Create(entityLit.Value));
            }

            return entity;
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
                throw source.Unexpected();
            }

            return items.ToArray();
        }
    }
}
