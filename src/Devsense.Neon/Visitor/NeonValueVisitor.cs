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

        protected virtual void Visit(KeyValuePair<INeonValue, INeonValue>[] pairs)
        {
            if (pairs != null)
            {
                for (int i = 0; i < pairs.Length; i++)
                {
                    Visit(pairs[i].Key);
                    Visit(pairs[i].Value);
                }
            }
        }

        public virtual void Visit(INeonArray value)
        {
            Visit(value.Values);
        }

        public virtual void Visit(INeonEntity value)
        {
            Visit(value.Attributes);
        }
    }
}
