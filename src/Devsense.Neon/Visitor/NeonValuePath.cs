using System;
using System.Collections.Generic;
using System.Text;
using Devsense.Neon.Model;

namespace Devsense.Neon.Visitor
{
    public static class NeonValuePath
    {
        /// <summary>
        /// Gets value at given path.
        /// </summary>
        /// <param name="value">Root node.</param>
        /// <param name="path">Path.</param>
        /// <returns>Node at given path.</returns>
        public static INeonValue? Query(this INeonValue value, params string[] path)
        {
            for (int i = 0; value != null && i < path.Length; i++)
            {
                var identifier = path[i];
                var found = false;

                if (value is INeonArray block)
                {
                    for (int j = 0; j < block.Values.Length; j++)
                    {
                        if (block.Values[j].Key is INeonLiteral key &&
                            key.Value is string keystr &&
                            keystr.Equals(identifier, StringComparison.InvariantCultureIgnoreCase))
                        {
                            value = block.Values[j].Value;
                            found = true;
                            break;
                        }
                    }
                }

                //
                if (!found)
                {
                    return null;
                }
            }

            return value;
        }
    }
}
