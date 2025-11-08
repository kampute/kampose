// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates.Helpers
{
    using HandlebarsDotNet;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides Handlebars helper methods for general utility operations.
    /// </summary>
    public static partial class UtilityHelpers
    {
        /// <summary>
        /// Registers the utility helper methods with the specified Handlebars environment.
        /// </summary>
        /// <param name="handlebars">The Handlebars environment to register the helpers with.</param>
        /// <param name="documentationContext">The documentation context used for resolving URLs and encoding.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlebars"/> or <paramref name="documentationContext"/> is <see langword="null"/>.</exception>
        public static void Register(IHandlebars handlebars, IDocumentationContext documentationContext)
        {
            ArgumentNullException.ThrowIfNull(handlebars);

            handlebars.RegisterHelper(nameof(IsUndefined), IsUndefined);
            handlebars.RegisterHelper(nameof(IsNull), IsNull);
            handlebars.RegisterHelper(nameof(IsOdd), IsOdd);

            handlebars.RegisterHelper(nameof(Select), Select);
            handlebars.RegisterHelper(nameof(Len), Len);
            handlebars.RegisterHelper(nameof(Now), Now);
            handlebars.RegisterHelper(nameof(Json), Json);

            handlebars.RegisterHelper(nameof(Literal), (output, context, arguments) => Literal(in output, arguments, documentationContext));
            handlebars.RegisterHelper(nameof(Cref), (output, context, arguments) => Cref(in output, arguments, documentationContext));

            handlebars.RegisterHelper(nameof(Markdown), (output, options, context, arguments) => Markdown(output, options, documentationContext));
            handlebars.RegisterHelper(nameof(StripTags), StripTags);
        }

        /// <summary>
        /// Determines whether the specified value is undefined.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if the specified value is undefined; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="HandlebarsException">Thrown if the number of arguments is not exactly one.</exception>
        private static object IsUndefined(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(IsUndefined)} template helper function requires one argument.");

            return arguments[0] is UndefinedBindingResult;
        }

        /// <summary>
        /// Determines whether the specified value is <see langword="null"/>.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if the specified value is <see langword="null"/>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="HandlebarsException">Thrown if the number of arguments is not exactly one.</exception>
        private static object IsNull(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(IsNull)} template helper function requires one argument.");

            return arguments[0] is null;
        }

        /// <summary>
        /// Determines whether the specified integer is odd.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns><see langword="true"/> if the specified integer is odd; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="HandlebarsException">Thrown if the number of arguments is not exactly one.</exception>
        private static object IsOdd(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(IsOdd)} template helper function requires one argument.");

            var value = Convert.ToInt32(arguments[0], CultureInfo.InvariantCulture);
            return value % 2 != 0;
        }

        /// <summary>
        /// Selects a value from a list of choices based on a selector value.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The selected value, or <see langword="null"/> if no valid selection could be made.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        /// <remarks>
        /// The selection behavior depends on the number of choices provided:
        /// <list type="bullet">
        ///   <item><description>If one choice is provided, returns that choice regardless of the selector.</description></item>
        ///   <item><description>If two choices are provided, returns the first choice if the selector is truthy, otherwise the second choice.</description></item>
        ///   <item><description>If more than two choices are provided, attempts to convert the selector to an integer and uses it as a zero-based index into the choices. Returns <see langword="null"/> if the conversion fails or the index is out of range.</description></item>
        /// </list>
        /// If the second argument is an enumerable collection (but not a string), it is treated as the list of choices.
        /// Otherwise, all arguments after the first are treated as individual choices.
        /// </remarks>
        private static object? Select(Context context, Arguments arguments)
        {
            if (arguments.Length < 2)
                throw new HandlebarsException($"{nameof(Select)} template helper function requires at least two arguments.");

            var selector = arguments[0];
            var choices = arguments.Length == 2 && arguments[1] is IEnumerable enumerable && arguments[1] is not string
                ? enumerable.Cast<object>()
                : arguments.AsEnumerable().Skip(1);
            var choiceList = choices.ToList();

            return choiceList.Count switch
            {
                0 => arguments[1],
                1 => choiceList[0],
                2 => TemplateHelpers.IsTruthy(selector) ? choiceList[0] : choiceList[1],
                _ when ToInteger(selector) is int index && index >= 0 && index < choiceList.Count => choiceList[index],
                _ => null
            };

            static int? ToInteger(object value)
            {
                try
                {
                    return Convert.ToInt32(value, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns the length of the specified string or collection.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The length of the specified string or collection.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object Len(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(Len)} template helper function requires one argument.");

            return arguments[0] switch
            {
                string str => str.Length,
                Array array => array.Length,
                ICollection collection => collection.Count,
                IEnumerable enumerable => enumerable.Cast<object>().Count(),
                _ => throw new HandlebarsException("Invalid argument type for length calculation.")
            };
        }

        /// <summary>
        /// Returns the current date and time formatted according to the specified format string, or the default format if no format is specified.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The current date and time at the moment of execution formatted as a string.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object Now(Context context, Arguments arguments)
        {
            if (arguments.Length > 1)
                throw new HandlebarsException($"{nameof(Now)} template helper function accepts at most one argument.");

            return DateTime.Now.ToString(arguments.Length == 1 ? arguments[0]?.ToString() : null, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns the JSON representation of the specified object.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The JSON representation of the object or <see langword="null"/> if the argument is undefined.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object? Json(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(Json)} template helper function requires one argument.");

            return arguments[0] is not UndefinedBindingResult
                ? Support.Json.Stringify(arguments[0])
                : null;
        }

        /// <summary>
        /// Renders the specified value formatted according to the syntax rules of the language of the documentation context.
        /// </summary>
        /// <param name="output">The output writer.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <param name="docContext">The documentation context used for resolving URLs and encoding.</param>
        /// <exception cref="HandlebarsException">Thrown when the type or number of arguments is not valid.</exception>
        private static void Literal(in EncodedTextWriter output, Arguments arguments, IDocumentationContext docContext)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(Literal)} template helper function requires one argument.");

            using var writer = output.CreateWrapper();
            docContext.Language.WriteConstantValue(writer, arguments[0]);
        }

        /// <summary>
        /// Renders a link to the documentation of a code element based on its code reference.
        /// </summary>
        /// <param name="output">The output writer.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <param name="docContext">The documentation context used for resolving URLs and encoding.</param>
        /// <exception cref="HandlebarsException">Thrown when the type or number of arguments is not valid.</exception>
        private static void Cref(in EncodedTextWriter output, Arguments arguments, IDocumentationContext docContext)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(Cref)} template helper function requires one argument.");

            var cref = arguments[0] as string
                ?? throw new HandlebarsException($"{nameof(Cref)} template helper function requires a code reference string as its argument.");

            using var docWriter = output.CreateMarkupWrapper(docContext);
            docWriter.WriteDocLink(cref, docContext);
        }

        /// <summary>
        /// Renders the block content in Markdown format in the format of output.
        /// </summary>
        /// <param name="output">The output writer.</param>
        /// <param name="options">The block helper options.</param>
        /// <param name="docContext">The documentation context used for text transformation.</param>
        /// <exception cref="HandlebarsException">Thrown when the transformation fails.</exception>
        private static void Markdown(EncodedTextWriter output, BlockHelperOptions options, IDocumentationContext docContext)
        {
            var markdown = options.Template();
            if (string.IsNullOrWhiteSpace(markdown))
                return;

            if (!docContext.TryTransformText(FileExtensions.Markdown, markdown, out var transformedText))
                throw new HandlebarsException($"{nameof(Markdown)} template helper function could not render the content: {markdown}");

            output.WriteSafeString(transformedText);
        }

        /// <summary>
        /// Renders the block content with all HTML tags stripped.
        /// </summary>
        /// <param name="output">The output writer.</param>
        /// <param name="options">The block helper options.</param>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        private static void StripTags(EncodedTextWriter output, BlockHelperOptions options, Context context, Arguments arguments)
        {
            var html = options.Template();
            if (!string.IsNullOrWhiteSpace(html))
            {
                var stripped = TagMatcher().Replace(html, string.Empty).Trim();
                output.WriteSafeString(stripped);
            }
        }

        [GeneratedRegex("<[^>]*>", RegexOptions.NonBacktracking | RegexOptions.Compiled)]
        private static partial Regex TagMatcher();
    }
}
