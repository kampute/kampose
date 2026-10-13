// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Transformers
{
    using Kampose.Models;
    using Kampose.Transformers;
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
    public class GitLabMarkdownTransformerTests
    {
        [TestCase("NOTE", "note")]
        [TestCase("TIP", "tip")]
        [TestCase("IMPORTANT", "important")]
        [TestCase("WARNING", "warning")]
        [TestCase("CAUTION", "caution")]
        [TestCase("Note", "note")]
        [TestCase("note", "note")]
        public void Transform_WithGitHubAlert_ConvertsToGitLabAlert(string kind, string gitLabKind)
        {
            var markdown = $"> [!{kind}]\n> Alert content.";

            var result = Transform(markdown);

            Assert.That(result, Is.EqualTo($"> [!{gitLabKind}]{Environment.NewLine}> Alert content."));
        }

        [Test]
        public void Transform_WithMultipleAlerts_ConvertsOnlyAlertDirectives()
        {
            const string markdown = "Before [!NOTE]\n\n> [!NOTE]\n> First [!WARNING] alert.\n\n>[!WARNING]  \n> Second alert.";

            var result = Transform(markdown);

            Assert.That(result, Is.EqualTo("Before [!NOTE]\n\n> [!note]\n> First [!WARNING] alert.\n\n>[!warning]  \n> Second alert.".ReplaceLineEndings()));
        }

        [TestCase("> [!UNKNOWN]\n> Content.")]
        [TestCase("> Ordinary quoted content.")]
        [TestCase("```markdown\n> [!NOTE]\n> Example content.\n```")]
        public void Transform_WithoutAlert_LeavesContentUnchanged(string markdown)
        {
            var result = Transform(markdown);

            Assert.That(result, Is.EqualTo(markdown.ReplaceLineEndings()));
        }

        [Test]
        public void DocContext_WithMarkdownFormatAndGitLabTransformer_UsesGitLabAlerts()
        {
            var context = new DocContext(
                new CSharp(),
                MockHelper.CreateAddressProvider(),
                new XmlDocProvider(MockHelper.CreateXmlDocResolver()),
                new MarkdownFormat(),
                [],
                [],
                [],
                markdownTransformer: new GitLabMarkdownTransformer());

            var transformed = context.TryTransformText(
                FileExtensions.Markdown,
                "> [!IMPORTANT]\n> Essential information.",
                out var result);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(transformed, Is.True);
                Assert.That(result, Is.EqualTo("> [!important]\n> Essential information."));
            }
        }

        private static string Transform(string markdown, IUrlTransformer? urlTransformer = null)
        {
            var transformer = new GitLabMarkdownTransformer();
            using var reader = new StringReader(markdown);
            using var writer = new StringWriter();
            transformer.Transform(reader, writer, urlTransformer);
            return writer.ToString();
        }
    }
}
