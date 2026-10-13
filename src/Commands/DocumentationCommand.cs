// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Commands
{
    using Kampose.Models;
    using Kampose.Outputs;
    using Kampose.Reporters;
    using Kampose.Services;
    using System.IO;

    /// <summary>
    /// Provides a base implementation for commands that run the documentation pipeline on a configuration file.
    /// </summary>
    /// <typeparam name="TOptions">The type of the command options.</typeparam>
    /// <remarks>
    /// Derived commands differ only in the <see cref="DocumentationOutput"/> that receives the generated files, so every command
    /// performs the same validations.
    /// </remarks>
    public abstract class DocumentationCommand<TOptions> : Command<TOptions> where TOptions : DocumentationOptions, new()
    {
        /// <summary>
        /// Gets the description of the outcome used in the summary, such as "Documentation generated".
        /// </summary>
        /// <value>
        /// The description of the outcome, completed in the summary by the number of reported errors and warnings.
        /// </value>
        protected abstract string OutcomeDescription { get; }

        /// <inheritdoc />
        protected override TOptions ParseArguments(string[] args)
        {
            var options = new TOptions();
            var configFile = default(string);

            foreach (var arg in args)
            {
                if (arg.StartsWith('-'))
                {
                    if (arg is "-d" or "--debug")
                        options.Debug = true;
                    else if (!TryParseOption(arg, options))
                        throw new CommandException($"Unknown option '{arg}'.", this);
                }
                else if (string.IsNullOrEmpty(configFile))
                {
                    configFile = arg;
                }
                else
                {
                    throw new CommandException("Only one configuration file can be specified.", this);
                }
            }

            if (!string.IsNullOrEmpty(configFile))
            {
                var extension = Path.GetExtension(configFile);
                if (extension.Length == 0)
                    configFile = Path.ChangeExtension(configFile, ".json");

                options.ConfigPath = configFile;
            }

            return options;
        }

        /// <summary>
        /// Parses a command-specific option.
        /// </summary>
        /// <param name="arg">The option argument.</param>
        /// <param name="options">The options to update.</param>
        /// <returns><see langword="true"/> if the option is recognized; otherwise, <see langword="false"/>.</returns>
        protected virtual bool TryParseOption(string arg, TOptions options) => false;

        /// <inheritdoc />
        protected override int ExecuteCommand(TOptions options, IActivityReporter reporter)
        {
            WriteApplicationHeader();

            reporter.Verbose = options.Debug;

            var config = Configuration.LoadFromFile(options.ConfigPath);
            var output = CreateOutput(config, options);

            new DocumentationService(reporter).GenerateDocumentation(config, output);

            reporter.LogInformation(FormatSummary(reporter.ErrorCount, reporter.WarningCount));
            return reporter.ErrorCount == 0 ? 0 : 1;
        }

        /// <summary>
        /// Creates the output that receives the generated files.
        /// </summary>
        /// <param name="config">The documentation configuration.</param>
        /// <param name="options">The parsed command options.</param>
        /// <returns>The <see cref="DocumentationOutput"/> for this command.</returns>
        protected abstract DocumentationOutput CreateOutput(Configuration config, TOptions options);

        /// <summary>
        /// Formats the summary reported when the command completes.
        /// </summary>
        /// <param name="errorCount">The number of reported errors.</param>
        /// <param name="warningCount">The number of reported warnings.</param>
        /// <returns>The summary message.</returns>
        private string FormatSummary(int errorCount, int warningCount) => (errorCount, warningCount) switch
        {
            (0, 0) => $"{OutcomeDescription} successfully.",
            (0, _) => $"{OutcomeDescription} with {warningCount} warning(s).",
            (_, 0) => $"{OutcomeDescription} with {errorCount} error(s).",
            _ => $"{OutcomeDescription} with {errorCount} error(s) and {warningCount} warning(s)."
        };
    }
}
