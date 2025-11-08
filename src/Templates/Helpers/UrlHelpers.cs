// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates.Helpers
{
    using HandlebarsDotNet;
    using Kampute.DocToolkit;
    using System;

    /// <summary>
    /// Provides Handlebars helper methods for URL operations.
    /// </summary>
    public static class UrlHelpers
    {
        /// <summary>
        /// Registers the URL helper methods with the specified Handlebars environment.
        /// </summary>
        /// <param name="handlebars">The Handlebars environment to register the helpers with.</param>
        /// <param name="documentationContext">The documentation context used for resolving URLs and encoding.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlebars"/> or <paramref name="documentationContext"/> is <see langword="null"/>.</exception>
        public static void Register(IHandlebars handlebars, IDocumentationContext documentationContext)
        {
            ArgumentNullException.ThrowIfNull(handlebars);
            ArgumentNullException.ThrowIfNull(documentationContext);

            handlebars.RegisterHelper(nameof(RootUrl), (context, arguments) => RootUrl(documentationContext));
            handlebars.RegisterHelper(nameof(RootRelativeUrl), (context, arguments) => RootRelativeUrl(arguments, documentationContext));
            handlebars.RegisterHelper(nameof(Fragment), Fragment);
        }

        /// <summary>
        /// Returns the URL to the documentation root relative to current page.
        /// </summary>
        /// <param name="docContext">The documentation context used for resolving root URL.</param>
        /// <returns>An absolute or document-relative URI representing the root URL of the documentation.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static Uri RootUrl(IDocumentationContext docContext)
        {
            return docContext.AddressProvider.ActiveScope.RootUrl;
        }

        /// <summary>
        /// Converts the specified site-root-relative URL to a document-relative URL.
        /// </summary>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <param name="docContext">The documentation context used for resolving URLs and encoding.</param>
        /// <returns>The transformed URL as a string, or the original argument if transformation fails.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object? RootRelativeUrl(Arguments arguments, IDocumentationContext docContext)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(RootRelativeUrl)} template helper function requires one argument.");

            var href = arguments[0]?.ToString();
            if (string.IsNullOrWhiteSpace(href))
                return null;

            if (docContext.UrlTransformer.TryTransformUrl(href.Trim(), out var url))
                return url;

            return arguments[0];
        }

        /// <summary>
        /// Extracts fragment identifier of a URL if present.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The fragment identifier as a string, or <see langword="null"/> if no fragment is present.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object? Fragment(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(Fragment)} template helper function requires one argument.");

            var href = arguments[0]?.ToString();
            if (string.IsNullOrEmpty(href))
                return null;

            var fragmentIndex = href.IndexOf('#');
            return fragmentIndex != -1 ? href[(fragmentIndex + 1)..] : null;
        }
    }
}
