# Command Line Interface

The Kampose CLI follows a command-based structure where you specify a command followed by command-specific options and arguments:

```shell
kampose <command> [OPTIONS] [ARGUMENTS]
```

When you run Kampose without a command, or with `--help` or `-h`, it displays help information. Use `--version` or `-v` to show the current version.

## Commands

### `build`

Generates documentation from .NET assemblies and XML comments.

The `build` command processes your .NET assemblies and XML documentation files to create API documentation using your chosen theme.

##### Usage:
```shell
kampose build [OPTIONS] [config-file]
```

##### Arguments:
- `config-file` - Path to your configuration file. Defaults to `kampose.json` in the current directory if not specified. You can omit the `.json` extension and it will be added automatically.

##### Options:
- `-c, --clean` - Clear the output directory before generating documentation.
- `-d, --debug` - Enable detailed logging to help diagnose issues during the documentation generation process.
- `-h, --help` - Show help information for the build command

> Be cautious when using the `--clean` option, as it deletes all contents of the configured output directory before generating new documentation. This can cause data loss if the output directory contains important files or is misconfigured.

> The `--debug` option increases logging verbosity when output is redirected (for example, to a log file or CI system). For normal console output it only enables stack traces for unhandled exceptions.

##### Examples:
```shell
# Use the default configuration file (kampose.json)
kampose build

# Use a custom configuration file
kampose build custom-config.json

# Specify a configuration file without the .json extension
kampose build my-config

# Generate documentation with debug output
kampose build my-config --debug > log.txt

# Clean the output directory before generating documentation
kampose build --clean
```

### `help`

Shows help information about commands.

Use the `help` command to learn about Kampose's commands. When called without arguments, it lists all available commands. Specify a command name to see detailed help for that specific command.

##### Usage:
```shell
kampose help [command]
```

##### Arguments:
- `command` - Optional. Name of the command to show help for.

##### Options:
- `-h, --help` - Show help information for the help command

##### Examples:
```shell
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
```shell
kampose version [OPTIONS]
```
##### Options:
- `-h, --help` - Show help information for the version command

##### Examples:
```shell
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
