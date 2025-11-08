// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Templates.Helpers
{
    using HandlebarsDotNet;
    using Kampose.Templates.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class StringHelpersTests
    {
        private IHandlebars handlebars = null!;

        [SetUp]
        public void SetUp()
        {
            handlebars = Handlebars.Create();
            StringHelpers.Register(handlebars);
        }

        [TestCase("Hello", ExpectedResult = "Hello")]
        [TestCase(12, ExpectedResult = "12")]
        [TestCase(12.75, "F0", ExpectedResult = "13")]
        public string Format_ReturnsStringRepresentation(object? value, string? format = null)
        {
            var template = handlebars.Compile("{{#format value format}}");
            return template(new { value, format });
        }

        [TestCase("Hello", ExpectedResult = "HELLO")]
        [TestCase("world", ExpectedResult = "WORLD")]
        public string UpperCase_ReturnsUpperCaseString(string value)
        {
            var template = handlebars.Compile("{{#upperCase value}}");
            return template(new { value });
        }

        [TestCase("Hello", ExpectedResult = "hello")]
        [TestCase("WORLD", ExpectedResult = "world")]
        public string LowerCase_ReturnsLowerCaseString(string value)
        {
            var template = handlebars.Compile("{{#lowerCase value}}");
            return template(new { value });
        }

        [TestCase("hello world", ExpectedResult = "Hello World")]
        [TestCase("hElLo WoRlD", ExpectedResult = "Hello World")]
        [TestCase("HELLO WORLD", ExpectedResult = "HELLO WORLD")]
        public string TitleCase_ReturnsTitleCaseString(string value)
        {
            var template = handlebars.Compile("{{#titleCase value}}");
            return template(new { value });
        }

        [TestCase("Simple Title", ExpectedResult = "simple-title")]
        [TestCase("simple-title_with_mixed-separators", ExpectedResult = "simple-title-with-mixed-separators")]
        [TestCase("already-kebab-cased", ExpectedResult = "already-kebab-cased")]
        [TestCase("multiple--separators___together", ExpectedResult = "multiple-separators-together")]
        [TestCase("PascalCaseName", ExpectedResult = "pascal-case-name")]
        [TestCase("camelCaseName", ExpectedResult = "camel-case-name")]
        [TestCase("MixedCase_with-separators", ExpectedResult = "mixed-case-with-separators")]
        [TestCase("XMLDocument", ExpectedResult = "xml-document")]
        [TestCase("EnhancedIOStream", ExpectedResult = "enhanced-io-stream")]
        [TestCase("DataProcessorAPI", ExpectedResult = "data-processor-api")]
        [TestCase("file@domain.com", ExpectedResult = "file-domain-com")]
        [TestCase("price$100&tax", ExpectedResult = "price-100-tax")]
        [TestCase("version2.0.1", ExpectedResult = "version2-0-1")]
        public string KebabCase_ReturnsKebabCaseString(string value)
        {
            var template = handlebars.Compile("{{#kebabCase value}}");
            return template(new { value });
        }

        [TestCase("Simple Title", ExpectedResult = "simple_title")]
        [TestCase("simple-title_with_mixed-separators", ExpectedResult = "simple_title_with_mixed_separators")]
        [TestCase("already_snake_cased", ExpectedResult = "already_snake_cased")]
        [TestCase("multiple--separators___together", ExpectedResult = "multiple_separators_together")]
        [TestCase("PascalCaseName", ExpectedResult = "pascal_case_name")]
        [TestCase("camelCaseName", ExpectedResult = "camel_case_name")]
        [TestCase("MixedCase_with-separators", ExpectedResult = "mixed_case_with_separators")]
        [TestCase("XMLDocument", ExpectedResult = "xml_document")]
        [TestCase("EnhancedIOStream", ExpectedResult = "enhanced_io_stream")]
        [TestCase("DataProcessorAPI", ExpectedResult = "data_processor_api")]
        [TestCase("file@domain.com", ExpectedResult = "file_domain_com")]
        [TestCase("price$100&tax", ExpectedResult = "price_100_tax")]
        [TestCase("version2.0.1", ExpectedResult = "version2_0_1")]
        public string SnakeCase_ReturnsKebabCaseString(string value)
        {
            var template = handlebars.Compile("{{#snakeCase value}}");
            return template(new { value });
        }

        [TestCase("apple,banana,cherry", ",", ExpectedResult = "[apple][banana][cherry]")]
        [TestCase("one-two-three", "-", ExpectedResult = "[one][two][three]")]
        [TestCase("hello::world::test", "::", ExpectedResult = "[hello][world][test]")]
        [TestCase("nomatch", ",", ExpectedResult = "[nomatch]")]
        [TestCase("", ",", ExpectedResult = "")]
        [TestCase(null, ",", ExpectedResult = "")]
        [TestCase("test", "", ExpectedResult = "")]
        [TestCase("test", null, ExpectedResult = "")]
        [TestCase("a,,b,c", ",", ExpectedResult = "[a][][b][c]")]
        public string Split_ReturnsExpectedResult(string? input, string? separator)
        {
            var template = handlebars.Compile("{{#each (split input separator)}}[{{this}}]{{/each}}");
            return template(new { input, separator });
        }

        [TestCase("Hello", ' ', "World", '!', ExpectedResult = "Hello World!")]
        [TestCase("Foo", "Bar", "Baz", ExpectedResult = "FooBarBaz")]
        public string Concat_ReturnsConcatenatedStrings(params object[] values)
        {
            var template = handlebars.Compile("{{#concat values}}");
            return template(new { values });
        }

        [TestCase("first", ExpectedResult = "first")]
        [TestCase("", ExpectedResult = "")]
        [TestCase("   ", ExpectedResult = "")]
        [TestCase(null, ExpectedResult = "")]
        [TestCase(null, "", "value", ExpectedResult = "value")]
        [TestCase("   ", "\t", "valid", ExpectedResult = "valid")]
        [TestCase(null, "", "   ", ExpectedResult = "")]
        [TestCase("first", "second", "third", ExpectedResult = "first")]
        [TestCase("", "second", "third", ExpectedResult = "second")]
        [TestCase(null, null, null, ExpectedResult = "")]
        [TestCase(null, "", 42, ExpectedResult = "42")]
        public string FirstNonBlank_ReturnsExpectedResult(params object?[] values)
        {
            var template = values.Length switch
            {
                1 => handlebars.Compile("{{#firstNonBlank first}}"),
                2 => handlebars.Compile("{{#firstNonBlank first second}}"),
                3 => handlebars.Compile("{{#firstNonBlank first second third}}"),
                _ => handlebars.Compile("{{#firstNonBlank first}}")
            };

            return template(values.Length switch
            {
                1 => new { first = values[0] },
                2 => new { first = values[0], second = values[1] },
                3 => new { first = values[0], second = values[1], third = values[2] },
                _ => new { first = values[0] }
            });
        }
    }
}
