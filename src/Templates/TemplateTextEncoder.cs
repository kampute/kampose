// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates
{
    using Kampute.DocToolkit.Formatters;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Encodes text for inclusion in a template.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TemplateTextEncoder"/> class.
    /// </remarks>
    /// <param name="encoder">The encoder used to encode the text.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="encoder"/> is <see langword="null"/>.</exception>
    public class TemplateTextEncoder(ITextEncoder encoder) : HandlebarsDotNet.ITextEncoder
    {
        private readonly ITextEncoder encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));

        /// <summary>
        /// Encodes the specified text for inclusion in a template.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <param name="target">The target <see cref="TextWriter"/> to which the encoded text will be written.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public void Encode(string text, TextWriter target)
        {
            ArgumentNullException.ThrowIfNull(text);
            ArgumentNullException.ThrowIfNull(target);

            encoder.Encode(text, target);
        }

        /// <summary>
        /// Encodes the specified text for inclusion in a template.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <param name="target">The target <see cref="TextWriter"/> to which the encoded text will be written.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public void Encode(StringBuilder text, TextWriter target)
        {
            ArgumentNullException.ThrowIfNull(text);
            ArgumentNullException.ThrowIfNull(target);

            foreach (var chunk in text.GetChunks())
                encoder.Encode(chunk.Span, target);
        }

        /// <summary>
        /// Encodes the specified text for inclusion in a template.
        /// </summary>
        /// <typeparam name="T">The type of text to encode.</typeparam>
        /// <param name="text">The text to encode.</param>
        /// <param name="target">The target <see cref="TextWriter"/> to which the encoded text will be written.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public void Encode<T>(T text, TextWriter target) where T : IEnumerator<char>
        {
            ArgumentNullException.ThrowIfNull(text);
            ArgumentNullException.ThrowIfNull(target);

            const int BufferSize = 1024;
            Span<char> buffer = stackalloc char[BufferSize];

            var size = 0;
            while (text.MoveNext())
            {
                buffer[size++] = text.Current;
                if (size == BufferSize)
                {
                    encoder.Encode(buffer, target);
                    size = 0;
                }
            }

            if (size > 0)
                encoder.Encode(buffer[..size], target);
        }
    }
}
