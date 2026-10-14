// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Services
{
    using Kampose.Models;
    using Kampose.Outputs;
    using Kampose.Reporters;
    using Kampose.Services;
    using Kampose.Support;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class DocumentationServiceTests
    {
        private string testDirectory = string.Empty;
        private string outputDirectory = string.Empty;

        [SetUp]
        public void SetUp()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), $"documentation-service-tests-{Guid.NewGuid()}");
            outputDirectory = Path.Combine(testDirectory, "output");
            Directory.CreateDirectory(Path.Combine(testDirectory, "docs", "assets"));
            File.WriteAllText(Path.Combine(testDirectory, "docs", "assets", "diagram.svg"), "<svg />");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(testDirectory))
                Directory.Delete(testDirectory, true);
        }

        [Test]
        public void GenerateDocumentation_DryRunOutput_WritesNothingAndRecordsTheFilesThatBuildWrites()
        {
            WriteTopic("[Diagram](~/assets/diagram.svg)");
            var config = CreateConfiguration();
            var dryRun = new DryRunDocumentationOutput(config.OutputDirectory);
            var fileSystem = new FileSystemDocumentationOutput(config.OutputDirectory);

            Generate(config, dryRun);
            var outputCreatedByDryRun = Directory.Exists(outputDirectory);
            Generate(config, fileSystem);

            var writtenFiles = Directory.EnumerateFiles(outputDirectory, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(outputDirectory, path).Replace('\\', '/'))
                .ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(outputCreatedByDryRun, Is.False);
                Assert.That(writtenFiles, Is.Not.Empty);
                Assert.That(writtenFiles, Has.Count.EqualTo(dryRun.GeneratedFiles.Count));
                Assert.That(writtenFiles.Where(path => !dryRun.GeneratedFiles.Contains(path)), Is.Empty);
                Assert.That(fileSystem.GeneratedFiles.Count, Is.EqualTo(dryRun.GeneratedFiles.Count));
            }
        }

        [Test]
        public void GenerateDocumentation_LinkToGeneratedAsset_PassesUrlVerification()
        {
            WriteTopic("[Diagram](~/assets/diagram.svg)");
            var config = CreateConfiguration();
            config.Audit.StopOnIssues = true;

            Assert.That(() => Generate(config, new DryRunDocumentationOutput(config.OutputDirectory)), Throws.Nothing);
        }

        [Test]
        public void GenerateDocumentation_LinkToFileThatOnlyExistsInOutputDirectory_FailsUrlVerification()
        {
            WriteTopic("[Old page](~/stale.html)");
            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(Path.Combine(outputDirectory, "stale.html"), "<html />");
            var config = CreateConfiguration();
            config.Audit.StopOnIssues = true;

            Assert.That(() => Generate(config, new FileSystemDocumentationOutput(config.OutputDirectory)), Throws.TypeOf<ValidationException>());
        }

        [TestCase("[Section](#guide)")]
        [TestCase("[Section](other.md#other)")]
        public void GenerateDocumentation_HtmlLinkToExistingAnchor_PassesUrlVerification(string link)
        {
            WriteTopic(link);
            WriteTopic("other", "# Other");
            var config = CreateConfiguration();
            config.Audit.StopOnIssues = true;

            Assert.That(() => Generate(config, new DryRunDocumentationOutput(config.OutputDirectory)), Throws.Nothing);
        }

        [TestCase("[Section](#missing)")]
        [TestCase("[Section](other.md#missing)")]
        [TestCase("[Section](other.md#Other)")]
        public void GenerateDocumentation_HtmlLinkToMissingAnchor_FailsUrlVerification(string link)
        {
            WriteTopic(link);
            WriteTopic("other", "# Other");
            var config = CreateConfiguration();
            config.Audit.StopOnIssues = true;

            Assert.That(() => Generate(config, new DryRunDocumentationOutput(config.OutputDirectory)), Throws.TypeOf<ValidationException>());
        }

        [Test]
        public void GenerateDocumentation_MarkdownLinkToUnknownAnchor_PassesUrlVerification()
        {
            WriteTopic("[Section](#missing)");
            var config = CreateConfiguration();
            config.Convention = DocConvention.DevOps;
            config.Audit.StopOnIssues = true;

            Assert.That(() => Generate(config, new DryRunDocumentationOutput(config.OutputDirectory)), Throws.Nothing);
        }

        private static void Generate(Configuration config, DocumentationOutput output)
        {
            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            new DocumentationService(reporter).GenerateDocumentation(config, output);
        }

        private void WriteTopic(string content) => WriteTopic("guide", $"# Guide\n\n{content}");

        private void WriteTopic(string name, string content)
        {
            File.WriteAllText(Path.Combine(testDirectory, "docs", $"{name}.md"), $"{content}\n");
        }

        private Configuration CreateConfiguration()
        {
            var config = new Configuration
            {
                BaseDirectory = testDirectory,
                OutputDirectory = outputDirectory
            };
            config.Topics.Clear();
            config.Topics.Add("docs/*.md");
            config.Assets.Add(new FileTransferFilter
            {
                Source = { "docs/assets/*" },
                TargetPath = "assets"
            });
            return config;
        }
    }
}
