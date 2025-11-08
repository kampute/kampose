# Configuration

This document describes the Kampose JSON configuration file, covering the fields used to locate source assemblies and topics, control output formatting and layout, configure references, and customize themes.

By default, Kampose reads the configuration from the `kampose.json` file in the current working directory. You can specify an alternate file path on the command line:

```bash
kampose build MyConfig.json
```

> All relative paths in configuration are resolved against `baseDirectory` (described below). If `baseDirectory` is omitted, Kampose uses the directory that contains the configuration file.

The [JSON schema](json-schemas/configuration.schema.json) for the configuration file is available to help with validation and editor support.

## Configuration Options

The following sections describe the various configuration options available in the Kampose JSON configuration file. Each option controls different aspects of the documentation generation process, from source file locations to output formatting and theme customization.

### `baseDirectory`

Specifies the root directory for resolving all relative paths in the configuration file.

When not specified, Kampose uses the directory containing the configuration file. All relative paths in the configuration are resolved from this directory.

##### Example

The following example sets a custom base directory for path resolution. Since the base directory is relative, it resolves against the directory containing the configuration file.

```json
{
  "baseDirectory": "path/to/project",
  "assemblies": ["src/**/*.dll"],
  "topics": ["docs/**/*.md"]
}
```

### `outputDirectory` _(required)_

Specifies the directory where Kampose writes all generated documentation files. Kampose creates the directory if it doesn't exist.

This setting is required.

##### Example

The following example specifies an output directory (`site`) relative to the base directory (`/project`):

```json
{
  "baseDirectory": "/project",
  "outputDirectory": "site"
}
```

### `convention` _(required)_

Specifies the page layout and URL format for the generated documentation.

| Convention | Format   | Page Layout                          | URL Format              | Example URL                                                                          |
|------------|----------|--------------------------------------|-------------------------|--------------------------------------------------------------------------------------|
| `dotNet`   | HTML     | Separate pages for types and members | .NET API Browser style  | `api/system.collections.generic.list-1.count.html`                                   |
| `docFx`    | HTML     | Types and members on same page       | DocFX style             | `api/System.Collections.Generic.List_1.html#System_Collections_Generic_List_1_Count` |
| `devOps`   | Markdown | Types and members on same page       | Azure DevOps Wiki style | `System.Collections.Generic/List%3CT%3E.md#count`                                    |

This setting is required.

##### Example

The following example configures the .NET convention, where each type and member has its own documentation page:

```json
{
  "convention": "dotNet"
}
```

### `baseUrl`

Specifies the root URL for the documentation, which must be an absolute URI.

When a `baseUrl` is specified, all internal links become absolute URLs. Otherwise, Kampose generates relative URLs.

Leaving `baseUrl` unset is recommended so that the documentation can be used in different environments, including local development, without updating all internal links.

##### Example

The following example configures absolute URLs for a versioned documentation site:

```json
{
  "baseUrl": "https://docs.example.com/v2/",
  "outputDirectory": "/var/www/docs/v2"
}
```

### `assemblies`

Specifies [glob patterns](globe-patterns.md) for locating .NET assembly files to document.

If an assembly appears in multiple locations, the first one found is used. You can use exclusion patterns to filter unwanted assemblies.

At least one assembly pattern is required when no topic patterns are specified.

##### Example

The following example includes release assemblies while excluding test and benchmark assemblies:

```json
{
  "assemblies": [
    "src/**/bin/Release/**/*.dll",
    "!**/*Test*.dll",
    "!**/*Benchmark*.dll"
  ]
}
```

### `xmlDocs`

Specifies [glob patterns](globe-patterns.md) for additional XML documentation files or those not in the same location as their assemblies.

##### Example

The following example includes XML documentation from custom locations:

```json
{
  "assemblies": [
    "dist/**/*.dll"
  ],
  "xmlDocs": [
    "docs/xml/**/*.xml",
    "third-party/docs/*.xml"
  ]
}
```

### `references`

Configures how to link to documentation of third-party libraries referenced within the codebase being documented.

Kampose automatically handles references to standard .NET API types (System.*, Microsoft.*, etc.). Use this setting to link to documentation for third-party libraries or internal company documentation.

Each entry in the `references` array configures how to link to external documentation for a specific namespace or set of namespaces.

#### `namespaces` _(required)_

Specifies the namespaces that should be linked using this entry.

The matching is case-sensitive and the `*` wildcard can be used as the final segment to match any sub-namespaces.

This setting is required for each reference entry.

#### `strategy` _(required)_

Specifies the strategy used for linking to the external documentation. It can be one of the following:

| Strategy       | Description                              | Example URL                                                                          |
|----------------|------------------------------------------|--------------------------------------------------------------------------------------|
| `dotNet`       | Microsoft .NET API Browser format        | `api/system.collections.generic.list-1.count`                                        |
| `docFx`        | DocFX-generated documentation            | `api/System.Collections.Generic.List_1.html#System_Collections_Generic_List_1_Count` |
| `devOps`       | Azure DevOps Wiki format                 | `System.Collections.Generic/List%3CT%3E.md#count`                                    |
| `onlineSearch` | Search engine fallback (no direct links) | N/A                                                                                  |

This setting is required for each reference entry.

#### `url` _(required)_

Specifies the base URL of the external documentation site.

This setting is required for each reference entry.

#### `extension`

Specifies the file extension override for non-standard documentation URLs.

##### Examples

The following example configures Kampose to use a search engine for `Newtonsoft.Json` references:

```json
{
  "references": [
    {
      "namespaces": ["Newtonsoft.Json.*"],
      "strategy": "onlineSearch",
      "url": "https://www.phind.com/search/"
    }
  ]
}
```

The example below links to internal company documentation with custom file extensions:

```json
{
  "references": [
    {
      "namespaces": ["MyCompany.SharedLib.*"],
      "strategy": "docFx",
      "url": "https://internal-docs.mycompany.com/sharedlib/",
      "extension": ".htm"
    }
  ]
}
```

### `topics`

Specifies [glob patterns](globe-patterns.md) for conceptual documentation files like guides, tutorials, and overviews. By default, it includes all Markdown files in the root of [`baseDirectory`](#basedirectory) (e.g., `['*.md']`).

Kampose processes these files and converts them to the target format.

##### Example

The following example includes documentation from multiple directories while excluding draft content:

```json
{
  "topics": [
    "docs/**/*.md",
    "guides/**/*.md",
    "!docs/drafts/**"
  ]
}
```

#### Special Topic Filenames

Certain filenames have special significance when organizing topics:

| Filename     | Behavior                                                                                                            |
|--------------|---------------------------------------------------------------------------------------------------------------------|
| `welcome`    | Serves as the main landing page for the documentation site. If not present, the `README` file is used if available. |
| `api`        | Serves as the landing page for API reference sections. If not present, a default page is generated.                 |

### `topicOrder`

Specifies the explicit ordering for topics in the documentation navigation.

Topics are displayed in the order specified in this list. If a topic is not listed, it maintains its discovery order.

##### Example

The following example defines a specific order for key documentation topics:

```json
{
  "topicOrder": [
    "docs/overview.md",
    "guides/installation",
    "guides/configuration.md",
    "advanced/customization"
  ]
}
```

### `topicHierarchy`

Specifies the method for organizing topics into parent-child relationships.

| Method      | Behavior                                                                                |
|-------------|-----------------------------------------------------------------------------------------|
| `none`      | All topics remain at the top level                                                      |
| `directory` | Files with names matching directory names become parents for files in those directories |
| `index`     | Files named "overview" become parents for other files in the same directory             |
| `prefix`    | Files with fewer dot-separated segments become parents for files with matching prefixes |

The default value is `none`, meaning no hierarchy is applied.

##### Example

**Directory hierarchy:**
```
guides.md              → Parent topic
guides/
  ├── installation.md  → Child of guides.md
  └── advanced.md      → Child of guides.md
```

**Index hierarchy:**
```
guides/
  ├── overview.md      → Parent topic
  ├── installation.md  → Child of overview.md
  └── advanced.md      → Child of overview.md
```

**Prefix hierarchy:**
```
api.md                 → Parent topic
api.classes.md         → Child of api.md
api.classes.core.md    → Child of api.classes.md
```

### `assets`

Configures the handling of static files in the documentation. Files are copied as-is and their directory structure is preserved.

Each entry in the `assets` array specifies a set of files for a specific destination directory within the output directory.

#### `source` _(required)_

Specifies [glob patterns](globe-patterns.md) for selecting files to copy directly to the output directory.

This setting is required.

#### `targetPath`

Specifies the directory within the output directory where the files are copied.

This setting is optional. If omitted, files are copied to the root of the output directory.

##### Example

The example below demonstrates copying different asset types to separate directories:

```json
{
  "assets": [
    {
      "source": ["company-logos/**/*"],
      "targetPath": "assets/branding"
    },
    {
      "source": ["legal-docs/**/*.pdf"],
      "targetPath": "downloads"
    }
  ]
}
```

### `theme`

Specifies the theme to be used for generating the documentation pages.

The available themes depend on the [`convention`](#convention) used. Please refer to the [themes documentation](themes.md) for a list of available themes.

If no theme is specified, Kampose uses the `"classic"` theme.

##### Example

The following example selects the classic theme:

```json
{
  "theme": "classic"
}
```

### `themeSettings`

Specifies theme-specific customization parameters that control appearance and behavior of the selected theme.

Available settings vary by theme and are defined by each theme's configuration. Please refer to the [themes documentation](themes.md) for information about available themes and their specific parameters.

##### Example

The following example configures various theme settings for the classic theme (HTML):

```json
{
  "themeSettings": {
    "projectName": "My Project",
    "faviconUri": "assets/favicon.ico",
    "pageFooter": "Copyright © {{now 'yyyy'}} [My Company](https://company.com)"
  }
}
```

### `audit`

The audit settings determine which aspects of the XML documentation should be checked and whether to proceed with documentation generation if issues are found.

These settings help ensure that the generated documentation meets quality standards by identifying missing or incomplete XML comments.

#### `options`

Specifies the documentation XML tags that should be checked. It can include the following options:

| Option         | Description                                                                                                                              |
|----------------|------------------------------------------------------------------------------------------------------------------------------------------|
| `summary`      | Report missing or empty `<summary>` tags                                                                                                 |
| `typeparam`    | Report missing or empty `<typeparam>` tags                                                                                               |
| `param`        | Report missing or empty `<param>` tags                                                                                                   |
| `returns`      | Report missing or empty `<returns>` tags                                                                                                 |
| `value`        | Report missing or empty `<value>` tags for properties                                                                                    |
| `exception`    | Report missing descriptions for `<exception>` tags                                                                                       |
| `permission`   | Report missing descriptions for `<permission>` tags                                                                                      |
| `event`        | Report missing descriptions for `<event>` tags                                                                                           |
| `remarks`      | Report missing or empty `<remarks>` tags                                                                                                 |
| `example`      | Report missing or empty `<example>` tags                                                                                                 |
| `seeAlso`      | Report `<seealso>` hyperlinks that lack descriptive titles                                                                               |
| `threadSafety` | Report missing or empty `<threadsafety>` tags for class and struct types                                                                 |
| `overloads`    | Report missing shared summary documentation for overloaded members using the `<overloads>` tag                                           |
| `required`     | Report issues related to the minimum required documentation (includes `summary`, `typeparam`, `param`, `returns`)                        |
| `recommended`  | Report issues related to the recommended documentation (includes `required` plus `value`, `exception`, `permission`, `event`, `seeAlso`) |
| `all`          | Report all issues                                                                                                                        |

The default value is the minimum required documentation (e.g., `[required]`).

#### `includeImplicitConstructors`

Specifies whether to include implicit public default constructors in the audit (default: `false`).

The default public constructors generated by the compiler are not documented in XML and are typically not included in audits. Kampose will automatically generate a summary for these constructors during documentation generation.

However, if you want to ensure that all default public constructors are explicitly defined and documented, you can set this option to `true` to get a warning for any implicit constructors.

#### `stopOnIssues`

Specifies whether to halt documentation generation when audit failures occur (default: `false`).

By default, Kampose will report audit issues but continue with the documentation generation process. If you want to enforce documentation quality, you can set this option to `true` to address documentation gaps before proceeding. This is particularly useful in continuous integration scenarios where maintaining documentation quality is essential.

##### Example

The following example enables auditing for minimum required documentation plus property value tags, and stops generation if issues are found in the XML documentation:

```json
{
  "audit": {
    "options": ["required", "value"],
    "stopOnIssues": true
  }
}
```