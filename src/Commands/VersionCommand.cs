// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Commands
{
    using Kampose.Reporters;
    using System;
    using System.Reflection;

    /// <summary>
    /// Implements the version command for displaying version information.
    /// </summary>
    public sealed class VersionCommand : Command<VersionCommand.Options>
    {
        /// <inheritdoc />
        public override string Name => "version";

        /// <inheritdoc />
        public override string Description => "Display version information";

        /// <inheritdoc />
        protected override Options ParseArguments(string[] args)
        {
            var options = new Options();

            foreach (var arg in args)
            {
                if (arg.StartsWith('-'))
                    throw new CommandException($"Unknown option '{arg}'.", this);

                throw new CommandException($"Unexpected argument '{arg}'.", this);
            }

            return options;
        }

        /// <inheritdoc />
        protected override int ExecuteCommand(Options options, IActivityReporter reporter)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"{nameof(Kampose)} {version}");
            return 0;
        }

        /// <inheritdoc />
        public override void WriteHelp()
        {
            Console.WriteLine($"Usage: {nameof(Kampose)} {Name} [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine(Description);
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  -h, --help                  Display this help message and exit.");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine($"  {nameof(Kampose)} {Name}                      Display version information.");
            Console.WriteLine();
        }

        /// <summary>
        /// Represents the options for the version command.
        /// </summary>
        public sealed class Options
        {
            // No options for version command
        }
    }
}
