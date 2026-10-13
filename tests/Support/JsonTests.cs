// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Support
{
    using Kampose.Support;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Text.Json;

    [TestFixture]
    public class JsonTests
    {
        private string testDirectory = string.Empty;

        [SetUp]
        public void Setup()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), $"json-tests-{Guid.NewGuid()}");
            Directory.CreateDirectory(testDirectory);
        }

        [TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(testDirectory))
                Directory.Delete(testDirectory, true);
        }

        [Test]
        public void ReadFile_WithReadOnlyValidationErrors_ThrowsValidationException()
        {
            var filePath = Path.Combine(testDirectory, "document.json");
            File.WriteAllText(filePath, "{}");
            IReadOnlyDictionary<string, string> errors = new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>
                {
                    ["test"] = "Validation failed."
                }
            );

            var exception = Assert.Throws<ValidationException>(
                () => Json.ReadFile<JsonElement>(filePath, _ => errors)
            );

            Assert.That(exception!.Errors, Contains.Item("Validation failed."));
        }
    }
}
