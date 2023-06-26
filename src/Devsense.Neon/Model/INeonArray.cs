using System;
using System.Collections.Generic;
using System.Text;
using Devsense.Neon.Visitor;

namespace Devsense.Neon.Model
{
    /// <summary>
    /// Collection of key-value pairs.
    /// </summary>
    public interface INeonArray : INeonValue
    {
        KeyValuePair<INeonValue, INeonValue>[] Values { get; }
    }

    class Block : INeonArray
    {
        public KeyValuePair<INeonValue, INeonValue>[] Values { get; }

        public void Visit(NeonValueVisitor visitor) => visitor.Visit(this);

        public Block(KeyValuePair<INeonValue, INeonValue>[] values)
        {
            this.Values = values;
        }
    }
}
