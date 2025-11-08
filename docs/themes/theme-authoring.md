# Creating Custom Themes

Kampose themes define the layout, content structure, and visual presentation of generated documentation. This guide covers the complete process of creating custom themes from initial setup to advanced customization.

## Theme Organization

Kampose organizes themes by output format within the installation's `themes/` directory:

```txt
themes/
├── html/          # HTML-based themes (dotNet/docFx convention)
│   └── myTheme/
└── md/            # Markdown-based themes (devOps convention)
    └── myTheme/
```

Each theme resides in its own subdirectory, where the directory name serves as the theme identifier for configuration references and CLI commands.

## Theme Structure

While Kampose does not enforce a rigid directory structure, the following organization provides clear separation of concerns and maintainability:

```txt
myTheme/
├── theme.json      # Theme configuration (required)
├── templates/      # Handlebars templates
├── styles/         # CSS stylesheets
├── scripts/        # JavaScript files
└── assets/         # Static resources (images, fonts, etc.)
```

## Theme Configuration

The `theme.json` file contains all theme settings and must be present in the theme root directory.

### Configuration Schema

The configuration file supports the following properties:

- **`base`** *(string|null)*
  Identifier of a parent theme to inherit from. Use `null` for standalone themes.

- **`metadata`** *(object|null)* \
  Optional theme information:
  - `name` *(string|null)*: Display name
  - `version` *(string|null)*: Version number
  - `format` *(string|null)*: Output format ("html" or "md")
  - `author` *(string|null)*: Author or organization name
  - `license` *(string|null)*: License information, typically a SPDX identifier
  - `homepage` *(string|null)*: Theme's homepage or documentation URL
  - `description` *(string|null)*: Brief description of the theme and its features

- **`templates`** *(array)* \
  Glob patterns matching template files (e.g., `templates/**/*.hbs`).

- **`scripts`** *(object)* \
  JavaScript bundling configuration:
  - `source` *(array)*: Glob patterns for JavaScript files, relative to the theme root.
  - `targetPath` *(string)*: Bundle output file path relative to the documentation root (default: `scripts.js`).

- **`styles`** *(object)* \
  CSS bundling configuration:
  - `source` *(array)*: Glob patterns for CSS files, relative to the theme root.
  - `targetPath` *(string)*: Bundle output file path relative to the documentation root (default: `styles.css`).

- **`assets`** *(array)* \
  Glob patterns for static files to copy to output directory, relative to the theme root.

- **`parameters`** *(object)* \
  Customizable settings exposed to templates and JavaScript. Each parameter includes:
  - `type` *(string)*: Type of parameter in one of `string`, `number`, `boolean`, `markdown`, `uri`, `array`, or `object` types.
  - `defaultValue` *(any)*: Default value used if not configured (must match `type`).
  - `description` *(string|null)*: Explanation of the parameter's purpose, usage, and behavior.

The [JSON schema](json-schemas/themeConfiguration.schema.json) provides detailed validation rules for the configuration file. You can use this schema in compatible editors to ensure correctness while authoring themes.

## Theme Inheritance

Themes can inherit from other themes by specifying a `base` theme identifier. Inheritance provides access to all templates, JavaScript files, CSS files, assets, and parameters from the parent theme, enabling customization without duplication.

This inheritance model allows you to:
- Build upon existing themes
- Override specific components while retaining others
- Create theme families with shared foundations

## Template System

Kampose uses Handlebars templates for rendering documentation pages. Templates are organized by filename only (case-insensitive), ignoring directory hierarchy, and must use the `.hbs` extension.

All theme templates are compiled before documentation generation begins. Syntax errors in any template will halt the process with detailed error information.

### Page Templates _(required)_

Each documentation page type requires a corresponding template file. Templates receive page-specific data through a `model` variable.

| Template File                    | Purpose                                                | Model Content                                                | Model                                                                                                                                                                                                                          |
|----------------------------------|--------------------------------------------------------|--------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `topic_page.hbs`                 | Conceptual documentation topic                         | Topic information and content                                | [TopicModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.topicmodel)                                                                                                                                   |
| `namespace_page.hbs`             | Namespace overview page                                | Namespace, including types and documentation                 | [NamespaceModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.namespacemodel)                                                                                                                           |
| `class_page.hbs`                 | Class type documentation (type-only)                   | Class type metadata, including members and documentation     | [ClassModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.classmodel)                                                                                                                                   |
| `class_members_page.hbs`         | Class type and member documentation on single page     | Class type metadata, including members and documentation     | [ClassModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.classmodel)                                                                                                                                   |
| `struct_page.hbs`                | Struct type documentation (type-only)                  | Struct type metadata, including members and documentation    | [StructModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.structmodel)                                                                                                                                 |
| `struct_members_page.hbs`        | Struct type and member documentation on single page    | Struct type metadata, including members and documentation    | [StructModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.structmodel)                                                                                                                                 |
| `interface_page.hbs`             | Interface type documentation (type-only)               | Interface type metadata, including members and documentation | [InterfaceModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.interfacemodel)                                                                                                                           |
| `interface_members_page.hbs`     | Interface type and member documentation on single page | Interface type metadata, including members and documentation | [InterfaceModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.interfacemodel)                                                                                                                           |
| `enum_page.hbs`                  | Enum type and value documentation                      | Enum type metadata, including values and documentation       | [EnumModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.enummodel)                                                                                                                                     |
| `delegate_page.hbs`              | Delegate type documentation                            | Delegate type metadata and documentation                     | [DelegateModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.delegatemodel)                                                                                                                             |
| `constructor_page.hbs`           | Constructor documentation                              | Constructor metadata and documentation                       | [ConstructorModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.constructormodel)                                                                                                                       |
| `constructor_overloads_page.hbs` | Constructor overloads documentation on single page     | Collection of overloaded constructors                        | [OverloadCollection](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.collections.overloadcollection-1)\<[ConstructorModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.constructormodel)> |
| `field_page.hbs`                 | Field documentation                                    | Field metadata and documentation                             | [FieldModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.fieldmodel)                                                                                                                                   |
| `property_page.hbs`              | Property documentation                                 | Property metadata and documentation                          | [PropertyModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.propertymodel)                                                                                                                             |
| `property_overloads_page.hbs`    | Property overloads documentation on single page        | Collection of overloaded properties                          | [OverloadCollection](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.collections.overloadcollection-1)\<[PropertyModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.propertymodel)>       |
| `method_page.hbs`                | Method documentation                                   | Method metadata and documentation                            | [MethodModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.methodmodel)                                                                                                                                 |
| `method_overloads_page.hbs`      | Method overloads documentation on single page          | Collection of overloaded methods                             | [OverloadCollection](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.collections.overloadcollection-1)\<[MethodModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.methodmodel)>           |
| `event_page.hbs`                 | Event documentation                                    | Event metadata and documentation                             | [EventModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.eventmodel)                                                                                                                                   |
| `operator_page.hbs`              | Operator documentation                                 | Operator metadata and documentation                          | [OperatorModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.operatormodel)                                                                                                                             |
| `operator_overloads_page.hbs`    | Operator overloads documentation on single page        | Collection of overloaded operators                           | [OverloadCollection](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.collections.overloadcollection-1)\<[OperatorModel](https://kampute.github.io/doc-toolkit/api/kampute.doctoolkit.models.operatormodel)>       |

Kampose uses the [Kampute.DocToolkit](https://kampute.github.io/doc-toolkit/api) library to extract reflection metadata and XML comments from .NET assemblies. To know more about each model's properties and structure, please refer to the linked API documentation.

### API Template _(required)_

In addition to the page templates listed above, `api.hbs` is a special, required template that Kampose uses to generate the API page when an explicit `api.md` topic file is not present.

Unlike page templates, `api.hbs` supplies the topic body (not a full page layout) and receives the entire documentation context (assemblies, namespaces, types, and topics) instead of the page `model` and global template variables.

### Partial Templates

Partial templates enable content reuse across multiple documentation pages. Kampose automatically registers every template file as a partial, making all templates available for inclusion.

Reference partials by filename without extension: `{{> class_members_page}}`. This approach promotes consistency and reduces duplication for common elements like headers, footers, navigation, and shared content blocks.

### Global Template Context

All page templates have access to these global variables in addition to the page-specific `model`:

| Variable            | Type    | Description                                       |
|---------------------|---------|---------------------------------------------------|
| `language`          | object  | Programming language name and identifier          |
| `generator`         | string  | Kampose version information                        |
| `absoluteUrls`      | boolean | Whether generated URLs are absolute               |
| `hasNamespacePages` | boolean | Whether namespaces have dedicated pages           |
| `hasTypePages`      | boolean | Whether types have dedicated pages                |
| `hasMemberPages`    | boolean | Whether type members have dedicated pages         |
| `homePageTitle`     | string  | Home page title if configured                     |
| `apiPageTitle`      | string  | API page title if configured                      |
| `theme`             | object  | Theme metadata information                        |
| `scripts`           | array   | Bundled JavaScript file URIs (alphabetical order) |
| `styles`            | array   | Bundled CSS file URIs (alphabetical order)        |

Theme parameters are available in the global template context in addition to the variables listed above, and can be referenced directly by name in templates.

At runtime the same global template values, including the variables listed above and configured theme parameters, are available to bundled scripts on the global JavaScript object `kampose.config`.

#### Debugging Templates

Use `{{#json this}}` to output the current template context as JSON for debugging and understanding available data structures.

> Note that circular references in the object graph are automatically handled during JSON serialization. When a cycle is detected, the cyclic reference is replaced with `null` in the JSON output. This means some properties may appear as `null` even though they reference actual objects—they are simply part of a circular reference that cannot be fully serialized.

### Template Enhancement Tools

Kampose provides extensive template functionality through built-in helpers and formatters:

**Handlebars Helpers**
A comprehensive set of helpers for logic operations, string manipulation, arithmetic calculations, and utility functions. These helpers simplify common template tasks and enhance functionality. See the [template helper functions documentation](theme-authoring/template-helpers.md) for complete usage details.

**Built-in Formatters**
Specialized formatters handle common documentation tasks like rendering XML comments and generating code member links. These formatters automatically format .NET objects and documentation content. See the [template formatters documentation](theme-authoring/template-formatters.md) for implementation details.

### Template Overrides

When inheriting from a base theme, templates with identical names override inherited templates. This selective override capability allows precise customization of specific documentation areas while preserving other components.

## Stylesheets

Configure CSS files using the `styles.source` setting with [glob patterns](../globe-patterns.md) to match desired CSS files.

Matched CSS files are bundled and minified into a single output file, with the path determined by `styles.targetPath`. File ordering within the bundle follows the sequence in the `styles.source` pattern.

### Style Inheritance and Overrides

When inheriting from a base theme, the `styles.targetPath` setting determines override behavior:

**Same Target Path**
CSS files with identical relative paths override inherited styles. Additional files extend the base theme's bundle.

**Different Target Path**
Creates a new bundle while preserving the base theme's styles. You control whether to use only your bundle or include inherited bundles.

## JavaScript

Configure JavaScript files using the `scripts.source` setting with [glob patterns](../globe-patterns.md) to match desired JavaScript files.

Matched JavaScript files are bundled and minified into a single output file, with the path determined by `scripts.targetPath`. File ordering within the bundle follows the sequence in the `scripts.source` pattern.

JavaScript code has access to global template variables through the `kampose.config` object and the complete documentation sitemap via the `kampose.sitemap` object.

### Script Inheritance and Overrides

Similar to stylesheets, the `scripts.targetPath` setting controls override behavior:

**Same Target Path**
JavaScript files with identical relative paths override inherited scripts. Additional files extend the base theme's bundle.

**Different Target Path**
Creates a new bundle while preserving the base theme's scripts. You control whether to use only your bundle or include inherited bundles.

## Static Assets

Configure static assets (images, fonts, etc.) using the `assets` setting with [glob patterns](../globe-patterns.md) to match desired files.

Asset files are copied to the output directory while preserving their directory structure relative to the theme folder.

### Asset Overrides

When inheriting from a base theme, provide replacement files with identical relative paths to override inherited assets. File name casing is ignored during override matching.

## Theme Parameters

Theme parameters enable customization of documentation behavior and appearance. Parameters are accessible in templates and JavaScript code, providing flexible configuration options.

Each parameter requires a unique name (case-insensitive) and specific type definition:

| Type       | Description                                           |
|------------|-------------------------------------------------------|
| `boolean`  | True or false values                                  |
| `number`   | Integer or floating-point numbers                     |
| `string`   | Plain text strings                                    |
| `markdown` | Markdown-formatted text (generates partial templates) |
| `uri`      | URI/URL strings                                       |
| `array`    | Arrays of values                                      |
| `object`   | Key-value pair objects                                |

### Markdown Parameter Processing

Markdown parameters receive special processing since they may contain template variables and helper functions. Kampose creates partial templates for these parameters using the naming convention `{parameterName}_partial`.

Example usage for a parameter named `overallFooter`:

```hbs
{{#if overallFooter}}
{{!-- Use triple braces to prevent double escaping --}}
{{{> overallFooter_partial}}}
{{/if}}
```

> Always reference Markdown parameters using their generated partial to ensure proper template variable expansion.

### Parameter Configuration

Parameters support default values used when not explicitly configured. Parameters without default values are ignored if not set in the build configuration.

Provide descriptive text for each parameter to help users understand its purpose, expected values, and usage context.

### Parameter Inheritance

When inheriting from a base theme, define parameters with identical names to override inherited parameter definitions. This allows customization of base theme behavior without modifying the original configuration.

## Configuration Example

The following `theme.json` demonstrates a complete theme configuration:

```json
{
    "metadata": {
        "name": "My Theme",
        "format": "html",
        "version": "0.1",
        "author": "Myself",
        "license": "MIT",
        "homepage": "https://kampute.github.io/doc-toolkit/api/Kampute.DocToolkit.my-theme",
        "description": "A custom HTML-based theme for my documentation."
    },
    "templates": [
        "templates/**/*.hbs"
    ],
    "styles": {
        "source": [
            "styles/**/*.css"
        ],
        "targetPath": "css/styles.css"
    },
    "scripts": {
        "source": [
            "scripts/**/*.js"
        ],
        "targetPath": "js/scripts.js"
    },
    "assets": [
        "assets/**"
    ],
    "parameters": {
        "disableSearch": {
            "type": "boolean",
            "defaultValue": false,
            "description": "Whether to disable the search functionality in the documentation."
        },
        "favicon": {
            "type": "uri",
            "description": "URI of the favicon."
        },
        "footer": {
            "type": "markdown",
            "defaultValue": "Copyright © {{now 'yyyy'}}",
            "description": "Footer content of the documentation."
        }
    }
}
```

## Best Practices

Create maintainable and extensible themes by following these guidelines:

**Leverage Existing Themes**
Build upon existing themes rather than starting from scratch. This reduces development time and ensures compatibility with established patterns.

**Use Modular Templates**
Create partial templates for reusable components. This promotes consistency, simplifies maintenance, and enables easy customization by derived themes.

**Organize Resources Logically**
Separate styles and scripts into logical files based on functionality. This approach facilitates selective overrides in derived themes.

**Provide Clear Parameters**
Define parameters with meaningful default values and comprehensive descriptions. This enables straightforward theme configuration for end users.

**Test Thoroughly**
Validate your theme across different documentation scenarios to ensure proper rendering of all content types and edge cases.

## Troubleshooting

Common theme development issues and their solutions:

### Template Issues

When templates are missing or not rendering correctly, check these common causes:

- Verify `templates` glob patterns match your `.hbs` files
- Confirm all required templates exist in your theme directory
- Check template file names match expected patterns exactly

When content is missing from rendered pages, investigate these areas:

- Review template files for incomplete or missing content sections
- Ensure required data is passed to templates correctly
- Verify the `model` object contains expected data for rendering
- Use `{{json model}}` to inspect available data

### Stylesheet Problems

When styles are not being applied to your documentation, troubleshoot these issues:

- Verify `styles` glob patterns match your CSS files
- Check CSS bundles are correctly referenced in templates
- Ensure styles load in the proper order
- Confirm no conflicting styles from base themes override your styles

### JavaScript Issues

When scripts are not working as expected, check these potential problems:

- Verify `scripts` glob patterns match your JavaScript files
- Check JavaScript bundles are correctly referenced in templates
- Ensure scripts load in the proper order
- Confirm no conflicting scripts from base themes
- Check browser console for JavaScript errors

### Asset Problems

When assets are missing from the generated documentation, verify these settings:

- Verify `assets` glob patterns match your asset files
- Check asset paths in templates for accuracy
- Ensure relative paths are correct from the output location
