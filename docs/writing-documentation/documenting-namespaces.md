---
title: Documenting Namespaces
summary: "How to provide namespace-level XML documentation with the NamespaceDoc convention."
---

# Documenting Namespaces

C# does not provide a namespace documentation comment that is emitted to the compiler-generated XML documentation file. Kampose supports the established `NamespaceDoc` convention as a source for namespace-level documentation.

Add a type named `NamespaceDoc` to the namespace and place the namespace documentation on that type:

```csharp
namespace MyLibrary.Collections;

/// <summary>
/// Provides collection types and related utilities.
/// </summary>
/// <remarks>
/// Start with <see href="collections-overview">the collections overview</see>.
/// </remarks>
/// <seealso cref="MyLibrary.Collections.Specialized" />
internal static class NamespaceDoc
{
}
```

The type can be internal because it exists only to carry documentation. Kampose uses its supported XML documentation elements as the namespace documentation rather than presenting `NamespaceDoc` as part of the public API.

Create one `NamespaceDoc` type in each namespace that needs its own summary, remarks, examples, or related links.
