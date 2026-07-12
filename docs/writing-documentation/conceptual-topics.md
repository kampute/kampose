---
title: Conceptual Topics
summary: "How to add, organize, identify, and cross-reference Markdown guides and other conceptual content."
---

# Conceptual Topics

Conceptual topics complement generated API reference with guides, tutorials, explanations, and project information. Kampose discovers topic files from the `topics` patterns in `kampose.json`, converts supported files to the theme's output format, and includes them in navigation and cross-references.

## Add Topics

The `topics` setting accepts glob patterns relative to `baseDirectory`. This example includes Markdown files anywhere under `docs`:

```json
{
  "topics": ["docs/**/*.md"]
}
```

See [Glob Patterns](../globe-patterns.md) for matching syntax and [Configuration](../configuration.md#topics) for the complete option reference.

## Describe a Topic

Add YAML front matter at the beginning of a Markdown file:

```markdown
---
title: Quick Start
summary: Configure and run Kampose for a .NET project.
---

# Quick Start
```

`title` supplies the display title and `summary` supplies a short description. Kampose derives the topic identifier from the source file name.

The built-in themes use the title and summary in generated pages and navigation. Custom front-matter values remain available to custom topic templates through `model.source.frontMatter`.

## Mark Important Information with Blockquotes

In Kampose topics, use a blockquote as an **important-information callout**: short, supplementary information that readers should notice before continuing. Examples include prerequisites, constraints, compatibility details, and consequences that do not belong in the main instruction flow.

Use standard Markdown blockquote syntax:

```markdown
> All relative paths in configuration are resolved against `baseDirectory`.
```

Keep callouts concise and use them selectively. Markdown blockquotes do not encode a more specific category. When readers need to distinguish a warning, caution, note, or tip, begin the content with that label, for example `> **Warning:** ...`.

## Organize the Hierarchy

`topicHierarchy` controls how Kampose constructs parent-child relationships:

- `none` keeps topics at one level.
- `directory` makes a topic the parent of topics in a same-named source directory.
- `index` uses `overview` files as parents for neighboring topics.
- `prefix` uses dot-separated filename prefixes.

Use `topicOrder` when the desired order is not the default alphabetical order. The [Configuration](../configuration.md#topichierarchy) reference describes each strategy and its naming rules.

The reserved source filenames `welcome` and `api` identify the home and API topics. They are not ordinary hierarchy nodes.

## Link Topics and Assets

In ordinary Markdown, prefer source-relative links so the files remain navigable before generation:

```markdown
[Advanced guide](../advanced.md)
![Diagram](../images/diagram.svg)
```

Kampose recognizes registered topic files and collected local assets and maps them to their generated URLs. Use `~/` only when referring to a location in the generated documentation, such as `[License](~/LICENSE)`.

See [URL Resolution](../url-resolution.md) for document-relative, site-root-relative, and documentation-root-relative behavior.

## Link from XML Documentation

Use `<see>` for an inline link and `<seealso>` for an entry in the generated **See Also** section. An `href` can identify a conceptual topic by its topic ID or source path:

```xml
/// See <see href="overview" /> before configuring advanced options.
/// <seealso href="docs/guides/overview.md" />
```

When link text is omitted, Kampose uses the topic title.
