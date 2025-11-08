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
    using System.Linq;
    using System.Reflection;

    [TestFixture]
    public class AttributeFormatterTests : FormatterTester<MarkdownFormat>
    {
        protected override IFormatter Formatter => new AttributeFormatter(DocContext);

        [Test]
        public void Format_WritesCorrectOutput()
        {
            var attributeData = MethodBase.GetCurrentMethod()!.GetMetadata().CustomAttributes.FirstOrDefault(a => a.Type.Name == nameof(TestAttribute));
            Assert.That(Format(attributeData), Is.EqualTo("[Test](https://example.com/nunit.framework.testattribute)"));
        }

        [Test]
        public void Format_WithUnsupportedValueType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Format("Invalid value type"));
        }
    }
}
