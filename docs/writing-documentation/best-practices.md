---
title: Best Practices
summary: "Practical guidance for clear, complete, and maintainable XML documentation comments."
---

# Best Practices

Write XML documentation for the people who use an API. Describe its contract and intent rather than restating its declaration.

## Start with the Public Contract

- Give every public and protected type and member a concise `<summary>`.
- Document each parameter with `<param>` and each generic type parameter with `<typeparam>`.
- Use `<returns>` to explain the meaning of a return value, including significant `null`, empty, or sentinel results.
- Use `<value>` when a property's valid range, default, units, or semantics need explanation.
- Document exceptions callers are expected to handle with `<exception>`, including the condition that causes each exception.

Keep the summary short enough to scan in a member list. Put background, constraints, side effects, and usage guidance in `<remarks>`.

## Make References Precise

Use `<paramref>` and `<typeparamref>` when referring to parameters so their names remain connected to the declaration. Use `<see>` for inline references and `<seealso>` for separate entries in the generated **See Also** section.

Use `<inheritdoc />` when an implementation genuinely shares an inherited contract. Add local documentation when the implementation changes behavior, constraints, or exceptions.

Link API comments to guides when a concept is too large for a member description. See [Conceptual Topics](conceptual-topics.md#link-from-xml-documentation) for topic-reference forms.

## Structure Longer Content

Use `<para>` to divide prose into readable paragraphs and `<list>` for real sequences, choices, or tables. Keep block content such as `<code>`, `<list>`, and `<note>` out of inline elements and table cells.

For Markdown output, a `<list type="table">` needs a `<listheader>` so the result has a valid header row. Keep table-cell content to one paragraph with inline formatting because Markdown tables do not reliably support blocks or line breaks in cells.

## Review for Usefulness

Before publishing, check that documentation:

- explains behavior that is not obvious from the signature;
- uses the same terminology across related APIs;
- describes units, ranges, defaults, ownership, and mutability where relevant;
- includes examples for workflows that are difficult to infer;
- links to related APIs and conceptual guidance without duplicating them;
- still matches the implementation after behavior changes.

## See Also

- [Recommended XML documentation tags](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags)
