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
    using Kampute.DocToolkit.Topics;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class TopicModelFormatterTests : FormatterTester<MarkdownFormat>
    {
        protected override IFormatter Formatter => new TopicModelFormatter();

        [Test]
        public void Format_TopicModel_WritesCorrectOutput()
        {
            var topicModel = new TopicModel(DocContext, new AdHocTopic("test", (w, c) => w.Write("Test Topic Content")));

            var result = Format(topicModel);

            Assert.That(result, Is.EqualTo("Test Topic Content"));
        }

        [Test]
        public void Format_WithUnsupportedValueType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Format("Invalid value type"));
        }
    }
}