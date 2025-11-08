// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Templates.Formatters
{
    using HandlebarsDotNet.IO;
    using Kampose.Templates;
    using Kampose.Test;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using System.IO;

    public abstract class FormatterTester<TFormat> where TFormat : IDocumentFormatter, new()
    {
        protected readonly IDocumentationContext DocContext = MockHelper.CreateDocumentationContext<TFormat>();

        protected abstract IFormatter Formatter { get; }

        protected string Format<T>(T value)
        {
            using var writer = new StringWriter();
            var encoder = new TemplateTextEncoder(DocContext.ContentFormatter);
            using var encodedWriter = new HandlebarsDotNet.EncodedTextWriter(writer, encoder, null);

            Formatter.Format(value, encodedWriter);
            return writer.ToString();
        }
    }
}
