// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Support
{
    using global::Json.Schema;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Helper methods for working with JSON files.
    /// </summary>
    public static class Json
    {
        /// <summary>
        /// The <see cref="JsonSerializerOptions"/> for parsing JSON files.
        /// </summary>
        private static readonly JsonSerializerOptions DeserializationOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            IgnoreReadOnlyFields = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// The <see cref="JsonSerializerOptions"/> for serializing objects to JSON strings.
        /// </summary>
        private static readonly JsonSerializerOptions SerializationOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            IgnoreReadOnlyFields = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// Parses a JSON string into the specified type.
        /// </summary>
        /// <typeparam name="T">The type to parse the JSON string into.</typeparam>
        /// <param name="json">The JSON string to parse.</param>
        /// <returns>The deserialized object.</returns>
        public static T? Parse<T>(string json) => JsonSerializer.Deserialize<T>(json, DeserializationOptions);

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <param name="value">The object to convert to a JSON string.</param>
        /// <returns>The JSON string representation of the object.</returns>
        public static string Stringify(object? value) => JsonSerializer.Serialize(value, SerializationOptions);

        /// <summary>
        /// Reads a JSON file and deserializes it into the specified type without validation.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON file into.</typeparam>
        /// <param name="path">The path to the JSON file.</param>
        /// <param name="validator">The validation function to apply to the JSON document.</param>
        /// <returns>The deserialized object.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
        /// <exception cref="ValidationException">Thrown when the file is empty or invalid.</exception>
        public static T ReadFile<T>(string path, Func<JsonDocument, IReadOnlyDictionary<string, string>?>? validator = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException($"{typeof(T).Name} file could not be found: {path}", path);

            JsonDocument jsonDocument;
            try
            {
                using var json = File.OpenRead(path);
                jsonDocument = JsonDocument.Parse(json);
            }
            catch (JsonException error)
            {
                throw new ValidationException($"{typeof(T).Name} file could not be parsed: {path}", [error.Message]);
            }

            if (validator?.Invoke(jsonDocument) is IReadOnlyDictionary<string, string> errors)
                throw new ValidationException($"{typeof(T).Name} file contains errors: {path}", errors.Values);

            try
            {
                return jsonDocument.Deserialize<T>(DeserializationOptions)!;
            }
            catch (JsonException error)
            {
                throw new ValidationException($"{typeof(T).Name} file contains error: {path}", [error.Message]);
            }
        }

        /// <summary>
        /// Reads a JSON file and deserializes it into the specified type with validation.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON file into.</typeparam>
        /// <param name="path">The path to the JSON file.</param>
        /// <returns>The deserialized object.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
        /// <exception cref="ValidationException">Thrown when the file is empty or invalid.</exception>
        public static T ReadFileWithSchemaValidation<T>(string path) => ReadFile<T>(path, ValidateJsonSchema<T>);

        /// <summary>
        /// Validates the specified JSON document against the schema for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to validate.</typeparam>
        /// <param name="jsonDocument">The JSON document to validate.</param>
        /// <returns>The validation errors, if any; otherwise, <see langword="null"/>.</returns>
        private static IReadOnlyDictionary<string, string>? ValidateJsonSchema<T>(JsonDocument jsonDocument)
        {
            var evaluationOptions = new EvaluationOptions
            {
                OutputFormat = OutputFormat.Hierarchical,
                RequireFormatValidation = true
            };

            using var schemaStream = GetJsonSchemaStream<T>();
            var schema = JsonSerializer.Deserialize<JsonSchema>(schemaStream, DeserializationOptions)!;
            var validation = schema.Evaluate(jsonDocument.RootElement, evaluationOptions);
            return validation.HasErrors ? validation.Errors : null;
        }

        /// <summary>
        /// Retrieves the JSON schema for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to retrieve the schema for.</typeparam>
        /// <returns>A stream containing the JSON schema.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the schema could not be found.</exception>
        private static FileStream GetJsonSchemaStream<T>()
        {
            var fileName = char.ToLowerInvariant(typeof(T).Name[0]) + typeof(T).Name[1..] + ".schema.json";
            var schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "json-schema", fileName);
            return File.OpenRead(schemaPath);
        }
    }
}
