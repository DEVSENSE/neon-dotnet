using System;
using Devsense.Neon.Parser;
using Xunit;

namespace Devsense.Neon.Tests
{
    public class ParseTest
    {
        [Theory]
        [InlineData(@"
person:
	name: Homer  	  # this is a comment
	hasHair: No

address:
	street: 742 Evergreen Terrace
	city: ""Springfield""
	country: USA
")]
        public void Parse(string neonContent)
        {
            var value = NeonParser.Parse(neonContent.AsSpan());
        }
    }
}