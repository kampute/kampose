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
        /// When output is redirected, a <see cref="TextWriterActivityReporter"/> is created with the following behavior:
        /// <list type="bullet">
        ///   <item>If only one stream is redirected: Both normal output and errors use the same writer to keep all output together.</item>
        ///   <item>If both streams are redirected: Each uses its own writer (normal output to stdout, errors/warnings to stderr).</item>
        /// </list>
        /// When output is not redirected, a <see cref="ConsoleActivityReporter"/> provides output with progress bars and colors.
        /// If console reporter creation fails, a basic <see cref="TextWriterActivityReporter"/> is used as fallback.
        /// </remarks>
        private static IActivityReporter CreateActivityReporter()
        {
            if (Console.IsOutputRedirected || Console.IsErrorRedirected)
            {
                return Console.IsOutputRedirected && Console.IsErrorRedirected
                    ? new TextWriterActivityReporter(Console.Out, Console.Error)
                    : new TextWriterActivityReporter(Console.IsOutputRedirected ? Console.Out : Console.Error);
            }

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
