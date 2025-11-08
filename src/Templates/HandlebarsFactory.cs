// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates
{
    using HandlebarsDotNet;
    using Kampose.Templates.Providers;
    using Kampute.DocToolkit;
    using System;
    using System.Globalization;

    /// <summary>
    /// Provides factory methods for creating Handlebars environments and configurations based on a documentation context.
    /// </summary>
    public static class HandlebarsFactory
    {
        /// <summary>
        /// Creates a Handlebars shared environment based on the provided documentation context.
        /// </summary>
        /// <param name="documentationContext">The documentation context.</param>
        /// <returns>A configured Handlebars environment.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="documentationContext"/> is <see langword="null"/>.</exception>
        public static IHandlebars CreateEnvironment(IDocumentationContext documentationContext)
        {
            ArgumentNullException.ThrowIfNull(documentationContext);

            var cfg = CreateConfiguration(documentationContext);
            var handlebars = Handlebars.CreateSharedEnvironment(cfg);
            TemplateHelpers.Register(handlebars, documentationContext);
            return handlebars;
        }

        /// <summary>
        /// Creates a Handlebars configuration based on the provided documentation context.
        /// </summary>
        /// <param name="documentationContext">The documentation context.</param>
        /// <returns>A Handlebars configuration object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="documentationContext"/> is <see langword="null"/>.</exception>
        public static HandlebarsConfiguration CreateConfiguration(IDocumentationContext documentationContext)
        {
            ArgumentNullException.ThrowIfNull(documentationContext);

            var cfg = new HandlebarsConfiguration
            {
                FormatProvider = CultureInfo.InvariantCulture,
                TextEncoder = new TemplateTextEncoder(documentationContext.ContentFormatter),
            };

            cfg.ObjectDescriptorProviders.Add(new InterfaceDefaultObjectDescriptorProvider());
            cfg.FormatterProviders.Add(new FormatterProvider(documentationContext));

            return cfg;
        }
    }
}
