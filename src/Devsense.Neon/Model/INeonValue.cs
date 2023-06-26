using System;
using System.Collections.Generic;
using System.Text;
using Devsense.Neon.Visitor;

namespace Devsense.Neon.Model
{
    /// <summary>
    /// Base value object.
    /// </summary>
    public interface INeonValue
    {
        /// <summary>
        /// Callback to corresponding <see cref="NeonValueVisitor"/>'s method.
        /// </summary>
        void Visit(NeonValueVisitor visitor);
    }
}
