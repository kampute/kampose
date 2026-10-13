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

## Highlight Important Information with Alerts

Use GitHub-style alerts for short callouts that readers should notice before continuing. Put the alert directive on the first line of a blockquote and prefix each content line with `>`:

```markdown
> [!NOTE]
> Additional context that helps readers understand the current section.

> [!TIP]
> Optional advice that helps readers complete a task.

> [!IMPORTANT]
> Information readers need to complete a task successfully.

> [!WARNING]
> A potential problem that requires immediate attention.

> [!CAUTION]
> A risk that could lead to data loss or another negative consequence.
```

The directive names are case-insensitive, but uppercase names match the GitHub convention and are easier to recognize. Alert content can contain the same inline and block Markdown supported by an ordinary blockquote.

Output depends on the selected convention:

- HTML themes render alerts with the same labels, icons, and colors used for XML documentation `<note>` elements.
- Markdown themes used with the `devOps` convention replace the alert directive with a bold title and preserve the content as a portable blockquote. For example, the warning above becomes:

```markdown
> **Warning** \
> A potential problem that requires immediate attention.
```

Use alerts selectively and keep them focused on one message. Use an ordinary blockquote for quotations or content that does not require special emphasis.

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

Kampose recognizes registered topic files and collected local assets and maps them to their generated URLs, including an asset's configured `targetPath` and preserved source hierarchy. Authors do not need to calculate that generated path. Use `~/` only when referring to a location in the generated documentation, such as `[License](~/LICENSE)`.

See [URL Resolution](../url-resolution.md) for document-relative, site-root-relative, and documentation-root-relative behavior.

## Link from XML Documentation

Use `<see>` for an inline link and `<seealso>` for an entry in the generated **See Also** section. An `href` can identify a conceptual topic by its topic ID or source path:

```xml
/// See <see href="overview" /> before configuring advanced options.
/// <seealso href="docs/guides/overview.md" />
```

When link text is omitted, Kampose uses the topic title.
