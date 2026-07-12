---
title: URL Helpers
summary: "Handlebars helpers for documentation-root URLs and URL fragments."
---

# URL Helpers

These helpers expose resolved URL values to Handlebars expressions and attributes that are not automatically treated as URLs.
For the meaning of `/`, `~/`, and ordinary relative paths, see [URL resolution](../../url-resolution.md).

| Helper                                | Purpose                                                       | Parameters | Returns   |
|---------------------------------------|---------------------------------------------------------------|------------|-----------|
| [`rootUrl`](#rooturl)                 | Returns the documentation-root URL                            |            | `uri`     |
| [`rootRelativeUrl`](#rootrelativeurl) | Resolves a path from the documentation root                   | `uri`      | `uri`     |
| [`fragment`](#fragment)               | Extracts fragment (anchor) from a URL for linking to sections | `uri`      | `string?` |

## `rootUrl`

Provides the documentation-root URL for the page currently being rendered. This can be an absolute URL or a sequence of
parent segments such as `../../`.

**Syntax:**
```hbs
{{#rootUrl}}
```

**Returns:**
A URI object that is either absolute or relative to the current page, depending on the context

**Examples:**
```hbs
{{#rootUrl}}    {{!-- Absolute: "https://example.github.io/project/" --}}
{{#rootUrl}}    {{!-- Relative: "../../" --}}
```

Use this helper when the root URL is needed as data rather than as an `href` or `src` value, for example:

```hbs
<nav data-base-url="{{#rootUrl}}"></nav>
```

## `rootRelativeUrl`

Resolves a path from the documentation root for the page currently being rendered.

**Syntax:**
```hbs
{{#rootRelativeUrl uri}}
```

**Parameters:**
- `uri` (string|uri) - A path relative to the documentation root, a `~/` URL, or an already absolute URL

**Returns:**
A URL string that is either absolute or relative to the current page, depending on the context

**Examples:**
```hbs
{{#if (eq model.url (rootRelativeUrl "index.html"))}}
  Home page
{{/if}}

<link rel="icon" href="{{#rootRelativeUrl faviconUri}}" />
```

Use this helper for documentation-root-relative URLs in Handlebars templates. The `~/` marker is resolved in conceptual topics
and Markdown theme settings, but it is not expanded in raw template output.

## `fragment`

Extract the fragment identifier of a URL for linking to specific sections.

**Syntax:**
```hbs
{{#fragment url}}
```

**Parameters:**
- `string` (string|uri) - The URI to extract the fragment identifier from

**Returns:**
URL fragment string (without # prefix)

**Examples:**
```hbs
{{#fragment "example/best-practices.html#cs"}}        {{!-- "cs" --}}
<h1 id="{{#fragment model.url}}">{{model.name}}</h1>
```
