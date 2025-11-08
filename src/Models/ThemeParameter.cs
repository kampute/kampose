// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a parameter definition for a theme with its associated metadata.
    /// </summary>
    public readonly struct ThemeParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeParameter"/> struct.
        /// </summary>
        /// <param name="type">The data type of the parameter.</param>
        /// <param name="description">An optional description of the parameter's purpose.</param>
        /// <param name="defaultValue">The value of the parameter.</param>
        /// <exception cref="FormatException">Thrown when the <paramref name="defaultValue"/>'s type doesn't match the expected <paramref name="type"/>.</exception>
        [JsonConstructor]
        public ThemeParameter(ThemeParameterType type, string? description = null, object? defaultValue = null)
        {
            Type = type;
            Description = description;
            DefaultValue = ValidateValue(defaultValue);
        }

        /// <summary>
        /// Gets the value of the parameter.
        /// </summary>
        /// <value>
        /// The value of the parameter, which can be a string, number, boolean, URI, list, or a dictionary
        /// </value>
        public readonly object? DefaultValue { get; }

        /// <summary>
        /// Gets the data type of the parameter.
        /// </summary>
        /// <value>
        /// The parameter's data type, which determines how the value should be interpreted and processed.
        /// </value>
        public readonly ThemeParameterType Type { get; }

        /// <summary>
        /// Gets an optional description of the parameter's purpose.
        /// </summary>
        /// <value>
        /// A human-readable description explaining the parameter's purpose and expected values, or <see langword="null"/> if no description is provided.
        /// </value>
        public readonly string? Description { get; }

        /// <summary>
        /// Validates the specified value against the parameter's data type.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>The validated value.</returns>
        /// <exception cref="FormatException">Thrown when the value's type doesn't match the expected parameter type.</exception>
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
                ThemeParameterType.String or ThemeParameterType.Markdown when obj is string => obj,
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
                ThemeParameterType.String or ThemeParameterType.Markdown when element.ValueKind is JsonValueKind.String => element.GetString(),
                ThemeParameterType.Number when element.ValueKind is JsonValueKind.Number => element.GetDouble(),
                ThemeParameterType.Boolean when element.ValueKind is JsonValueKind.False or JsonValueKind.True => element.GetBoolean(),
                ThemeParameterType.Uri when element.ValueKind is JsonValueKind.String => Uri.TryCreate(element.GetString(), UriKind.RelativeOrAbsolute, out var uri)
                    ? uri : throw new JsonException($"A valid URI was expected: {element.GetRawText()}"),
                ThemeParameterType.Array when element.ValueKind is JsonValueKind.Array => element.Deserialize<IEnumerable<object>>(),
                ThemeParameterType.Object when element.ValueKind is JsonValueKind.Object => element.Deserialize<IDictionary<string, object>>(),
                _ => throw new JsonException($"{expectedType} was expected but {AsString(element.ValueKind)} was provided: {element.GetRawText()}")
            };

            static string AsString(JsonValueKind kind) => kind is JsonValueKind.True or JsonValueKind.False ? "boolean" : kind.ToString().ToLowerInvariant();
        }
    }
}
