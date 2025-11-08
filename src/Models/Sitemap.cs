// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a hierarchical navigation structure for a documentation site.
    /// </summary>
    /// <remarks>
    /// The sitemap structure adapts to the page granularity configuration, providing appropriate organizational
    /// depth for different documentation styles and user navigation patterns.
    /// </remarks>
    public class Sitemap : IEnumerable<SitemapNode>
    {
        private readonly List<SitemapNode> nodes = [];
        private readonly Lazy<int> pageCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sitemap"/> class from a documentation context.
        /// </summary>
        /// <param name="context">The documentation context from which to generate the site map.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        public Sitemap(IDocumentationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            // Begin a new URL scope to ensure that all URLs are relative to the documentation root.
            using var _ = context.AddressProvider.BeginScope(string.Empty, null);

            BaseUrl = context.AddressProvider.ActiveScope.RootUrl;
            pageCount = new Lazy<int>(() => nodes.Sum(node => node.CountPages()));
            if (context.Assemblies.Count != 0)
                nodes.Add(CreateApiNode(context));
            if (context.Topics.Count != 0)
                nodes.Add(CreateTopicsNode(context));
        }

        /// <summary>
        /// Gets the base URL for the documentation site.
        /// </summary>
        /// <value>
        /// The base URL of the documentation site.
        /// </value>
        public Uri BaseUrl { get; }

        /// <summary>
        /// Gets the total number of pages available.
        /// </summary>
        /// <value>
        /// The total number of pages represented by the sitemap.
        /// </value>
        public int PageCount => pageCount.Value;

        /// <summary>
        /// Returns an enumerator that iterates through the collection of <see cref="SitemapNode"/> objects.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> for iterating through the collection of <see cref="SitemapNode"/> objects.</returns>
        public IEnumerator<SitemapNode> GetEnumerator() => nodes.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Builds the root sitemap node for documentation topics, grouping all top-level topics.
        /// </summary>
        /// <param name="context">The documentation context containing the topics to be included.</param>
        /// <returns>A <see cref="SitemapNode"/> representing the root of the topics documentation hierarchy.</returns>
        private SitemapNode CreateTopicsNode(IDocumentationContext context)
        {
            var topLevelTopics = context.Topics.Where(static t => t.ParentTopic is null);
            return new SitemapNode(GroupNames.Topics, CreateTopicHierarchyNodes(topLevelTopics));
        }

        /// <summary>
        /// Recursively builds a hierarchy of sitemap nodes for the given topics and their subtopics.
        /// </summary>
        /// <param name="topics">The collection of topics to process.</param>
        /// <returns>An enumerable sequence of <see cref="SitemapNode"/> objects representing the topic hierarchy.</returns>
        private IEnumerable<SitemapNode> CreateTopicHierarchyNodes(IEnumerable<TopicModel> topics)
        {
            foreach (var topic in topics)
            {
                if (topic.Subtopics.Count > 0)
                {
                    var subtopicElements = CreateTopicHierarchyNodes(topic.Subtopics);
                    yield return new SitemapNode(topic.Name, EnsureRelative(topic.Url), subtopicElements);
                }
                else
                {
                    yield return new SitemapNode(topic.Name, EnsureRelative(topic.Url));
                }
            }
        }

        /// <summary>
        /// Builds the root API documentation node, grouping namespaces or types according to the configured page granularity.
        /// </summary>
        /// <param name="context">The documentation context used to retrieve namespaces, types, and address information.</param>
        /// <returns>A <see cref="SitemapNode"/> representing the root of the API documentation hierarchy.</returns>
        private SitemapNode CreateApiNode(IDocumentationContext context)
        {
            var granularity = context.AddressProvider.Granularity;
            var nodes = granularity.HasFlag(PageGranularity.Namespace)
                ? context.Namespaces.Select(ns => CreateNamespaceNode(ns, granularity))
                : context.Types.Select(type => CreateTypeNode(type, granularity));

            return new SitemapNode(GroupNames.Api, nodes);
        }

        /// <summary>
        /// Builds a sitemap node for a single namespace, optionally grouping its types by category.
        /// </summary>
        /// <param name="ns">The namespace information, including its name, URL, and associated types.</param>
        /// <param name="granularity">The granularity level that determines how the namespace is represented in the sitemap.</param>
        /// <returns>A <see cref="SitemapNode"/> representing the namespace and its types.</returns>
        private SitemapNode CreateNamespaceNode(NamespaceModel ns, PageGranularity granularity)
        {
            var nsTypes = granularity.HasFlag(PageGranularity.Type)
                ? ns.Types.Select(type => CreateTypeNode(type, granularity))
                : [];

            return new(ns.Name, EnsureRelative(ns.Url), nsTypes);
        }

        /// <summary>
        /// Builds a sitemap node for a single type, optionally grouping its members if it is a composite type.
        /// </summary>
        /// <param name="type">The type to process.</param>
        /// <param name="granularity">The page granularity to use for grouping members.</param>
        /// <returns>A <see cref="SitemapNode"/> representing the type and its members (if applicable).</returns>
        private SitemapNode CreateTypeNode(TypeModel type, PageGranularity granularity)
        {
            var typeMembers = granularity.HasFlag(PageGranularity.Member) && type is not EnumModel
                ? CreateMemberGroups(type.Members)
                : [];

            return new(type.Name, EnsureRelative(type.Url), typeMembers);
        }

        /// <summary>
        /// Groups type members by their category (e.g., constructors, methods, properties) for navigation purposes.
        /// </summary>
        /// <param name="members">The members to categorize and group.</param>
        /// <returns>An enumerable sequence of <see cref="SitemapNode"/> objects representing member categories and their members.</returns>
        private IEnumerable<SitemapNode> CreateMemberGroups(IEnumerable<TypeMemberModel> members)
        {
            foreach (var group in members.GroupBy(GetMemberGroupName))
            {
                if (string.IsNullOrEmpty(group.Key))
                    continue;

                if (group.Key == GroupNames.Constructors)
                {
                    var representative = group.First();
                    yield return new(group.Key, EnsureRelative(representative.Url));
                }
                else
                {
                    var representative = group
                        .GroupBy(member => member.Name)
                        .Select(overloads => overloads.First())
                        .Select(member => new SitemapNode(member.Name, EnsureRelative(member.Url)));

                    yield return new(group.Key, representative);
                }
            }
        }

        /// <summary>
        /// Returns the display group name for a member, based on its type and explicitness.
        /// </summary>
        /// <param name="member">The member to categorize.</param>
        /// <returns>The display name of the group to which the member belongs, or an empty string if uncategorized.</returns>
        private static string GetMemberGroupName(TypeMemberModel member) => member switch
        {
            PropertyModel p => p.Metadata.IsExplicitInterfaceImplementation ? GroupNames.ExplicitInterfaceImplementations : GroupNames.Properties,
            MethodModel m => m.Metadata.IsExplicitInterfaceImplementation ? GroupNames.ExplicitInterfaceImplementations : GroupNames.Methods,
            EventModel e => e.Metadata.IsExplicitInterfaceImplementation ? GroupNames.ExplicitInterfaceImplementations : GroupNames.Events,
            ConstructorModel => GroupNames.Constructors,
            OperatorModel => GroupNames.Operators,
            FieldModel => GroupNames.Fields,
            _ => string.Empty
        };

        /// <summary>
        /// Converts the specified <see cref="Uri"/> to a relative URL (relative to the documentation root) and removes any fragment.
        /// </summary>
        /// <param name="url">The <see cref="Uri"/> to convert.</param>
        /// <returns>A relative <see cref="Uri"/> without a fragment, suitable for use in navigation.</returns>
        private Uri EnsureRelative(Uri url)
        {
            var noFragmentUrl = url.WithoutFragment();
            return BaseUrl.IsAbsoluteUri
                ? BaseUrl.MakeRelativeUri(noFragmentUrl)
                : noFragmentUrl;
        }

        /// <summary>
        /// Represents the names of the various groups in the sitemap.
        /// </summary>
        private static class GroupNames
        {
            public const string Topics = "Topics";
            public const string Api = "API";
            public const string Constructors = "Constructors";
            public const string Fields = "Fields";
            public const string Properties = "Properties";
            public const string Methods = "Methods";
            public const string Events = "Events";
            public const string Operators = "Operators";
            public const string ExplicitInterfaceImplementations = "Explicit Interface Implementations";
        }
    }
}
