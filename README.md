# Kampose

A cross-platform .NET tool that transforms XML documentation comments into professional API documentation. Generate beautiful HTML or Markdown documentation with minimal configuration.

## Why Kampose

- **Zero Configuration Start** - Works out of the box with sensible defaults
- **Multiple Output Formats** - HTML for websites, Markdown for wikis
- **CI/CD Ready** - Integrate seamlessly into your build pipelines
- **Extensible Themes** - Customize appearance to match your brand
- **Quality Enforcement** - Audit documentation completeness automatically
- **Topic Integration** - Combine API docs with guides and tutorials

## Common Use Cases

#### API Library Documentation
Generate comprehensive reference documentation for libraries and SDKs with detailed member information, inheritance hierarchies, and code examples. Kampose automatically organizes documentation by namespace and type.

#### Framework Documentation
Document complex frameworks with multiple assemblies and extensive type hierarchies. Kampose's support for multiple assemblies enables unified documentation generation across entire ecosystems.

#### Project Documentation Sites
Create complete documentation sites combining API reference with conceptual content, tutorials, and guides. Kampose processes Markdown topics alongside API documentation.

#### Topic-Only Documentation
Generate documentation sites from Markdown files without assemblies, useful for user guides, tutorials, or project documentation without API references.

#### Continuous Documentation
Integrate documentation generation into CI/CD pipelines to keep documentation synchronized with code changes, eliminating manual maintenance and reducing documentation drift.

## Installation

```bash
dotnet tool install --global Kampose
```

## Quick Start

**Step 1**: Enable XML documentation in your project:
```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
```

**Step 2**: Create `kampose.json` in your project root:
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

## Documentation

Please visit [the official documentation site](https://kampute.github.io/kampose) for detailed guides and references:

- [Getting Started Guide](https://kampute.github.io/kampose/welcome.html)
- [Configuration Reference](https://kampute.github.io/kampose/configuration.html)
- [CI/CD Integration](https://kampute.github.io/kampose/ci-cd-integration.html)
- [Theme Customization](https://kampute.github.io/kampose/themes.html)
- [Best Practices](https://kampute.github.io/kampose/best-practices.html)

For technical details on the core DocToolkit library, see the [DocToolkit documentation](https://kampute.github.io/doc-toolkit/).

## Contributing

Contributions are welcome! Submit issues or pull requests on [GitHub](https://github.com/kampute/kampose).

## License

MIT License - see [LICENSE](LICENSE) for details.

## Acknowledgments

Kampose is built on outstanding open-source projects maintained by the .NET and broader development community:

- **[Handlebars.Net](https://github.com/Handlebars-Net/Handlebars.Net)** - Powerful template engine enabling flexible theme development
- **[JsonSchema.Net](https://github.com/gregsdennis/json-everything)** - Robust JSON schema validation for configuration files
- **[Markdig](https://github.com/xoofx/markdig)** - Fast and extensible Markdown processing
- **[Microsoft.Extensions.FileSystemGlobbing](https://github.com/dotnet/runtime)** - Flexible file pattern matching for assembly and content discovery
- **[NUglify](https://github.com/trullock/NUglify)** - Asset minification and optimization

The dedication and expertise of these project maintainers and contributors make Kampose possible. Thank you to everyone who contributes to the open-source ecosystem.
