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
    }

    class Entity : INeonEntity
    {
        public INeonValue Value { get; }

        public KeyValuePair<INeonValue, INeonValue>[] Attributes { get; }

        public Entity(INeonValue value, KeyValuePair<INeonValue, INeonValue>[] attributes)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        }

        public void Visit(NeonValueVisitor visitor) => visitor.Visit(this);
    }
}
