// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Templates.Helpers
{
    using HandlebarsDotNet;
    using Kampose.Support;
    using Kampose.Templates.Helpers;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System;
    using System.Linq;

    [TestFixture]
    public class UtilityHelpersTests
    {
        private IHandlebars handlebars = null!;

        [SetUp]
        public void SetUp()
        {
            handlebars = Handlebars.Create();
            var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>();
            docContext.ContentFormatter.TextTransformers.Register<MarkdownToHtmlTransformer>(FileExtensions.Markdown);
            UtilityHelpers.Register(handlebars, docContext);
        }

        [Test]
        public void IsUndefined_ReturnsTrueIfValueIsUndefined()
        {
            var template = handlebars.Compile("{{#isUndefined value}}");
            var result = template(new { });

            Assert.That(result, Is.EqualTo("True"));
        }

        [Test]
        public void IsUndefined_ReturnsFalseIfValueIsNotUndefined()
        {
            var template = handlebars.Compile("{{#isUndefined value}}");
            var result = template(new { value = (object)null! });

            Assert.That(result, Is.EqualTo("False"));
        }

        [TestCase(null, ExpectedResult = "True")]
        [TestCase(false, ExpectedResult = "False")]
        [TestCase(0, ExpectedResult = "False")]
        [TestCase("", ExpectedResult = "False")]
        public string IsNull_ReturnsTrueIfValueIsNull(object? value)
        {
            var template = handlebars.Compile("{{#isNull value}}");
            return template(new { value });
        }

        [TestCase(1, ExpectedResult = "True")]
        [TestCase(2, ExpectedResult = "False")]
        [TestCase(0, ExpectedResult = "False")]
        [TestCase(-1, ExpectedResult = "True")]
        [TestCase(-2, ExpectedResult = "False")]
        [TestCase("101", ExpectedResult = "True")]
        [TestCase("100", ExpectedResult = "False")]
        public string IsOdd_ReturnsTrueIfValueIsOdd(object value)
        {
            var template = handlebars.Compile("{{#isOdd value}}");
            return template(new { value });
        }

        [TestCase(true, "A", "B", ExpectedResult = "A")]
        [TestCase(false, "A", "B", ExpectedResult = "B")]
        [TestCase(1, "A", "B", ExpectedResult = "A")]
        [TestCase(0, "A", "B", ExpectedResult = "B")]
        [TestCase("nonempty", "A", "B", ExpectedResult = "A")]
        [TestCase("", "A", "B", ExpectedResult = "B")]
        [TestCase(null, "A", "B", ExpectedResult = "B")]
        [TestCase(new int[] { 1 }, "A", "B", ExpectedResult = "A")]
        [TestCase(new int[] { }, "A", "B", ExpectedResult = "B")]
        [TestCase(0, "A", "B", "C", ExpectedResult = "A")]
        [TestCase(1, "A", "B", "C", ExpectedResult = "B")]
        [TestCase(2, "A", "B", "C", ExpectedResult = "C")]
        [TestCase(3, "A", "B", "C", ExpectedResult = "")]
        [TestCase(-1, "A", "B", "C", ExpectedResult = "")]
        [TestCase("1", "A", "B", "C", ExpectedResult = "B")]
        [TestCase("invalid", "A", "B", "C", ExpectedResult = "")]
        public string Select_ReturnsCorrectValue(object? selector, params string[] choices)
        {
            var template = handlebars.Compile("{{#select selector choices}}");
            return template(new { selector, choices });
        }

        [TestCase("hello", 5)]
        [TestCase("", 0)]
        public void Len_ReturnsStringLength(string input, int expected)
        {
            var template = handlebars.Compile("{{#len value}}");
            var result = template(new { value = input });
            Assert.That(result, Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void Len_ReturnsCollectionLength()
        {
            var items = new[] { 1, 2, 3 };
            var template = handlebars.Compile("{{#len value}}");
            var result = template(new { value = items });
            Assert.That(result, Is.EqualTo("3"));
        }

        [Test]
        public void Len_ReturnsEnumerableLength()
        {
            var enumerable = Enumerable.Range(1, 4);
            var template = handlebars.Compile("{{#len value}}");
            var result = template(new { value = enumerable });
            Assert.That(result, Is.EqualTo("4"));
        }

        [TestCase("yyyy-MM-dd")]
        [TestCase("HH:mm")]
        public void Now_ReturnsFormattedDateTime(string format)
        {
            var template = handlebars.Compile("{{#now format}}");
            var result = template(new { format });

            var expected = DateTime.Now.ToString(format);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Json_ReturnsSerializedJson()
        {
            var data = new object[] { "str", new { Integer = 1, Boolean = true } };
            var expectedJson = "[\r\n  \"str\",\r\n  {\r\n    \"integer\": 1,\r\n    \"boolean\": true\r\n  }\r\n]";

            var template = handlebars.Compile("{{{#json data}}}");
            Assert.That(template(new { data }), Is.EqualTo(expectedJson));
        }

        [TestCase(null, ExpectedResult = "null")]
        [TestCase(true, ExpectedResult = "true")]
        [TestCase(false, ExpectedResult = "false")]
        [TestCase(42, ExpectedResult = "42")]
        [TestCase(0, ExpectedResult = "0")]
        [TestCase(-123.45, ExpectedResult = "-123.45")]
        [TestCase('a', ExpectedResult = "'a'")]
        [TestCase('\n', ExpectedResult = "'\\n'")]
        [TestCase("Hello\tWorld!", ExpectedResult = "&quot;Hello\\tWorld!&quot;")]
        [TestCase("", ExpectedResult = "&quot;&quot;")]
        public string Literal_RendersValueAsCode(object? value)
        {
            var template = handlebars.Compile("{{#literal value}}");
            return template(new { value });
        }

        [TestCase("N:System.Collections.Generic", ExpectedResult = "<a href=\"https://example.com/system.collections.generic\" rel=\"code-reference\">System.Collections.Generic</a>")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "<a href=\"https://example.com/system.collections.generic.list-1\" rel=\"code-reference\">List&lt;T&gt;</a>")]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = "<a href=\"https://example.com/system.console.writeline\" rel=\"code-reference\">Console.WriteLine()</a>")]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = "<a href=\"https://example.com/system.console.writeline(system.string)\" rel=\"code-reference\">Console.WriteLine(string)</a>")]
        [TestCase("P:System.Collections.Generic.List`1.Count", ExpectedResult = "<a href=\"https://example.com/system.collections.generic.list-1.count\" rel=\"code-reference\">List&lt;T&gt;.Count</a>")]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = "<a href=\"https://example.com/system.datetime.maxvalue\" rel=\"code-reference\">DateTime.MaxValue</a>")]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = "<a href=\"https://example.com/system.appdomain.assemblyload\" rel=\"code-reference\">AppDomain.AssemblyLoad</a>")]
        public string Cref_RendersDocumentationLink(string cref)
        {
            var template = handlebars.Compile("{{{#cref value}}}");
            return template(new { value = cref });
        }

        [TestCase("<p>Test</p>", ExpectedResult = "Test")]
        [TestCase("<div><p>Nested</p></div>", ExpectedResult = "Nested")]
        [TestCase("<span>Multiple <b>tags</b></span>", ExpectedResult = "Multiple tags")]
        [TestCase("<a href='#'>Link</a>", ExpectedResult = "Link")]
        [TestCase("<p>Test with <br> line break</p>", ExpectedResult = "Test with  line break")]
        public string StripTags_RendersHtmlWithoutTags(string html)
        {
            var template = handlebars.Compile("{{#stripTags}}{{{content}}}{{/stripTags}}");
            return template(new { content = html });
        }

        [TestCase("Hello, world!", ExpectedResult = "<p>Hello, world!</p>")]
        [TestCase("This is an [example](https://example.com/) link.", ExpectedResult = "<p>This is an <a href=\"https://example.com/\">example</a> link.</p>")]
        [TestCase("**Bold** and *italic* text.", ExpectedResult = "<p><strong>Bold</strong> and <em>italic</em> text.</p>")]
        public string Markdown_RendersMarkdown(string markdown)
        {
            var template = handlebars.Compile("{{#markdown}}{{{content}}}{{/markdown}}");
            return template(new { content = markdown }).TrimEnd();
        }
    }
}
