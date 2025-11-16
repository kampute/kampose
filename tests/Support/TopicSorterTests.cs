// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Support
{
    using Kampose.Support;
    using Kampute.DocToolkit.Topics;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class TopicSorterTests
    {
        [Test]
        public void SortTopics_WithNullTopics_ThrowsArgumentNullException()
        {
            var explicitOrder = new[] { "file1.md" };

            Assert.Throws<ArgumentNullException>(() => TopicSorter.SortTopics(null!, explicitOrder));
        }

        [Test]
        public void SortTopics_WithNullExplicitOrder_ThrowsArgumentNullException()
        {
            var topics = new List<FileTopic>();

            Assert.Throws<ArgumentNullException>(() => TopicSorter.SortTopics(topics, null!));
        }

        [Test]
        public void SortTopics_WithEmptyExplicitOrder_ReturnsOriginalTopics()
        {
            var topics = new[]
            {
                CreateMockTopic("topic1", "docs/topic1.md"),
                CreateMockTopic("topic2", "docs/topic2.md"),
                CreateMockTopic("topic3", "docs/topic3.md")
            };
            var explicitOrder = Array.Empty<string>();

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result, Is.EqualTo(topics));
        }

        [Test]
        public void SortTopics_WithExactFilePathMatch_ReturnsOrderedTopics()
        {
            var topic1 = CreateMockTopic("Getting Started", "docs/getting-started.md");
            var topic2 = CreateMockTopic("Installation", "docs/installation.md");
            var topic3 = CreateMockTopic("Configuration", "docs/configuration.md");
            var topics = new[] { topic1, topic2, topic3 };
            var explicitOrder = new[] { "docs/configuration.md", "docs/getting-started.md" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["Configuration", "Getting Started", "Installation"]));
        }

        [Test]
        public void SortTopics_WithFilenameOnlyMatch_ReturnsOrderedTopics()
        {
            var topic1 = CreateMockTopic("Getting Started", "docs/getting-started.md");
            var topic2 = CreateMockTopic("Installation", "docs/installation.md");
            var topic3 = CreateMockTopic("Configuration", "docs/configuration.md");
            var topics = new[] { topic1, topic2, topic3 };
            var explicitOrder = new[] { "configuration.md", "getting-started.md" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["Configuration", "Getting Started", "Installation"]));
        }

        [Test]
        public void SortTopics_WithoutExtension_ReturnsOrderedTopics()
        {
            var topic1 = CreateMockTopic("Getting Started", "docs/getting-started.md");
            var topic2 = CreateMockTopic("Installation", "docs/installation.md");
            var topic3 = CreateMockTopic("Configuration", "docs/configuration.md");
            var topics = new[] { topic1, topic2, topic3 };
            var explicitOrder = new[] { "docs/configuration", "docs/getting-started" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["Configuration", "Getting Started", "Installation"]));
        }

        [Test]
        public void SortTopics_WithBackslashPath_ReturnsOrderedTopics()
        {
            var topic1 = CreateMockTopic("Getting Started", "docs\\getting-started.md");
            var topic2 = CreateMockTopic("Installation", "docs\\installation.md");
            var topic3 = CreateMockTopic("Configuration", "docs\\configuration.md");
            var topics = new[] { topic1, topic2, topic3 };
            var explicitOrder = new[] { "docs\\configuration.md", "docs\\getting-started.md" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["Configuration", "Getting Started", "Installation"]));
        }

        [Test]
        public void SortTopics_WithMixedSlashes_ReturnsOrderedTopics()
        {
            var topic1 = CreateMockTopic("Getting Started", "docs/getting-started.md");
            var topic2 = CreateMockTopic("Installation", "docs/installation.md");
            var topics = new[] { topic1, topic2 };
            var explicitOrder = new[] { "docs\\installation.md", "docs/getting-started.md" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["Installation", "Getting Started"]));
        }

        [Test]
        public void SortTopics_WithUnmatchedOrderItems_IgnoresUnmatched()
        {
            var topic1 = CreateMockTopic("Getting Started", "docs/getting-started.md");
            var topic2 = CreateMockTopic("Installation", "docs/installation.md");
            var topics = new[] { topic1, topic2 };
            var explicitOrder = new[] { "docs/nonexistent.md", "docs/installation.md", "docs/another-missing.md" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["Installation", "Getting Started"]));
        }

        [Test]
        public void SortTopics_WithPartialOrdering_SortsRemainingAlphabetically()
        {
            var topic1 = CreateMockTopic("Zebra", "docs/zebra.md");
            var topic2 = CreateMockTopic("Apple", "docs/apple.md");
            var topic3 = CreateMockTopic("Cherry", "docs/cherry.md");
            var topic4 = CreateMockTopic("Banana", "docs/banana.md");
            var topics = new[] { topic1, topic2, topic3, topic4 };
            var explicitOrder = new[] { "docs/cherry.md" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["Cherry", "Apple", "Banana", "Zebra"]));
        }

        [Test]
        public void SortTopics_WithWhitespaceInOrder_IgnoresWhitespace()
        {
            var topic1 = CreateMockTopic("Getting Started", "docs/getting-started.md");
            var topic2 = CreateMockTopic("Installation", "docs/installation.md");
            var topics = new[] { topic1, topic2 };
            var explicitOrder = new[] { "  ", "docs/installation.md", "", "docs/getting-started.md" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["Installation", "Getting Started"]));
        }

        [Test]
        public void SortTopics_WithNestedPath_MatchesCorrectly()
        {
            var topic1 = CreateMockTopic("Overview", "docs/guides/advanced/overview.md");
            var topic2 = CreateMockTopic("Getting Started", "docs/getting-started.md");
            var topics = new[] { topic1, topic2 };
            var explicitOrder = new[] { "guides/advanced/overview.md" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["Overview", "Getting Started"]));
        }

        [Test]
        public void SortTopics_WithRelativePathMatch_MatchesCorrectly()
        {
            var topic1 = CreateMockTopic("Overview", "docs/guides/overview.md");
            var topic2 = CreateMockTopic("Getting Started", "docs/getting-started.md");
            var topics = new[] { topic1, topic2 };
            var explicitOrder = new[] { "guides/overview" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["Overview", "Getting Started"]));
        }

        [Test]
        public void SortTopics_WithCaseSensitivePath_SortsAlphabeticallyByTitle()
        {
            var topic1 = CreateMockTopic("Zebra", "docs/zebra.md");
            var topic2 = CreateMockTopic("apple", "docs/apple.md");
            var topics = new[] { topic1, topic2 };
            var explicitOrder = Array.Empty<string>();

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["apple", "Zebra"]));
        }

        [Test]
        public void SortTopics_WithDuplicateExplicitOrder_UsesFirstMatch()
        {
            var topic1 = CreateMockTopic("Getting Started", "docs/getting-started.md");
            var topic2 = CreateMockTopic("Installation", "docs/installation.md");
            var topics = new[] { topic1, topic2 };
            var explicitOrder = new[] { "docs/installation.md", "docs/installation.md", "docs/getting-started.md" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["Installation", "Getting Started"]));
        }

        [Test]
        public void SortTopics_WithEmptyTopics_ReturnsEmpty()
        {
            var topics = Array.Empty<FileTopic>();
            var explicitOrder = new[] { "docs/file.md" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void SortTopics_WithAllTopicsInExplicitOrder_MaintainsExplicitOrder()
        {
            var topic1 = CreateMockTopic("First", "docs/first.md");
            var topic2 = CreateMockTopic("Second", "docs/second.md");
            var topic3 = CreateMockTopic("Third", "docs/third.md");
            var topics = new[] { topic1, topic2, topic3 };
            var explicitOrder = new[] { "docs/third.md", "docs/first.md", "docs/second.md" };

            var result = TopicSorter.SortTopics(topics, explicitOrder);

            Assert.That(result.Select(t => t.Title), Is.EqualTo(["Third", "First", "Second"]));
        }

        private static FileTopic CreateMockTopic(string title, string filePath)
        {
            return new FileTopic(Path.GetFileNameWithoutExtension(filePath), filePath)
            {
                Title = title
            };
        }
    }
}
