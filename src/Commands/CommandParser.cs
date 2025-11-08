// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Commands
{
    using System;

    /// <summary>
    /// Provides functionality for parsing command-line arguments and routing to commands.
    /// </summary>
    public static class CommandParser
    {
        /// <summary>
        /// Parses the command-line arguments and returns the appropriate command with remaining arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A tuple containing the command and the remaining arguments for the command.</returns>
        /// <exception cref="CommandException">Thrown when an invalid command is specified.</exception>
        public static (Command Command, string[] Args) Parse(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);

            if (args.Length == 0 || args[0] is "--help" or "-h")
                return (CommandRegistry.GetCommand("help")!, []);
            if (args[0] is "--version" or "-v")
                return (CommandRegistry.GetCommand("version")!, []);

            var commandName = args[0];
            var command = CommandRegistry.GetCommand(commandName)
                ?? throw new CommandException($"Unknown command '{commandName}'.");

            return (command, args[1..]);
        }
    }
}
