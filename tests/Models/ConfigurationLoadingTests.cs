// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Models
{
    using Kampose.Models;
    using Kampose.Support;
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class ConfigurationLoadingTests
    {
        private string testDirectory = string.Empty;

        [SetUp]
        public void Setup()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), $"config-tests-{Guid.NewGuid()}");
            Directory.CreateDirectory(testDirectory);
        }

        [TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(testDirectory))
                Directory.Delete(testDirectory, true);
        }

        private string GenerateJsonFile(string fileName, string jsonContent)
        {
            var filePath = Path.Combine(testDirectory, fileName);
            File.WriteAllText(filePath, jsonContent);
            return filePath;
        }

        #region Valid Configuration Loading

        [Test]
        public void LoadFromFile_WithMinimalValidConfig_ReturnsConfiguration()
        {
            var filePath = GenerateJsonFile("minimal.json", @"{
                ""outputDirectory"": ""./output""
            }");

            var config = Configuration.LoadFromFile(filePath);

            Assert.That(config, Is.Not.Null);
            Assert.That(config.OutputDirectory, Does.EndWith("output"));
        }

        [Test]
        public void LoadFromFile_WithFullConfig_ReturnsConfigurationWithAllProperties()
        {
            var filePath = GenerateJsonFile("full.json", @"{
                ""outputDirectory"": ""./docs"",
                ""convention"": ""dotnet"",
                ""theme"": ""custom"",
                ""baseUrl"": ""https://docs.example.com/"",
                ""assemblies"": [""bin/**/*.dll""],
                ""xmlDocs"": [""bin/**/*.xml""],
                ""topics"": [""docs/**/*.md""],
                ""topicOrder"": [""introduction.md"", ""getting-started.md""],
                ""topicHierarchy"": ""directory"",
                ""themeSettings"": {
                    ""siteTitle"": ""My Docs""
                },
                ""audit"": {
                    ""options"": [""recommended""],
                    ""includeImplicitConstructors"": true,
                    ""verifyExternalLinks"": true,
                    ""stopOnIssues"": true
                }
            }");

            var config = Configuration.LoadFromFile(filePath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.OutputDirectory, Does.EndWith("docs"));
                Assert.That(config.Convention, Is.EqualTo(DocConvention.DotNet));
                Assert.That(config.Theme, Is.EqualTo("custom"));
                Assert.That(config.BaseUrl?.ToString(), Is.EqualTo("https://docs.example.com/"));
                Assert.That(config.TopicOrder, Has.Count.EqualTo(2));
                Assert.That(config.TopicHierarchy, Is.EqualTo(FileTopicHierarchyMethod.Directory));
                Assert.That(config.ThemeSettings["siteTitle"]?.ToString(), Is.EqualTo("My Docs"));
                Assert.That(config.Audit.InspectionOptions, Is.EqualTo(XmlDocInspectionOptions.Recommended));
                Assert.That(config.Audit.IncludeImplicitConstructors, Is.True);
                Assert.That(config.Audit.VerifyExternalLinks, Is.True);
                Assert.That(config.Audit.StopOnIssues, Is.True);
            }
        }

        [Test]
        public void LoadFromFile_WithDefaultValues_ReturnsConfigurationWithDefaults()
        {
            var filePath = GenerateJsonFile("defaults.json", @"{
                ""outputDirectory"": ""./output"",
                ""assemblies"": [""bin/**/*.dll""]
            }");

            var config = Configuration.LoadFromFile(filePath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.Convention, Is.EqualTo(DocConvention.DocFx));
                Assert.That(config.Theme, Is.EqualTo("classic"));
                Assert.That(config.Topics, Is.Not.Empty); // Has default pattern
                Assert.That(config.Audit.Options, Is.Not.Empty); // Has default options
            }
        }

        [Test]
        public void LoadFromFile_WithEmptyTopics_ReturnsConfigurationWithNoTopics()
        {
            var filePath = GenerateJsonFile("empty-topics.json", @"{
                ""outputDirectory"": ""./output"",
                ""assemblies"": [""bin/**/*.dll""],
                ""topics"": []
            }");

            var config = Configuration.LoadFromFile(filePath);

            Assert.That(config.Topics, Is.Empty);
        }

        [Test]
        public void LoadFromFile_WithEmptyAuditOptions_ReturnsConfigurationWithNoAuditOptions()
        {
            var filePath = GenerateJsonFile("empty-audit-options.json", @"{
                ""outputDirectory"": ""./output"",
                ""assemblies"": [""bin/**/*.dll""],
                ""audit"": {
                    ""options"": []
                }
            }");

            var config = Configuration.LoadFromFile(filePath);

            Assert.That(config.Audit.InspectionOptions, Is.EqualTo(XmlDocInspectionOptions.OmitImplicitlyCreatedConstructors));
        }

        [Test]
        public void LoadFromFile_WithMultipleReferences_ReturnsConfigurationWithAllReferences()
        {
            var filePath = GenerateJsonFile("multiple-refs.json", @"{
                ""outputDirectory"": ""./output"",
                ""assemblies"": [""bin/**/*.dll""],
                ""references"": [
                    {
                        ""namespaces"": [""System""],
                        ""strategy"": ""dotNet"",
                        ""url"": ""https://docs.microsoft.com/""
                    },
                    {
                        ""namespaces"": [""Custom""],
                        ""strategy"": ""docFx"",
                        ""url"": ""https://custom.docs.com/""
                    }
                ]
            }");

            var config = Configuration.LoadFromFile(filePath);

            Assert.That(config.References, Has.Count.EqualTo(2));
        }

        #endregion

        #region Invalid Configuration Loading

        [Test]
        public void LoadFromFile_WithNonexistentFile_ThrowsFileNotFoundException()
        {
            var filePath = Path.Combine(testDirectory, "nonexistent.json");

            Assert.That(
                () => Configuration.LoadFromFile(filePath),
                Throws.TypeOf<FileNotFoundException>()
            );
        }

        [Test]
        public void LoadFromFile_WithInvalidJson_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("invalid.json", "{invalid json}");

            Assert.That(
                () => Configuration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>()
            );
        }

        [Test]
        public void LoadFromFile_WithEmptyFile_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("empty.json", string.Empty);

            Assert.That(
                () => Configuration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>()
            );
        }

        [Test]
        public void LoadFromFile_WithOnlyWhitespace_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("whitespace.json", "   \n\t  ");

            Assert.That(
                () => Configuration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>()
            );
        }

        [Test]
        public void LoadFromFile_WithNullFile_ThrowsArgumentNullException()
        {
            Assert.That(
                () => Configuration.LoadFromFile(null!),
                Throws.ArgumentNullException
            );
        }

        [Test]
        public void LoadFromFile_WithMissingRequiredFields_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("missing-required.json", @"{}");

            Assert.That(
                () => Configuration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>()
            );
        }

        [Test]
        public void LoadFromFile_WithInvalidConvention_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("invalid-convention.json", @"{
                ""outputDirectory"": ""./output"",
                ""convention"": ""invalid"",
                ""assemblies"": [""bin/**/*.dll""]
            }");

            Assert.That(
                () => Configuration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>()
            );
        }

        [Test]
        public void LoadFromFile_WithInvalidBaseUrl_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("invalid-base-url.json", @"{
                ""outputDirectory"": ""./output"",
                ""baseUrl"": ""not a valid url"",
                ""assemblies"": [""bin/**/*.dll""]
            }");

            Assert.That(
                () => Configuration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>()
            );
        }

        [Test]
        public void LoadFromFile_WithRelativeBaseUrl_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("relative-base-url.json", @"{
                ""outputDirectory"": ""./output"",
                ""baseUrl"": ""/relative/path"",
                ""assemblies"": [""bin/**/*.dll""]
            }");

            var exception = Assert.Throws<ValidationException>(() => Configuration.LoadFromFile(filePath));
            Assert.That(exception!.Message, Does.Contain("invalid"));
        }

        [Test]
        public void LoadFromFile_WithBothAssembliesAndTopicsEmpty_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("empty-assemblies-topics.json", @"{
                ""outputDirectory"": ""./output"",
                ""assemblies"": [],
                ""topics"": []
            }");

            Assert.That(
                () => Configuration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>().With.Message.Contains("assemblies")
            );
        }

        [Test]
        public void LoadFromFile_WithReferenceWithoutNamespaces_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("reference-without-namespaces.json", @"{
                ""outputDirectory"": ""./output"",
                ""assemblies"": [""bin/**/*.dll""],
                ""references"": [
                    {
                        ""namespaces"": [],
                        ""strategy"": ""dotNet"",
                        ""url"": ""https://docs.microsoft.com/""
                    }
                ]
            }");

            Assert.That(
                () => Configuration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>().With.Message.Contains("namespace")
            );
        }

        [Test]
        public void LoadFromFile_WithReferenceWithRelativeUrl_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("reference-relative-url.json", @"{
                ""outputDirectory"": ""./output"",
                ""assemblies"": [""bin/**/*.dll""],
                ""references"": [
                    {
                        ""namespaces"": [""System""],
                        ""strategy"": ""dotNet"",
                        ""url"": ""/relative/path""
                    }
                ]
            }");

            Assert.That(
                () => Configuration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>().With.Message.Contains("relative")
            );
        }

        [Test]
        public void LoadFromFile_WithEmptyOutputDirectory_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("empty-output-directory.json", @"{
                ""outputDirectory"": """",
                ""assemblies"": [""bin/**/*.dll""]
            }");

            Assert.That(
                () => Configuration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>().With.Message.Contains("output")
            );
        }

        [Test]
        public void LoadFromFile_WithWhitespaceOnlyOutputDirectory_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("whitespace-output-directory.json", @"{
                ""outputDirectory"": ""   "",
                ""assemblies"": [""bin/**/*.dll""]
            }");

            Assert.That(
                () => Configuration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>().With.Message.Contains("output")
            );
        }

        #endregion
    }
}
