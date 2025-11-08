# Classic Theme (HTML)

The Classic HTML theme produces responsive, interactive HTML documentation suitable for publishing to websites and project portals.

## Theme Settings

The theme supports the following customization settings:

| Name                                                        | Type     | Default                                                  | Description                                                                                                              |
|-------------------------------------------------------------|----------|----------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------|
| [`projectName`](#projectname)                               | string   |                                                          | The name of the project displayed in the header and navigation.                                                          |
| [`projectSlogan`](#projectslogan)                           | string   |                                                          | The slogan or tagline of the project displayed alongside the project name.                                               |
| [`projectLogoUri`](#projectlogouri)                         | uri      |                                                          | The URI of the project logo regardless of color mode.                                                                    |
| [`projectLogoDarkUri`](#projectlogodarkuri)                 | uri      |                                                          | The URI of the project logo for dark color mode (requires projectLogoLightUri).                                          |
| [`projectLogoLightUri`](#projectlogolighturi)               | uri      |                                                          | The URI of the project logo for light color mode (requires projectLogoDarkUri).                                          |
| [`faviconUri`](#faviconuri)                                 | uri      |                                                          | The URI of the favicon displayed in browser tabs.                                                                        |
| [`styleUri`](#styleuri)                                     | uri      |                                                          | The URI of a custom stylesheet to extend or override default styles.                                                     |
| [`scriptUri`](#scripturi)                                   | uri      |                                                          | The URI of a custom script to extend the default functionality.                                                          |
| [`pageHeader`](#pageheader)                                 | markdown |                                                          | Markdown content displayed at the top of the main content area on every page.                                            |
| [`pageFooter`](#pagefooter)                                 | markdown |                                                          | Markdown content displayed at the bottom of the main content area on every page.                                         |
| [`menuItems`](#menuitems)                                   | array    | `[]`                                                     | Defines the menu bar structure as an array of items with titles, URLs, and optional sub-items.                           |
| [`excludeBreadcrumb`](#excludebreadcrumb)                   | boolean  | `false`                                                  | Determines whether to avoid rendering the breadcrumb navigation.                                                         |
| [`excludeLeftSidebar`](#excludeleftsidebar)                 | boolean  | `false`                                                  | Determines whether to avoid rendering the left sidebar containing main navigation.                                       |
| [`excludeRightSidebar`](#excluderightsidebar)               | boolean  | `false`                                                  | Determines whether to avoid rendering the right sidebar containing page-specific navigation.                             |
| [`groupTypesByNamespace`](#grouptypesbynamespace)           | boolean  | `false`                                                  | Determines whether to group types by their namespace in the left sidebar.                                                |
| [`showTypeMembersSummary`](#showtypememberssummary)         | boolean  | `false`                                                  | Determines whether to include a summary of type members in type documentation pages when each member has its own page.   |
| [`seeAlsoSubtopics`](#seealsosubtopics)                     | boolean  | `false`                                                  | Determines whether to automatically include related subtopics as "See Also" references.                                  |
| [`popupFileExtensions`](#popupfileextensions)               | array    | `["", "txt", "png", "jpeg", "jpg", "gif", "svg", "pdf"]` | An array of file extensions (without the leading dot) for which links to local asset files should open in a modal pop-up. |

### `projectName`

The `projectName` setting specifies the name of your project that will be displayed prominently in the documentation header, and helps users identify the project they're viewing.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "projectName": "MyAwesome.Library"
    }
}
```

### `projectSlogan`

The `projectSlogan` setting allows you to add a tagline or brief description that appears alongside your project name in the header. This helps provide context about your project's purpose or key benefit.

The slogan appears below the project name in a smaller, subdued style.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "projectName": "MyAwesome.Library",
        "projectSlogan": "A library for awesome things"
    }
}
```

### `projectLogoUri`

The `projectLogoUri` setting specifies the URI of your project's logo that will be displayed regardless of the user's selected color mode (light or dark). The logo appears on the same row before the project name and slogan in the header.

The URI can be relative to the documentation root or an absolute URL. When using relative paths, ensure the logo file is included in your documentation assets.

Note that when both `projectLogoDarkUri` and `projectLogoLightUri` are specified, this setting is ignored in favor of the adaptive logo system that switches based on the user's color mode preference.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "projectName": "MyProject",
        "projectLogoUri": "assets/logo.png"
    }
}
```

### `projectLogoDarkUri`

The `projectLogoDarkUri` setting specifies the URI of your project's logo optimized for display when users select the dark color mode. This setting must be used together with `projectLogoLightUri` to enable the adaptive logo system.

When both dark and light logo URIs are provided, the theme will automatically switch between them based on the user's color mode selection in the web interface, ensuring optimal logo visibility and appearance in both light and dark themes. The logo appears on the same row before the project name and slogan in the header.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "projectName": "MyProject",
        "projectLogoDarkUri": "assets/logo-dark.png",
        "projectLogoLightUri": "assets/logo-light.png"
    }
}
```

### `projectLogoLightUri`

The `projectLogoLightUri` setting specifies the URI of your project's logo optimized for display when users select the light color mode. This setting must be used together with `projectLogoDarkUri` to enable the adaptive logo system.

When both light and dark logo URIs are provided, the theme will automatically switch between them based on the user's color mode selection in the web interface, ensuring optimal logo visibility and appearance in both light and dark themes. The logo appears on the same row before the project name and slogan in the header.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "projectName": "MyProject",
        "projectLogoDarkUri": "assets/logo-dark.png",
        "projectLogoLightUri": "assets/logo-light.png"
    }
}
```

### `faviconUri`

The `faviconUri` setting specifies the URI of the favicon that will be displayed in browser tabs and bookmarks. The favicon helps users identify your documentation among multiple open tabs.

The URI can be relative to the documentation root or an absolute URL. When using relative paths, ensure the favicon file is included in your documentation assets.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "faviconUri": "assets/favicon.ico"
    }
}
```

### `styleUri`

The `styleUri` setting allows you to specify a custom CSS stylesheet that will extend or override the theme's default styles. This enables you to customize the visual appearance without modifying the base theme.

The custom stylesheet is loaded after the theme's default styles, allowing your rules to take precedence.

The URI can be relative to the documentation root or an absolute URL. When using relative paths, ensure the CSS file is included in your documentation assets.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "styleUri": "custom/styles.css"
    }
}
```

### `scriptUri`

The `scriptUri` setting allows you to specify a custom JavaScript file that will extend the theme's default functionality. This enables you to add interactive features or customize behavior without modifying the base theme.

The custom script is loaded after the theme's default scripts but before the syntax highlighting script. Your script can safely use the same global objects and APIs as the theme.

The URI can be relative to the documentation root or an absolute URL. When using relative paths, ensure the JavaScript file is included in your documentation assets.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "scriptUri": "custom/enhanced-search.js"
    }
}
```

### `pageHeader`

The `pageHeader` setting allows you to add custom Markdown content that will be displayed at the top of the main content area on every generated page. You can use Handlebars expressions within this content.

This is commonly used for displaying status badges (build status, version, coverage), project announcements, important notices, or contextual information that should appear prominently on all pages.

> When linking to a relative content path, the path should be relative to the current page. You can use `rootRelativeUrl` helper to convert a root-relative path to a page-relative path (e.g., `{{#rootRelativeUrl 'path/to/file'}}`).

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "pageHeader": "[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/myorg/myproject/actions) [![Version](https://img.shields.io/badge/version-2.1.0-blue)](https://github.com/myorg/myproject/releases)"
    }
}
```

### `pageFooter`

The `pageFooter` setting allows you to add custom Markdown content that will be displayed at the bottom of the main content area on every generated page. You can use Handlebars expressions within this content.

This is commonly used for copyright notices, legal information, attribution, or additional navigation links that should appear consistently across all pages.

You cam use Markdown bullet points in the footer content. For better visual integration with the theme, the footer will render bullet points as inline items separated by bullets (`•`).

> When linking to a relative content path, the path should be relative to the current page. You can use `rootRelativeUrl` helper to convert a root-relative path to a page-relative path (e.g., `{{#rootRelativeUrl 'path/to/file'}}`).

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "pageFooter": "- Copyright © {{now 'yyyy'}} [Example Corp](https://example.com)\n- [MIT]({{#rootRelativeUrl 'LICENSE'}}) License"
    }
}
```

### `menuItems`

The `menuItems` setting defines the structure of the top navigation menu as an array of menu items. Each item is an object with `title` and `url` properties, and can optionally contain a nested `items` array for dropdown menus.

#### Structure

Each menu item object supports these properties:
- `title` (string): The display text for the menu item
- `url` (string): The destination URL (relative or absolute)
- `items` (array, optional): Sub-items for dropdown menus

The relative paths for the `url` properties should be defined in relation to the documentation root.

A menu item can also be a string with special values:
- `<topic-slug>` - Generates a menu entry linking to the specified topic by its slug (e.g., `getting-started`), including subtopics as a dropdown
- `*` - Generates menu entries for all topics at the root level, including their subtopics as dropdowns
- `-` - Adds a visual divider

#### Examples

Simple menu with static links:

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "menuItems": [
            {
                "title": "API Reference",
                "url": "api/index.html"
            },
            {
                "title": "Repository",
                "url": "https://github.com/myorg/myproject"
            }
        ]
    }
}
```

Complex menu with dropdowns and auto-inserted topics:

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "menuItems": [
            {
                "title": "Documentation",
                "items": [
                    "*", /* Auto-insert all topics here */
                    "-", /* Divider */
                    {
                        "title": "Repository",
                        "url": "https://github.com/myorg/myproject"
                    }
                ]
            }
        ]
    }
}
```

### `excludeBreadcrumb`

The `excludeBreadcrumb` setting determines whether to hide the breadcrumb navigation that typically appears near the top of each page. Breadcrumbs help users understand their current location within the documentation hierarchy and provide quick navigation back to parent pages.

Set this to `true` to remove breadcrumb navigation from all pages, which can be useful for simpler documentation structures or when you prefer a cleaner page layout. By default, this setting is `false`, meaning breadcrumbs will be displayed.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "excludeBreadcrumb": true
    }
}
```

### `excludeLeftSidebar`

The `excludeLeftSidebar` setting determines whether to hide the left sidebar that contains the main navigation menu. The left sidebar typically shows the documentation structure including namespaces, types, and topics.

Set this to `true` to create a full-width layout without the main navigation sidebar. This can be useful for documentation that relies primarily on top navigation or for presentation-focused layouts. By default, this setting is `false`, meaning the left sidebar will be displayed.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "excludeLeftSidebar": true
    }
}
```

### `excludeRightSidebar`

The `excludeRightSidebar` setting determines whether to hide the right sidebar that contains the table of contents for the current page.

Set this to `true` to remove the right sidebar and create more space for the main content area. This can be beneficial for pages with wide content or when you prefer a simpler layout. By default, this setting is `false`, meaning the right sidebar will be displayed.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "excludeRightSidebar": true
    }
}
```

### `groupTypesByNamespace`

The `groupTypesByNamespace` setting determines how types are organized in the left sidebar navigation. When enabled, types are grouped under their respective namespace headings. When disabled, types are listed individually in a flat structure.

Set this to `true` to organize types hierarchically by namespace, which can improve navigation for projects with many namespaces. Set to `false` for a simpler flat list that may be easier to scan for projects with fewer types. By default, this setting is `false`, meaning types will be listed individually.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "groupTypesByNamespace": true
    }
}
```

### `showTypeMembersSummary`

The `showTypeMembersSummary` setting determines whether to include a summary section of type members on type documentation pages. This setting is only relevant when each type member has its own dedicated documentation page (e.g., configured convention is `DotNet`).

When enabled, type pages will include a summary table of the type's members with brief descriptions, providing an overview before users navigate to individual member pages. This can be beneficial if the left sidebar is excluded, as it allows users to see all relevant information at a glance. By default, this setting is `false`, meaning the summary will not be displayed.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "showTypeMembersSummary": true
    }
}
```

### `seeAlsoSubtopics`

The `seeAlsoSubtopics` setting determines whether to automatically include related subtopics as "See Also" references on topic pages. When enabled, topic pages with child topics (subtopics) will include a "See Also" section with links to direct child topics of the current topic.

This feature helps improve content discoverability by automatically creating cross-references between related documentation sections.

#### Example

Suppose you have the following topic structure:

```txt
- Best Practices
  - Coding Guidelines
    - Style Guide
  - Testing Strategies
    - Unit Testing
    - Integration Testing
```

After enabling the `seeAlsoSubtopics` setting as follows:

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "seeAlsoSubtopics": true
    }
}
```

The following section will be added at the bottom of "Best Practices" page:

```html
<h2>See Also</h2>
<ul>
  <li><a href="best-practices/coding-guidelines.html">Coding Guidelines</a></li>
  <li><a href="best-practices/testing-strategies.html">Testing Strategies</a></li>
</ul>
```

### `popupFileExtensions`

The `popupFileExtensions` setting specifies an array of file extensions (without the leading dot) for which links to local asset files should open in a modal overlay. This keeps users within the documentation context rather than navigating away or triggering a download.

When a user clicks a link to a file with one of the specified extensions, the theme fetches the file and verifies its MIME type before displaying it. Only files with text, image, or PDF MIME types will open in the modal. Files with other MIME types are ignored and use standard browser navigation behavior.

By default, the theme displays text files (`.txt`), common image formats (`.png`, `.jpg`, `.jpeg`, `.gif`, `.svg`), and PDFs (`.pdf`) in the modal.

To customize which extensions trigger the modal, modify the array to include only the desired file extensions. To disable this feature entirely, set this to an empty array (`[]`).

> The special `LICENSE` and `DISCLAIMER` files (without extensions) are always opened in the modal regardless of this setting.

If a configured file extension appear to be neither text, image, nor PDF based on its MIME type, the theme will convert it to plain text for safer display in the modal. It is your responsibility to ensure that only appropriate file types are included in this setting.

> This setting only affects local links to non-HTML files within the documentation. External links and links to HTML files always use standard navigation behavior.

#### Example

```json
{
    "convention": "dotnet",
    "theme": "classic",
    "themeSettings": {
        "popupFileExtensions": ["txt", "json"]
    }
}
```

## File Protocol Limitations

When viewing the generated documentation locally using the `file://` protocol, be aware that browsers impose restrictions on pages loaded from the local file system. Many browsers block or limit access to localStorage, fetch/XHR requests, and service workers for `file://` pages. As a result, theme preferences may not persist across page navigations, and the navigation sidebar might briefly flicker.

For realistic testing and reliable deployment, serve the documentation via an HTTP(S) server, or use a browser that permits localStorage access for `file://` pages.

#### Example

Install the official dotnet static file server tool and run a simple HTTP server:

```bash
# Install once (global tool)
dotnet tool install --global dotnet-serve

# Run from the documentation output folder
dotnet-serve --directory . --port 8000 --open-browser:index.html
```

Many static servers (including `dotnet-serve`) accept certificate files so you can run HTTPS locally by pointing the server at the cert/key files.
