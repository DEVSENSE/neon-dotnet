using System;
using Devsense.Neon.Model;
using Devsense.Neon.Parser;
using Devsense.Neon.Visitor;
using Xunit;

namespace Devsense.Neon.Tests
{
    public class PathTest
    {
        [Theory]
        [InlineData(@"
person:
	name: Homer  	  # this is a comment
	hasHair: No       # (bool)false
", "person.name", "Homer")]
        public void Parse(string neonContent, string path, string expectedValue)
        {
            var root = NeonParser.Parse(neonContent.AsSpan());
            var value = root.Query(path.Split('.'));

            Assert.NotNull(value);
            Assert.IsAssignableFrom<INeonLiteral>(value);
            Assert.Equal(expectedValue, ((INeonLiteral)value).Value?.ToString());
        }
    }
}