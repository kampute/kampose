// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates.Helpers
{
    using HandlebarsDotNet;
    using System;
    using System.Collections;
    using System.Linq;

    /// <summary>
    /// Provides Handlebars helper methods for logical operations.
    /// </summary>
    public static class LogicalHelpers
    {
        /// <summary>
        /// Registers the logical helper methods with the specified Handlebars environment.
        /// </summary>
        /// <param name="handlebars">The Handlebars environment to register the helpers with.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlebars"/> is <see langword="null"/>.</exception>
        public static void Register(IHandlebars handlebars)
        {
            ArgumentNullException.ThrowIfNull(handlebars);

            handlebars.RegisterHelper("eq", Equal);
            handlebars.RegisterHelper("ne", NotEqual);
            handlebars.RegisterHelper("lt", LessThan);
            handlebars.RegisterHelper("le", LessThanOrEqual);
            handlebars.RegisterHelper("gt", GreaterThan);
            handlebars.RegisterHelper("ge", GreaterThanOrEqual);
            handlebars.RegisterHelper("in", In);
            handlebars.RegisterHelper("not", Not);
            handlebars.RegisterHelper("and", And);
            handlebars.RegisterHelper("or", Or);
        }

        /// <summary>
        /// Determines whether two values are equal.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if the values are equal; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object Equal(Context context, Arguments arguments)
        {
            if (arguments.Length != 2)
                throw new HandlebarsException($"{nameof(Equal)} template helper function requires two arguments.");

            return Equals(arguments[0], arguments[1]);
        }

        /// <summary>
        /// Determines whether two values are not equal.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if the values are not equal; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object NotEqual(Context context, Arguments arguments)
        {
            if (arguments.Length != 2)
                throw new HandlebarsException($"{nameof(NotEqual)} template helper function requires two arguments.");

            return !Equals(arguments[0], arguments[1]);
        }

        /// <summary>
        /// Determines whether the first value is less than the second value.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if the first value is less than the second value; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid or values cannot be compared.</exception>
        private static object LessThan(Context context, Arguments arguments)
        {
            if (arguments.Length != 2)
                throw new HandlebarsException($"{nameof(LessThan)} template helper function requires two arguments.");

            try
            {
                return Comparer.Default.Compare(arguments[0], arguments[1]) < 0;
            }
            catch (ArgumentException ex)
            {
                throw new HandlebarsException($"Cannot compare the provided values in {nameof(LessThan)} helper.", ex);
            }
        }

        /// <summary>
        /// Determines whether the first value is less than or equal to the second value.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if the first value is less than or equal to the second value; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid or values cannot be compared.</exception>
        private static object LessThanOrEqual(Context context, Arguments arguments)
        {
            if (arguments.Length != 2)
                throw new HandlebarsException($"{nameof(LessThanOrEqual)} template helper function requires two arguments.");

            try
            {
                return Comparer.Default.Compare(arguments[0], arguments[1]) <= 0;
            }
            catch (ArgumentException ex)
            {
                throw new HandlebarsException($"Cannot compare the provided values in {nameof(LessThanOrEqual)} helper.", ex);
            }
        }

        /// <summary>
        /// Determines whether the first value is greater than the second value.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if the first value is greater than the second value; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid or values cannot be compared.</exception>
        private static object GreaterThan(Context context, Arguments arguments)
        {
            if (arguments.Length != 2)
                throw new HandlebarsException($"{nameof(GreaterThan)} template helper function requires two arguments.");

            try
            {
                return Comparer.Default.Compare(arguments[0], arguments[1]) > 0;
            }
            catch (ArgumentException ex)
            {
                throw new HandlebarsException($"Cannot compare the provided values in {nameof(GreaterThan)} helper.", ex);
            }
        }

        /// <summary>
        /// Determines whether the first value is greater than or equal to the second value.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if the first value is greater than or equal to the second value; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid or values cannot be compared.</exception>
        private static object GreaterThanOrEqual(Context context, Arguments arguments)
        {
            if (arguments.Length != 2)
                throw new HandlebarsException($"{nameof(GreaterThanOrEqual)} template helper function requires two arguments.");

            try
            {
                return Comparer.Default.Compare(arguments[0], arguments[1]) >= 0;
            }
            catch (ArgumentException ex)
            {
                throw new HandlebarsException($"Cannot compare the provided values in {nameof(GreaterThanOrEqual)} helper.", ex);
            }
        }

        /// <summary>
        /// Determines whether a value exists in a collection.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if the value exists in the collection; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid or the collection is not enumerable.</exception>
        private static object In(Context context, Arguments arguments)
        {
            if (arguments.Length < 2)
                throw new HandlebarsException($"{nameof(In)} template helper function requires at least two arguments.");

            var needle = arguments[0];
            var haystack = arguments.Length == 2 && arguments[1] is IEnumerable enumerable && arguments[1] is not string
                ? enumerable.Cast<object>()
                : arguments.AsEnumerable().Skip(1);

            return haystack.Contains(needle);
        }

        /// <summary>
        /// Performs a logical NOT operation on the provided value.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if the value is not truthy; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object Not(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(Not)} template helper function requires one argument.");

            return !TemplateHelpers.IsTruthy(arguments[0]);
        }

        /// <summary>
        /// Performs a logical AND operation on the provided values.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if all values are truthy; otherwise, <see langword="false"/>.</returns>
        private static object And(Context context, Arguments arguments)
        {
            return arguments.Length == 1 && arguments[0] is IEnumerable enumerable && arguments[0] is not string
                ? enumerable.Cast<object>().Any() && enumerable.Cast<object>().All(TemplateHelpers.IsTruthy)
                : arguments.Length > 0 && arguments.All(TemplateHelpers.IsTruthy);
        }

        /// <summary>
        /// Performs a logical OR operation on the provided values.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if any of the values is truthy; otherwise, <see langword="false"/>.</returns>
        private static object Or(Context context, Arguments arguments)
        {
            return arguments.Length == 1 && arguments[0] is IEnumerable enumerable && arguments[0] is not string
                ? enumerable.Cast<object>().Any(TemplateHelpers.IsTruthy)
                : arguments.Any(TemplateHelpers.IsTruthy);
        }
    }
}
