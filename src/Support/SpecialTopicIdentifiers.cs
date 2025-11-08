// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Support
{
    using System;

    /// <summary>
    /// Provides constants for special topic identifiers used in documentation.
    /// </summary>
    public static class SpecialTopicIdentifiers
    {
        /// <summary>
        /// The identifier of the home topic, which is the main entry point for documentation.
        /// </summary>
        public const string Home = "WELCOME";

        /// <summary>
        /// The identifier of the API topic, which provides an overview of the API documentation.
        /// </summary>
        public const string Api = "API";

        /// <summary>
        /// Determines whether the specified topic identifier is considered a special topic.
        /// </summary>
        /// <param name="topicId">The identifier of the topic to evaluate.</param>
        /// <returns><see langword="true"/> if the topic identifier belongs to a special topic; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="topicId"/> is <see langword="null"/>.</exception>"
        public static bool IsSpecialTopic(string topicId)
        {
            ArgumentNullException.ThrowIfNull(topicId, nameof(topicId));

            var comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Equals(topicId, Home) || comparer.Equals(topicId, Api);
        }
    }
}
