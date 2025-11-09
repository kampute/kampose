// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Support
{
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Provides utility methods for sorting topics based on explicit ordering configuration.
    /// </summary>
    public static class TopicSorter
    {
        /// <summary>
        /// Sorts a collection of file topics according to an explicit ordering list.
        /// </summary>
        /// <param name="topics">The collection of topics to sort.</param>
        /// <param name="explicitOrder">The explicit ordering list containing filenames or file paths.</param>
        /// <returns>A sorted collection of topics.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="topics"/> or <paramref name="explicitOrder"/> is <see langword="null"/>.</exception>
        public static IEnumerable<FileTopic> SortTopics(IEnumerable<FileTopic> topics, IReadOnlyList<string> explicitOrder)
        {
            ArgumentNullException.ThrowIfNull(topics);
            ArgumentNullException.ThrowIfNull(explicitOrder);

            if (explicitOrder.Count == 0)
                return topics;

            var remaining = new List<FileTopic>(topics);
            var explicitlyOrdered = new List<FileTopic>();

            foreach (var orderItem in explicitOrder)
            {
                if (string.IsNullOrWhiteSpace(orderItem))
                    continue;

                var relativePath = orderItem.Replace('\\', '/');
                var matchingTopic = remaining.FirstOrDefault(topic => IsMatch(topic, relativePath));
                if (matchingTopic is null)
                    continue;

                explicitlyOrdered.Add(matchingTopic);
                remaining.Remove(matchingTopic);
            }

            return explicitlyOrdered.Concat(remaining.OrderBy(topic => topic.Title, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines if a topic matches an order item based on relative path with or without extension.
        /// </summary>
        /// <param name="topic">The topic to check for a match.</param>
        /// <param name="relativePath">The relative path to match against the topic's source path.</param>
        /// <returns><see langword="true"/> if the topic's file path matches the relative path; otherwise, <see langword="false"/>.</returns>
        private static bool IsMatch(FileTopic topic, string relativePath)
        {
            var sourcePath = topic.FilePath.Replace('\\', '/');
            if (PathHelper.IsSubpath(sourcePath, relativePath))
                return true;

            var extension = Path.GetExtension(sourcePath);
            if (extension.Length > 0 && PathHelper.IsSubpath(sourcePath[..^extension.Length], relativePath))
                return true;

            return false;
        }
    }
}
