// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Commands
{
    using Kampose.Models;
    using Kampose.Outputs;
    using System;

    /// <summary>
    /// Implements the build command for generating documentation.
    /// </summary>
    public sealed class BuildCommand : DocumentationCommand<BuildCommand.Options>
    {
        /// <inheritdoc />
        public override string Name => "build";

        /// <inheritdoc />
        public override string Description => "Generate documentation from .NET assemblies and XML comments";

        /// <inheritdoc />
        protected override string OutcomeDescription => "Documentation generated";

        /// <inheritdoc />
        protected override bool TryParseOption(string arg, Options options)
        {
            if (arg is "-c" or "--clean")
            {
                options.Clean = true;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        protected override DocumentationOutput CreateOutput(Configuration config, Options options)
            => new FileSystemDocumentationOutput(config.OutputDirectory, options.Clean);

        /// <inheritdoc />
        public override void WriteHelp()
        {
            Console.WriteLine($"Usage: {nameof(Kampose)} {Name} [OPTIONS] [config-file]");
            Console.WriteLine();
            Console.WriteLine(Description);
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  -c, --clean                 Clear the output directory before generating documentation.");
            Console.WriteLine("  -d, --debug                 Enable detailed logging to help diagnose issues during the documentation generation process.");
            Console.WriteLine("  -h, --help                  Display this help message and exit.");
            Console.WriteLine();
            Console.WriteLine($"If no configuration file is specified, the tool defaults to '{Options.DefaultConfigFile}' in the current directory.");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine($"  {nameof(Kampose)} {Name}                      Use the default configuration file '{Options.DefaultConfigFile}' in the current directory.");
            Console.WriteLine($"  {nameof(Kampose)} {Name} custom-config.json   Use 'custom-config.json' as the configuration file.");
            Console.WriteLine($"  {nameof(Kampose)} {Name} --debug              Generate documentation with debug output.");
            Console.WriteLine($"  {nameof(Kampose)} {Name} --clean              Clear the output directory before generating documentation.");
            Console.WriteLine();
        }

        /// <summary>
        /// Represents the options for the build command.
        /// </summary>
        public sealed class Options : DocumentationOptions
        {
            /// <summary>
            /// Gets or sets a value indicating whether the output directory should be cleaned before generating documentation.
            /// </summary>
            /// <value>
            /// <see langword="true"/> if the output directory should be cleaned; otherwise, <see langword="false"/>.
            /// </value>
            public bool Clean { get; set; } = false;
        }
    }
}
