// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates.Providers
{
    using HandlebarsDotNet.Iterators;
    using HandlebarsDotNet.MemberAccessors;
    using HandlebarsDotNet.ObjectDescriptors;
    using HandlebarsDotNet.PathStructure;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides object descriptor resolution for Handlebars templates with support for interface default implementations.
    /// </summary>
    /// <remarks>
    /// This descriptor provider extends standard reflection-based member access to handle interface properties with default
    /// implementations, which are not automatically exposed by standard .NET reflection on implementing types.
    /// <para>
    /// When a property is not found on the concrete type, this provider searches all implemented interfaces for a matching
    /// property getter with a default implementation and creates a descriptor that can invoke it to retrieve the value.
    /// </para>
    /// This is a workaround for Handlebars.NET's limitation in accessing C# 8.0+ interface default implementations and can
    /// be removed when the library natively supports this feature.
    /// <para>
    /// <note type="important" title="Important">
    /// This provider only supports parameterless properties; index properties are ignored.
    /// </note>
    /// </para>
    /// </remarks>
    public sealed class InterfaceDefaultObjectDescriptorProvider : IObjectDescriptorProvider
    {
        private readonly ConcurrentDictionary<Type, ObjectDescriptor?> cache = new();
        private readonly InterfaceDefaultMemberAccessor accessor = new();

        /// <summary>
        /// Initializes a new instance of <see cref="InterfaceDefaultObjectDescriptorProvider"/> class.
        /// </summary>
        public InterfaceDefaultObjectDescriptorProvider()
        {
        }

        /// <summary>
        /// Attempts to create an object descriptor for the specified type that can access interface default implementations.
        /// </summary>
        /// <param name="type">The type to create a descriptor for.</param>
        /// <param name="descriptor">The created object descriptor if the type has interface default implementations; otherwise, the default value.</param>
        /// <returns><see langword="true"/> if the type has interface default implementations and a descriptor was created; otherwise, <see langword="false"/>.</returns>
        public bool TryGetDescriptor(Type type, [NotNullWhen(true)] out ObjectDescriptor? descriptor)
        {
            if (cache.TryGetValue(type, out descriptor))
                return descriptor is not null;

            var map = PropertyMap.Get(type);
            if (map is null)
            {
                cache[type] = descriptor = null;
                return false;
            }

            cache[type] = descriptor = accessor.ToDescriptor(type);
            return true;
        }

        /// <summary>
        /// Custom member accessor that resolves both regular properties and interface default implementations.
        /// </summary>
        private sealed class InterfaceDefaultMemberAccessor : IMemberAccessor
        {
            /// <summary>
            /// Attempts to retrieve the value of a member, resolving properties dynamically based on the actual instance type.
            /// </summary>
            /// <param name="instance">The object instance to retrieve the member value from.</param>
            /// <param name="memberName">The chain segment representing the member name.</param>
            /// <param name="value">The member value if found; otherwise, <see langword="null"/>.</param>
            /// <returns><see langword="true"/> if the member was found and its value retrieved; otherwise, <see langword="false"/>.</returns>
            public bool TryGetValue(object instance, ChainSegment memberName, out object? value)
            {
                if (PropertyMap.Get(instance)!.TryGetValue(memberName.ToString(), out var property))
                {
                    value = property.GetValue(instance);
                    return true;
                }

                value = default;
                return false;
            }

            /// <summary>
            /// Creates an object descriptor for this accessor.
            /// </summary>
            /// <param name="type">The type the descriptor will be associated with.</param>
            /// <returns>An object descriptor that uses this accessor.</returns>
            public ObjectDescriptor ToDescriptor(Type type)
                => new(type, this, (descriptor, instance) => PropertyMap.Get(instance), descriptor => new EnumerableIterator<IEnumerable>(), []);
        }

        /// <summary>
        /// Provides cached property maps for inspected types.
        /// </summary>
        private static class PropertyMap
        {
            private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>?> cache = [];

            /// <summary>
            /// Gets the property map for the specified object instance.
            /// </summary>
            /// <param name="instance">The object instance to get the property map for.</param>
            /// <returns>The property map if the object's type has interface default implementations; otherwise, <see langword="null"/>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IReadOnlyDictionary<string, PropertyInfo>? Get(object instance) => cache.GetOrAdd(instance.GetType(), BuildMap);

            /// <summary>
            /// Gets the property map for the specified type.
            /// </summary>
            /// <param name="type">The type to get the property map for.</param>
            /// <returns>The property map if the type has interface default implementations; otherwise, <see langword="null"/>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IReadOnlyDictionary<string, PropertyInfo>? Get(Type type) => cache.GetOrAdd(type, BuildMap);

            /// <summary>
            /// Creates the property map for the specified type.
            /// </summary>
            /// <param name="type">The type to create the property map for.</param>
            /// <returns>The property map if the type has interface default implementations; otherwise, <see langword="null"/>.</returns>
            private static IReadOnlyDictionary<string, PropertyInfo>? BuildMap(Type type)
            {
                // Look for interface properties with default implementations
                var interfaceProperties = type
                    .GetInterfaces()
                    .SelectMany(i => i.GetProperties())
                    .Where(p => p.GetGetMethod() is { IsAbstract: false })
                    .ToList();

                // Only provide properties if there are any interface default implementations
                if (interfaceProperties.Count == 0)
                    return null;

                // Get all regular properties as well
                var regularProperties = type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetGetMethod() is { IsPublic: true });

                var properties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in regularProperties.Concat(interfaceProperties))
                {
                    if (p.GetIndexParameters().Length > 0)
                        continue;

                    if (!properties.TryAdd(p.Name, p) && ReferenceEquals(p.DeclaringType, type))
                        properties[p.Name] = p;
                }

                return properties;
            }
        }
    }
}
