using Devsense.Neon.Model;
using Devsense.Neon.Visitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Devsense.Neon.Tests
{
    internal class NeonToJson : NeonValueVisitor
    {
        readonly Utf8JsonWriter writer;

        public NeonToJson(Utf8JsonWriter writer)
        {
            this.writer = writer;
        }

        public static string Serialize(INeonValue value, JsonWriterOptions options = default(JsonWriterOptions))
        {
            var stream = new MemoryStream();

            using (var writer = new Utf8JsonWriter(stream, options))
            {
                new NeonToJson(writer).Visit(value);
            }

            return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
        }

        #region NeonValueVisitor

        public override void Visit(INeonLiteral value)
        {
            if (value.Value == null)
            {
                this.writer.WriteNullValue();
            }
            else if (value.Value is int i)
            {
                this.writer.WriteNumberValue(i);
            }
            else if (value.Value is long l)
            {
                this.writer.WriteNumberValue(l);
            }
            else if (value.Value is string s)
            {
                this.writer.WriteStringValue(s);
            }
            else if (value.Value is bool b)
            {
                this.writer.WriteBooleanValue(b);
            }
            else
            {
                throw new NotSupportedException($"Literal value of type {value.Value.GetType().ToString()}");
            }
        }

        public override void Visit(INeonArray value)
        {
            var items = value.Values;

            if (value.IsOrdinalArray())
            {
                // [...]

                this.writer.WriteStartArray();

                for (int i = 0; i < items.Length; i++)
                {
                    Visit(items[i].Value);
                }

                this.writer.WriteEndArray();
            }
            else
            {
                // {...}

                this.writer.WriteStartObject();

                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].Key is INeonLiteral literal)
                    {
                        // key:
                        this.writer.WritePropertyName(literal.ToString());
                        
                        // value
                        Visit(items[i].Value);
                    }
                    else
                    {
                        throw new NotSupportedException($"List key is not a literal.");
                    }
                }

                this.writer.WriteEndObject();
            }
        }

        public override void Visit(INeonEntity value)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
