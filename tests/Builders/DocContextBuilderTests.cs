// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Builders
{
    using Kampose.Builders;
    using Kampose.Commands;
    using Kampose.Models;
    using Kampose.Reporters;
    using Kampose.Support;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Routing;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    [TestFixture]
    public class DocContextBuilderTests
    {
        private string testDirectory = string.Empty;

        [SetUp]
        public void SetUp()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), $"asset-tests-{Guid.NewGuid()}");
            Directory.CreateDirectory(Path.Combine(testDirectory, "docs", "guides"));
            Directory.CreateDirectory(Path.Combine(testDirectory, "docs", "assets", "diagrams"));
            File.WriteAllText(Path.Combine(testDirectory, "docs", "guides", "setup.md"), "# Setup");
            File.WriteAllText(Path.Combine(testDirectory, "docs", "assets", "diagrams", "setup-flow.svg"), "<svg />");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(testDirectory))
                Directory.Delete(testDirectory, true);
        }

        [Test]
        public void Configure_WithNestedAsset_PreservesHierarchyBelowTargetPath()
        {
            var config = CreateConfiguration();
            config.Assets.Add(new FileTransferFilter
            {
                Source = { "docs/assets/**/*" },
                TargetPath = "assets"
            });

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            using var context = new DocContextBuilder(reporter)
                .Configure(config, Theme.Load("classic", config.Convention))
                .Build();

            var asset = context.Assets.Single(asset => asset.SourcePath.EndsWith("setup-flow.svg", StringComparison.Ordinal));
            Assert.That(asset.TargetPath, Is.SamePath(Path.Combine(testDirectory, "output", "assets", "diagrams", "setup-flow.svg")));
        }

        [Test]
        public void Configure_WithLoadedEmptyTargetPath_PreservesHierarchyBelowOutputRoot()
        {
            var configPath = Path.Combine(testDirectory, "kampose.json");
            File.WriteAllText(configPath, """
                {
                  "topics": ["docs/**/*.md"],
                  "assets": [
                    {
                      "source": ["docs/assets/**/*"]
                    }
                  ],
                  "outputDirectory": "output"
                }
                """);
            var config = Configuration.LoadFromFile(configPath);

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            using var context = new DocContextBuilder(reporter)
                .Configure(config, Theme.Load("classic", config.Convention))
                .Build();

            var asset = context.Assets.Single(asset => asset.SourcePath.EndsWith("setup-flow.svg", StringComparison.Ordinal));
            Assert.That(asset.TargetPath, Is.SamePath(Path.Combine(testDirectory, "output", "diagrams", "setup-flow.svg")));
        }

        [TestCase("docFx", ".html")]
        [TestCase("devOps", ".md")]
        [TestCase("gitLab", ".md")]
        public void Build_WithSourceRelativeAssetLinks_MapsAndVerifiesGeneratedUrls(string convention, string topicExtension)
        {
            Directory.CreateDirectory(Path.Combine(testDirectory, "docs", "assets", "other"));
            Directory.CreateDirectory(Path.Combine(testDirectory, "docs", "assets", "excluded"));
            File.WriteAllText(Path.Combine(testDirectory, "docs", "assets", "other", "setup-flow.svg"), "<svg id=\"other\" />");
            File.WriteAllText(Path.Combine(testDirectory, "docs", "assets", "excluded", "unused.svg"), "<svg />");
            File.WriteAllText(
                Path.Combine(testDirectory, "docs", "guides", "setup.md"),
                "# Setup\n\n![Setup flow](../assets/diagrams/setup-flow.svg)\n\n![Other flow](../assets/other/setup-flow.svg)");

            var configPath = Path.Combine(testDirectory, "kampose.json");
            File.WriteAllText(configPath, $$"""
                {
                  "topics": ["docs/**/*.md"],
                  "assets": [
                    {
                      "source": ["docs/assets/**/*", "!docs/assets/excluded/**/*"],
                      "targetPath": "static/assets"
                    }
                  ],
                  "outputDirectory": "output",
                  "convention": "{{convention}}",
                  "audit": { "stopOnIssues": true }
                }
                """);

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            var exitCode = new BuildCommand().Execute([configPath], reporter);

            var outputDirectory = Path.Combine(testDirectory, "output");
            var topicOutput = Directory.EnumerateFiles(outputDirectory, $"*{topicExtension}", SearchOption.AllDirectories)
                .Single(path => File.ReadAllText(path).Contains("Setup flow", StringComparison.Ordinal));
            var topicContent = File.ReadAllText(topicOutput).Replace('\\', '/');

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(reporter.WarningCount, Is.Zero);
                Assert.That(Path.Combine(outputDirectory, "static", "assets", "diagrams", "setup-flow.svg"), Does.Exist);
                Assert.That(Path.Combine(outputDirectory, "static", "assets", "other", "setup-flow.svg"), Does.Exist);
                Assert.That(Path.Combine(outputDirectory, "static", "assets", "excluded", "unused.svg"), Does.Not.Exist);
                Assert.That(topicContent, Does.Contain("static/assets/diagrams/setup-flow.svg"));
                Assert.That(topicContent, Does.Contain("static/assets/other/setup-flow.svg"));
            }
        }

        [Test]
        public void Build_WithSourceRelativeLinkToAssetWithPlusInName_MapsAndVerifiesGeneratedUrl()
        {
            File.WriteAllText(Path.Combine(testDirectory, "docs", "assets", "diagrams", "c++.svg"), "<svg />");
            File.WriteAllText(Path.Combine(testDirectory, "docs", "guides", "setup.md"), "# Setup\n\n![C++ flow](../assets/diagrams/c++.svg)");

            var configPath = Path.Combine(testDirectory, "kampose.json");
            File.WriteAllText(configPath, """
                {
                  "topics": ["docs/**/*.md"],
                  "assets": [
                    {
                      "source": ["docs/assets/**/*"],
                      "targetPath": "static/assets"
                    }
                  ],
                  "outputDirectory": "output",
                  "convention": "docFx",
                  "audit": { "stopOnIssues": true }
                }
                """);

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            var exitCode = new BuildCommand().Execute([configPath], reporter);

            var outputDirectory = Path.Combine(testDirectory, "output");
            var topicContent = Directory.EnumerateFiles(outputDirectory, "*.html", SearchOption.AllDirectories)
                .Select(File.ReadAllText)
                .SingleOrDefault(content => content.Contains("C++ flow", StringComparison.Ordinal));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(reporter.WarningCount, Is.Zero);
                Assert.That(topicContent, Does.Contain("static/assets/diagrams/c++.svg"));
            }
        }

        [Test]
        public void Configure_WithMultipleIncludes_UsesEachIncludesWildcardRootAndAppliesExclusions()
        {
            Directory.CreateDirectory(Path.Combine(testDirectory, "vendor", "media", "icons"));
            Directory.CreateDirectory(Path.Combine(testDirectory, "vendor", "media", "private"));
            File.WriteAllText(Path.Combine(testDirectory, "vendor", "media", "icons", "vendor.svg"), "<svg />");
            File.WriteAllText(Path.Combine(testDirectory, "vendor", "media", "private", "secret.svg"), "<svg />");
            var config = CreateConfiguration();
            config.Assets.Add(new FileTransferFilter
            {
                Source = { "docs/assets/**/*", "vendor/media/**/*", "!vendor/media/private/**/*" },
                TargetPath = "public/files"
            });

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            using var context = new DocContextBuilder(reporter)
                .Configure(config, Theme.Load("classic", config.Convention))
                .Build();

            var configuredAssets = context.Assets.Where(asset => asset.SourcePath.StartsWith(testDirectory, StringComparison.OrdinalIgnoreCase)).ToArray();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(configuredAssets.Select(asset => asset.TargetPath), Does.Contain(Path.Combine(testDirectory, "output", "public", "files", "diagrams", "setup-flow.svg")));
                Assert.That(configuredAssets.Select(asset => asset.TargetPath), Does.Contain(Path.Combine(testDirectory, "output", "public", "files", "icons", "vendor.svg")));
                Assert.That(configuredAssets, Has.None.Matches<AssetReference>(asset => asset is not null && asset.SourcePath.EndsWith("secret.svg", StringComparison.Ordinal)));
            }
        }

        [Test]
        public void Configure_WithAmbiguousOverlappingIncludes_ThrowsValidationException()
        {
            var config = CreateConfiguration();
            config.Assets.Add(new FileTransferFilter
            {
                Source = { "docs/assets/**/*", "docs/assets/diagrams/**/*" },
                TargetPath = "assets"
            });

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            var exception = Assert.Throws<ValidationException>(() => new DocContextBuilder(reporter)
                .Configure(config, Theme.Load("classic", config.Convention)));

            Assert.That(exception!.Message, Does.Contain("ambiguous"));
        }

        [Test]
        public void Configure_WithCompleteGeneratedPathCollision_ThrowsValidationException()
        {
            Directory.CreateDirectory(Path.Combine(testDirectory, "docs", "assets", "other"));
            File.WriteAllText(Path.Combine(testDirectory, "docs", "assets", "other", "setup-flow.svg"), "<svg />");
            var config = CreateConfiguration();
            config.Assets.Add(new FileTransferFilter
            {
                Source = { "docs/assets/diagrams/**/*" },
                TargetPath = "assets"
            });
            config.Assets.Add(new FileTransferFilter
            {
                Source = { "docs/assets/other/**/*" },
                TargetPath = "assets"
            });

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            var exception = Assert.Throws<ValidationException>(() => new DocContextBuilder(reporter)
                .Configure(config, Theme.Load("classic", config.Convention)));

            Assert.That(exception!.Message, Does.Contain("same generated path"));
        }

        [TestCase("../escape")]
        [TestCase("assets/../../escape")]
        public void Configure_WithParentTraversalTargetPath_ThrowsValidationException(string targetPath)
        {
            var config = CreateConfiguration();
            config.Assets.Add(new FileTransferFilter
            {
                Source = { "docs/assets/**/*" },
                TargetPath = targetPath
            });

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            Assert.That(
                () => new DocContextBuilder(reporter).Configure(config, Theme.Load("classic", config.Convention)),
                Throws.TypeOf<ValidationException>());
        }

        [Test]
        public void Configure_WithRootedTargetPath_ThrowsValidationException()
        {
            var config = CreateConfiguration();
            config.Assets.Add(new FileTransferFilter
            {
                Source = { "docs/assets/**/*" },
                TargetPath = Path.Combine(Path.GetPathRoot(testDirectory)!, "outside")
            });

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            Assert.That(
                () => new DocContextBuilder(reporter).Configure(config, Theme.Load("classic", config.Convention)),
                Throws.TypeOf<ValidationException>());
        }

        [Test]
        public void Build_GitLabConvention_WritesHomeTopicToHomePageWithTitleInFrontMatter()
        {
            File.WriteAllText(
                Path.Combine(testDirectory, "docs", "README.md"),
                "---\ntitle: 'Project \"Overview\": Start'\n---\n\nOverview content.");

            var configPath = Path.Combine(testDirectory, "kampose.json");
            File.WriteAllText(configPath, """
                {
                  "topics": ["docs/**/*.md"],
                  "outputDirectory": "output",
                  "convention": "gitLab",
                  "audit": { "stopOnIssues": true }
                }
                """);

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            var exitCode = new BuildCommand().Execute([configPath], reporter);

            var homePagePath = Path.Combine(testDirectory, "output", "home.md");
            var homePageLines = File.Exists(homePagePath) ? File.ReadAllLines(homePagePath) : [];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(Directory.EnumerateFiles(Path.Combine(testDirectory, "output"), "*.md").Select(Path.GetFileName), Is.EquivalentTo(new[] { "home.md" }));
                Assert.That(Path.Combine(testDirectory, "output", "home", "Setup.md"), Does.Exist);
                Assert.That(homePageLines.ElementAtOrDefault(0), Is.EqualTo("---"));
                Assert.That(homePageLines.ElementAtOrDefault(1), Does.StartWith("title: \""));
                Assert.That(homePageLines.ElementAtOrDefault(2), Is.EqualTo("---"));
                Assert.That(homePageLines, Does.Contain("Overview content."));
            }

            // A JSON string is a valid YAML double-quoted scalar, so decoding it as JSON yields the YAML value.
            var title = JsonSerializer.Deserialize<string>(homePageLines[1]["title: ".Length..]);
            Assert.That(title, Is.EqualTo("Project \"Overview\": Start"));
        }

        [TestCase("gitLab", "> [!note]")]
        [TestCase("devOps", "> **Note** \\")]
        public void Build_MarkdownConventionWithGitHubAlert_WritesAlertForWiki(string convention, string expectedDirective)
        {
            File.WriteAllText(Path.Combine(testDirectory, "docs", "guides", "setup.md"), "# Setup\n\n> [!NOTE]\n> Alert content.");

            var configPath = Path.Combine(testDirectory, "kampose.json");
            File.WriteAllText(configPath, $$"""
                {
                  "topics": ["docs/**/*.md"],
                  "outputDirectory": "output",
                  "convention": "{{convention}}",
                  "audit": { "stopOnIssues": true }
                }
                """);

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            var exitCode = new BuildCommand().Execute([configPath], reporter);

            var setupPageLines = Directory.EnumerateFiles(Path.Combine(testDirectory, "output"), "Setup.md", SearchOption.AllDirectories)
                .Select(File.ReadAllLines)
                .SingleOrDefault() ?? [];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(setupPageLines, Does.Contain(expectedDirective));
                Assert.That(setupPageLines, Does.Contain("> Alert content."));
                Assert.That(setupPageLines, Does.Not.Contain("> [!NOTE]"));
            }
        }

        [TestCase(DocConvention.DotNet, typeof(DotNetApiStrategy))]
        [TestCase(DocConvention.DocFx, typeof(DocFxStrategy))]
        [TestCase(DocConvention.DevOps, typeof(DevOpsWikiStrategy))]
        [TestCase(DocConvention.GitLab, typeof(GitLabWikiStrategy))]
        public void Configure_WithConvention_UsesMatchingAddressingStrategy(DocConvention convention, Type expectedStrategyType)
        {
            var config = CreateConfiguration();
            config.Convention = convention;

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            var builder = new DocContextBuilder(reporter).Configure(config, Theme.Load("classic", config.Convention));

            Assert.That(builder.Strategy, Is.TypeOf(expectedStrategyType));
        }

        [TestCase(DocReferenceStrategy.DevOps, "https://docs.example.com/System.Collections.Generic/List%3CT%3E.md")]
        [TestCase(DocReferenceStrategy.GitLab, "https://docs.example.com/System.Collections.Generic/List{T}.md")]
        public void Configure_WithWikiReferenceStrategy_ResolvesExternalTypeUrlWithWikiPageNames(DocReferenceStrategy strategy, string expectedUrl)
        {
            var config = CreateConfiguration();
            config.References.Add(new DocReference
            {
                Strategy = strategy,
                Url = new Uri("https://docs.example.com/"),
                Namespaces = { "System.Collections.Generic" }
            });

            using var reporter = new TextWriterActivityReporter(TextWriter.Null);
            var builder = new DocContextBuilder(reporter).Configure(config, Theme.Load("classic", config.Convention));

            var resolver = builder.ExternalReferences.OfType<RemoteApiDocUrlResolver>().Single();
            var resolved = resolver.TryGetMemberUrl(typeof(List<>).GetMetadata(), out var url);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(resolved, Is.True);
                Assert.That(url, Is.EqualTo(new Uri(expectedUrl)));
            }
        }

        private Configuration CreateConfiguration()
        {
            var config = new Configuration
            {
                BaseDirectory = testDirectory,
                OutputDirectory = Path.Combine(testDirectory, "output")
            };
            config.Topics.Clear();
            config.Topics.Add("docs/**/*.md");
            return config;
        }
    }
}
