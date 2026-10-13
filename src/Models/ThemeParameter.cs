// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Defines a theme parameter's expected type, description, and fallback value.
    /// </summary>
    public readonly struct ThemeParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeParameter"/> struct.
        /// </summary>
        /// <param name="type">The data type of the parameter.</param>
        /// <param name="description">An optional explanation of the parameter's purpose and expected value.</param>
        /// <param name="defaultValue">The fallback value used when the theme setting is absent or <see langword="null"/>.</param>
        /// <exception cref="FormatException">
        /// <paramref name="defaultValue"/> does not match <paramref name="type"/>.
        /// </exception>
        /// <exception cref="JsonException">
        /// <paramref name="defaultValue"/> is a JSON value that does not match <paramref name="type"/>.
        /// </exception>
        [JsonConstructor]
        public ThemeParameter(ThemeParameterType type, string? description = null, object? defaultValue = null)
        {
            Type = type;
            Description = description;
            DefaultValue = ValidateValue(defaultValue);
        }

        /// <summary>
        /// Gets the parameter's fallback value.
        /// </summary>
        /// <value>
        /// The validated fallback value, or <see langword="null"/> when no fallback is defined.
        /// </value>
        public readonly object? DefaultValue { get; }

        /// <summary>
        /// Gets the data type of the parameter.
        /// </summary>
        /// <value>
        /// The expected data type used to validate configured and fallback values.
        /// </value>
        public readonly ThemeParameterType Type { get; }

        /// <summary>
        /// Gets the user-facing description of the parameter.
        /// </summary>
        /// <value>
        /// An explanation of where the parameter is used, its effect, and any expected structure or interactions;
        /// or <see langword="null"/> when no description is provided.
        /// </value>
        public readonly string? Description { get; }

        /// <summary>
        /// Validates and normalizes a value according to the parameter's declared type.
        /// </summary>
        /// <param name="value">The value to validate, or <see langword="null"/>.</param>
        /// <returns>
        /// The validated value; a normalized <see cref="Uri"/> for URI parameters; newline-joined text for Markdown
        /// arrays; or <see langword="null"/> when <paramref name="value"/> is <see langword="null"/>.
        /// </returns>
        /// <exception cref="FormatException">
        /// A non-JSON value does not match <see cref="Type"/> or a Markdown sequence contains a non-string item.
        /// </exception>
        /// <exception cref="JsonException">
        /// A JSON value does not match <see cref="Type"/>, is not a valid URI, or a Markdown array contains a
        /// non-string item.
        /// </exception>
        public object? ValidateValue(object? value)
        {
            switch (value)
            {
                case null:
                    return null;
                case JsonElement jsonElement:
                    return ValidateJsonValue(jsonElement, Type);
                default:
                    return ValidateObjectValue(value, Type);
            }

            static object ValidateObjectValue(object obj, ThemeParameterType expectedType) => expectedType switch
            {
                ThemeParameterType.String when obj is string => obj,
                ThemeParameterType.Markdown when obj is string => obj,
                ThemeParameterType.Markdown when obj is IEnumerable enumerable => JoinMarkdownArray(enumerable),
                ThemeParameterType.Number when obj is int or long or float or double or decimal => obj,
                ThemeParameterType.Boolean when obj is bool => obj,
                ThemeParameterType.Uri when obj is string uriString => Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri)
                    ? uri : throw new FormatException($"A valid URI was expected: {uriString}"),
                ThemeParameterType.Array when obj is IEnumerable => obj,
                ThemeParameterType.Object when obj is IDictionary => obj,
                _ => throw new FormatException($"{expectedType} was expected but {obj.GetType().Name} was provided: {obj}")
            };

            static object? ValidateJsonValue(JsonElement element, ThemeParameterType expectedType) => expectedType switch
            {
                ThemeParameterType.String when element.ValueKind is JsonValueKind.String => element.GetString(),
                ThemeParameterType.Markdown when element.ValueKind is JsonValueKind.String => element.GetString(),
                ThemeParameterType.Markdown when element.ValueKind is JsonValueKind.Array => JoinMarkdownArray(element),
                ThemeParameterType.Number when element.ValueKind is JsonValueKind.Number => element.GetDouble(),
                ThemeParameterType.Boolean when element.ValueKind is JsonValueKind.False or JsonValueKind.True => element.GetBoolean(),
                ThemeParameterType.Uri when element.ValueKind is JsonValueKind.String => Uri.TryCreate(element.GetString(), UriKind.RelativeOrAbsolute, out var uri)
                    ? uri : throw new JsonException($"A valid URI was expected: {element.GetRawText()}"),
                ThemeParameterType.Array when element.ValueKind is JsonValueKind.Array => element.Deserialize<IEnumerable<object>>(),
                ThemeParameterType.Object when element.ValueKind is JsonValueKind.Object => element.Deserialize<IDictionary<string, object>>(),
                _ => throw new JsonException($"{expectedType} was expected but {AsString(element.ValueKind)} was provided: {element.GetRawText()}")
            };

            static string AsString(JsonValueKind kind) => kind is JsonValueKind.True or JsonValueKind.False ? "boolean" : kind.ToString().ToLowerInvariant();

            static string JoinMarkdownArray(object value)
            {
                var items = value switch
                {
                    JsonElement element when element.ValueKind is JsonValueKind.Array =>
                        element.EnumerateArray().Select(e => e.ValueKind is JsonValueKind.String
                            ? e.GetString() ?? string.Empty
                            : throw new JsonException($"All array items must be strings for Markdown type: {e.GetRawText()}")),
                    IEnumerable enumerable =>
                        enumerable.Cast<object>().Select(item => item is string str
                            ? str
                            : throw new FormatException($"All array items must be strings for Markdown type, but {item.GetType().Name} was provided")),
                    _ => throw new InvalidOperationException("Unexpected value type for JoinMarkdownArray")
                };

                return string.Join(Environment.NewLine, items);
            }
        }
    }
}
