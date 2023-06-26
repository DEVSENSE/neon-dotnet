using System;
using System.Collections.Generic;
using System.Text;
using Devsense.Neon.Model;

namespace Devsense.Neon.Visitor
{
    public class NeonValueVisitor
    {
        public virtual void Visit(INeonValue value)
        {
            value?.Visit(this);
        }

        public virtual void Visit(INeonLiteral value)
        {

        }

        public virtual void Visit(INeonArray value)
        {
            var items = value.Values;
            if (items != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    Visit(items[i].Key);
                    Visit(items[i].Value);
                }
            }
        }
    }
}
