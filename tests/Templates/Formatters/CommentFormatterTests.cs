// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Templates.Formatters
{
    using HandlebarsDotNet.IO;
    using Kampose.Templates.Formatters;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.XmlDoc.Comments;
    using NUnit.Framework;
    using System;
    using System.Xml.Linq;

    [TestFixture]
    public class CommentFormatterTests : FormatterTester<MarkdownFormat>
    {
        protected override IFormatter Formatter => new CommentFormatter(DocContext);

        [TestCase("<summary>The <c>code</c> summary</summary>", ExpectedResult = "The `code` summary")]
        public string Format_XElement_WritesCorrectOutput(string xml)
        {
            var element = XElement.Parse(xml);
            return Format(element);
        }

        [TestCase("<summary>The <c>code</c> summary</summary>", ExpectedResult = "The `code` summary")]
        public string Format_Comment_WritesCorrectOutput(string xml)
        {
            var element = XElement.Parse(xml);
            var comment = Comment.Create(element);
            return Format(comment);
        }

        [Test]
        public void Format_WithUnsupportedValueType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Format("Invalid value type"));
        }
    }
}
