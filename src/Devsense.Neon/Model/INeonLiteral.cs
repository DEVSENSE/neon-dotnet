using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Devsense.Neon.Parser;
using Devsense.Neon.Visitor;

namespace Devsense.Neon.Model
{
    /// <summary>
    /// A lIteral value: string, number, boolean, date, datetime.
    /// </summary>
    public interface INeonLiteral : INeonValue
    {
        object? Value { get; }
    }

    class LiteralFactory
    {
        public static INeonLiteral Create(long value) => new Literal<long>(value);

        public static INeonLiteral CreateString(ReadOnlySpan<char> value) => new Literal<string>(value.ToString());

        public static INeonLiteral Create(ReadOnlySpan<char> literal) => new Literal<string>(literal.ToString()); // TODO

        public static INeonLiteral Null() => new Literal<object>(null);
    }

    [DebuggerDisplay("{Value,nq}")]
    class Literal<T> : INeonLiteral
    {
        object? INeonLiteral.Value => Value;

        public T? Value { get; }

        public Literal(T? value)
        {
            this.Value = value;
        }

        public void Visit(NeonValueVisitor visitor) => visitor.Visit(this);
    }
}
