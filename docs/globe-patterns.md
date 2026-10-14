---
title: Glob Patterns
summary: "Supported glob syntax and examples for selecting assemblies, XML documentation files, topics, and assets."
---

# Glob Patterns

A glob pattern describes which files or folders to select. Patterns can combine wildcards, directory traversal, and exclusion rules. This guide explains the supported syntax and provides usage examples.

## Supported Syntax

The following table summarizes the glob pattern features available in Kampose:

| Pattern    | Meaning                                             |
|------------|-----------------------------------------------------|
| `*`        | Matches any number of characters in a name          |
| `**`       | Matches any number of directory levels              |
| `..`       | Refers to the parent directory                      |
| `!pattern` | Excludes files or directories matching the pattern  |
| `/`        | Directory separator (use forward slash only)        |

Use these features to build patterns that match specific files, groups of files, or entire directory trees.

## Usage Examples

The following examples show how to use glob patterns in Kampose configuration files:

- `src/**/*.dll` matches all assembly files in `src` and its subdirectories.
- `docs/*` matches all files directly under the `docs` folder.
- `**/*.xml` matches all XML files anywhere in the project.
- `!tests/**` excludes all files in the `tests` folder at the root of the base directory. Use `!**/tests/**` to exclude every `tests` folder.
- `assets/images/*.png` matches all PNG images in `assets/images`.
- `../shared/**/*.md` matches all Markdown files in a sibling `shared` folder.

## Notes

- Patterns are case-insensitive.
- Exclusion patterns override inclusion patterns if both match the same file.
- Use forward slashes (`/`) for directory separators, even on Windows.
- When the last segment of a pattern has no file extension, Kampose appends the default extension of the setting: `.dll` for `assemblies`, `.xml` for `xmlDocs`, and `.md` for `topics`. For example, `docs/**` in `topics` matches `docs/**/*.md`. Asset patterns have no default extension.

For more details, see the [Microsoft File Globbing documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing).
