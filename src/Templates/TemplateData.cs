// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents the data context for a template, including both common data and entity-specific data.
    /// </summary>
    public sealed class TemplateData : IReadOnlyDictionary<string, object?>
    {
        private readonly IReadOnlyDictionary<string, object?> commonData;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateData"/> class with the specified common data and primary data.
        /// </summary>
        /// <param name="commonData">The common data shared across all templates.</param>
        /// <param name="primaryData">The primary data for the template.</param>
        /// <param name="primaryKey">The key used to access the primary data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="commonData"/> or <paramref name="primaryData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="primaryKey"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="commonData"/> contains the <paramref name="primaryKey"/>.</exception>
        public TemplateData(IReadOnlyDictionary<string, object?> commonData, object primaryData, string primaryKey)
        {
            ArgumentNullException.ThrowIfNull(commonData);
            ArgumentNullException.ThrowIfNull(primaryData);
            ArgumentException.ThrowIfNullOrEmpty(primaryKey);

            if (commonData.ContainsKey(primaryKey))
                throw new ArgumentException($"The common data dictionary must not contain the primary key '{primaryKey}'.", nameof(commonData));

            this.commonData = commonData;
            Primary = primaryData;
            PrimaryKey = primaryKey;
        }

        /// <summary>
        /// Gets the primary data associated with the current instance.
        /// </summary>
        /// <value>
        /// The primary data for the template, which is typically an entity or a specific piece of information relevant to the template.
        /// </value>
        public object Primary { get; }

        /// <summary>
        /// Gets the primary key used to identify the primary data.
        /// </summary>
        /// <value>
        /// The key used to access the primary data in the template data dictionary.
        /// </value>
        public string PrimaryKey { get; }

        /// <summary>
        /// Gets the number of items in the template data.
        /// </summary>
        /// <value>
        /// The number of items in the template data.
        /// </value>
        public int Count => commonData.Count + 1;

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the template data.</param>
        /// <value>
        /// The value associated with the specified key, or <see langword="null"/> if the key is not found.
        /// </value>
        public object? this[string key] => TryGetValue(key, out var value) ? value : null;

        /// <summary>
        /// Gets the collection of keys in the template data.
        /// </summary>
        /// <value>
        /// The collection of keys in the template data.
        /// </value>
        public IEnumerable<string> Keys
        {
            get
            {
                yield return PrimaryKey;
                foreach (var key in commonData.Keys)
                    yield return key;
            }
        }

        /// <summary>
        /// Gets the collection of values in the template data.
        /// </summary>
        /// <value>
        /// The collection of values in the template data.
        /// </value>
        public IEnumerable<object?> Values
        {
            get
            {
                yield return Primary;
                foreach (var key in commonData.Keys)
                    yield return commonData[key];
            }
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the template data.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the key was found; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(string key, out object? value)
        {
            if (IsPrimaryDataKey(key))
            {
                value = Primary;
                return true;
            }

            return commonData.TryGetValue(key, out value);
        }

        /// <summary>
        /// Determines whether the template data contains a specific key.
        /// </summary>
        /// <param name="key">The key to locate in the template data.</param>
        /// <returns><see langword="true"/> if the key is found; otherwise, <see langword="false"/>.</returns>
        public bool ContainsKey(string key) => IsPrimaryDataKey(key) || commonData.ContainsKey(key);

        /// <summary>
        /// Gets an enumerator that iterates through the template data.
        /// </summary>
        /// <returns>
        /// An enumerator that iterates through the template data.
        /// </returns>
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            yield return new KeyValuePair<string, object?>(PrimaryKey, Primary);
            foreach (var pair in commonData)
                yield return pair;
        }

        /// <summary>
        /// Gets an enumerator that iterates through the template data.
        /// </summary>
        /// <returns>
        /// An enumerator that iterates through the template data.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Determines whether the specified key is the primary data key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><see langword="true"/> if the key is the primary data key; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsPrimaryDataKey(string key) => string.Equals(key, PrimaryKey, StringComparison.OrdinalIgnoreCase);
    }
}
