// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose
{
    using Kampose.Commands;
    using Kampose.Reporters;
    using Kampose.Support;
    using System;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>The exit code of the application.</returns>
        public static int Main(string[] args)
        {
            using var reporter = CreateActivityReporter();

            try
            {
                var (command, commandArgs) = CommandParser.Parse(args);
                return command.Execute(commandArgs, reporter);
            }
            catch (CommandException e)
            {
                Console.Error.WriteLine(e.Message);
                if (e.Command is not null)
                    Console.Error.WriteLine($"Try '{nameof(Kampose)} help {e.Command.Name}' for more information.");
                else
                    Console.Error.WriteLine($"Try '{nameof(Kampose)} help' to see available commands.");
                return 2;
            }
            catch (ValidationException error)
            {
                reporter.LogError(error.Message, error.Errors);
                return 3;
            }
            catch (Exception error)
            {
                reporter.LogError(error);
                return 4;
            }
        }

        /// <summary>
        /// Creates an instance of an activity reporter based on the current console output state.
        /// </summary>
        /// <returns>An <see cref="IActivityReporter"/> instance suitable for the current console output configuration.</returns>
        /// <remarks>
        /// If the console output is redirected, a <see cref="TextWriterActivityReporter"/> is returned with verbose logging enabled
        /// and separate writers for standard output and error streams.
        /// If the console output is not redirected, a <see cref="ConsoleActivityReporter"/> is attempted to be created.
        /// If the creation of <see cref="ConsoleActivityReporter"/> fails, a <see cref="TextWriterActivityReporter"/> is returned
        /// with verbose logging disabled.
        /// </remarks>
        private static IActivityReporter CreateActivityReporter()
        {
            if (Console.IsOutputRedirected)
                return new TextWriterActivityReporter(Console.Out, Console.Error);

            try
            {
                return new ConsoleActivityReporter();
            }
            catch
            {
                return new TextWriterActivityReporter(Console.Out);
            }
        }
    }
}
