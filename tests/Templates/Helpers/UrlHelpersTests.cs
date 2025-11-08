// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Templates.Helpers
{
    using HandlebarsDotNet;
    using Kampose.Templates.Helpers;
    using Kampose.Test;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using NUnit.Framework;

    [TestFixture]
    public class UrlHelpersTests
    {
        private IHandlebars handlebars = null!;
        private IDocumentationContext docContext = null!;

        [SetUp]
        public void SetUp()
        {
            handlebars = Handlebars.Create();
            docContext = MockHelper.CreateDocumentationContext<HtmlFormat>();
            UrlHelpers.Register(handlebars, docContext);
        }

        [TearDown]
        public void TearDown()
        {
            docContext.Dispose();
        }

        [TestCase("", ExpectedResult = "")]
        [TestCase("api", ExpectedResult = "../")]
        [TestCase("api/namespace", ExpectedResult = "../../")]
        public string RootUrl_ReturnsRootUrl(string currentDirectory)
        {
            using var documentScope = docContext.AddressProvider.BeginScope(currentDirectory, null);
            var template = handlebars.Compile("{{#rootUrl}}");
            return template(null);
        }

        [TestCase("", "page.html", ExpectedResult = "page.html")]
        [TestCase("api", "page.html", ExpectedResult = "../page.html")]
        [TestCase("api/namespace", "page.html", ExpectedResult = "../../page.html")]
        [TestCase("api/namespace", "api/page.html", ExpectedResult = "../page.html")]
        [TestCase("api", "api/namespace/page.html", ExpectedResult = "namespace/page.html")]
        [TestCase("api/namespace", "/page.html", ExpectedResult = "/page.html")]
        [TestCase("api/namespace", "https://example.com/docs/page.html", ExpectedResult = "https://example.com/docs/page.html")]
        [TestCase("api/namespace", "page.html#section", ExpectedResult = "../../page.html#section")]
        [TestCase("api/namespace", "page.html?query=value", ExpectedResult = "../../page.html?query=value")]
        [TestCase("api/namespace/class", "api/namespace/interface.html", ExpectedResult = "../interface.html")]
        [TestCase("api", "#section", ExpectedResult = "#section")]
        [TestCase("api", "?query=param", ExpectedResult = "?query=param")]
        [TestCase("", "", ExpectedResult = "")]
        public string RootRelativeUrl_ReturnsTransformedUrl(string currentDirectory, string url)
        {
            using var documentScope = docContext.AddressProvider.BeginScope(currentDirectory, null);
            var template = handlebars.Compile("{{#rootRelativeUrl value}}");
            return template(new { value = url });
        }

        [TestCase("", ExpectedResult = "")]
        [TestCase("page.html", ExpectedResult = "")]
        [TestCase("page.html#section", ExpectedResult = "section")]
        [TestCase("page.html?q=test#section", ExpectedResult = "section")]
        [TestCase("https://example.com/docs/page.html#section", ExpectedResult = "section")]
        public string Fragment_ReturnsFragmentIdentifier(string url)
        {
            var template = handlebars.Compile("{{#fragment value}}");
            return template(new { value = url });
        }
    }
}
