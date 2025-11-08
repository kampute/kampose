// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Templates.Helpers
{
    using HandlebarsDotNet;
    using Kampose.Templates.Helpers;
    using NUnit.Framework;
    using System.Collections.Generic;

    [TestFixture]
    public class LogicalHelpersTests
    {
        private IHandlebars handlebars = null!;

        [SetUp]
        public void SetUp()
        {
            handlebars = Handlebars.Create();
            LogicalHelpers.Register(handlebars);
        }

        [TestCase("Hello", "Hello", ExpectedResult = "Equal")]
        [TestCase("Hello", "World", ExpectedResult = "Not Equal")]
        [TestCase(42, 42, ExpectedResult = "Equal")]
        [TestCase(42, 43, ExpectedResult = "Not Equal")]
        [TestCase(42, "42", ExpectedResult = "Not Equal")]
        [TestCase(true, true, ExpectedResult = "Equal")]
        [TestCase(true, false, ExpectedResult = "Not Equal")]
        [TestCase(null, null, ExpectedResult = "Equal")]
        [TestCase("Hello", null, ExpectedResult = "Not Equal")]
        [TestCase(null, "Hello", ExpectedResult = "Not Equal")]
        public string Equal_ReturnsExpectedResult(object? first, object? second)
        {
            var template = handlebars.Compile("{{#if (eq first second)}}Equal{{else}}Not Equal{{/if}}");
            return template(new { first, second });
        }

        [TestCase("Hello", "Hello", ExpectedResult = "Equal")]
        [TestCase("Hello", "World", ExpectedResult = "Not Equal")]
        [TestCase(42, 42, ExpectedResult = "Equal")]
        [TestCase(42, 43, ExpectedResult = "Not Equal")]
        [TestCase(42, "42", ExpectedResult = "Not Equal")]
        [TestCase(true, true, ExpectedResult = "Equal")]
        [TestCase(true, false, ExpectedResult = "Not Equal")]
        [TestCase(null, null, ExpectedResult = "Equal")]
        [TestCase("Hello", null, ExpectedResult = "Not Equal")]
        [TestCase(null, "Hello", ExpectedResult = "Not Equal")]
        public string NotEqual_ReturnsExpectedResult(object? first, object? second)
        {
            var template = handlebars.Compile("{{#if (ne first second)}}Not Equal{{else}}Equal{{/if}}");
            return template(new { first, second });
        }

        [TestCase(1, 2, ExpectedResult = "Less Than")]
        [TestCase(2, 1, ExpectedResult = "Not Less Than")]
        [TestCase(1, 1, ExpectedResult = "Not Less Than")]
        [TestCase("a", "b", ExpectedResult = "Less Than")]
        [TestCase("b", "a", ExpectedResult = "Not Less Than")]
        [TestCase("a", "a", ExpectedResult = "Not Less Than")]
        [TestCase(1.5, 2.5, ExpectedResult = "Less Than")]
        [TestCase(2.5, 1.5, ExpectedResult = "Not Less Than")]
        public string LessThan_ReturnsExpectedResult(object first, object second)
        {
            var template = handlebars.Compile("{{#if (lt first second)}}Less Than{{else}}Not Less Than{{/if}}");
            return template(new { first, second });
        }

        [TestCase(1, 2, ExpectedResult = "Less Than Or Equal")]
        [TestCase(2, 1, ExpectedResult = "Not Less Than Or Equal")]
        [TestCase(1, 1, ExpectedResult = "Less Than Or Equal")]
        [TestCase("a", "b", ExpectedResult = "Less Than Or Equal")]
        [TestCase("b", "a", ExpectedResult = "Not Less Than Or Equal")]
        [TestCase("a", "a", ExpectedResult = "Less Than Or Equal")]
        public string LessThanOrEqual_ReturnsExpectedResult(object first, object second)
        {
            var template = handlebars.Compile("{{#if (le first second)}}Less Than Or Equal{{else}}Not Less Than Or Equal{{/if}}");
            return template(new { first, second });
        }

        [TestCase(2, 1, ExpectedResult = "Greater Than")]
        [TestCase(1, 2, ExpectedResult = "Not Greater Than")]
        [TestCase(1, 1, ExpectedResult = "Not Greater Than")]
        [TestCase("b", "a", ExpectedResult = "Greater Than")]
        [TestCase("a", "b", ExpectedResult = "Not Greater Than")]
        [TestCase("a", "a", ExpectedResult = "Not Greater Than")]
        public string GreaterThan_ReturnsExpectedResult(object first, object second)
        {
            var template = handlebars.Compile("{{#if (gt first second)}}Greater Than{{else}}Not Greater Than{{/if}}");
            return template(new { first, second });
        }

        [TestCase(2, 1, ExpectedResult = "Greater Than Or Equal")]
        [TestCase(1, 2, ExpectedResult = "Not Greater Than Or Equal")]
        [TestCase(1, 1, ExpectedResult = "Greater Than Or Equal")]
        [TestCase("b", "a", ExpectedResult = "Greater Than Or Equal")]
        [TestCase("a", "b", ExpectedResult = "Not Greater Than Or Equal")]
        [TestCase("a", "a", ExpectedResult = "Greater Than Or Equal")]
        public string GreaterThanOrEqual_ReturnsExpectedResult(object first, object second)
        {
            var template = handlebars.Compile("{{#if (ge first second)}}Greater Than Or Equal{{else}}Not Greater Than Or Equal{{/if}}");
            return template(new { first, second });
        }

        [TestCase(true, ExpectedResult = "False")]
        [TestCase(false, ExpectedResult = "True")]
        [TestCase(null, ExpectedResult = "True")]
        [TestCase("Hello", ExpectedResult = "False")]
        [TestCase("", ExpectedResult = "True")]
        [TestCase(42, ExpectedResult = "False")]
        [TestCase(0, ExpectedResult = "True")]
        public string Not_ReturnsExpectedResult(object? value)
        {
            var template = handlebars.Compile("{{#if (not value)}}True{{else}}False{{/if}}");
            return template(new { value });
        }

        [TestCase(true, true, ExpectedResult = "True")]
        [TestCase(true, false, ExpectedResult = "False")]
        [TestCase(false, true, ExpectedResult = "False")]
        [TestCase(false, false, ExpectedResult = "False")]
        public string And_WithTwoArguments_ReturnsExpectedResult(bool first, bool second)
        {
            var template = handlebars.Compile("{{#if (and first second)}}True{{else}}False{{/if}}");
            return template(new { first, second });
        }

        [Test]
        public void And_WithMultipleArguments_WhenAllArgumentsAreTruthy_ReturnsTrue()
        {
            var template = handlebars.Compile("{{#if (and true 'hello' 42 1.5)}}True{{else}}False{{/if}}");
            var result = template(new { });

            Assert.That(result, Is.EqualTo("True"));
        }

        [Test]
        public void And_WithMultipleArguments_WhenAnyArgumentsIsNotTruthy_ReturnsTrue()
        {
            var template = handlebars.Compile("{{#if (and true 'hello' 42 null)}}True{{else}}False{{/if}}");
            var result = template(new { });

            Assert.That(result, Is.EqualTo("False"));
        }

        [TestCase(true, true, ExpectedResult = "True")]
        [TestCase(true, false, ExpectedResult = "True")]
        [TestCase(false, true, ExpectedResult = "True")]
        [TestCase(false, false, ExpectedResult = "False")]
        public string Or_WithTwoArguments_ReturnsExpectedResult(bool first, bool second)
        {
            var template = handlebars.Compile("{{#if (or first second)}}True{{else}}False{{/if}}");
            return template(new { first, second });
        }

        [Test]
        public void Or_WithMultipleArguments_WhenAnyArgumentIsTruthy_ReturnsTrue()
        {
            var template = handlebars.Compile("{{#if (or false null '' 0 'hello')}}True{{else}}False{{/if}}");
            var result = template(new { });

            Assert.That(result, Is.EqualTo("True"));
        }

        [Test]
        public void Or_WithMultipleArguments_WhenNoArgumentIsTruthy_ReturnsTrue()
        {
            var template = handlebars.Compile("{{#if (or false null '' 0)}}True{{else}}False{{/if}}");
            var result = template(new { });

            Assert.That(result, Is.EqualTo("False"));
        }

        [Test]
        public void In_WithArrayCollection_WhenValueExists_ReturnsTrue()
        {
            var template = handlebars.Compile("{{#if (in needle haystack)}}Found{{else}}Not Found{{/if}}");
            var result = template(new { needle = "b", haystack = new[] { "a", "b", "c" } });

            Assert.That(result, Is.EqualTo("Found"));
        }

        [Test]
        public void In_WithArrayCollection_WhenValueDoesNotExist_ReturnsFalse()
        {
            var template = handlebars.Compile("{{#if (in needle haystack)}}Found{{else}}Not Found{{/if}}");
            var result = template(new { needle = "d", haystack = new[] { "a", "b", "c" } });

            Assert.That(result, Is.EqualTo("Not Found"));
        }

        [Test]
        public void In_WithListCollection_WhenValueExists_ReturnsTrue()
        {
            var template = handlebars.Compile("{{#if (in needle haystack)}}Found{{else}}Not Found{{/if}}");
            var result = template(new { needle = 2, haystack = new List<int> { 1, 2, 3 } });

            Assert.That(result, Is.EqualTo("Found"));
        }

        [Test]
        public void In_WithListCollection_WhenValueDoesNotExist_ReturnsFalse()
        {
            var template = handlebars.Compile("{{#if (in needle haystack)}}Found{{else}}Not Found{{/if}}");
            var result = template(new { needle = 4, haystack = new List<int> { 1, 2, 3 } });

            Assert.That(result, Is.EqualTo("Not Found"));
        }

        [Test]
        public void In_WithMultipleArguments_WhenValueMatches_ReturnsTrue()
        {
            var template = handlebars.Compile("{{#if (in status 'active' 'pending' 'processing')}}Valid{{else}}Invalid{{/if}}");
            var result = template(new { status = "pending" });

            Assert.That(result, Is.EqualTo("Valid"));
        }

        [Test]
        public void In_WithMultipleArguments_WhenValueDoesNotMatch_ReturnsFalse()
        {
            var template = handlebars.Compile("{{#if (in status 'active' 'pending' 'processing')}}Valid{{else}}Invalid{{/if}}");
            var result = template(new { status = "cancelled" });

            Assert.That(result, Is.EqualTo("Invalid"));
        }
    }
}
