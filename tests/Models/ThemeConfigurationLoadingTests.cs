// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Models
{
    using Kampose.Models;
    using Kampose.Support;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class ThemeConfigurationLoadingTests
    {
        private string testDirectory = string.Empty;

        [SetUp]
        public void Setup()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), $"theme-config-tests-{Guid.NewGuid()}");
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

        #region Valid ThemeConfiguration Loading

        [Test]
        public void LoadFromFile_WithMinimalValidThemeConfig_ReturnsThemeConfiguration()
        {
            var filePath = GenerateJsonFile("minimal-theme.json", @"{
                ""templates"": [""**/*.hbs""]
            }");

            var config = ThemeConfiguration.LoadFromFile(filePath);

            Assert.That(config, Is.Not.Null);
        }

        [Test]
        public void LoadFromFile_WithMetadata_LoadsMetadata()
        {
            var filePath = GenerateJsonFile("theme-with-metadata.json", @"{
                ""templates"": [""**/*.hbs""],
                ""metadata"": {
                    ""name"": ""Classic HTML Theme"",
                    ""version"": ""1.0.0"",
                    ""author"": ""Kampute"",
                    ""description"": ""A classic HTML documentation theme""
                }
            }");

            var config = ThemeConfiguration.LoadFromFile(filePath);

            Assert.That(config.Metadata, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.Metadata.Name, Is.EqualTo("Classic HTML Theme"));
                Assert.That(config.Metadata.Version, Is.EqualTo("1.0.0"));
            }
        }

        [Test]
        public void LoadFromFile_WithBaseTheme_LoadsBaseTheme()
        {
            var filePath = GenerateJsonFile("theme-with-base.json", @"{
                ""base"": ""classic"",
                ""metadata"": {
                    ""name"": ""Custom Theme""
                }
            }");

            var config = ThemeConfiguration.LoadFromFile(filePath);

            Assert.That(config.Base, Is.EqualTo("classic"));
        }

        [Test]
        public void LoadFromFile_WithParameters_LoadsThemeParameters()
        {
            var filePath = GenerateJsonFile("theme-with-params.json", @"{
                ""templates"": [""**/*.hbs""],
                ""parameters"": {
                    ""siteTitle"": {
                        ""type"": ""string"",
                        ""description"": ""The title of the site"",
                        ""defaultValue"": ""My Documentation""
                    },
                    ""logoSize"": {
                        ""type"": ""number"",
                        ""description"": ""Size of the logo in pixels"",
                        ""defaultValue"": 100
                    },
                    ""showLogo"": {
                        ""type"": ""boolean"",
                        ""description"": ""Whether to show the logo"",
                        ""defaultValue"": true
                    }
                }
            }");

            var config = ThemeConfiguration.LoadFromFile(filePath);

            Assert.That(config.Parameters, Has.Count.EqualTo(3));

            Assert.That(config.Parameters, Does.ContainKey("siteTitle"));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.Parameters["siteTitle"].Type, Is.EqualTo(ThemeParameterType.String));
                Assert.That(config.Parameters["siteTitle"].Description, Is.EqualTo("The title of the site"));
                Assert.That(config.Parameters["siteTitle"].DefaultValue, Is.EqualTo("My Documentation"));
            }

            Assert.That(config.Parameters, Does.ContainKey("logoSize"));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.Parameters["logoSize"].Type, Is.EqualTo(ThemeParameterType.Number));
                Assert.That(config.Parameters["logoSize"].Description, Is.EqualTo("Size of the logo in pixels"));
                Assert.That(config.Parameters["logoSize"].DefaultValue, Is.EqualTo(100));
            }

            Assert.That(config.Parameters, Does.ContainKey("showLogo"));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(config.Parameters["showLogo"].Type, Is.EqualTo(ThemeParameterType.Boolean));
                Assert.That(config.Parameters["showLogo"].Description, Is.EqualTo("Whether to show the logo"));
                Assert.That(config.Parameters["showLogo"].DefaultValue, Is.True);
            }
        }

        [Test]
        public void LoadFromFile_WithTemplates_LoadsTemplatePatterns()
        {
            var filePath = GenerateJsonFile("theme-with-templates.json", @"{
                ""templates"": [""**/*.hbs""]
            }");

            var config = ThemeConfiguration.LoadFromFile(filePath);

            Assert.That(config.Templates, Is.Not.Empty);
        }

        [Test]
        public void LoadFromFile_WithScripts_LoadsScriptFilter()
        {
            var filePath = GenerateJsonFile("theme-with-scripts.json", @"{
                ""templates"": [""**/*.hbs""],
                ""scripts"": {
                    ""source"": [""scripts/**/*.js""],
                    ""targetPath"": ""script.js""
                }
            }");

            var config = ThemeConfiguration.LoadFromFile(filePath);

            Assert.That(config.Scripts, Is.Not.Null);
            Assert.That(config.Scripts.TargetPath, Is.EqualTo("script.js"));
        }

        [Test]
        public void LoadFromFile_WithScriptsMissingTargetPath_LoadsDefaultTargetPath()
        {
            var filePath = GenerateJsonFile("theme-scripts-no-target.json", @"{
                ""templates"": [""**/*.hbs""],
                ""scripts"": {
                    ""source"": [""scripts/**/*.js""]
                }
            }");

            var config = ThemeConfiguration.LoadFromFile(filePath);

            Assert.That(config.Scripts, Is.Not.Null);
            Assert.That(config.Scripts.TargetPath, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void LoadFromFile_WithStyles_LoadsStylesFilter()
        {
            var filePath = GenerateJsonFile("theme-with-styles.json", @"{
                ""templates"": [""**/*.hbs""],
                ""styles"": {
                    ""source"": [""styles/**/*.css""],
                    ""targetPath"": ""styles.css""
                }
            }");

            var config = ThemeConfiguration.LoadFromFile(filePath);

            Assert.That(config.Styles, Is.Not.Null);
            Assert.That(config.Styles.TargetPath, Is.EqualTo("styles.css"));
        }

        [Test]
        public void LoadFromFile_WithStylesMissingTargetPath_LoadsDefaultTargetPath()
        {
            var filePath = GenerateJsonFile("theme-styles-no-target.json", @"{
                ""templates"": [""**/*.hbs""],
                ""styles"": {
                    ""source"": [""styles/**/*.css""]
                }
            }");

            var config = ThemeConfiguration.LoadFromFile(filePath);

            Assert.That(config.Styles, Is.Not.Null);
            Assert.That(config.Styles.TargetPath, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void LoadFromFile_WithAssets_LoadsAssetPatterns()
        {
            var filePath = GenerateJsonFile("theme-with-assets.json", @"{
                ""templates"": [""**/*.hbs""],
                ""assets"": [""images/**"", ""fonts/**""]
            }");

            var config = ThemeConfiguration.LoadFromFile(filePath);

            Assert.That(config.Assets, Has.Count.EqualTo(2));
        }

        #endregion

        #region Invalid ThemeConfiguration Loading

        [Test]
        public void LoadFromFile_WithNonexistentFile_ThrowsFileNotFoundException()
        {
            var filePath = Path.Combine(testDirectory, "nonexistent.json");

            Assert.That(
                () => ThemeConfiguration.LoadFromFile(filePath),
                Throws.TypeOf<FileNotFoundException>()
            );
        }

        [Test]
        public void LoadFromFile_WithInvalidJson_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("invalid.json", "{invalid json}");

            Assert.That(
                () => ThemeConfiguration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>()
            );
        }

        [Test]
        public void LoadFromFile_WithEmptyFile_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("empty.json", string.Empty);

            Assert.That(
                () => ThemeConfiguration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>()
            );
        }

        [Test]
        public void LoadFromFile_WithOnlyWhitespace_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("whitespace.json", "   \n\t  ");

            Assert.That(
                () => ThemeConfiguration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>()
            );
        }

        [Test]
        public void LoadFromFile_WithNullFile_ThrowsArgumentNullException()
        {
            Assert.That(
                () => ThemeConfiguration.LoadFromFile(null!),
                Throws.ArgumentNullException
            );
        }

        [Test]
        public void LoadFromFile_WithInvalidParameterType_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("invalid-param-type.json", @"{
                ""templates"": [""**/*.hbs""],
                ""parameters"": {
                    ""setting"": {
                        ""type"": ""invalidType"",
                        ""defaultValue"": ""value""
                    }
                }
            }");

            Assert.That(
                () => ThemeConfiguration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>()
            );
        }

        [Test]
        public void LoadFromFile_WithoutBothTemplatesAndBaseTheme_ThrowsValidationException()
        {
            var filePath = GenerateJsonFile("missing-templates-and-base.json", @"{
                ""metadata"": {
                    ""name"": ""Theme Without Templates or Base""
                }
            }");

            Assert.That(
                () => ThemeConfiguration.LoadFromFile(filePath),
                Throws.TypeOf<ValidationException>()
            );
        }

        #endregion
    }
}
