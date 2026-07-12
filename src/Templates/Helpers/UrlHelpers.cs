// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates.Helpers
{
    using HandlebarsDotNet;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Support;
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
        /// <param name="docContext">The documentation context used for resolving documentation root URL.</param>
        /// <returns>An absolute or document-relative URI pointing to the documentation root.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static Uri RootUrl(IDocumentationContext docContext)
        {
            return docContext.AddressProvider.ActiveScope.DocumentationRootUrl;
        }

        /// <summary>
        /// Converts a path relative to the documentation root to an absolute or current-page-relative URL.
        /// </summary>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <param name="docContext">The documentation context used for resolving URLs and encoding.</param>
        /// <returns>
        /// The transformed URL, or the original argument when it is empty or cannot be resolved within the documentation root.
        /// </returns>
        /// <remarks>
        /// The argument does not require the <c>~/</c> documentation-root marker. Query strings and fragments attached to a path
        /// are preserved; query-only and fragment-only references remain relative to the current document. The helper does not
        /// change the active document URL context.
        /// </remarks>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object? RootRelativeUrl(Arguments arguments, IDocumentationContext docContext)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(RootRelativeUrl)} template helper function requires one argument.");

            var href = arguments[0]?.ToString();
            if (string.IsNullOrWhiteSpace(href))
                return null;

            if (UriHelper.IsQueryOrFragmentOnly(href) || UriHelper.IsAbsoluteOrRooted(href))
                return arguments[0];

            if (!href.StartsWith("~/", StringComparison.Ordinal))
                href = "~/" + href;

            return docContext.AddressProvider.ActiveScope.TryResolveUrl(href, out var resolvedUrl)
                ? resolvedUrl
                : arguments[0];
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
