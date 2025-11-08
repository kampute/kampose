// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates.Helpers
{
    using HandlebarsDotNet;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Provides Handlebars helper methods for string manipulation and formatting.
    /// </summary>
    public static class StringHelpers
    {
        /// <summary>
        /// Registers the string helper methods with the specified Handlebars environment.
        /// </summary>
        /// <param name="handlebars">The Handlebars environment to register the helpers with.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlebars"/> is <see langword="null"/>.</exception>
        public static void Register(IHandlebars handlebars)
        {
            ArgumentNullException.ThrowIfNull(handlebars);

            handlebars.RegisterHelper(nameof(Format), Format);

            handlebars.RegisterHelper(nameof(UpperCase), UpperCase);
            handlebars.RegisterHelper(nameof(LowerCase), LowerCase);
            handlebars.RegisterHelper(nameof(TitleCase), TitleCase);
            handlebars.RegisterHelper(nameof(KebabCase), KebabCase);
            handlebars.RegisterHelper(nameof(SnakeCase), SnakeCase);

            handlebars.RegisterHelper(nameof(Split), Split);
            handlebars.RegisterHelper(nameof(Concat), Concat);

            handlebars.RegisterHelper(nameof(FirstNonBlank), FirstNonBlank);
        }

        /// <summary>
        /// Splits a string into an array of substrings based on the specified separator.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>An array of substrings.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object Split(Context context, Arguments arguments)
        {
            if (arguments.Length != 2)
                throw new HandlebarsException($"{nameof(Split)} template helper function requires two arguments.");

            var input = arguments[0]?.ToString();
            var separator = arguments[1]?.ToString();
            return !string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(separator)
                ? input.Split(separator, StringSplitOptions.None)
                : [];
        }

        /// <summary>
        /// Concatenates an arbitrary number arguments as a single string.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The concatenated string.</returns>
        private static object Concat(Context context, Arguments arguments)
        {
            var values = arguments.Length == 1 && arguments[0] is IEnumerable enumerable && arguments[0] is not string
                ? enumerable.Cast<object>()
                : arguments.AsEnumerable();

            return string.Join(string.Empty, values);
        }

        /// <summary>
        /// Converts the specified argument to a string representation.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The string representation of the first argument.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object Format(Context context, Arguments arguments)
        {
            if (arguments.Length is < 1 or > 2)
                throw new HandlebarsException($"{nameof(UpperCase)} template helper function requires one or two arguments.");

            return arguments[0] switch
            {
                null => string.Empty,
                string str => str,
                IFormattable formattable => formattable.ToString(arguments.Length == 2 ? arguments[1]?.ToString() : null, CultureInfo.InvariantCulture),
                _ => arguments[0].ToString() ?? string.Empty
            };
        }

        /// <summary>
        /// Converts the specified string to uppercase.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The uppercase string.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object? UpperCase(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(UpperCase)} template helper function requires one argument.");

            return arguments[0]?.ToString()?.ToUpper();
        }

        /// <summary>
        /// Converts the specified string to lowercase.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The lowercase string.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object? LowerCase(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(LowerCase)} template helper function requires one argument.");

            return arguments[0]?.ToString()?.ToLower();
        }

        /// <summary>
        /// Converts the specified string to title case (first letter of each word capitalized).
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The title-cased string.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object TitleCase(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(TitleCase)} template helper function requires one argument.");

            var text = arguments[0]?.ToString();
            return !string.IsNullOrWhiteSpace(text)
                ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text)
                : string.Empty;
        }

        /// <summary>
        /// Converts the specified string to kebab case (lowercase with words separated by underscores).
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The string in kebab-case.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object KebabCase(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(KebabCase)} template helper function requires one argument.");

            var text = arguments[0]?.ToString();
            return !string.IsNullOrWhiteSpace(text)
                ? string.Join('-', TextUtility.SplitWords(text).Select(word => text[word.Range])).ToLower()
                : string.Empty;
        }

        /// <summary>
        /// Converts the specified string to snake case (lowercase with words separated by hyphens).
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The string in kebab-case.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object SnakeCase(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(SnakeCase)} template helper function requires one argument.");

            var text = arguments[0]?.ToString();
            return !string.IsNullOrWhiteSpace(text)
                ? string.Join('_', TextUtility.SplitWords(text).Select(word => text[word.Range])).ToLower()
                : string.Empty;
        }

        /// <summary>
        /// Retrieves the string representation of the first argument that is not empty, whitespace, or <see langword="null"/>.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The string representation of the first argument that is not empty, whitespace, or <see langword="null"/>; otherwise, <see langword="null"/>.</returns>
        public static object? FirstNonBlank(Context context, Arguments arguments)
        {
            return arguments.Select(arg => arg?.ToString()).FirstOrDefault(str => !string.IsNullOrWhiteSpace(str));
        }
    }
}
