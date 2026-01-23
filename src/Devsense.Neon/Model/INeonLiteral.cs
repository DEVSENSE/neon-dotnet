using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Devsense.Neon.Parser;
using Devsense.Neon.Visitor;

namespace Devsense.Neon.Model
{
    /// <summary>
    /// A literal value: string, number, boolean, date, datetime.
    /// </summary>
    public interface INeonLiteral : INeonValue
    {
        object? Value { get; }

        Type Type { get; }

        /// <summary>
        /// Gets value as string. Cannot be <c>null</c>.
        /// </summary>
        string ToString();
    }

    class LiteralFactory
    {
        public static INeonLiteral Create(long value) => new Literal<long>(value);

        public static INeonLiteral CreateString(ReadOnlySpan<char> value) => new Literal<string>(value.ToString());

        public static INeonLiteral Create(ReadOnlySpan<char> literal)
        {
            if (literal.IsEmpty)
            {
                return Null();
            }

            if (StringUtils.IsValidKeyword(literal))
            {
                // all lowercase or
                // a capital first or
                // all uppercase letters

                // null: [null] 
                if (literal.Equals("null".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    return Null();
                }

                // bool: [true, false, yes, no]
                if (literal.Equals("true".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                    literal.Equals("yes".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    return new Literal<bool>(true);
                }
                if (literal.Equals("false".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                    literal.Equals("no".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    return new Literal<bool>(false);
                }
            }

            // number:
            // 12         # an integer
            // 12.3       # a float
            // +1.2e-34   # an exponential number
            // 0b11010    # binary number
            // 0o666      # octal number
            // 0x7A       # hexa number
            if (long.TryParse(
#if NET5_0_OR_GREATER
                literal,
#else
                literal.ToString(),
#endif

                NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var longvalue))
            {
                return Create(longvalue);
            }
            // TODO: number styles

            // dates:
            // 2016-06-03                  # date
            // 2016-06-03 19:00:00         # date & time
            // 2016-06-03 19:00:00.1234    # date & microtime
            // 2016-06-03 19:00:00 +0200   # date & time & timezone
            // 2016-06-03 19:00:00 +02:00  # date & time & timezone

            //
            return new Literal<string>(literal.ToString());
        }

        public static INeonLiteral Null() => new Literal<object>(null);
    }

    [DebuggerDisplay("{Value,nq}")]
    class Literal<T> : INeonLiteral
    {
        Type INeonLiteral.Type => typeof(T);

        object? INeonLiteral.Value => Value;

        public T? Value { get; }

        public Literal(T? value)
        {
            this.Value = value;
        }

        public void Visit(NeonValueVisitor visitor) => visitor.Visit(this);

        public override string ToString()
        {
            if (typeof(T) == typeof(object))
            {
                if (ReferenceEquals(Value, null)) return "null";
            }

            if (typeof(T) == typeof(bool) && Value is bool b)
            {
                return b ? "true" : "false";
            }

            if (typeof(T) == typeof(string) && Value is string s)
            {
                return s;
            }

            //
            return Value?.ToString() ?? string.Empty;
        }
    }
}
