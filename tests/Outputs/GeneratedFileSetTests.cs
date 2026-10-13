// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Outputs
{
    using Kampose.Outputs;
    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class GeneratedFileSetTests
    {
        private readonly string outputDir = Path.Combine(Path.GetTempPath(), "generated-file-set-tests", "output");

        [Test]
        public void Contains_AbsoluteAndRelativeFormsOfAddedPath_ReturnsTrue()
        {
            var files = new GeneratedFileSet(outputDir);

            files.Add(Path.Combine(outputDir, "api", "page.html"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(files.Contains(Path.Combine(outputDir, "api", "page.html")), Is.True);
                Assert.That(files.Contains("api/page.html"), Is.True);
                Assert.That(files.Contains(Path.Combine("api", "page.html")), Is.True);
            }
        }

        [Test]
        public void Contains_PathDifferingOnlyInCase_ReturnsFalse()
        {
            var files = new GeneratedFileSet(outputDir);

            files.Add("api/page.html");

            Assert.That(files.Contains("api/Page.html"), Is.False);
        }

        [Test]
        public void Add_SamePathTwice_CountsOnce()
        {
            var files = new GeneratedFileSet(outputDir);

            files.Add("page.html");
            files.Add(Path.Combine(outputDir, "page.html"));

            Assert.That(files.Count, Is.EqualTo(1));
        }

        [TestCase("../outside.html")]
        [TestCase(".")]
        public void Add_PathNotBelowOutputDirectory_ThrowsArgumentException(string path)
        {
            var files = new GeneratedFileSet(outputDir);

            Assert.That(() => files.Add(path), Throws.ArgumentException);
        }
    }
}
