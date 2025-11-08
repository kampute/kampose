# Themes

Kampose themes define the convention, content structure, and visual presentation of generated documentation. Themes control how your API documentation appears and behaves in the final output.

## HTML-Based Themes (DotNet/DocFx Convention)

When the `convention` is set to `dotNet` or `docFx`, Kampose uses HTML themes. These themes are optimized for web-based documentation. The HTML themes support advanced features like responsive design, interactive elements, and dynamic content.

### Available Themes

The following table summarizes the available HTML themes:

| Name (Identifier)                       | Description                                                         |
|-----------------------------------------|---------------------------------------------------------------------|
| [classic](themes/classic-html-theme.md) | A classic theme for rendering API documentation in HTML format.     |

### Installing New Themes

To install an HTML theme, simply copy the directory containing the theme files into the `themes/html/` directory of your Kampose installation. The name of the folder determines the theme's name (identifier).

## Markdown-Based Themes (DevOps Convention)

When the `convention` is set to `devOps`, Kampose uses Markdown themes. These themes are optimized for generating Markdown documentation. These type of themes focus on simplicity and readability, making it easy to create and maintain documentation in plain text format.

### Available Themes

The following table summarizes the available Markdown themes:

| Name (Identifier)                       | Description                                                         |
|-----------------------------------------|---------------------------------------------------------------------|
| [classic](themes/classic-md-theme.md)   | A classic theme for rendering API documentation in Markdown format. |

### Installing New Themes

To install a Markdown theme, copy the directory containing the theme files into the `themes/md/` directory of your Kampose installation. The name of the folder determines the theme's name (identifier).

## Theme Configuration

Themes expose configurable settings that allow you to customize their behavior and appearance. You can configure these settings in the `Kampose.json` file under the `themeSettings` section.

Each theme documents its available settings, including their purpose and usage. Consult the specific theme documentation to understand which settings are available and how to configure them.

### Configuration Example

The following example demonstrates basic theme configuration in the `Kampose.json` file:

> Note: This is an excerpt (not the entire configuration).

```json
{
  "theme": "classic",
  "themeSettings": {
    "pageFooter": "Copyright © {{now 'yyyy'}} [Example Corp](https://example.com)"
  }
}
```

### Settings with Rich Content Support

Many theme settings accept Markdown content and support Handlebars expressions, enabling dynamic and rich customization. This allows you to include formatted text, links, and template-generated content in your theme settings.

When configuring settings that accept Markdown content, follow these guidelines:

- **Relative Links**: Use root-relative paths with the `{{#rootRelativeUrl 'path/to/file'}}` helper to generate correct links regardless of the current document's location.
- **Template Integration**: Leverage [template helpers](themes/theme-authoring/template-helpers.md) and [template variables](themes/theme-authoring.md#global-template-context) to create dynamic content.

For comprehensive information on expression syntax, refer to the [Handlebars documentation](https://handlebarsjs.com/guide/).

## Creating Custom Themes

Kampose allows you to create custom themes by following a structured approach. This involves setting up the theme directory, creating the necessary files, and configuring the theme settings.

You can find the detailed steps for creating custom themes, including examples, in the [theme authoring guide](themes/theme-authoring.md).
