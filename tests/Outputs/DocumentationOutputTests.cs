// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Outputs
{
    using Kampose.Outputs;
    using Kampose.Reporters;
    using Kampute.DocToolkit;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class DocumentationOutputTests
    {
        private string testDirectory = string.Empty;
        private string outputDirectory = string.Empty;

        [SetUp]
        public void SetUp()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), $"documentation-output-tests-{Guid.NewGuid()}");
            outputDirectory = Path.Combine(testDirectory, "output");
            Directory.CreateDirectory(testDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(testDirectory))
                Directory.Delete(testDirectory, true);
        }

        [Test]
        public void DryRun_CopyFileWithMissingSource_ThrowsAndDoesNotRecordTarget()
        {
            var output = new DryRunDocumentationOutput(outputDirectory);
            var targetPath = Path.Combine(outputDirectory, "missing.svg");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(() => output.CopyFile(Path.Combine(testDirectory, "missing.svg"), targetPath), Throws.InstanceOf<IOException>());
                Assert.That(output.GeneratedFiles.Contains(targetPath), Is.False);
            }
        }

        [Test]
        public void PageWriter_HtmlPage_RecordsAnchorsWhenDisposed()
        {
            var output = new DryRunDocumentationOutput(outputDirectory);

            using (var writer = ((FileSystemDocumentWriterFactory)output.CreatePageWriterFactory()).CreateTextWriter("page.html"))
            {
                writer.Write("<meta name=\"description\" content=\"x\" /><h2 id=\"intro\">Intro</h2>");
                writer.Write("<a name='legacy'></a><div data-id=\"data\" id=\"a&amp;b\"></div><code>id=&quot;sample&quot;</code>");
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(output.GeneratedFiles.TryGetAnchors("page.html", out var anchors), Is.True);
                Assert.That(anchors, Is.EquivalentTo(new[] { "intro", "legacy", "a&b" }));
            }
        }

        [Test]
        public void PageWriter_HtmlPageWithUnquotedAttributes_RecordsAnchors()
        {
            var output = new DryRunDocumentationOutput(outputDirectory);

            using (var writer = ((FileSystemDocumentWriterFactory)output.CreatePageWriterFactory()).CreateTextWriter("page.html"))
                writer.Write("<h2 id=intro>Intro</h2><a name=legacy></a><a data-name=\"data\" href=\"#intro\"></a>");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(output.GeneratedFiles.TryGetAnchors("page.html", out var anchors), Is.True);
                Assert.That(anchors, Is.EquivalentTo(new[] { "intro", "legacy" }));
            }
        }

        [Test]
        public void PageWriter_HtmlPageDisposedTwice_KeepsRecordedAnchors()
        {
            var output = new DryRunDocumentationOutput(outputDirectory);

            var writer = ((FileSystemDocumentWriterFactory)output.CreatePageWriterFactory()).CreateTextWriter("page.html");
            writer.Write("<h2 id=\"intro\">Intro</h2>");
            writer.Dispose();
            writer.Dispose();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(output.GeneratedFiles.TryGetAnchors("page.html", out var anchors), Is.True);
                Assert.That(anchors, Is.EquivalentTo(new[] { "intro" }));
            }
        }

        [Test]
        public void PageWriter_MarkdownPage_RecordsNoAnchors()
        {
            var output = new DryRunDocumentationOutput(outputDirectory);

            using (var writer = ((FileSystemDocumentWriterFactory)output.CreatePageWriterFactory()).CreateTextWriter("page.md"))
                writer.Write("<a id=\"intro\"></a>");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(output.GeneratedFiles.Contains("page.md"), Is.True);
                Assert.That(output.GeneratedFiles.TryGetAnchors("page.md", out _), Is.False);
            }
        }

        [Test]
        public void FileSystem_PrepareWithClean_EmptiesOutputDirectory()
        {
            Directory.CreateDirectory(Path.Combine(outputDirectory, "old"));
            File.WriteAllText(Path.Combine(outputDirectory, "old", "page.html"), "old");
            File.WriteAllText(Path.Combine(outputDirectory, "stale.html"), "stale");
            var output = new FileSystemDocumentationOutput(outputDirectory, clean: true);

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            output.Prepare(reporter);

            Assert.That(Directory.EnumerateFileSystemEntries(outputDirectory), Is.Empty);
        }

        [Test]
        public void FileSystem_PrepareWithoutClean_KeepsOutputDirectoryContents()
        {
            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(Path.Combine(outputDirectory, "stale.html"), "stale");
            var output = new FileSystemDocumentationOutput(outputDirectory);

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            output.Prepare(reporter);

            Assert.That(File.Exists(Path.Combine(outputDirectory, "stale.html")), Is.True);
        }
    }
}
