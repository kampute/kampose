# Kampose CLI - AI Agent Instructions

## Project Overview
Kampose is a command-line tool for generating API documentation from .NET assemblies and XML comments. It uses a plugin-based architecture with Handlebars templating and supports multiple output formats (HTML, Markdown).

## Architecture & Key Components

Key integration: The CLI wraps `Kampute.DocToolkit` types (`DocContext`, `DocumentPageRenderer`) with CLI-specific builders and services.

### Core Flow
1. **Configuration** (`kampose.json`) → **DocContextBuilder** → **DocumentationService** → **TemplateRenderer** → Output
2. Entry point: `Program.cs` handles CLI parsing, creates activity reporters, and orchestrates the main flow
3. The `DocContext` serves as the central data model containing assemblies, XML docs, topics, and theme data

### Critical Service Boundaries
- **DocContextBuilder** (`src/Builders/`): Transforms configuration into executable context with assemblies, topics, and addressing strategy
- **DocumentationService** (`src/Services/`): Orchestrates template rendering and asset bundling with background thread-local renderer state
- **TemplateRenderer** (`src/Templates/`): Handlebars-based templating engine with custom helpers and formatters
- **Activity Reporters** (`src/Reporters/`): Pluggable logging system that switches between console and text writer based on output redirection

### CLI Command Structure
- **Command Base Classes**: `Command` and `Command<TOptions>` provide consistent parsing, help, and execution patterns
- **Error Handling**: `CommandException` (exit code 2) for CLI errors, `ValidationException` (exit code 3) for config/input validation
- **Exit Codes**: 0=success, 1=errors, 2=CLI args error, 3=config validation error, 4=unexpected error
- **Help System**: Automatic `-h/--help` detection with command-specific usage examples

### Builder Pattern Implementation
- **Fluent Configuration**: `DocContextBuilder.Configure()` chains assembly/topic/asset collection with strategy setup
- **Renderer Building**: `DocRendererBuilder.Build()` configures Handlebars environment with theme templates and common data
- **Immutable Construction**: Builders create fully configured objects without partial state exposure

### Template System Pattern
Templates use Handlebars with a two-tier structure:
- Theme resources in `resources/themes/{html|md}/`
- Custom helpers in `src/Templates/Helpers/` and formatters in `src/Templates/Formatters/`
- Access current renderer via static `DocumentationService.CurrentRenderer` in template contexts

## Development Workflows

### Building & Testing
```powershell
# Build the solution
dotnet build Kampose.sln

# Run tests
dotnet test tests/Kampose.Test.csproj

# Build and test in one command
dotnet build Kampose.sln && dotnet test tests/Kampose.Test.csproj
```

### Running Kampose
```powershell
# Run with default config (Kampose.json)
dotnet run --project src

# Run with custom config and audit
dotnet run --project src -- MyConfig.json -a=qvrx

# Show theme parameters
dotnet run --project src -- -t
```

### Adding New Features
- **New CLI options**: Update `ProgramOptions.cs` parse methods and help text
- **Configuration options**: Add to `Models/Configuration.cs` and update JSON schema in `resources/schemas/ConfigurationSchema.json`
- **Template helpers**: Add to `Templates/Helpers/` and register in `HandlebarsFactory.cs`
- **Output formats**: Extend theme system in `resources/themes/`

## Project-Specific Conventions

### Error Handling Strategy
- Use `ValidationException` for configuration/input validation errors (exit code 3)
- Use `OptionException` for CLI argument errors (exit code 2)
- Activity reporters collect warnings/errors and determine final exit codes (0 = success, 1 = errors)

### File Organization Pattern
- Models are data-only classes in `src/Models/`
- Services contain business logic in `src/Services/`
- Builders use fluent patterns for complex object construction in `src/Builders/`
- Support utilities are in `src/Support/` (file globbing, JSON helpers, validation)

### Assembly & XML Doc Handling
- Assemblies and XML docs are resolved via glob patterns in configuration
- Missing XML docs trigger warnings unless audit mode with `StopOnIssues=true`
- XML doc audit supports granular flags: `q` (quit on issues), `v` (value), `x` (exception), `p` (permission), etc.

### Asset & Theme Management
- Themes are loaded from external directories with strongly typed `ThemeConfiguration`
- Assets are bundled via `AssetBundlerService` with configurable transfer filters
- Theme settings are validated against parameter definitions with type conversion
- Script/style bundling uses NUglify for minification with source map support

## External Dependencies
- **Kampute.DocToolkit**: Core documentation processing library (separate NuGet package)
- **Handlebars.Net**: Template engine for theme rendering
- **JsonSchema.Net**: Configuration validation
- **Markdig**: Markdown processing for topics
- **Microsoft.Extensions.FileSystemGlobbing**: File pattern matching
- **NUglify**: CSS and Javascript minification

## JSON Schema Validation Patterns
- Configuration validation uses `JsonSchema.Net` with schema files located in `resources/schemas/`
- `Configuration.json` defines the main Kampose.json structure
- `ThemeConfiguration.json` validates theme-specific settings
- Use `Configuration.LoadFromFile()` and `ThemeConfiguration.LoadFromFile()` which automatically validates against schema
- Validation errors throw `ValidationException` with detailed error paths

## Handlebars Template System Details
- Templates registered in `HandlebarsFactory.cs` with custom configuration
- **Helper registration**: Add to `src/Templates/Helpers/` and register via `RegisterHelper()` calls
- **Formatter registration**: Add to `src/Templates/Formatters/` and register via `RegisterFormatter()` calls
- **Template context**: Access to `DocumentationService.CurrentRenderer` provides renderer state
- **Data binding**: Use `TemplateData` class to structure context data for templates
- **Text encoding**: Custom `TemplateTextEncoder` handles HTML/Markdown output formatting

## Test Organization & Mocking Patterns
- Uses NUnit framework with Moq for mocking
- Use `[TestCase(..., ExpectedResult=...)]` for parameterized tests.
- Name test methods: `MemberName_StateUnderTest_ExpectedBehavior` (example: `CalculateTotal_ValidInput_ReturnsCorrectSum`).
- Prefer constraint-based assertions.
- Write only tests for public members and business logic (skip private methods and parameter validation).
- Tests in `tests/` mirror `src/` structure (e.g., `tests/Models/`, `tests/Services/`)
- `MockHelper.cs` provides common mock setups for `IActivityReporter` and other interfaces

## Activity Reporter System Deep Dive
- **Reporter selection**: `CreateActivityReporter()` chooses between Console/TextWriter based on output redirection
- **ConsoleActivityReporter**: Interactive progress bars and colored output for terminals
- **TextWriterActivityReporter**: Plain text output for redirected/CI scenarios
- **Error aggregation**: Reporters track warning/error counts for final exit code determination
- **Specialized reporters**: `XmlDocAuditReporter` and `XmlDocErrorReporter` for documentation validation

## Theme Development Workflow
- Themes located in `resources/themes/{format}/` (html, markdown)
- Theme structure: templates, assets, and configuration files
- **Adding themes**: Create new directory under appropriate format folder
- **Theme parameters**: Define in theme config, expose via `-t` CLI option
- **Asset bundling**: Use `AssetBundlerService` with `FileTransferFilter` for selective copying
- **Template inheritance**: Themes can extend base templates with overrides

## Development Guidelines
- **SOLID principles**: Prioritize Single Responsibility and Open/Closed principles
- **Simplicity over complexity**: Avoid excessive helper methods and unnecessary abstractions
- **Self-documenting code**: Code should be readable without redundant inline comments
- **Interface pragmatism**: Use interfaces judiciously - avoid "Ravioli" (too many small interfaces) and "Lasagna" (too many layers)
- **Performance & clarity**: Optimize for both execution speed and code understanding
- **Problem-solving approach**: Question existing solutions, propose improvements, document limitations when needed

## Documentation Guidelines
- Avoid promotional, flowery, or overly embellished language, and use adjectives and adverbs only when strictly necessary.
- Emphasize purpose and usage; do not document implementation details or obvious information.
- Provide meaningful context and call out important behavioral nuances or edge cases.
- Keep content concise and focused by using short sentences and brief paragraphs to convey information clearly.
- Organize content using bullet points, numbered lists, or tables when appropriate, and explain the context or purpose of the list or table in at least one paragraph.
- Numbered lists should be used for steps in a process or sequence only.
- When writing XML documentation comments, use appropriate XML tags for references, language keywords, and for organizing information in lists and tables. Ensure tags are used to clarify context, structure information, and improve readability for consumers of the documentation.