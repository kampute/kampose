// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Support
{
    using Kampose.Support;
    using Kampute.DocToolkit.Routing;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class MarkdownToHtmlTransformerTests
    {
        [TestCase("", ExpectedResult = "")]
        [TestCase("   ", ExpectedResult = "")]
        // Basic formatting tests
        [TestCase("Simple text", ExpectedResult = "<p>Simple text</p>\n")]
        [TestCase("**Bold text**", ExpectedResult = "<p><strong>Bold text</strong></p>\n")]
        [TestCase("*Italic text*", ExpectedResult = "<p><em>Italic text</em></p>\n")]
        [TestCase("# Heading 1", ExpectedResult = "<h1 id=\"heading-1\">Heading 1</h1>\n")]
        [TestCase("## Heading 2", ExpectedResult = "<h2 id=\"heading-2\">Heading 2</h2>\n")]
        [TestCase("First paragraph.\n\nSecond paragraph.", ExpectedResult = "<p>First paragraph.</p>\n<p>Second paragraph.</p>\n")]
        [TestCase("[Link](https://example.com)", ExpectedResult = "<p><a href=\"https://example.com\">Link</a></p>\n")]
        [TestCase("`inline code`", ExpectedResult = "<p><code>inline code</code></p>\n")]
        [TestCase("- List item 1\n- List item 2", ExpectedResult = "<ul>\n<li>List item 1</li>\n<li>List item 2</li>\n</ul>\n")]
        [TestCase("```\nvar x = 10;\nvar y = 20;\n```", ExpectedResult = "<pre><code>var x = 10;\nvar y = 20;\n</code></pre>\n")]
        [TestCase("```csharp\npublic class Example {\n    public void Test() {}\n}\n```", ExpectedResult = "<pre><code class=\"language-csharp\">public class Example {\n    public void Test() {}\n}\n</code></pre>\n")]
        [TestCase("| Header1 | Header2 |\n|---------|---------|\n| Cell1   | Cell2   |", ExpectedResult = "<table>\n<thead>\n<tr>\n<th>Header1</th>\n<th>Header2</th>\n</tr>\n</thead>\n<tbody>\n<tr>\n<td>Cell1</td>\n<td>Cell2</td>\n</tr>\n</tbody>\n</table>\n")]
        // Handlebars expression tests
        [TestCase("{{name}}", ExpectedResult = "<p>{{name}}</p>\n")]
        [TestCase("{{{html}}}", ExpectedResult = "<p>{{{html}}}</p>\n")]
        [TestCase("{{#if condition}}yes{{/if}}", ExpectedResult = "<p>{{#if condition}}yes{{/if}}</p>\n")]
        [TestCase("**{{bold}}**", ExpectedResult = "<p><strong>{{bold}}</strong></p>\n")]
        [TestCase("```\n{{code}}\n```", ExpectedResult = "<pre><code>{{code}}\n</code></pre>\n")]
        [TestCase("```csharp\n{{code}}\n```", ExpectedResult = "<pre><code class=\"language-csharp\">{{code}}\n</code></pre>\n")]
        public string ToHtml_ReturnsExpectedHtml(string markdown)
        {
            var transformer = new MarkdownToHtmlTransformer();
            return transformer.Transform(markdown);
        }

        [Test]
        public void ToHtml_WithUrlTransformer_ReplacesUrlsCorrectly()
        {
            var markdown = "# URL Testing\n"
                         + "Relative links like [home](/home) and [docs](docs) should be replaced.\n"
                         + "Absolute links like [this link](https://example.com) should be preserved.\n"
                         + "[Query params](docs?param=value) should be handled separately from [hash](/home#section).\n"
                         + "[Email links](mailto:test@example.com) should remain untouched.\n";

            var transformer = new MarkdownToHtmlTransformer();

            var result = transformer.Transform(markdown, new PathToUrlMapper
            {
                { "/home", new Uri("/index.html", UriKind.Relative) },
                {  "docs", new Uri("documentation", UriKind.Relative) },
            });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Does.Contain("href=\"/index.html\""));
                Assert.That(result, Does.Contain("href=\"documentation\""));
                Assert.That(result, Does.Contain("href=\"documentation?param=value\""));
                Assert.That(result, Does.Contain("href=\"/index.html#section\""));
                Assert.That(result, Does.Contain("href=\"https://example.com\""));
                Assert.That(result, Does.Contain("mailto:test@example.com"));
                Assert.That(result, Does.Not.Contain("href=\"/home\""));
                Assert.That(result, Does.Not.Contain("href=\"docs\""));
                Assert.That(result, Does.Not.Contain("href=\"docs?param=value\""));
                Assert.That(result, Does.Not.Contain("href=\"/home#section\""));
            }
        }
    }
}
