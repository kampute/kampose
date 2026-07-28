// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Support
{
    using Kampose.Models;
    using Kampose.Support;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class MarkdownToMarkdownTransformerTests
    {
        [TestCase("NOTE")]
        [TestCase("TIP")]
        [TestCase("IMPORTANT")]
        [TestCase("WARNING")]
        [TestCase("CAUTION")]
        [TestCase("note")]
        public void Transform_WithGitHubAlert_ConvertsToTitledBlockquote(string kind)
        {
            var markdown = $"> [!{kind}]\n> Alert content.";

            var result = Transform(markdown);
            var title = char.ToUpperInvariant(kind[0]) + kind[1..].ToLowerInvariant();

            Assert.That(result, Is.EqualTo($"> **{title}** \\{Environment.NewLine}> Alert content."));
        }

        [Test]
        public void Transform_WithMultipleAlerts_PreservesContentAndExistingLineEndingBehavior()
        {
            var markdown = "Before\r\n\r\n> [!NOTE]\r\n> First alert.\r\n\r\n> [!WARNING]\r\n> Second alert.\r\n\r\nAfter\r\n";

            var result = Transform(markdown);

            var newline = Environment.NewLine;
            Assert.That(result, Is.EqualTo(
                $"Before{newline}{newline}> **Note** \\{newline}> First alert.{newline}{newline}" +
                $"> **Warning** \\{newline}> Second alert.{newline}{newline}After"));
        }

        [Test]
        public void Transform_WithOrdinaryBlockquote_LeavesContentUnchanged()
        {
            const string markdown = "> Ordinary quoted content.";

            var result = Transform(markdown);

            Assert.That(result, Is.EqualTo(markdown));
        }

        [TestCase("```markdown\n> [!NOTE]\n> Example content.\n```")]
        [TestCase("~~~md\n> [!WARNING]\n> Example content.\n~~~")]
        [TestCase("> ```markdown\n> [!IMPORTANT]\n> Example content.\n> ```")]
        [TestCase("- ```markdown\n  > [!CAUTION]\n  > Example content.\n  ```")]
        [TestCase("    > [!TIP]\n    > Indented code.")]
        public void Transform_WithAlertExampleInCode_LeavesContentUnchanged(string markdown)
        {
            var result = Transform(markdown);

            Assert.That(result, Is.EqualTo(markdown.ReplaceLineEndings()));
        }

        [Test]
        public void Transform_WithMalformedAlert_LeavesContentUnchanged()
        {
            const string markdown = "> [!UNKNOWN]\n> Content.";

            var result = Transform(markdown);

            Assert.That(result, Is.EqualTo(markdown.ReplaceLineEndings()));
        }

        [Test]
        public void Transform_WithUrlTransformer_RewritesUrlsAfterConvertingAlert()
        {
            const string markdown = "> [!NOTE]\n> Read the [guide](guide.md).";
            var urlTransformer = new PathToUrlMapper
            {
                { "guide.md", new Uri("documentation/guide", UriKind.Relative) }
            };

            var result = Transform(markdown, urlTransformer);

            Assert.That(result, Is.EqualTo("> **Note** \\\n> Read the [guide](documentation/guide)."));
        }

        [Test]
        public void DocContext_WithMarkdownFormat_UsesAlertFallback()
        {
            var context = new DocContext(
                new CSharp(),
                MockHelper.CreateAddressProvider(),
                new XmlDocProvider(MockHelper.CreateXmlDocResolver()),
                new MarkdownFormat(),
                [],
                [],
                []);

            var transformed = context.TryTransformText(
                FileExtensions.Markdown,
                "> [!IMPORTANT]\n> Essential information.",
                out var result);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(transformed, Is.True);
                Assert.That(result, Is.EqualTo("> **Important** \\\n> Essential information."));
            }
        }

        private static string Transform(string markdown, IUrlTransformer? urlTransformer = null)
        {
            var transformer = new MarkdownToMarkdownTransformer();
            using var reader = new StringReader(markdown);
            using var writer = new StringWriter();
            transformer.Transform(reader, writer, urlTransformer);
            return writer.ToString();
        }
    }
}
