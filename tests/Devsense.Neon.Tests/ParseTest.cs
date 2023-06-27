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
	hasHair: No       # (bool)false

address:
	street: 742 Evergreen Terrace
	city: ""Springfield""
	country: USA
	data:             # nothing is null

phones: { home: 555-6528, work: 555-7334 }

children:
	- Bart
	- Lisa
	- Maggie

entity: Column(type=int, nulls=yes)

string: 'quotes string'

multi line string: '''
	one line
	second line
	third line
'''
")]
        public void Parse(string neonContent)
        {
            var value = NeonParser.Parse(neonContent.AsSpan());
        }
    }
}