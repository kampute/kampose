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
- `!tests/**` excludes all files in any `tests` folder.
- `assets/images/*.png` matches all PNG images in `assets/images`.
- `../shared/**/*.md` matches all Markdown files in a sibling `shared` folder.

## Notes

- Patterns are case-insensitive.
- Exclusion patterns override inclusion patterns if both match the same file.
- Use forward slashes (`/`) for directory separators, even on Windows.
- If a pattern does not specify a file extension, Kampose may append a default extension depending on context.

For more details, see the [Microsoft File Globbing documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing).
