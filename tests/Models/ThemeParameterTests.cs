// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Models
{
    using Kampose.Models;
    using Kampose.Support;
    using NUnit.Framework;
    using System.Text.Json;

    [TestFixture]
    public class ThemeParameterTests
    {
        [TestCase(@"{""type"": ""string"", ""description"": ""Test description"", ""defaultValue"": ""Test value""}")]
        [TestCase(@"{""type"": ""markdown"", ""defaultValue"": ""# Test Heading""}")]
        [TestCase(@"{""type"": ""number"", ""defaultValue"": 123}")]
        [TestCase(@"{""type"": ""boolean"", ""defaultValue"": true}")]
        [TestCase(@"{""type"": ""uri"", ""defaultValue"": ""https://example.com/""}")]
        [TestCase(@"{""type"": ""array"", ""defaultValue"": [""item1"", ""item2""]}")]
        [TestCase(@"{""type"": ""object"", ""defaultValue"": {""key1"": ""value1"", ""key2"": 123}}")]
        public void JsonConstructor_WithValidJson_CreatesThemeParameter(string json)
        {
            Assert.That(() => Json.Parse<ThemeParameter>(json), Throws.Nothing);
        }

        [TestCase(@"{""type"": ""string"", ""defaultValue"": 999}", "String was expected")]
        [TestCase(@"{""type"": ""markdown"", ""defaultValue"": 999}", "Markdown was expected")]
        [TestCase(@"{""type"": ""number"", ""defaultValue"": ""not a number""}", "Number was expected")]
        [TestCase(@"{""type"": ""boolean"", ""defaultValue"": ""not a boolean""}", "Boolean was expected")]
        [TestCase(@"{""type"": ""uri"", ""defaultValue"": ""http://[invalid]""}", "A valid URI was expected")]
        [TestCase(@"{""type"": ""array"", ""defaultValue"": ""not an array""}", "Array was expected")]
        [TestCase(@"{""type"": ""object"", ""defaultValue"": ""not an object""}", "Object was expected")]
        public void JsonConstructor_WithInvalidDefaultValue_ThrowsJsonException(string json, string expectedErrorMessage)
        {
            Assert.That(() => Json.Parse<ThemeParameter>(json), Throws.TypeOf<JsonException>().With.Message.Contains(expectedErrorMessage));
        }
    }
}
