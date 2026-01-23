using Devsense.Neon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devsense.Neon
{
    public static class NeonValueExtensions
    {
        /// <summary>
        /// Gets value indicating the given array is indexed with integer numbers from <c>0</c>.
        /// </summary>
        public static bool IsOrdinalArray(this INeonArray array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            var items = array.Values;

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Key is INeonLiteral keyliteral && keyliteral.Value is long l && l == i)
                {
                    continue;
                }

                return false;
            }

            //
            return true;
        }
    }
}
