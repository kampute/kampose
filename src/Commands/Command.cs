// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Commands
{
    using Kampose.Reporters;
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides a base implementation for commands with common functionality.
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        /// <value>
        /// The name of the command, which is used to identify it in the CLI.
        /// </value>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the description of the command.
        /// </summary>
        /// <value>
        /// The description of the command, which provides more information about its purpose and usage.
        /// </value>
        public abstract string Description { get; }

        /// <summary>
        /// Executes the command with the specified arguments.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <param name="reporter">The activity reporter for logging.</param>
        /// <returns>The exit code (0 for success, non-zero for failure).</returns>
        public abstract int Execute(string[] args, IActivityReporter reporter);

        /// <summary>
        /// Writes the help information for this command.
        /// </summary>
        public abstract void WriteHelp();

        /// <summary>
        /// Determines whether help should be shown based on the provided arguments.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns><see langword="true"/> if help should be shown; otherwise, <see langword="false"/>.</returns>
        protected virtual bool ShouldShowHelp(string[] args) => args.Contains("--help") || args.Contains("-h");

        /// <summary>
        /// Writes the application header to the console.
        /// </summary>
        protected static void WriteApplicationHeader()
        {
            Console.WriteLine($"{nameof(Kampose)} [Version {Assembly.GetExecutingAssembly().GetName().Version}]");
            Console.WriteLine("(c) 2025 Kampute. All rights reserved.");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Provides a type-safe base implementation for commands with strongly-typed options.
    /// </summary>
    /// <typeparam name="TOptions">The type of the command options.</typeparam>
    public abstract class Command<TOptions> : Command where TOptions : class
    {
        /// <inheritdoc />
        public override int Execute(string[] args, IActivityReporter reporter)
        {
            ArgumentNullException.ThrowIfNull(args);
            ArgumentNullException.ThrowIfNull(reporter);

            if (ShouldShowHelp(args))
            {
                WriteHelp();
                return 0;
            }

            var options = ParseArguments(args);
            return ExecuteCommand(options, reporter);
        }

        /// <summary>
        /// Parses the command-specific arguments.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>The parsed command options.</returns>
        /// <exception cref="CommandException">Thrown when an invalid option is provided.</exception>
        protected abstract TOptions ParseArguments(string[] args);

        /// <summary>
        /// Executes the command with the parsed options.
        /// </summary>
        /// <param name="options">The parsed command options.</param>
        /// <param name="reporter">The activity reporter for logging.</param>
        /// <returns>The exit code (0 for success, non-zero for failure).</returns>
        protected abstract int ExecuteCommand(TOptions options, IActivityReporter reporter);
    }
}
