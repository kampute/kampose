// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Support
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using System;
    using System.IO;
    using System.Net.Http;

    /// <summary>
    /// Verifies the validity of URLs referenced in the documentation.
    /// </summary>
    public sealed class UrlVerifier : IDisposable
    {
        private HttpClient? httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlVerifier"/> class.
        /// </summary>
        /// <param name="context">The documentation context.</param>
        /// <param name="baseDir">The documentation output directory.</param>
        /// <param name="baseUri">The base URI of the documentation site, or <see langword="null"/> if URLs are relative.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="baseDir"/> is <see langword="null"/>.</exception>
        public UrlVerifier(IDocumentationContext context, string baseDir, Uri? baseUri)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            BaseDir = baseDir ?? throw new ArgumentNullException(nameof(baseDir));
            BaseUri = baseUri;
        }

        /// <summary>
        /// Gets the documentation output directory.
        /// </summary>
        /// <value>The documentation output directory.</value>
        public string BaseDir { get; }

        /// <summary>
        /// Gets the base URI of the documentation site.
        /// </summary>
        /// <value>The base URI of the documentation site, or <see langword="null"/> URLs are relative.</value>
        public Uri? BaseUri { get; }

        /// <summary>
        /// Gets the documentation context.
        /// </summary>
        /// <value>The documentation context.</value>
        public IDocumentationContext Context { get; }

        /// <summary>
        /// Disposes the resources used by the <see cref="UrlVerifier"/> instance.
        /// </summary>
        public void Dispose()
        {
            httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Verifies the specified URL reference.
        /// </summary>
        /// <param name="urlReference">The URL reference to verify.</param>
        /// <param name="verifyExternalLinks">If set to <see langword="true"/>, external links will be checked for reachability.</param>
        /// <returns>The result of the verification.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="urlReference"/> is <see langword="null"/>.</exception>
        public VerificationResult VerifyUrl(UrlReference urlReference, bool verifyExternalLinks = false)
        {
            ArgumentNullException.ThrowIfNull(urlReference);

            if (UriHelper.IsQueryOrFragmentOnly(urlReference.SourceUrl))
                return VerificationResult.OK;

            if (urlReference.TargetUrl is not null)
                return IsFilePresent(urlReference) ? VerificationResult.OK : VerificationResult.Unreachable;

            if (!Uri.TryCreate(urlReference.SourceUrl, UriKind.RelativeOrAbsolute, out var uri))
                return VerificationResult.Malformed;

            if (!uri.IsAbsoluteUri)
                return VerificationResult.Unresolved;

            if (verifyExternalLinks && !IsUrlReachable(uri))
                return VerificationResult.Unreachable;

            return VerificationResult.OK;
        }

        /// <summary>
        /// Checks if the file referenced by the URL is present in the documentation directory.
        /// </summary>
        /// <param name="urlReference">The URL reference.</param>
        /// <returns><see langword="true"/> if the file is present; otherwise, <see langword="false"/>.</returns>
        private bool IsFilePresent(UrlReference urlReference)
        {
            if (urlReference.TargetUrl is null || urlReference.TargetUrl.IsAbsoluteUri != (BaseUri is not null))
                return false;

            var relativeUrlString = BaseUri is not null
                ? BaseUri.MakeRelativeUri(urlReference.TargetUrl).ToString()
                : Path.Combine(urlReference.BaseDirectory, urlReference.TargetUrl.ToString());

            var relativeFilePath = UriHelper.GetPathPart(relativeUrlString);
            var absoluteFilePath = Path.Combine(BaseDir, relativeFilePath);

            if (File.Exists(absoluteFilePath))
                return true;

            return Path.GetExtension(absoluteFilePath) != Context.ContentFormatter.FileExtension
                && File.Exists(absoluteFilePath + Context.ContentFormatter.FileExtension);
        }

        /// <summary>
        /// Checks if the specified URL is reachable.
        /// </summary>
        /// <param name="uri">The URI to check.</param>
        /// <returns><see langword="true"/> if the URL is reachable; otherwise, <see langword="false"/>.</returns>
        private bool IsUrlReachable(Uri uri)
        {
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return true; // Non-HTTP/HTTPS URIs are considered reachable.

            httpClient ??= CreateHttpClient();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, uri);
                using var response = httpClient.Send(request);
                return response.StatusCode != System.Net.HttpStatusCode.NotFound;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="HttpClient"/> for URL reachability checks.
        /// </summary>
        /// <returns>A new instance of <see cref="HttpClient"/>.</returns>
        private static HttpClient CreateHttpClient()
        {
            var messageHandler = new HttpClientHandler
            {
                UseCookies = false,
                AllowAutoRedirect = true,
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            };

            return new HttpClient(messageHandler, disposeHandler: true)
            {
                Timeout = TimeSpan.FromSeconds(3),
            };
        }

        /// <summary>
        /// Represents the result of a URL verification.
        /// </summary>
        public enum VerificationResult
        {
            /// <summary>
            /// The URL is valid and reachable.
            /// </summary>
            OK,

            /// <summary>
            /// The URL is malformed.
            /// </summary>
            Malformed,

            /// <summary>
            /// The URL could not be resolved.
            /// </summary>
            Unresolved,

            /// <summary>
            /// The URL is unreachable.
            /// </summary>
            Unreachable
        }
    }
}