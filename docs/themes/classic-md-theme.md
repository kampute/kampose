# Classic Theme (Markdown)

The Classic Markdown theme generates clean, portable documentation suitable for wikis, repositories, and text-based documentation systems.

## Theme Settings

The theme supports the following customization settings:

| Name                                    | Type     | Default | Description                                                                          |
|-----------------------------------------|----------|---------|--------------------------------------------------------------------------------------|
| [`pageHeader`](#pageheader)             | markdown |         | Markdown inserted at the top of every generated page.                                |
| [`pageFooter`](#pagefooter)             | markdown |         | Markdown appended to the bottom of every generated page.                             |
| [`seeAlsoSubtopics`](#seealsosubtopics) | boolean  | `false` | Determines whether to include a "See Also" list of related subtopics on topic pages. |

### `pageHeader`

The `pageHeader` setting allows you to customize the content at the top of each generated page. You can use it for short branding lines, badges, or quick links. You can use Handlebars expressions within this content.

Consider the following when using the `pageHeader` setting:
- Verify that any HTML or raw tags you include are compatible with the Markdown processor you plan to use downstream.
- When linking to a relative content path, the path should be relative to the documentation root. Use `{{#rootRelativeUrl 'path/to/file'}}` for links.

#### Examples

The following example inserts a build status badge on top of each page:

```json
{
    "convention": "devops",
    "theme": "classic",
    "themeSettings": {
        "pageHeader": "[![Build](https://dev.azure.com/ORG/PROJECT/_apis/build/status/PIPELINE)](https://dev.azure.com/ORG/PROJECT/_build)"
    }
}
```

The example below inserts a table of contents using Azure DevOps specific marker:

```json
{
    "convention": "devops",
    "theme": "classic",
    "themeSettings": {
        "pageHeader": "[_TOC_]"
    }
}
```

You can also define your own template variables and use them in the header:

```json
{
    "convention": "devops",
    "theme": "classic",
    "themeSettings": {
        "prerelease": true,
        "pageHeader": "> {{#select prerelease 'Pre-release' 'Stable'}} Version"
    }
}
```

### `pageFooter`

The `pageFooter` setting allows you to add Markdown that will be appended to the bottom of every generated page. It's commonly used for license notes, contributor links, footnotes, or small badges. You can use Handlebars expressions within this content.

Consider the following when using the `pageFooter` setting:
- Verify that any HTML or raw tags you include are compatible with the Markdown processor you plan to use downstream.
- When linking to a relative content path, the path should be relative to the documentation root. Use `{{#rootRelativeUrl 'path/to/file'}}` for links.
- Do not include a horizontal rule (`---`) at the beginning of the footer content, the theme will automatically insert one for you.

#### Examples

The following example adds a link to the license file at the bottom of each page:

```json
{
    "convention": "devops",
    "theme": "classic",
    "themeSettings": {
        "pageFooter": "See [License]({{#rootRelativeUrl 'LICENSE'}}) for details."
    }
}
```

The example below demonstrates how to use template helpers in the footer:

```json
{
    "convention": "devops",
    "theme": "classic",
    "themeSettings": {
        "pageFooter": "_Copyright (c) {{now 'yyyy'}} [Example Corp](https://example.com)_"
    }
}
```

You can also define your own template variables and use them in the footer:

```json
{
    "convention": "devops",
    "theme": "classic",
    "themeSettings": {
        "buildNumber": "123",
        "pageFooter": "Build: {{buildNumber}}"
    }
}
```

### `seeAlsoSubtopics`

The `seeAlsoSubtopics` setting determines whether to include a "See Also" section after the main content of conceptual topics. By default, this setting is disabled.

When `seeAlsoSubtopics` is enabled (set to `true`) and a topic has child topics, the theme inserts a "See Also" section on the topic's page, containing a bullet list of direct child topics (subtopics) of the current topic. This is useful for improving discoverability and creating lightweight cross-navigation between related pages.

#### Example

The following configuration enables the "See Also" section for topics with child topics:

```json
{
    "convention": "devops",
    "theme": "classic",
    "themeSettings": {
        "seeAlsoSubtopics": true
    }
}
```

Suppose you have the following topic structure:

```txt
- Best Practices
  - Coding Guidelines
    - Style Guide
  - Testing Strategies
    - Unit Testing
    - Integration Testing
```

After enabling the `seeAlsoSubtopics` setting, the rendered output for "Best Practices" will include a "See Also" section as follows:

```md
## See Also

- [Coding Guidelines](coding-guidelines.md)
- [Testing Strategies](testing-strategies.md)
```
