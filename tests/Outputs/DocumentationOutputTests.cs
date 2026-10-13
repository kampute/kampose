// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Outputs
{
    using Kampose.Outputs;
    using Kampose.Reporters;
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
