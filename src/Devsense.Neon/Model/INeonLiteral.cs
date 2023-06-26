using System;
using System.Collections.Generic;
using System.Text;

namespace Devsense.Neon.Model
{
    /// <summary>
    /// A lIteral value: string, number, boolean, date, datetime.
    /// </summary>
    public interface INeonLiteral : INeonValue
    {
        object Value { get; }
    }
}
