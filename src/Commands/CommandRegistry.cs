// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Commands
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Manages the registration and retrieval of commands.
    /// </summary>
    public static class CommandRegistry
    {
        /// <summary>
        /// Initializes the default commands.
        /// </summary>
        static CommandRegistry()
        {
            RegisterCommand<BuildCommand>();
            RegisterCommand<VersionCommand>();
            RegisterCommand<HelpCommand>();
        }

        /// <summary>
        /// A dictionary that stores commands, keyed by their names, using a case-insensitive string comparer.
        /// </summary>
        private static readonly Dictionary<string, Command> commands = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the registered commands.
        /// </summary>
        /// <value>
        /// A read-only collection of all registered commands.
        /// </value>
        public static IReadOnlyCollection<Command> Commands => commands.Values;

        /// <summary>
        /// Gets a command by name.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <returns>The command if found; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
        public static Command? GetCommand(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            return commands.TryGetValue(name, out var command) ? command : null;
        }

        /// <summary>
        /// Registers a command.
        /// </summary>
        /// <param name="command">The command to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when a command with the same name is already registered.</exception>
        private static void RegisterCommand<TCommand>() where TCommand : Command, new()
        {
            var command = new TCommand();
            if (!commands.TryAdd(command.Name, command))
                throw new ArgumentException($"A command with the name '{command.Name}' is already registered.", nameof(command));
        }
    }
}
