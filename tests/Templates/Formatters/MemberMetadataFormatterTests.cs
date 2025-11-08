// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Templates.Formatters
{
    using HandlebarsDotNet.IO;
    using Kampose.Templates.Formatters;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class MemberMetadataFormatterTests : FormatterTester<MarkdownFormat>
    {
        protected override IFormatter Formatter => new MemberMetadataFormatter(DocContext);

        [TestCase(typeof(Range), ExpectedResult = @"[Range](https://example.com/system.range)")]
        [TestCase(typeof(Range[]), ExpectedResult = @"[Range](https://example.com/system.range)\[\]")]
        [TestCase(typeof(Range[,]), ExpectedResult = @"[Range](https://example.com/system.range)\[,\]")]
        [TestCase(typeof(Range[,][]), ExpectedResult = @"[Range](https://example.com/system.range)\[,\]\[\]")]
        [TestCase(typeof(Range[][,]), ExpectedResult = @"[Range](https://example.com/system.range)\[\]\[,\]")]
        [TestCase(typeof(Range*), ExpectedResult = @"[Range](https://example.com/system.range)\*")]
        [TestCase(typeof(Range**), ExpectedResult = @"[Range](https://example.com/system.range)\*\*")]
        [TestCase(typeof(Range?), ExpectedResult = @"[Range](https://example.com/system.range)?")]
        [TestCase(typeof(Range?[]), ExpectedResult = @"[Range](https://example.com/system.range)?\[\]")]
        [TestCase(typeof(Range*[]), ExpectedResult = @"[Range](https://example.com/system.range)\*\[\]")]
        [TestCase(typeof(Range?*[][,]), ExpectedResult = @"[Range](https://example.com/system.range)?\*\[\]\[,\]")]
        [TestCase(typeof(Range?*[,,][]), ExpectedResult = @"[Range](https://example.com/system.range)?\*\[,,\]\[\]")]
        [TestCase(typeof(Lazy<Range?*[,,][]>), ExpectedResult = @"[Lazy](https://example.com/system.lazy-1)\<[Range](https://example.com/system.range)?\*\[,,\]\[\]>")]
        public string Format_WritesCorrectOutput(Type type)
        {
            return Format(type.GetMetadata());
        }

        [Test]
        public void Format_WithUnsupportedValueType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Format("Invalid value type"));
        }
    }
}
