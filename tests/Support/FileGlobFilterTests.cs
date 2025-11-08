// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Support
{
    using Kampose.Support;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;

    [TestFixture]
    public class FileGlobFilterTests
    {
        private readonly string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        [SetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(tempDir);
            File.WriteAllText(Path.Combine(tempDir, "file1.txt"), "Text Content");
            File.WriteAllText(Path.Combine(tempDir, "file2.log"), "Log Content");
            File.WriteAllText(Path.Combine(tempDir, "pii.txt"), "PII Content");

            Directory.CreateDirectory(Path.Combine(tempDir, "sub"));
            File.WriteAllText(Path.Combine(tempDir, "sub", "file3.txt"), "Sub Text Content");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        [Test]
        public void FindMatchingFiles_WithMatchingFiles_ReturnsExpectedFiles()
        {
            var filter = new FileGlobFilter
            {
                "**/*.txt",
                "!**/pii.*"
            };
            var expectedFiles = new[]
            {
                Path.Combine(tempDir, "file1.txt"),
                Path.Combine(tempDir, "sub", "file3.txt")
            };

            var files = filter.FindMatchingFiles(tempDir);

            Assert.That(files, Is.EquivalentTo(expectedFiles).Using(new PathEqualityComparer()));
        }

        [Test]
        public void FindMatchingFiles_WithParentDirectoryPattern_ReturnsExpectedFiles()
        {
            var filter = new FileGlobFilter
            {
                "../**/*.txt",
                "!../**/pii.*"
            };
            var expectedFiles = new[]
            {
                Path.Combine(tempDir, "file1.txt"),
                Path.Combine(tempDir, "sub", "file3.txt")
            };

            var files = filter.FindMatchingFiles(Path.Combine(tempDir, "sub"));

            Assert.That(files, Is.EquivalentTo(expectedFiles).Using(new PathEqualityComparer()));
        }

        [Test]
        public void FindMatchingFiles_WithSubDirectoryPattern_ReturnsExpectedFiles()
        {
            var filter = new FileGlobFilter
            {
                "sub/*.txt",
                "!**/pii.*"
            };
            var expectedFiles = new[]
            {
                Path.Combine(tempDir, "sub", "file3.txt")
            };

            var files = filter.FindMatchingFiles(tempDir);

            Assert.That(files, Is.EquivalentTo(expectedFiles).Using(new PathEqualityComparer()));
        }

        [TestCase("test/", ".txt", ExpectedResult = "test/*.txt")]
        [TestCase("test/**", ".txt", ExpectedResult = "test/**/*.txt")]
        [TestCase("test/*", ".txt", ExpectedResult = "test/*.txt")]
        [TestCase("test.me/*", ".txt", ExpectedResult = "test.me/*.txt")]
        [TestCase("test", ".txt", ExpectedResult = "test.txt")]
        [TestCase("test.md", ".txt", ExpectedResult = "test.md")]
        [TestCase("subfolder/test", ".txt", ExpectedResult = "subfolder/test.txt")]
        [TestCase("subfolder/test.md", ".txt", ExpectedResult = "subfolder/test.md")]
        [TestCase("test.doc/file", ".txt", ExpectedResult = "test.doc/file.txt")]
        [TestCase("*", ".txt", ExpectedResult = "*.txt")]
        [TestCase("**/test", ".txt", ExpectedResult = "**/test.txt")]
        public string AddExtensionIfMissing_AppendsExtensionIfNeeded(string pattern, string extension)
        {
            return FileGlobFilter.AddExtensionIfMissing(pattern, extension);
        }

        private struct PathEqualityComparer : IEqualityComparer<string>
        {
            public readonly bool Equals(string? expected, string? actual) => Is.SamePath(expected!).ApplyTo(actual).IsSuccess;
            public readonly int GetHashCode(string obj) => obj.GetHashCode();
        }
    }
}
