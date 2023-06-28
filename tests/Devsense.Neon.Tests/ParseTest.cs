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


special types:
	escape sequences: ""\u2026 \t \n \r \f \b""
	bool: [true, false]
	bool alternative: [yes, no]
	exponent number: +0123.45e6
	hex number: 0xAB01
	octal number: 0o666
	binary number: 0b11100111
	date: 2020-02-02
	date time: 2020-02-02 12:34:56
")]
		[InlineData(@"
includes:
    - phpstan.neon.dist

parameters:
    typeAliases:
        Name: 'string'
        NameResolver: 'callable(): string'
        NameOrResolver: 'Name|NameResolver'

    ignoreErrors:
        - '#Function pcntl_open not found\.#'
    parallel:
        maximumNumberOfProcesses: 1
")]
        public void Parse(string neonContent)
        {
            var value = NeonParser.Parse(neonContent.AsSpan());
        }
    }
}