using System;
using System.Collections.Generic;
using System.Text;
using Devsense.Neon.Visitor;

namespace Devsense.Neon.Model
{
    /// <summary>
    /// Entity on form of <c>{Value}( {attributes} )</c>
    /// </summary>
    public interface INeonEntity : INeonValue
    {
        INeonValue Value { get; }

        KeyValuePair<INeonValue, INeonValue>[] Attributes { get; }

        INeonEntity? Next { get; }
    }

    class Entity : INeonEntity
    {
        public INeonValue Value { get; }

        public KeyValuePair<INeonValue, INeonValue>[] Attributes { get; }

        public INeonEntity? Next { get; set; }

        public Entity(INeonValue value, KeyValuePair<INeonValue, INeonValue>[]? attributes = null)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Attributes = attributes ?? new KeyValuePair<INeonValue, INeonValue>[0];
            Next = null;
        }

        public void Visit(NeonValueVisitor visitor) => visitor.Visit(this);
    }
}
