// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Support
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Provides a JSON converter for collections that clears existing items before deserialization.
    /// This ensures that explicit collection assignments in JSON overwrite existing items rather than merging with them.
    /// </summary>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
    internal sealed class ClearableCollectionJsonConverter<TCollection, TItem> : JsonConverter<TCollection>
        where TCollection : ICollection<TItem>, new()
    {
        /// <summary>
        /// Reads and deserializes a JSON array into a collection, clearing it first.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The JSON serializer options.</param>
        /// <returns>The deserialized collection.</returns>
        /// <exception cref="JsonException">Thrown when the JSON is not in the expected format.</exception>
        public override TCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected start of array");

            var collection = new TCollection();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return collection;

                var item = JsonSerializer.Deserialize<TItem>(ref reader, options);
                if (item != null)
                    collection.Add(item);
            }

            throw new JsonException("Expected end of array");
        }

        /// <summary>
        /// Writes the collection to JSON.
        /// </summary>
        /// <param name="writer">The JSON writer.</param>
        /// <param name="value">The collection to write.</param>
        /// <param name="options">The JSON serializer options.</param>
        public override void Write(Utf8JsonWriter writer, TCollection value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
