# Special Rendering in Templates

When working with Handlebars templates, most template variables render as plain text using their string representation. However, Kampose automatically applies special rendering for certain documentation entities to create properly formatted output with cross-reference links.

The following entities receive special rendering:

| Entity                              | Rendering Behavior                                                   |
|-------------------------------------|----------------------------------------------------------------------|
| [Code Members](#code-members)       | Rendered as clickable links to their documentation pages             |
| [Attributes](#attributes)           | Rendered as they appear in source code with links to attribute types |
| [Documentation Comments](#documentation-comments) | Rendered with content transformed to output format     |
| [Topics](#topics)                   | Rendered as formatted content with adjusted cross-references         |

This special rendering activates automatically when these entities appear in your template data. Kampose handles the complexity of creating proper links, applying language-specific formatting, and adjusting references to work within your documentation structure.

## Code Members

Code members such as namespaces, types, methods, properties, and fields are automatically rendered as links to their documentation pages.

### Behavior

Complex member references are broken down into components, with each part linked to its documentation page. The form of these links depends on your chosen output format.

Member names are qualified with their declaring type when appropriate, creating clear navigation paths through your API documentation.

For example, consider this member reference:

```csharp
ICollection<DateTime>.Add(DateTime item)
```

Kampose will render it with links for each component:

- `ICollection<>` links to the `ICollection<T>` documentation
- `DateTime` links to the `DateTime` documentation
- `Add` links to the `Add` method documentation

The rendered output will look like this:
> [ICollection](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1)<[DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime)>.[Add](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1.add)([DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime))

### Usage Examples

```hbs
{{!-- Renders a link to documentation for the return type --}}
<div class="method-return-value">{{model.metadata.return.type}}</div>

{{!-- Renders a link to documentation of each declared member --}}
<ul>
  {{#each model.Members}}
    <li>{{this}}</li> {{!-- Each member automatically rendered with links --}}
  {{/each}}
</ul>
```

## Attributes

Attributes applied to code elements are rendered as they appear in source code, with links to the attribute type documentation and proper syntax formatting.

### Behavior

Constructor parameters and properties are formatted using language-specific syntax. Type references within attribute arguments are automatically linked to their documentation pages.

For example, consider this attribute usage:

```csharp
[XmlElement(typeof(Manager), ElementName = "Manager", Form = XmlSchemaForm.Qualified)]
public Employee Manager { get; set; }
```

Kampose will render it with links for the relevant types:

- `XmlElement` links to the `XmlElementAttribute` documentation
- `Manager` links to the `Manager` type documentation
- `XmlSchemaForm` links to the enumeration documentation

The rendered output will look like this:
> [XmlElement](https://learn.microsoft.com/dotnet/api/system.xml.serialization.xmlelementattribute)(typeof([Manager](#)), ElementName = "Manager", Form = [XmlSchemaForm](https://learn.microsoft.com/dotnet/api/system.xml.schema.xmlschemaform).Qualified)

### Usage Examples

```hbs
{{!-- Render attributes applied to a member --}}
{{#if model.metadata.customAttributes}}
  <div class="attributes">
    <h3>Attributes</h3>
    <ul>
      {{#each model.metadata.customAttributes}}
        <li>{{this}}</li> {{!-- Each attribute automatically rendered with links --}}
      {{/each}}
    </ul>
  </div>
{{/if}}
```

## Documentation Comments

Top-level XML documentation tags (such as `<summary>`, `<remarks>`, `<returns>`) have their content transformed to match your chosen output format.

### Behavior

When rendering documentation comment content:

- Special characters are escaped according to the output format (HTML or Markdown)
- Inline XML tags (like `<see>`, `<paramref>`, `<c>`, `<code>`) are converted to their equivalent HTML or Markdown elements
- Cross-references are adjusted to point to the correct documentation pages

### Usage Examples

```hbs
{{!-- Summary comments --}}
<div class="summary">{{model.doc.summary}}</div>

{{!-- Remarks section --}}
{{#if model.doc.remarks}}
  <div class="remarks">
    <h3>Remarks</h3>
    {{model.doc.remarks}}
  </div>
{{/if}}

{{!-- Return value documentation --}}
<h3>Returns</h3>
{{model.metadata.return.type}}
{{#if model.doc.returnDescription}}
  <div class="returns">
    {{model.doc.returnDescription}}
  </div>
{{/if}}
```

## Topics

Custom documentation topics (conceptual content, guides, tutorials) are rendered with proper formatting and working cross-references.

### Behavior

Topic content is automatically adjusted to work within your documentation structure. Cross-references are updated to point to the correct pages, and the content is formatted according to your chosen output format.

### Usage Examples

```hbs
{{!-- Topics are usually top-level models --}}
<div class="topic-content">
{{model}}  {{!-- Topic content automatically rendered --}}
</div>
