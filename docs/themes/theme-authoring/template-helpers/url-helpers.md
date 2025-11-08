# URL Helpers

URL generation and manipulation functions that handle path resolution and fragment extraction.

| Helper                                | Purpose                                                       | Parameters | Returns   |
|---------------------------------------|---------------------------------------------------------------|------------|-----------|
| [`rootUrl`](#rooturl)                 | Returns the root URL of the site                              |            | `uri`     |
| [`rootRelativeUrl`](#rootrelativeurl) | Converts site-relative URL to document-relative               | `uri`      | `uri`     |
| [`fragment`](#fragment)               | Extracts fragment (anchor) from a URL for linking to sections | `uri`      | `string?` |

## `rootUrl`

Provides the root URL of the site.

**Syntax:**
```hbs
{{#rootUrl}}
```

**Returns:**
A URI object that is either absolute or relative to the current page, depending on the context

**Examples:**
```hbs
{{#rootUrl}}    {{!-- Absolute: "https://example.com/" --}}
{{#rootUrl}}    {{!-- Relative: "../../" --}}
```

## `rootRelativeUrl`

Converts a site-relative URL to a document-relative URL based on the current page context.

**Syntax:**
```hbs
{{#rootRelativeUrl uri}}
```

**Parameters:**
- `uri` (string|uri) - The URI to make it relative to the current page

**Returns:**
A URL string that is either absolute or relative to the current page, depending on the context

**Examples:**
```hbs
{{#rootRelativeUrl "styles/main.css"}}     {{!-- "https://example.com/styles/main.css" --}}
{{#rootRelativeUrl "index.html"}}          {{!-- "https://example.com/index.html" --}}
```

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
