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
		[InlineData(@"
parameters:
    checkMissingIterableValueType: false

    ignoreErrors:
        - '#Call to an undefined method DateTimeInterface::add\(\)#'
        - '#Call to an undefined method DateTimeInterface::modify\(\)#'
        - '#Call to an undefined method DateTimeInterface::setDate\(\)#'
        - '#Call to an undefined method DateTimeInterface::setTime\(\)#'
        - '#Call to an undefined method DateTimeInterface::setTimezone\(\)#'
        - '#Call to an undefined method DateTimeInterface::sub\(\)#'

    level: max

    paths:
        - src/
")]
        [InlineData(@"
parameters:
    level: 9
    checkGenericClassInNonGenericObjectType: false
    paths:
        - gs-product-configurator.php
        - class-gs-product-configurator.php
        - includes/
    scanDirectories:
      - ../gravityforms
      - ../woocommerce
    excludePaths:
      analyse:
        - includes/third-party
    ignoreErrors:
      # gf_apply_filters only specifies two params.
      - '#Function gf_apply_filters invoked with \d+ parameters, 2 required.#'
    typeAliases:
      GFFeed: '''
        array{
          id: int,
          form_id: int,
          meta: array<string, mixed>,
          is_active: 0|1,
          addon_slug: string,
        }
      '''
      GSPCFeed: '''
        GFFeed & array{
          meta: array<string, mixed> & array{
            display_price: string,
            item_meta_display: string,
            item_meta_display_template: string,
          },
        }
      '''
")]
        public void Parse(string neonContent)
        {
            var value = NeonParser.Parse(neonContent.AsSpan());
        }
    }
}