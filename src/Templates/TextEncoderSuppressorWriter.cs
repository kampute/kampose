// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates
{
    using HandlebarsDotNet;
    using Kampute.DocToolkit.IO.Writers;
    using System;
    using System.IO;

    /// <summary>
    /// A <see cref="TextWriter"/> that suppresses text encoding for an <see cref="EncodedTextWriter"/> while it is in use.
    /// </summary>
    public sealed class TextEncoderSuppressorWriter : WrappedTextWriter
    {
        private readonly EncodedTextWriter encodedWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEncoderSuppressorWriter"/> class.
        /// </summary>
        /// <param name="writer">The <see cref="EncodedTextWriter"/> to suppress encoding for.</param>
        /// <exception cref="InvalidOperationException">Thrown if the writer is already suppressing encoding.</exception>
        public TextEncoderSuppressorWriter(in EncodedTextWriter writer)
            : base(CreateSuppressedWriter(writer), leaveOpen: false)
        {
            encodedWriter = writer;
        }

        /// <summary>
        /// Disposes the writer and restores encoding.
        /// </summary>
        /// <param name="disposing">A value indicating whether the method is called from <see cref="IDisposable.Dispose()"/>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                encodedWriter.SuppressEncoding = false;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates a <see cref="TextWriter"/> with encoding suppressed.
        /// </summary>
        /// <param name="writer">The encoded text writer.</param>
        /// <returns>A new instance of <see cref="TextWriter"/> with encoding suppressed.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the writer is already suppressing encoding.</exception>
        private static TextWriter CreateSuppressedWriter(in EncodedTextWriter writer)
        {
            if (writer.SuppressEncoding)
                throw new InvalidOperationException("The writer is already suppressing encoding.");

            writer.SuppressEncoding = true;
            return writer.CreateWrapper();
        }
    }
}
