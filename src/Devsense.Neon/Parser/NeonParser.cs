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

        static INeonValue ParseBlock(ref Tokenizer source, ReadOnlySpan<char> baseindent)
        {
            // TODO
            throw new NotImplementedException();
        }

        static INeonValue ParseValue(ref Tokenizer source)
        {
            if (source.Consume(NeonTokens.String, out var str))
            {
                // TODO
            }
            else if (source.Consume(NeonTokens.Literal, out var literal))
            {
                // TODO
            }
            else if (source.Consume('[', out var c) || source.Consume('{', out c) || source.Consume('(', out c))
            {
                // TODO   
            }
            else
            {
                throw new NeonParseException("Unexpected", source.line);
            }

            // ( entity arguments )

            // TODO return
            throw new NotImplementedException();
        }
    }
}
