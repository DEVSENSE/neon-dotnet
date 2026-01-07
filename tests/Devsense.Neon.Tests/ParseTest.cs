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
        [InlineData("""
            parameters:
                phpVersion: 80200
                level: 3
                paths:
                    - src
                    - tests
                excludePaths:
                    - tests/Common/Proxy/InvalidReturnTypeClass.php
                    - tests/Common/Proxy/InvalidTypeHintClass.php
                    - tests/Common/Proxy/LazyLoadableObjectWithTypedProperties.php
                    - tests/Common/Proxy/MagicIssetClassWithInteger.php
                    - tests/Common/Proxy/NullableNonOptionalHintClass.php
                    - tests/Common/Proxy/PHP81NeverType.php
                    - tests/Common/Proxy/PHP81IntersectionTypes.php
                    - tests/Common/Proxy/Php8UnionTypes.php
                    - tests/Common/Proxy/Php8StaticType.php
                    - tests/Common/Proxy/ProxyGeneratorTest.php
                    - tests/Common/Proxy/ProxyLogicTypedPropertiesTest.php
                    - tests/Common/Proxy/SerializedClass.php
                    - tests/Common/Proxy/VariadicTypeHintClass.php
                    - tests/Common/Proxy/Php71NullableDefaultedNonOptionalHintClass.php
                    - tests/Common/Proxy/generated
                ignoreErrors:
                    - '#Access to an undefined property Doctrine\\Common\\Proxy\\Proxy::\$publicField#'
                    -
                        message: '#^Result of method Doctrine\\Tests\\Common\\Proxy\\LazyLoadableObjectWithVoid::(adding|incrementing)AndReturningVoid\(\) \(void\) is used\.$#'
                        path: 'tests/Common/Proxy/ProxyLogicVoidReturnTypeTest.php'
                    -
                        message: '#^Property Doctrine\\Tests\\Common\\Proxy\\ProxyLogicTest::\$initializerCallbackMock \(callable\(\): mixed&PHPUnit\\Framework\\MockObject\\MockObject\) does not accept PHPUnit\\Framework\\MockObject\\MockObject&stdClass\.$#'
                        path: 'tests/Common/Proxy/ProxyLogicTest.php'
                    -
                        message: '#.*LazyLoadableObject.*#'
                        paths:
                           - 'tests/Common/Proxy/ProxyLogicTest.php'
                           - 'tests/Common/Proxy/ProxyLogicVoidReturnTypeTest.php'
                    -
                        message: '#^Instantiated class Doctrine\\Tests\\Common\\ProxyProxy\\__CG__\\Doctrine\\Tests\\Common\\Proxy\\.* not found.$#'
                        path: 'tests/Common/Proxy/ProxyLogicTest.php'
                    -
                        message: '#^Instantiated class Doctrine\\Tests\\Common\\ProxyProxy\\__CG__\\Doctrine\\Tests\\Common\\Proxy\\.* not found.$#'
                        path: 'tests/Common/Proxy/ProxyLogicVoidReturnTypeTest.php'
                    -
                        message: '#^Property Doctrine\\Tests\\Common\\Proxy\\ProxyLogicVoidReturnTypeTest::\$initializerCallbackMock \(callable\(\): mixed&PHPUnit\\Framework\\MockObject\\MockObject\) does not accept PHPUnit\\Framework\\MockObject\\MockObject&stdClass\.$#'
                        path: 'tests/Common/Proxy/ProxyLogicVoidReturnTypeTest.php'
                    -
                        message: '#^Method Doctrine\\Tests\\Common\\Proxy\\MagicIssetClassWithInteger::__isset\(\) should return bool but returns int\.$#'
                        path: 'tests/Common/Proxy/MagicIssetClassWithInteger.php'
                    -
                        message: '#^Access to an undefined property Doctrine\\Tests\\Common\\Proxy\\MagicGetByRefClass\:\:\$nonExisting\.$#'
                        path: 'tests/Common/Proxy/ProxyMagicMethodsTest.php'

                    -
                        message: "#^Class Doctrine\\\\Tests\\\\Common\\\\Proxy\\\\MagicIssetClassWithInteger not found\\.$#"
                        count: 1
                        path: tests/Common/Proxy/ProxyMagicMethodsTest.php
            includes:
                - vendor/phpstan/phpstan-phpunit/extension.neon
                - vendor/phpstan/phpstan-phpunit/rules.neon

            """)]
        [InlineData("""
            -
              a: 1
              b: 2
            - c
            """)]
        [InlineData(@"parameters:
  reportUnmatchedIgnoredErrors: false
  scanDirectories:
    - ../../../../vendor/drush/drush/src-symfony-compatibility
  ignoreErrors:
    - identifier: missingType.iterableValue
    - identifier: missingType.generics
    - '#^Call to method Drupal\\Core\\Entity\\Query\\QueryInterface::accessCheck\(\) with false will always evaluate to true.#'
    - '#^Trait Drupal\\eca\\Hook\\ConfigSchemaHooksTrait is used zero times and is not analysed.#'
  excludePaths:
    - '*/tests/*.php'

")]
        public void Parse(string neonContent)
        {
            var value = NeonParser.Parse(neonContent.AsSpan());
        }
    }
}