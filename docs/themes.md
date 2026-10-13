---
title: Themes
summary: "Overview of Kampose HTML and Markdown themes, theme selection, configuration, and rich content settings."
---

# Themes

Kampose themes define the templates, assets, parameters, and presentation of generated documentation. The selected documentation convention determines whether Kampose loads an HTML or Markdown theme.

## HTML-Based Themes (DotNet/DocFx Convention)

When `convention` is `dotNet` or `docFx`, Kampose loads an HTML theme. The built-in Classic theme provides a responsive documentation website with navigation, search, and configurable page components.

### Available Themes

The following table summarizes the available HTML themes:

| Name (Identifier)                       | Description                                                         |
|-----------------------------------------|---------------------------------------------------------------------|
| [classic](themes/classic-html-theme.md) | A classic theme for rendering API documentation in HTML format.     |

### Installing New Themes

To install an HTML theme, copy its directory into the `themes/html/` directory of the Kampose installation. The directory name is the theme identifier.

## Markdown-Based Themes (DevOps Convention)

When `convention` is `devOps`, Kampose loads a Markdown theme. The generated files are suitable for systems such as Azure DevOps Wiki that consume Markdown documentation.

### Available Themes

The following table summarizes the available Markdown themes:

| Name (Identifier)                       | Description                                                         |
|-----------------------------------------|---------------------------------------------------------------------|
| [classic](themes/classic-md-theme.md)   | A classic theme for rendering API documentation in Markdown format. |

### Installing New Themes

To install a Markdown theme, copy its directory into the `themes/md/` directory of the Kampose installation. The directory name is the theme identifier.

## Theme Configuration

Themes expose settings that customize their behavior and appearance. Configure them in `kampose.json` under `themeSettings`.

Each theme documents its available settings, including their purpose and usage. Consult the specific theme documentation to understand which settings are available and how to configure them.

### Configuration Example

The following excerpt demonstrates basic theme configuration:

> [!NOTE]
> This is a focused configuration excerpt. A complete configuration also requires `outputDirectory`.

```json
{
  "theme": "classic",
  "themeSettings": {
    "pageFooter": "Copyright © {{now 'yyyy'}} [Example Corp](https://example.com)"
  }
}
```

### Settings with Rich Content Support

Settings declared with the `markdown` parameter type accept Markdown and Handlebars expressions. The built-in themes use this type for settings such as page headers and footers.

Markdown settings accept either a single string or an array of strings. When an array is provided, the items are joined with newlines, offering a more readable and maintainable alternative to using escape sequences in JSON strings.

When configuring settings that accept Markdown content, follow these guidelines:

- **Documentation Links**: Prefix documentation-root-relative paths with `~/`, for example `[License](~/LICENSE)`. See
  [URL resolution](url-resolution.md) for document-relative, site-root-relative,
  and documentation-root-relative behavior.
- **Template Integration**: Markdown theme settings can use [template helpers](themes/theme-authoring/template-helpers.md) and [template variables](themes/theme-authoring.md#global-template-context). Ordinary Markdown topic files cannot.
- **Multi-Section Content**: Use an array of strings to define content with multiple sections or lines without concatenation.

For comprehensive information on expression syntax, refer to the [Handlebars documentation](https://handlebarsjs.com/guide/).

## Creating Custom Themes

Kampose allows you to create custom themes by following a structured approach. This involves setting up the theme directory, creating the necessary files, and configuring the theme settings.

You can find the detailed steps for creating custom themes, including examples, in the [theme authoring guide](themes/theme-authoring.md).
