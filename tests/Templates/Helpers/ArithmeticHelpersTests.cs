// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Templates.Helpers
{
    using HandlebarsDotNet;
    using Kampose.Templates.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class ArithmeticHelpersTests
    {
        private IHandlebars handlebars = null!;

        [SetUp]
        public void SetUp()
        {
            handlebars = Handlebars.Create();
            ArithmeticHelpers.Register(handlebars);
        }

        [TestCase(1, ExpectedResult = "2")]
        [TestCase(0, ExpectedResult = "1")]
        [TestCase(-1, ExpectedResult = "0")]
        [TestCase("100", ExpectedResult = "101")]
        public string Inc_ReturnsIncrementedValueByOne(object value)
        {
            var template = handlebars.Compile("{{#inc value}}");
            return template(new { value });
        }

        [TestCase(1, ExpectedResult = "0")]
        [TestCase(0, ExpectedResult = "-1")]
        [TestCase(-1, ExpectedResult = "-2")]
        [TestCase("100", ExpectedResult = "99")]
        public string Dec_ReturnsDecrementedValueByOne(object value)
        {
            var template = handlebars.Compile("{{#dec value}}");
            return template(new { value });
        }

        [TestCase(1, 2, ExpectedResult = "3")]
        [TestCase(-1, 1, ExpectedResult = "0")]
        [TestCase(0, 0, ExpectedResult = "0")]
        [TestCase("100", "200", ExpectedResult = "300")]
        public string Add_ReturnsSumOfArguments(object a, object b)
        {
            var template = handlebars.Compile("{{#add a b}}");
            return template(new { a, b });
        }

        [TestCase(5, 3, ExpectedResult = "2")]
        [TestCase(0, 1, ExpectedResult = "-1")]
        [TestCase(-1, -1, ExpectedResult = "0")]
        [TestCase("100", "50", ExpectedResult = "50")]
        public string Sub_ReturnsDifferenceOfArguments(object a, object b)
        {
            var template = handlebars.Compile("{{#sub a b}}");
            return template(new { a, b });
        }

        [TestCase(2, 3, ExpectedResult = "6")]
        [TestCase(-2, 3, ExpectedResult = "-6")]
        [TestCase(0, 100, ExpectedResult = "0")]
        [TestCase("10", "5", ExpectedResult = "50")]
        public string Mul_ReturnsProductOfArguments(object a, object b)
        {
            var template = handlebars.Compile("{{#mul a b}}");
            return template(new { a, b });
        }

        [TestCase(6, 3, ExpectedResult = "2")]
        [TestCase(7, 2, ExpectedResult = "3")]
        [TestCase(-10, 2, ExpectedResult = "-5")]
        [TestCase("100", "4", ExpectedResult = "25")]
        public string Div_ReturnsQuotientOfArguments(object a, object b)
        {
            var template = handlebars.Compile("{{#div a b}}");
            return template(new { a, b });
        }

        [TestCase(7, 3, ExpectedResult = "1")]
        [TestCase(10, 2, ExpectedResult = "0")]
        [TestCase(-10, 3, ExpectedResult = "-1")]
        [TestCase("100", "30", ExpectedResult = "10")]
        public string Mod_ReturnsModulusOfArguments(object a, object b)
        {
            var template = handlebars.Compile("{{#mod a b}}");
            return template(new { a, b });
        }

        [TestCase(1, ExpectedResult = "1")]
        [TestCase(-2, ExpectedResult = "2")]
        [TestCase(0, ExpectedResult = "0")]
        public string Abs_ReturnsAbsoluteValue(object value)
        {
            var template = handlebars.Compile("{{#abs value}}");
            return template(new { value });
        }
    }
}
