// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Commands
{
    using Kampose.Reporters;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Linq;

    /// <summary>
    /// Implements the help command for displaying usage information.
    /// </summary>
    public sealed class HelpCommand : Command<HelpCommand.Options>
    {
        /// <inheritdoc />
        public override string Name => "help";

        /// <inheritdoc />
        public override string Description => "Display help information about commands";

        /// <inheritdoc />
        protected override Options ParseArguments(string[] args)
        {
            var options = new Options();

            foreach (var arg in args)
            {
                if (arg.StartsWith('-'))
                    throw new CommandException($"Unknown option '{arg}'.", this);

                if (options.Command is not null)
                    throw new CommandException("Only one command can be specified.", this);

                options.Command = CommandRegistry.GetCommand(arg)
                    ?? throw new CommandException($"Unknown command '{arg}'.", this);
            }

            return options;
        }

        /// <inheritdoc />
        protected override int ExecuteCommand(Options options, IActivityReporter reporter)
        {
            (options.Command ?? this).WriteHelp();
            return 0;
        }

        /// <inheritdoc />
        public override void WriteHelp()
        {
            const int Padding = 20;

            var indent = new string(' ', Padding + 2);
            var maxWidth = Console.WindowWidth > 0 ? Console.WindowWidth : 80;
            var maxDescriptionWidth = maxWidth - indent.Length;

            Console.WriteLine($"Usage: {nameof(Kampose)} <command> [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("A command-line tool for generating API documentation from .NET assemblies and XML comments.");
            Console.WriteLine();
            Console.WriteLine("COMMANDS:");
            foreach (var command in CommandRegistry.Commands.OrderBy(c => c.Name))
            {
                Console.Write("  ");
                Console.Write(command.Name.PadRight(Padding));

                var needsIndent = false;
                foreach (var line in TextUtility.SplitLines(command.Description, maxDescriptionWidth))
                {
                    if (needsIndent)
                        Console.Write(indent);
                    else
                        needsIndent = true;

                    Console.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Represents the options for the help command.
        /// </summary>
        public sealed class Options
        {
            /// <summary>
            /// Gets or sets the the command to display help for.
            /// </summary>
            /// <value>
            /// The command to display help for, or <see langword="null"/> to list all commands.
            /// </value>
            public Command? Command { get; set; }
        }
    }
}
