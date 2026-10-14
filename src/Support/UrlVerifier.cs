// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Support
{
    using Kampose.Outputs;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;

    /// <summary>
    /// Verifies the validity of URLs referenced in the documentation.
    /// </summary>
    public sealed class UrlVerifier : IDisposable
    {
        private readonly ConcurrentDictionary<Uri, VerificationResult> externalUrlResults = new();
        private readonly Lazy<HttpClient> httpClient = new(CreateHttpClient, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlVerifier"/> class.
        /// </summary>
        /// <param name="context">The documentation context.</param>
        /// <param name="generatedFiles">The files generated for the documentation, against which internal URLs are resolved.</param>
        /// <param name="baseUri">The base URI of the documentation site, or <see langword="null"/> if URLs are relative.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="generatedFiles"/> is <see langword="null"/>.</exception>
        public UrlVerifier(IDocumentationContext context, GeneratedFileSet generatedFiles, Uri? baseUri)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            GeneratedFiles = generatedFiles ?? throw new ArgumentNullException(nameof(generatedFiles));
            BaseUri = baseUri;
        }

        /// <summary>
        /// Gets the files generated for the documentation.
        /// </summary>
        /// <value>
        /// The files generated for the documentation. An internal URL is valid only when it refers to one of these files; files
        /// that exist in the output directory but were not generated do not count. When anchors are recorded for the referenced
        /// file, the fragment of the URL must also name one of them.
        /// </value>
        public GeneratedFileSet GeneratedFiles { get; }

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
            if (httpClient.IsValueCreated)
                httpClient.Value.Dispose();

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
                return TryGetReferencingFile(urlReference, out var referencingFilePath)
                    ? VerifyFragment(referencingFilePath, urlReference.SourceUrl)
                    : VerificationResult.OK;

            if (urlReference.TargetUrl is not null)
                return TryGetGeneratedFile(urlReference, out var targetFilePath)
                    ? VerifyFragment(targetFilePath, urlReference.TargetUrl.OriginalString)
                    : VerificationResult.Unreachable;

            if (!Uri.TryCreate(urlReference.SourceUrl, UriKind.RelativeOrAbsolute, out var uri))
                return VerificationResult.Malformed;

            if (!uri.IsAbsoluteUri)
                return TryGetGeneratedFile(urlReference.BaseDirectory, uri.ToString(), out var filePath)
                    ? VerifyFragment(filePath, uri.OriginalString)
                    : VerificationResult.Unreachable;

            if (verifyExternalLinks)
                return externalUrlResults.GetOrAdd(uri, url => IsUrlReachable(url)
                    ? VerificationResult.OK
                    : VerificationResult.Unreachable);

            return VerificationResult.OK;
        }

        /// <summary>
        /// Attempts to get the generated file that the URL refers to.
        /// </summary>
        /// <param name="urlReference">The URL reference.</param>
        /// <param name="filePath">When this method returns, contains the full path of the generated file, if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the file was generated; otherwise, <see langword="false"/>.</returns>
        private bool TryGetGeneratedFile(UrlReference urlReference, [NotNullWhen(true)] out string? filePath)
        {
            filePath = null;
            if (urlReference.TargetUrl is null || urlReference.TargetUrl.IsAbsoluteUri != (BaseUri is not null))
                return false;

            var relativeUrlString = BaseUri is not null
                ? BaseUri.MakeRelativeUri(urlReference.TargetUrl).ToString()
                : Path.Combine(urlReference.BaseDirectory, urlReference.TargetUrl.ToString());

            return TryGetGeneratedFile(string.Empty, relativeUrlString, out filePath);
        }

        /// <summary>
        /// Attempts to get the file generated at the specified document-relative URL.
        /// </summary>
        /// <param name="baseDirectory">The directory of the referencing document.</param>
        /// <param name="relativeUrl">The URL relative to the referencing document.</param>
        /// <param name="filePath">When this method returns, contains the full path of the generated file, if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the file was generated; otherwise, <see langword="false"/>.</returns>
        private bool TryGetGeneratedFile(string baseDirectory, string relativeUrl, [NotNullWhen(true)] out string? filePath)
        {
            filePath = null;

            var relativeFilePath = UriHelper.GetPathPart(relativeUrl);
            var absoluteBaseDir = GeneratedFiles.OutputDirectory;
            var absoluteFilePath = Path.GetFullPath(Path.Combine(absoluteBaseDir, baseDirectory, relativeFilePath));
            var pathFromBase = Path.GetRelativePath(absoluteBaseDir, absoluteFilePath);

            if (!PathHelper.IsSubpath(pathFromBase, absoluteFilePath))
                return false;

            if (GeneratedFiles.Contains(absoluteFilePath))
            {
                filePath = absoluteFilePath;
                return true;
            }

            if (Path.GetExtension(absoluteFilePath) != Context.ContentFormatter.FileExtension
                && GeneratedFiles.Contains(absoluteFilePath + Context.ContentFormatter.FileExtension))
            {
                filePath = absoluteFilePath + Context.ContentFormatter.FileExtension;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to get the generated file of the page in which the URL is referenced.
        /// </summary>
        /// <param name="urlReference">The URL reference.</param>
        /// <param name="filePath">When this method returns, contains the full path of the referencing page, if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the referencing page was generated; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// A model without its own page, such as a member documented on the page of its declaring type, is referenced from the page
        /// of its nearest ancestor that has one.
        /// </remarks>
        private bool TryGetReferencingFile(UrlReference urlReference, [NotNullWhen(true)] out string? filePath)
        {
            foreach (var model in urlReference.ReferencingModel.HierarchyPath.Reverse().Prepend(urlReference.ReferencingModel))
            {
                if (!model.TryGetDocumentationFile(out var relativePath))
                    continue;

                var absoluteFilePath = Path.GetFullPath(Path.Combine(GeneratedFiles.OutputDirectory, relativePath));
                if (GeneratedFiles.Contains(absoluteFilePath))
                {
                    filePath = absoluteFilePath;
                    return true;
                }
            }

            filePath = null;
            return false;
        }

        /// <summary>
        /// Verifies that the fragment of the URL names an anchor of the generated file.
        /// </summary>
        /// <param name="filePath">The full path of the generated file that the URL refers to.</param>
        /// <param name="url">The URL whose fragment is verified.</param>
        /// <returns>
        /// <see cref="VerificationResult.OK"/> if the URL has no fragment, its fragment refers to the top of the page, no anchors
        /// were recorded for the file, or the fragment names one of its anchors; otherwise, <see cref="VerificationResult.Unresolved"/>.
        /// </returns>
        /// <remarks>
        /// The fragment is matched case-sensitively, first as written and then with its percent-encoded characters decoded, as browsers
        /// do. Anchors are recorded only for files whose anchors are known, such as HTML pages.
        /// </remarks>
        private VerificationResult VerifyFragment(string filePath, string url)
        {
            var fragmentIndex = url.IndexOf('#');
            if (fragmentIndex == -1)
                return VerificationResult.OK;

            var fragment = url[(fragmentIndex + 1)..];
            if (fragment.Length == 0 || fragment.Equals("top", StringComparison.OrdinalIgnoreCase))
                return VerificationResult.OK;

            if (!GeneratedFiles.TryGetAnchors(filePath, out var anchors))
                return VerificationResult.OK;

            return anchors.Contains(fragment) || anchors.Contains(Uri.UnescapeDataString(fragment))
                ? VerificationResult.OK
                : VerificationResult.Unresolved;
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

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, uri);
                using var response = httpClient.Value.Send(request);
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
            /// The URL could not be resolved, such as when its fragment names no anchor of the referenced page.
            /// </summary>
            Unresolved,

            /// <summary>
            /// The URL is unreachable.
            /// </summary>
            Unreachable
        }
    }
}
