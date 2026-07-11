// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Support
{
    using Kampose.Support;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Routing;
    using Moq;
    using Moq.Protected;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    [TestFixture]
    public class UrlVerifierTests
    {
        private IDocumentationContext context = null!;
        private readonly IDocumentModel model = Mock.Of<IDocumentModel>();
        private readonly string baseDir = @"C:\temp\docs";
        private readonly Uri baseUri = new("https://example.com/docs/");

        [SetUp]
        public void SetUp()
        {
            context = MockHelper.CreateDocumentationContext<HtmlFormat>();
        }

        [TearDown]
        public void TearDown()
        {
            (context as IDisposable)?.Dispose();
        }

        [Test]
        public void VerifyUrl_QueryOrFragmentOnly_ReturnsOK()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);
            var urlReference = new UrlReference(model, "dir", "#anchor");

            var result = verifier.VerifyUrl(urlReference);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.OK));
        }

        [Test]
        public void VerifyUrl_QueryOnly_ReturnsOK()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);
            var urlReference = new UrlReference(model, "dir", "?query=value");

            var result = verifier.VerifyUrl(urlReference);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.OK));
        }

        [Test]
        public void VerifyUrl_TargetUrlRelative_BaseUrlNull_FilePresent_ReturnsOK()
        {
            var verifier = new UrlVerifier(context, baseDir, null);

            var targetUrl = new Uri("../page.html", UriKind.Relative);
            var urlReference = new UrlReference(model, "dir", "page", targetUrl);

            var absolutePath = Path.Combine(baseDir, "page.html");
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
            File.WriteAllText(absolutePath, "content");
            try
            {
                var result = verifier.VerifyUrl(urlReference);

                Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.OK));
            }
            finally
            {
                File.Delete(absolutePath);
            }
        }

        [Test]
        public void VerifyUrl_TargetUrlRelative_BaseUrlNull_FileNotPresent_ReturnsUnreachable()
        {
            var verifier = new UrlVerifier(context, baseDir, null);

            var targetUrl = new Uri("../missing.html", UriKind.Relative);
            var urlReference = new UrlReference(model, "dir", "missing", targetUrl);

            var result = verifier.VerifyUrl(urlReference);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.Unreachable));
        }

        [Test]
        public void VerifyUrl_TargetUrlRelative_BaseUrlNotNull_ReturnsUnreachable()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);

            var targetUrl = new Uri("../page.html", UriKind.Relative);
            var urlReference = new UrlReference(model, "dir", "page", targetUrl);

            var result = verifier.VerifyUrl(urlReference);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.Unreachable));
        }

        [Test]
        public void VerifyUrl_TargetUrlAbsolute_BaseUriNotNull_FilePresent_ReturnsOK()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);

            var targetUrl = new Uri("https://example.com/docs/page.html", UriKind.Absolute);
            var urlReference = new UrlReference(model, "dir", "page", targetUrl);

            var absolutePath = Path.Combine(baseDir, "page.html");
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
            File.WriteAllText(absolutePath, "content");
            try
            {
                var result = verifier.VerifyUrl(urlReference);

                Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.OK));
            }
            finally
            {
                File.Delete(absolutePath);
            }
        }

        [Test]
        public void VerifyUrl_TargetUrlAbsolute_BaseUriNotNull_FileNotPresent_ReturnsUnreachable()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);

            var targetUrl = new Uri("https://example.com/docs/missing.html", UriKind.Absolute);
            var urlReference = new UrlReference(model, "dir", "missing", targetUrl);

            var result = verifier.VerifyUrl(urlReference);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.Unreachable));
        }

        [Test]
        public void VerifyUrl_TargetUrlAbsolute_BaseUrlNull_ReturnsUnreachable()
        {
            var verifier = new UrlVerifier(context, baseDir, null);

            var targetUrl = new Uri("https://example.com/page.html", UriKind.Absolute);
            var urlReference = new UrlReference(model, "dir", "page", targetUrl);

            var result = verifier.VerifyUrl(urlReference);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.Unreachable));
        }

        [Test]
        public void VerifyUrl_MalformedUrl_ReturnsMalformed()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);
            var urlReference = new UrlReference(model, "dir", "http://[::1");

            var result = verifier.VerifyUrl(urlReference);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.Malformed));
        }

        [Test]
        public void VerifyUrl_AbsoluteUri_NoExternalVerify_ReturnsOK()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);
            var urlReference = new UrlReference(model, "dir", "https://example.com/page");

            var result = verifier.VerifyUrl(urlReference, verifyExternalLinks: false);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.OK));
        }

        [Test]
        public void VerifyUrl_AbsoluteUri_ExternalVerify_Reachable_ReturnsOK()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);
            SetupHttpClient(verifier, HttpStatusCode.OK);

            var urlReference = new UrlReference(model, "dir", "https://example.com/page");

            var result = verifier.VerifyUrl(urlReference, verifyExternalLinks: true);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.OK));
        }

        [Test]
        public void VerifyUrl_AbsoluteUri_ExternalVerify_NotFound_ReturnsUnreachable()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);
            SetupHttpClient(verifier, HttpStatusCode.NotFound);

            var urlReference = new UrlReference(model, "dir", "https://example.com/missing");

            var result = verifier.VerifyUrl(urlReference, verifyExternalLinks: true);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.Unreachable));
        }

        [Test]
        public void VerifyUrl_AbsoluteUri_ExternalVerify_Exception_ReturnsUnreachable()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);
            SetupHttpClient(verifier, HttpStatusCode.Unused); // This causes exception

            var urlReference = new UrlReference(model, "dir", "https://example.com/error");

            var result = verifier.VerifyUrl(urlReference, verifyExternalLinks: true);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.Unreachable));
        }

        [TestCase(HttpStatusCode.OK, UrlVerifier.VerificationResult.OK)]
        [TestCase(HttpStatusCode.NotFound, UrlVerifier.VerificationResult.Unreachable)]
        [TestCase(HttpStatusCode.Unused, UrlVerifier.VerificationResult.Unreachable)]
        public void VerifyUrl_AbsoluteUri_ExternalVerify_RepeatedUrlChecksOnce(HttpStatusCode statusCode, UrlVerifier.VerificationResult expectedResult)
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);
            var messageHandlerMock = SetupHttpClient(verifier, statusCode);
            var firstReference = new UrlReference(model, "first", "https://example.com/page");
            var secondReference = new UrlReference(model, "second", "https://example.com/page");

            var firstResult = verifier.VerifyUrl(firstReference, verifyExternalLinks: true);
            var secondResult = verifier.VerifyUrl(secondReference, verifyExternalLinks: true);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(firstResult, Is.EqualTo(expectedResult));
                Assert.That(secondResult, Is.EqualTo(expectedResult));
            }

            messageHandlerMock.Protected().Verify(
                "Send",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Test]
        public void VerifyUrl_NonHttpUri_ReturnsOK()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);
            var urlReference = new UrlReference(model, "dir", "mailto:test@example.com");

            var result = verifier.VerifyUrl(urlReference, verifyExternalLinks: true);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.OK));
        }

        [Test]
        public void VerifyUrl_RelativeUri_FileNotPresent_ReturnsUnreachable()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);
            var urlReference = new UrlReference(model, "dir", "relative/path");

            var result = verifier.VerifyUrl(urlReference);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.Unreachable));
        }

        [Test]
        public void VerifyUrl_RelativeUri_FilePresent_ReturnsOK()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);
            var urlReference = new UrlReference(model, "dir", "../page");
            var absolutePath = Path.Combine(baseDir, "page.html");
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
            File.WriteAllText(absolutePath, "content");

            try
            {
                var result = verifier.VerifyUrl(urlReference);

                Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.OK));
            }
            finally
            {
                File.Delete(absolutePath);
            }
        }

        [Test]
        public void VerifyUrl_RelativeUri_OutsideBaseDirectory_ReturnsUnreachable()
        {
            var verifier = new UrlVerifier(context, baseDir, baseUri);
            var urlReference = new UrlReference(model, "dir", "../../outside");

            var result = verifier.VerifyUrl(urlReference);

            Assert.That(result, Is.EqualTo(UrlVerifier.VerificationResult.Unreachable));
        }

        private static Mock<HttpMessageHandler> SetupHttpClient(UrlVerifier verifier, HttpStatusCode statusCode)
        {
            var messageHandlerMock = new Mock<HttpMessageHandler>();
            messageHandlerMock.Protected()
                .Setup<HttpResponseMessage>(
                    "Send",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Returns(
                    (HttpRequestMessage request, CancellationToken _) =>
                    {
                        if (HttpStatusCode.Unused == statusCode)
                            throw new HttpRequestException("Simulated request failure.");

                        return new HttpResponseMessage
                        {
                            StatusCode = statusCode,
                            RequestMessage = request
                        };
                    }
                )
                .Verifiable();

            var httpClient = new Lazy<HttpClient>(() => new HttpClient(messageHandlerMock.Object));

            typeof(UrlVerifier)
                .GetField("httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(verifier, httpClient);

            return messageHandlerMock;
        }
    }
}
