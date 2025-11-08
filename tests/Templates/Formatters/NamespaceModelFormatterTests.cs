// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Templates.Formatters
{
    using HandlebarsDotNet.IO;
    using Kampose.Templates.Formatters;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Models;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class NamespaceModelFormatterTests : FormatterTester<MarkdownFormat>
    {
        protected override IFormatter Formatter => new NamespaceModelFormatter();

        [Test]
        public void Format_WritesCorrectOutput()
        {
            var namespaceModel = new NamespaceModel(DocContext, "Test.Namespace");

            var result = Format(namespaceModel);

            Assert.That(result, Is.EqualTo("[Test.Namespace](https://example.com/test.namespace)"));
        }

        [Test]
        public void Format_WithUnsupportedValueType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Format("Invalid value type"));
        }
    }
}