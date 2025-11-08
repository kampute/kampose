// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates
{
    using HandlebarsDotNet;
    using Kampose.Templates.Helpers;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.IO.Writers;
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides helper methods for Handlebars templates.
    /// </summary>
    public static class TemplateHelpers
    {
        /// <summary>
        /// Registers all helper methods with the specified Handlebars environment.
        /// </summary>
        /// <param name="handlebars">The Handlebars environment to register the helpers with.</param>
        /// <param name="documentationContext">The documentation context used for resolving URLs and encoding.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlebars"/> or <paramref name="documentationContext"/> is <see langword="null"/>.</exception>
        public static void Register(IHandlebars handlebars, IDocumentationContext documentationContext)
        {
            ArgumentNullException.ThrowIfNull(handlebars);
            ArgumentNullException.ThrowIfNull(documentationContext);

            MemberHelpers.Register(handlebars, documentationContext);
            UrlHelpers.Register(handlebars, documentationContext);
            StringHelpers.Register(handlebars);
            LogicalHelpers.Register(handlebars);
            ArithmeticHelpers.Register(handlebars);
            UtilityHelpers.Register(handlebars, documentationContext);
        }

        /// <summary>
        /// Determines whether a value is considered truthy in Handlebars templates.
        /// </summary>
        /// <param name="value">The value to evaluate.</param>
        /// <returns><see langword="true"/> if the value is truthy; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTruthy(object? value) => value switch
        {
            null => false,
            bool b => b,
            string s => !string.IsNullOrEmpty(s),
            int i => i != 0,
            double d => d != 0d && !double.IsNaN(d),
            float f => f != 0f && !float.IsNaN(f),
            decimal dec => dec != 0m,
            ICollection c => c.Count > 0,
            IEnumerable e => e.Cast<object>().Any(),
            UndefinedBindingResult _ => false,
            _ => true
        };

        /// <summary>
        /// Determines whether a value is considered falsy in Handlebars templates.
        /// </summary>
        /// <param name="value">The value to evaluate.</param>
        /// <returns><see langword="true"/> if the value is falsy; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFalsy(object? value) => !IsTruthy(value);

        /// <summary>
        /// Creates a <see cref="TextWriter"/> wrapper around the specified <see cref="EncodedTextWriter"/> with text encoding suppressed.
        /// </summary>
        /// <param name="writer">The encoded text writer to write to.</param>
        /// <returns>A new instance of <see cref="TextWriter"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextWriter CreateSuppressedWrapper(this in EncodedTextWriter writer) => writer.SuppressEncoding
            ? writer.CreateWrapper()
            : new TextEncoderSuppressorWriter(writer);

        /// <summary>
        /// Creates a <see cref="MarkupWriter"/> wrapper around the specified <see cref="EncodedTextWriter"/>.
        /// </summary>
        /// <param name="writer">The encoded text writer to write to.</param>
        /// <param name="docContext">The documentation context used for encoding and formatting.</param>
        /// <returns>A new instance of <see cref="MarkupWriter"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MarkupWriter CreateMarkupWrapper(this in EncodedTextWriter writer, IDocumentationContext docContext)
        {
            ArgumentNullException.ThrowIfNull(docContext);

            return docContext.ContentFormatter.CreateMarkupWriter(writer.CreateSuppressedWrapper(), disposeWriter: true);
        }
    }
}
