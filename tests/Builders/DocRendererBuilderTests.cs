// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Builders
{
    using Kampose.Builders;
    using Kampose.Models;
    using Kampose.Reporters;
    using Kampose.Support;
    using Kampose.Templates;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.XmlDoc;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class DocRendererBuilderTests
    {
        private string testDirectory = null!;

        [SetUp]
        public void SetUp()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), $"renderer-builder-tests-{Guid.NewGuid()}");
            Directory.CreateDirectory(testDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(testDirectory))
                Directory.Delete(testDirectory, true);
        }

        [Test]
        [TestCase(DocConvention.DotNet, "dotNet")]
        [TestCase(DocConvention.DocFx, "docFx")]
        [TestCase(DocConvention.DevOps, "devOps")]
        public void Build_ConfiguredConvention_ExposesCanonicalNameToJavaScript(DocConvention convention, string expected)
        {
            using var context = CreateContext<HtmlFormat>();
            using var warningWriter = new StringWriter();
            using var reporter = new TextWriterActivityReporter(TextWriter.Null, warningWriter);
            var renderer = BuildRenderer(context, convention, new Dictionary<string, object?>
            {
                ["convention"] = "theme-setting-must-not-override"
            }, reporter);

            var serializedConfig = Json.Stringify(renderer.CommonData);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(serializedConfig, Does.Contain($"\"convention\": \"{expected}\""));
                Assert.That(reporter.WarningCount, Is.EqualTo(1));
                Assert.That(warningWriter.ToString(), Does.Contain("Theme setting 'convention' conflicts with a built-in global value and was ignored."));
            }
        }

        [Test]
        public void Build_DeclaredThemeParameterConflictsWithGlobalValue_IgnoresParameterAndWarns()
        {
            var themeName = CreateThemeWithConflictingParameter();
            using var context = CreateContext<HtmlFormat>();
            using var warningWriter = new StringWriter();
            using var reporter = new TextWriterActivityReporter(TextWriter.Null, warningWriter);
            var theme = Theme.Load(themeName, DocConvention.DotNet);

            var renderer = new DocRendererBuilder(reporter)
                .Build(context, CreateConfiguration(DocConvention.DotNet), theme);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(renderer.CommonData["generator"], Is.EqualTo($"Kampose v{Program.Version}"));
                Assert.That(reporter.WarningCount, Is.EqualTo(1));
                Assert.That(warningWriter.ToString(), Does.Contain("Theme setting 'generator' conflicts with a built-in global value and was ignored."));
            }
        }

        [Test]
        public void Build_ThemeSettingConflictsWithAbsentOptionalGlobal_IgnoresSettingAndWarns()
        {
            using var context = CreateContext<HtmlFormat>();
            using var reporter = new TextWriterActivityReporter(TextWriter.Null);

            var renderer = BuildRenderer(context, DocConvention.DotNet, new Dictionary<string, object?>
            {
                ["HomePageTitle"] = "Theme home"
            }, reporter);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(renderer.CommonData, Does.Not.ContainKey("homePageTitle"));
                Assert.That(reporter.WarningCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void Build_MarkdownSetting_PreservesPageRelativeUrlsInEachPageScope()
        {
            using var context = CreateContext<HtmlFormat>();
            var renderer = BuildRenderer(context, DocConvention.DotNet, new Dictionary<string, object?>
            {
                ["pageFooter"] = "[License](LICENSE) for {{projectName}}.",
                ["projectName"] = "Kampose"
            });

            Assert.That(renderer.CommonData["pageFooter"], Is.EqualTo("[License](LICENSE) for {{projectName}}."));
            Assert.That(context.UrlReferences, Is.Empty);

            AddPartialHostTemplate(renderer);
            var rootOutput = Render(renderer, context, string.Empty);
            var nestedOutput = Render(renderer, context, "guide");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(rootOutput, Is.EqualTo("<p><a href=\"LICENSE\">License</a> for Kampose.</p>\n"));
                Assert.That(nestedOutput, Is.EqualTo("<p><a href=\"LICENSE\">License</a> for Kampose.</p>\n"));
                Assert.That(context.UrlReferences, Has.Count.EqualTo(2));
            }

            File.WriteAllText(Path.Combine(testDirectory, "LICENSE"), "license");
            using var verifier = new UrlVerifier(context, testDirectory, null);
            var rootReference = context.UrlReferences.Single(reference => reference.BaseDirectory.Length == 0);
            var nestedReference = context.UrlReferences.Single(reference => reference.BaseDirectory == "guide");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(verifier.VerifyUrl(rootReference), Is.EqualTo(UrlVerifier.VerificationResult.OK));
                Assert.That(verifier.VerifyUrl(nestedReference), Is.EqualTo(UrlVerifier.VerificationResult.Unreachable));
            }
        }

        [Test]
        public void Build_DocumentationRootRelativeUrlInMarkdown_ResolvesFromEveryPageScope()
        {
            using var context = CreateContext<HtmlFormat>();
            var renderer = BuildRenderer(context, DocConvention.DotNet, new Dictionary<string, object?>
            {
                ["pageFooter"] = "[License](~/LICENSE)"
            });

            AddPartialHostTemplate(renderer);
            Render(renderer, context, string.Empty);
            Render(renderer, context, "guide");

            File.WriteAllText(Path.Combine(testDirectory, "LICENSE"), "license");
            using var verifier = new UrlVerifier(context, testDirectory, null);

            Assert.That(context.UrlReferences, Is.Not.Empty);
            Assert.That
            (
                context.UrlReferences.All(reference => verifier.VerifyUrl(reference) is UrlVerifier.VerificationResult.OK),
                Is.True,
                string.Join(Environment.NewLine, context.UrlReferences.Select(reference => $"{reference.BaseDirectory}: {reference.SourceUrl} -> {reference.TargetUrl}"))
            );
        }

        [Test]
        public void Build_MarkdownOutput_RendersMarkdownAndCollectsScopedUrls()
        {
            using var context = CreateContext<MarkdownFormat>();
            var renderer = BuildRenderer(context, DocConvention.DevOps, new Dictionary<string, object?>
            {
                ["pageFooter"] = "**{{projectName}}** [License](LICENSE)",
                ["projectName"] = "Kampose"
            });

            AddPartialHostTemplate(renderer);
            var output = Render(renderer, context, "guide");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(output, Is.EqualTo("**Kampose** [License](LICENSE)"));
                Assert.That(context.UrlReferences, Has.Count.EqualTo(1));
                Assert.That(context.UrlReferences.Single().BaseDirectory, Is.EqualTo("guide"));
            }
        }

        [Test]
        public void Build_DefaultMarkdownSetting_RegistersPartialAndConfiguredValueOverridesIt()
        {
            var themeName = CreateThemeWithMarkdownDefault();
            using var context = CreateContext<HtmlFormat>();
            var theme = Theme.Load(themeName, DocConvention.DotNet);
            var builder = new DocRendererBuilder(new TextWriterActivityReporter(TextWriter.Null));

            var defaultRenderer = builder.Build(context, CreateConfiguration(DocConvention.DotNet), theme);
            AddPartialHostTemplate(defaultRenderer, "notice_partial");
            var defaultOutput = Render(defaultRenderer, context, string.Empty);

            var configuredRenderer = builder.Build(context, CreateConfiguration(DocConvention.DotNet, new Dictionary<string, object?>
            {
                ["notice"] = "*Configured*"
            }), theme);
            AddPartialHostTemplate(configuredRenderer, "notice_partial");
            var configuredOutput = Render(configuredRenderer, context, string.Empty);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(defaultRenderer.CommonData["notice"], Is.EqualTo("**Default**"));
                Assert.That(defaultOutput, Is.EqualTo("<p><strong>Default</strong></p>\n"));
                Assert.That(configuredRenderer.CommonData["notice"], Is.EqualTo("*Configured*"));
                Assert.That(configuredOutput, Is.EqualTo("<p><em>Configured</em></p>\n"));
            }
        }

        private static TemplateRenderer BuildRenderer(
            DocContext context,
            DocConvention convention,
            IReadOnlyDictionary<string, object?> settings,
            IActivityReporter? reporter = null)
        {
            var theme = Theme.Load("classic", convention);
            return new DocRendererBuilder(reporter ?? new TextWriterActivityReporter(TextWriter.Null))
                .Build(context, CreateConfiguration(convention, settings), theme);
        }

        private static Configuration CreateConfiguration(
            DocConvention convention,
            IReadOnlyDictionary<string, object?>? settings = null)
        {
            return new Configuration
            {
                OutputDirectory = "output",
                Convention = convention,
                ThemeSettings = settings is null ? [] : new Dictionary<string, object?>(settings)
            };
        }

        private static DocContext CreateContext<TFormat>()
            where TFormat : IDocumentFormatter, new()
        {
            var xmlDocProvider = new XmlDocProvider(MockHelper.CreateXmlDocResolver());
            return new DocContext(new CSharp(), MockHelper.CreateAddressProvider(), xmlDocProvider, new TFormat(), [], [], []);
        }

        private void AddPartialHostTemplate(TemplateRenderer renderer, string partialName = "pageFooter_partial")
        {
            var templatePath = Path.Combine(testDirectory, $"{Guid.NewGuid()}.hbs");
            File.WriteAllText(templatePath, $"{{{{{{>{partialName}}}}}}}");
            renderer.AddTemplate("partial_host", templatePath);
        }

        private static string Render(TemplateRenderer renderer, DocContext context, string directory)
        {
            using var scope = context.AddressProvider.BeginScope(directory, Mock.Of<IDocumentModel>());
            using var writer = new StringWriter();
            renderer.RenderTemplate(writer, "partial_host", renderer.CommonData);
            return writer.ToString();
        }

        private string CreateThemeWithMarkdownDefault()
        {
            var customThemeDirectory = Path.Combine(testDirectory, "themes", "html", $"test-{Guid.NewGuid()}");
            Directory.CreateDirectory(customThemeDirectory);
            File.WriteAllText
            (
                Path.Combine(customThemeDirectory, "theme.json"),
                """
                {
                  "templates": ["**/*.hbs"],
                  "parameters": {
                    "notice": {
                      "type": "markdown",
                      "defaultValue": "**Default**"
                    }
                  }
                }
                """
            );
            return customThemeDirectory;
        }

        private string CreateThemeWithConflictingParameter()
        {
            var customThemeDirectory = Path.Combine(testDirectory, "themes", "html", $"test-{Guid.NewGuid()}");
            Directory.CreateDirectory(customThemeDirectory);
            File.WriteAllText
            (
                Path.Combine(customThemeDirectory, "theme.json"),
                """
                {
                  "templates": ["**/*.hbs"],
                  "parameters": {
                    "generator": {
                      "type": "string",
                      "defaultValue": "Theme generator"
                    }
                  }
                }
                """
            );
            return customThemeDirectory;
        }
    }
}
