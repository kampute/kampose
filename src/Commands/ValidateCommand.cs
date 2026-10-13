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
    /// Implements the validate command, which performs every validation of the build command without writing any file.
    /// </summary>
    public sealed class ValidateCommand : DocumentationCommand<DocumentationOptions>
    {
        /// <inheritdoc />
        public override string Name => "validate";

        /// <inheritdoc />
        public override string Description => "Run all validations of the build command without generating any file";

        /// <inheritdoc />
        protected override string OutcomeDescription => "Validation completed";

        /// <inheritdoc />
        protected override DocumentationOutput CreateOutput(Configuration config, DocumentationOptions options)
            => new DryRunDocumentationOutput(config.OutputDirectory);

        /// <inheritdoc />
        public override void WriteHelp()
        {
            Console.WriteLine($"Usage: {nameof(Kampose)} {Name} [OPTIONS] [config-file]");
            Console.WriteLine();
            Console.WriteLine(Description);
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  -d, --debug                 Enable detailed logging to help diagnose issues during validation.");
            Console.WriteLine("  -h, --help                  Display this help message and exit.");
            Console.WriteLine();
            Console.WriteLine($"If no configuration file is specified, the tool defaults to '{DocumentationOptions.DefaultConfigFile}' in the current directory.");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine($"  {nameof(Kampose)} {Name}                      Use the default configuration file '{DocumentationOptions.DefaultConfigFile}' in the current directory.");
            Console.WriteLine($"  {nameof(Kampose)} {Name} custom-config.json   Use 'custom-config.json' as the configuration file.");
            Console.WriteLine($"  {nameof(Kampose)} {Name} --debug              Validate with debug output.");
            Console.WriteLine();
        }
    }
}
