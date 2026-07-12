---
title: URL Resolution
summary: "How Kampose resolves URLs in conceptual topics, Markdown theme settings, and Handlebars templates."
---

# URL Resolution

Kampose distinguishes three relative URL scopes. Choose the scope that describes the intended target instead of relying on where a page happens to be generated.

| Syntax | Scope | Meaning |
|--------|-------|---------|
| `guide.md` or `../guide.md` | Document-relative | Relative to the current source topic when it identifies a topic or local asset; otherwise relative to the generated page |
| `/assets/logo.svg` | Site-root-relative | Relative to the root of the hosting site |
| `~/assets/logo.svg` | Documentation-root-relative | Relative to the root of the generated documentation |

Absolute URLs are not rebased.

## Source References in Conceptual Topics

In an ordinary Markdown topic, link to another topic or local asset by its path relative to the current source file. Kampose maps registered topics and assets to their generated locations.

For example, from `docs/guides/overview.md`:

```markdown
[Configuration reference](../configuration.md)
![Architecture](../images/architecture.svg)
```

These links remain useful when the source is viewed in an editor or repository browser. Do not use Handlebars helpers in ordinary topic files; topic Markdown is not a Handlebars template.

## Generated-Document URLs

Use `~/` when the target is identified by its location in the generated documentation rather than by a source-file relationship:

```markdown
[License](~/LICENSE)
```

Kampose writes a documentation-root-relative URL in the form required by the current page. With an absolute `baseUrl`, the result is absolute. Without one, a nested output page receives the necessary `../` segments.

Use `/` only for a resource rooted at the hosting website. The site root and documentation root differ when documentation is hosted below a path such as `https://example.github.io/project/`.

Path normalization does not allow a relative URL to escape its applicable root.

## Markdown Theme Settings

Theme settings declared as `markdown` are rendered in each page's scope. Ordinary relative links therefore remain relative to that page, while `~/` consistently targets the documentation root.

Markdown settings may contain Handlebars expressions because Kampose turns them into template partials. This is specific to theme settings and does not make helpers available in ordinary Markdown topics.

## Handlebars Templates

Raw template attributes are not Markdown and do not pass through Markdown URL transformation. Use the [`rootRelativeUrl`](themes/theme-authoring/template-helpers/url-helpers.md#rootrelativeurl) helper when a template needs a documentation-root-relative URL:

```hbs
<link rel="stylesheet" href="{{#rootRelativeUrl 'styles/main.css'}}" />
```

The helper belongs to the theme-template environment. It must not be copied into a conceptual topic.

## See Also

- [Conceptual Topics](writing-documentation/conceptual-topics.md)
