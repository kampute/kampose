// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates.Helpers
{
    using HandlebarsDotNet;
    using System;
    using System.Globalization;

    /// <summary>
    /// Provides Handlebars helper methods for arithmetic operations.
    /// </summary>
    public static class ArithmeticHelpers
    {
        /// <summary>
        /// Registers the arithmetic helper methods with the specified Handlebars environment.
        /// </summary>
        /// <param name="handlebars">The Handlebars environment to register the helpers with.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlebars"/> is <see langword="null"/>.</exception>
        public static void Register(IHandlebars handlebars)
        {
            ArgumentNullException.ThrowIfNull(handlebars);

            handlebars.RegisterHelper(nameof(Inc), Inc);
            handlebars.RegisterHelper(nameof(Dec), Dec);
            handlebars.RegisterHelper(nameof(Add), Add);
            handlebars.RegisterHelper(nameof(Sub), Sub);
            handlebars.RegisterHelper(nameof(Mul), Mul);
            handlebars.RegisterHelper(nameof(Div), Div);
            handlebars.RegisterHelper(nameof(Mod), Mod);
            handlebars.RegisterHelper(nameof(Abs), Abs);
        }

        /// <summary>
        /// Increments the specified integer value by one.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The incremented value.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not exactly one.</exception>
        private static object Inc(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(Inc)} template helper function requires one argument.");

            var value = Convert.ToInt32(arguments[0], CultureInfo.InvariantCulture);
            return value + 1;
        }

        /// <summary>
        /// Decrements the specified integer value by one.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The decremented value.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not exactly one.</exception>
        private static object Dec(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(Dec)} template helper function requires one argument.");

            var value = Convert.ToInt32(arguments[0], CultureInfo.InvariantCulture);
            return value - 1;
        }

        /// <summary>
        /// Adds two integer values provided as arguments.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The sum of the two integer arguments.</returns>
        /// <exception cref="HandlebarsException">Thrown if the number of arguments is not exactly two.</exception>
        private static object Add(Context context, Arguments arguments)
        {
            if (arguments.Length != 2)
                throw new HandlebarsException($"{nameof(Add)} template helper function requires two arguments.");

            var operand1 = Convert.ToInt32(arguments[0], CultureInfo.InvariantCulture);
            var operand2 = Convert.ToInt32(arguments[1], CultureInfo.InvariantCulture);
            return operand1 + operand2;
        }

        /// <summary>
        /// Subtracts the second integer argument from the first integer argument.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The result of subtracting the second integer from the first integer.</returns>
        /// <exception cref="HandlebarsException">Thrown if the number of arguments is not exactly two.</exception>
        private static object Sub(Context context, Arguments arguments)
        {
            if (arguments.Length != 2)
                throw new HandlebarsException($"{nameof(Sub)} template helper function requires two arguments.");

            var operand1 = Convert.ToInt32(arguments[0], CultureInfo.InvariantCulture);
            var operand2 = Convert.ToInt32(arguments[1], CultureInfo.InvariantCulture);
            return operand1 - operand2;
        }

        /// <summary>
        /// Multiplies two integer arguments and returns the result.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The product of the two integer arguments.</returns>
        /// <exception cref="HandlebarsException">Thrown if the number of arguments is not exactly two.</exception>
        private static object Mul(Context context, Arguments arguments)
        {
            if (arguments.Length != 2)
                throw new HandlebarsException($"{nameof(Mul)} template helper function requires two arguments.");

            return Convert.ToInt32(arguments[0], CultureInfo.InvariantCulture)
                 * Convert.ToInt32(arguments[1], CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Divides the first integer argument by the second integer argument.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The result of dividing the first argument by the second argument as an integer.</returns>
        /// <exception cref="HandlebarsException">Thrown if the number of arguments is not exactly two.</exception>
        private static object Div(Context context, Arguments arguments)
        {
            if (arguments.Length != 2)
                throw new HandlebarsException($"{nameof(Div)} template helper function requires two arguments.");

            var operand1 = Convert.ToInt32(arguments[0], CultureInfo.InvariantCulture);
            var operand2 = Convert.ToInt32(arguments[1], CultureInfo.InvariantCulture);
            return operand1 / operand2;
        }

        /// <summary>
        /// Computes the modulus of two integer arguments.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The remainder of the division of the first argument by the second argument.</returns>
        /// <exception cref="HandlebarsException">Thrown if the number of arguments is not exactly two.</exception>
        private static object Mod(Context context, Arguments arguments)
        {
            if (arguments.Length != 2)
                throw new HandlebarsException($"{nameof(Mod)} template helper function requires two arguments.");

            var operand1 = Convert.ToInt32(arguments[0], CultureInfo.InvariantCulture);
            var operand2 = Convert.ToInt32(arguments[1], CultureInfo.InvariantCulture);
            return operand1 % operand2;
        }

        /// <summary>
        /// Computes the absolute value of a given integer.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The absolute value of the integer provided as the first argument.</returns>
        /// <exception cref="HandlebarsException">Thrown if the number of arguments is not exactly one.</exception>
        private static object Abs(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(Abs)} template helper function requires one argument.");

            var value = Convert.ToInt32(arguments[0], CultureInfo.InvariantCulture);
            return value < 0 ? -value : value;
        }
    }
}
