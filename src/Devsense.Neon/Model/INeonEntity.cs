using System;
using System.Collections.Generic;
using System.Text;

namespace Devsense.Neon.Model
{
    /// <summary>
    /// Entity on form of <c>{Value}( {attributes} )</c>
    /// </summary>
    public interface INeonEntity : INeonArray
    {
        string Value { get; }
    }
}
