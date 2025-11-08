// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Commands
{
    using System;

    /// <summary>
    /// Represents an exception that occurs during parsing of command line options.
    /// </summary>
    public sealed class CommandException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="command">The command that caused the exception.</param>
        public CommandException(string message, Command? command = null)
            : base(message)
        {
            Command = command;
        }

        /// <summary>
        /// Gets the command that caused the exception.
        /// </summary>
        public Command? Command { get; }
    }
}