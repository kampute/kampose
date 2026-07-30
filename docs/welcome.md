---
title: Home
summary: A command-line tool that combines .NET assemblies, XML documentation comments, and conceptual topics into HTML or Markdown documentation.
---

[![Version](https://img.shields.io/github/v/release/kampute/kampose?label=Version&color=darkred)](https://github.com/kampute/kampose/releases)

# Welcome to Kampose

Kampose `/kam-pohz/` is a cross-platform command-line documentation composer for .NET projects. It combines assembly metadata, XML documentation comments, and conceptual topics, then renders them through configurable Handlebars themes.

## Overview

Kampose generates API reference for libraries, frameworks, and internal APIs. It can also combine that reference with guides and tutorials, or build a topic-only documentation site without assemblies.

Auditing can report missing or incomplete XML documentation and verify referenced URLs. Configuration controls which checks run and whether reported issues stop the build.

### Key Capabilities

- **Multiple Output Formats** - Generate HTML for websites or Markdown for wikis (GitHub, Azure DevOps)
- **Flexible Configuration** - JSON-based configuration with glob patterns for precise control over content discovery and organization
- **Extensible Themes** - Built-in themes with comprehensive customization options and support for custom theme development
- **Content Integration** - Combine API documentation with topics, tutorials, and guides, or generate topic-only documentation sites
- **Quality Assurance** - Built-in auditing validates documentation completeness and enforces quality standards
- **CI/CD Integration** - Designed for automated generation in GitHub Actions, Azure Pipelines, and other CI/CD platforms

## Getting Started

### Installation

Install Kampose as a .NET Global Tool:

```shell
dotnet tool install --global kampose
```

To update to the latest version, run:

```shell
dotnet tool update --global kampose
```

### Basic Usage

Follow these simple steps to generate your first API documentation with Kampose.

**Step 1**: Enable XML documentation generation in your project file:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

**Step 2**: Create a configuration file (`kampose.json`) in your project root:

```json
{
    "convention": "dotNet",
    "outputDirectory": "docs",
    "assemblies": ["bin/Release/**/*.dll"],
    "theme": "classic"
}
```

For more configuration options and examples, refer to the [Configuration](configuration.md) guide.

**Step 3**: Build your project and generate documentation:

```shell
dotnet build -c Release
kampose build
```

For detailed usage instructions and examples, see the [Command-Line Interface](command-line.md) guide.

## AI Agent Skill

A maintained Kampose skill is available for compatible AI coding agents. It
helps agents install and configure Kampose, author conceptual documentation,
customize themes, integrate documentation builds with CI, review existing
setups, and diagnose generated output.

[Review the skill and installation instructions][kampose-skill].

After installation, try:

```text
How would you recommend setting up Kampose documentation for this repository?
```

[kampose-skill]: https://github.com/Khojasteh/degardis-skills/tree/main/skills/kampose

## Guide to This Documentation

This guide organizes Kampose's documentation into key areas to help you navigate and find the information you need efficiently.

### Commands and Configuration
- **[Command-Line Interface](command-line.md)** - Commands, options, and usage patterns
- **[Configuration Guide](configuration.md)** - Complete configuration reference with examples
- **[Glob Patterns](globe-patterns.md)** - File selection patterns for assemblies, XML docs, and topics

### Integration and Automation
- **[CI/CD Integration](ci-cd-integration.md)** - Automate documentation generation in GitHub Actions, Azure Pipelines, and GitLab CI/CD

### Writing Documentation
- **[Writing Documentation](writing-documentation.md)** - Choose and author conceptual or API documentation
- **[URL Resolution](url-resolution.md)** - Choose document-, site-, or documentation-root-relative URLs

### Customizing Output
- **[Themes Overview](themes.md)** - Available themes and configuration
- **[Classic HTML Theme](themes/classic-html-theme.md)** - HTML theme customization
- **[Classic Markdown Theme](themes/classic-md-theme.md)** - Markdown theme and DevOps wiki integration
- **[Theme Authoring Guide](themes/theme-authoring.md)** - Creating custom themes

## Support and Community

Kampose is an open-source project released under the [MIT License](LICENSE). Contributions, bug reports, and feature requests are welcome.

## Acknowledgments

Kampose is built on outstanding open-source projects maintained by the .NET and broader development community:

- **[Handlebars.Net](https://github.com/Handlebars-Net/Handlebars.Net)** - Powerful template engine enabling flexible theme development
- **[JsonSchema.Net](https://github.com/gregsdennis/json-everything)** - Robust JSON schema validation for configuration files
- **[Markdig](https://github.com/xoofx/markdig)** - Fast and extensible Markdown processing
- **[Microsoft.Extensions.FileSystemGlobbing](https://github.com/dotnet/runtime)** - Flexible file pattern matching for assembly and content discovery
- **[NUglify](https://github.com/trullock/NUglify)** - Asset minification and optimization
- **[PrismJS](https://prismjs.com/)** - Syntax highlighting for code samples in documentation

The dedication and expertise of these project maintainers and contributors make Kampose possible. Thank you to everyone who contributes to the open-source ecosystem.
