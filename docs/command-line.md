# Command Line Interface

The Kampose CLI follows a command-based structure where you specify a command followed by command-specific options and arguments:

```bash
kampose <command> [OPTIONS] [ARGUMENTS]
```

When you run Kampose without a command, or with `--help` or `-h`, it displays help information. Use `--version` or `-v` to show the current version.

## Commands

### `build`

Generates documentation from .NET assemblies and XML comments.

The `build` command processes your .NET assemblies and XML documentation files to create API documentation using your chosen theme.

##### Usage:
```bash
kampose build [OPTIONS] [config-file]
```

##### Arguments:
- `config-file` - Path to your configuration file. Defaults to `kampose.json` in the current directory if not specified. You can omit the `.json` extension and it will be added automatically.

##### Options:
- `-d, --debug` - Enable detailed logging to help diagnose issues during the documentation generation process.
- `-h, --help` - Show help information for the build command

> The `--debug` option only affects the verbosity of the output when redirected (e.g., to a log file or CI/CD system).

##### Examples:
```bash
# Use the default configuration file (kampose.json)
kampose build

# Use a custom configuration file
kampose build custom-config.json

# Specify a configuration file without the .json extension
kampose build my-config

# Generate documentation with debug output
kampose build my-config --debug > log.txt
```

### `help`

Shows help information about commands.

Use the `help` command to learn about Kampose's commands. When called without arguments, it lists all available commands. Specify a command name to see detailed help for that specific command.

##### Usage:
```bash
kampose help [command]
```

##### Arguments:
- `command` - Optional. Name of the command to show help for.

##### Options:
- `-h, --help` - Show help information for the help command

##### Examples:
```bash
# Show general help and list all commands
kampose help

# Show help for the build command
kampose help build

# Alternative ways to show general help
kampose --help
kampose -h
```

### `version`

Shows the current version of Kampose.

##### Usage:
```bash
kampose version [OPTIONS]
```
##### Options:
- `-h, --help` - Show help information for the version command

##### Examples:
```bash
# Show version information
kampose version

# Alternative ways to show version
kampose --version
kampose -v
```

## Global Options

These options work with any command:

- `--help`, `-h` - Show help information
- `--version`, `-v` - Show version information

## Exit Codes

Kampose returns these exit codes to indicate what happened:

- `0` - Success
- `1` - Errors occurred during documentation generation
- `2` - Invalid command or options
- `3` - Configuration or theme validation errors
- `4` - Unexpected errors
