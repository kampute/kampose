# Kampose

Kampose `/kam-pohz/` is a cross-platform command-line documentation generator for .NET projects that transforms XML comments and assembly metadata into professional API documentation. Built with an extensible architecture and powered by Handlebars templating, Kampose enables development teams to maintain high-quality documentation as part of their continuous integration workflows.

## Overview

Kampose automates API documentation generation from XML documentation comments, eliminating manual documentation maintenance. Whether building libraries, frameworks, or internal APIs, Kampose creates professional documentation that integrates seamlessly with modern development workflows.

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

```bash
dotnet tool install --global Kampose
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

**Step 3**: Build your project and generate documentation:

```bash
dotnet build -c Release
kampose build
```

For detailed usage instructions, see the [Command-Line Interface](command-line.md) guide.

## Guide to This Documentation

This guide organizes Kampose's documentation into key areas to help you navigate and find the information you need efficiently.

#### Getting Started
- **[Command-Line Interface](command-line.md)** - Commands, options, and usage patterns
- **[Configuration Guide](configuration.md)** - Complete configuration reference with examples
- **[Glob Patterns](globe-patterns.md)** - File selection patterns for assemblies, XML docs, and topics

#### Integration and Automation
- **[CI/CD Integration](ci-cd-integration.md)** - Automate documentation generation in GitHub Actions, Azure Pipelines, and GitLab CI/CD

#### Writing Documentation
- **[XML Documentation Tags](xmldoc-tags.md)** - Supported XML tags with usage examples
- **[Best Practices](best-practices.md)** - Guidelines for effective XML documentation

#### Customizing Output
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

The dedication and expertise of these project maintainers and contributors make Kampose possible. Thank you to everyone who contributes to the open-source ecosystem.
