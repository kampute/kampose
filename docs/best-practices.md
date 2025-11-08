# Best Practices

To create high-quality API documentation with Kampose, follow these recommendations for writing effective and maintainable XML documentation comments.

## XML Documentation Structure

Proper XML documentation structure ensures that Kampose can generate comprehensive and well-organized API documentation. The following guidelines establish the essential elements and recommended practices for structuring your XML documentation comments.

### Essential Documentation Elements

- Every public and protected API should include at least a `<summary>` tag that clearly describes the purpose and functionality of the member. This forms the foundation of your API documentation and appears prominently in generated output
- All method or constructor parameters must be documented with `<param>` tags, and generic type parameters should be described using `<typeparam>` tags
- When referencing parameters or type parameters within documentation, use `<paramref>` and `<typeparamref>` as appropriate
- The `<returns>` tag should be used to explain method return values, including edge cases and nullability

### Additional Documentation Elements

- For properties, `<value>` tags should clarify semantics, acceptable ranges, and defaults
- All exceptions that may be thrown should be documented with `<exception>` tags, specifying the conditions under which they occur
- When a method triggers an event, it should be documented with `<event>` tags, describing the event and the conditions under which it is raised
- If applicable, security requirements should be described using `<permission>` tags

### Cross-References and Relationships

- Use `<see>` and `<seealso>` tags to cross-reference related members
- Take advantage of `<inheritdoc />` to reduce duplication in derived classes
- For improved readability, use `<para>` to structure longer documentation into logical paragraphs

### Common Pitfalls

- Avoid using `<seealso>` inline within text, as it is intended as a top-level tag that creates a separate "See Also" section. Inline references should use `<see>`
- Only one `<summary>` and one `<remarks>` tag are permitted per member; use `<para>` for additional paragraphs within these sections
- Do not nest block-level tags such as `<code>`, `<list>`, or `<note>` inside inline tags or table cells, as this can disrupt formatting in some output formats

## Documenting Namespaces

.NET compilers do not natively support XML documentation comments for namespaces. To provide documentation for a namespace, define a class named `NamespaceDoc` within the desired namespace and add XML comments to that class. Kampose recognizes this convention and uses the documentation from `NamespaceDoc` as the namespace-level documentation.

```csharp
namespace MyNamespace
{
    /// <summary>
    /// Summary for MyNamespace.
    /// </summary>
    /// <remarks>
    /// Remarks for MyNamespace.
    /// </remarks>
    /// <example>
    /// Example usage of MyNamespace.
    /// </example>
    /// <seealso cref="MyOtherNamespace"/>
    internal static class NamespaceDoc
    {
        // This class will not be exported into the assembly.
        // It exists only to hold the namespace documentation.
    }
}
```

## Referencing Conceptual Topics

Kampose allows you to seamlessly reference conceptual topics from within your API documentation, creating comprehensive documentation that connects code-level details with broader concepts and guides. This capability enables you to provide users with a complete understanding by linking API members to relevant tutorials, design patterns, or architectural explanations.

### Linking from Markdown Files

For referencing a topic in another topic, use a relative link from the current file's location. For example, if you want to link to `docs/advanced.md` from `docs/basics/overview.md`, you could use `[Advanced Concepts](../advanced.md)`.

The same approach applies when linking to asset files, like images: `![Diagram](../images/diagram.png)`.

When your editor is able to follow relative links, you can be confident that the links will work correctly in the generated documentation.

### Linking from XML Documentation

For linking to a topic in XML documentation comments, use the `<see>` or `<seealso>` tags with the appropriate `href` attribute. You can use the path to its source location. The path does not need to be absolute but it should uniquely identify the target topic.

If you do not specify a text for the link, the title of the referenced topic will be used as the link text.

## Markdown Output Considerations

When authoring documentation intended for Markdown output, ensure that all tables include a header row. Omitting the `<listheader>` element in a `<list type="table">` will result in an invalid table that may not render correctly.

Furthermore, Markdown renderers do not support block elements or line breaks within table cells; restrict content in these cells to a single paragraph of text and inline formatting only.

## Related Resources
- [Supported XML Documentation Tags](xmldoc-tags.md)
- [Recommended XML documentation tags](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags)
