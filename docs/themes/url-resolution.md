---
title: URL Resolution
summary: "How Kampose resolves links in conceptual topics and Markdown theme settings."
---

# URL Resolution

Kampose supports three relative URL scopes in conceptual topics and Markdown theme settings.

| Syntax         | Scope            | Meaning                                                    |
|----------------|------------------|------------------------------------------------------------|
| `path/file`    | Current document | Resolved from the directory of the generated document      |
| `../path/file` | Current document | Resolved from a parent directory of the generated document |
| `/path/file`   | Web site         | Resolved from the root of the hosting web site             |
| `~/path/file`  | Documentation    | Resolved from the root of the generated documentation      |

Absolute URLs retain their original meaning and are not rebased.

## Web-Site and Documentation Roots

The web-site root and documentation root may refer to different locations. A GitHub Pages project published at `https://example.github.io/project/`, for example, has the web-site root at `https://example.github.io/` and the documentation root at `https://example.github.io/project/`.

Use `/` for resources owned by the hosting site and `~/` for resources within the generated documentation. This distinction keeps documentation links valid when the output is hosted below the web-site root.

## Generated URLs

Kampose replaces `~/` according to the output configuration and the location of the document being rendered:

| Input               | Rendering context                         | Result                                                |
|---------------------|-------------------------------------------|-------------------------------------------------------|
| `~/styles/main.css` | Absolute documentation base URL configured | `https://example.github.io/project/styles/main.css`   |
| `~/styles/main.css` | Document two directories below the root   | `../../styles/main.css`                               |
| `~/styles/main.css` | Document at the documentation root        | `styles/main.css`                                     |

Path normalization cannot escape the documentation root.

## Processing Contexts

Documentation-root-relative URLs are resolved in conceptual topics and Markdown theme settings as that content is transformed to
the target output format.

In file-based conceptual topics, Kampose also recognizes source-relative references to registered topics and existing local assets. These references are mapped to their generated locations before the final URL is written.

```markdown
[License](~/LICENSE)
```

Raw Handlebars template output does not pass through URL transformation. Use the
[`rootRelativeUrl`](theme-authoring/template-helpers/url-helpers.md#rootrelativeurl) helper for documentation-root-relative links
in template attributes:

```hbs
<link rel="stylesheet" href="{{#rootRelativeUrl 'styles/main.css'}}" />
```
