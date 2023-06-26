using System;
using System.Collections.Generic;
using System.Text;

namespace Devsense.Neon.Model
{
    /// <summary>
    /// Collection of key-value pairs.
    /// </summary>
    public interface INeonArray : INeonValue
    {
        KeyValuePair<INeonValue, INeonValue>[] Values { get; }
    }
}
