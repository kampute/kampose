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
    using Kampute.DocToolkit.Models;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class MemberModelFormatterTests : FormatterTester<MarkdownFormat>
    {
        protected override IFormatter Formatter => new MemberModelFormatter();

        [Test]
        public void Format_WritesCorrectOutput()
        {
            var member = typeof(MemberModelFormatterTests).GetMetadata();

            var AssemblyModel = new AssemblyModel(DocContext, member.Assembly);
            var memberModel = AssemblyModel.FindMember(member);

            var result = Format(memberModel);

            Assert.That(result, Is.EqualTo("[MemberModelFormatterTests](https://example.com/kampose.test.templates.formatters.membermodelformattertests)"));
        }

        [Test]
        public void Format_WithUnsupportedValueType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Format("Invalid value type"));
        }
    }
}