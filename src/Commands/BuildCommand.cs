// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Commands
{
    using Kampose.Builders;
    using Kampose.Models;
    using Kampose.Reporters;
    using Kampose.Services;
    using Kampose.Support;
    using Kampute.DocToolkit;
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Implements the build command for generating documentation.
    /// </summary>
    public sealed class BuildCommand : Command<BuildCommand.Options>
    {
        private const string DefaultConfigFile = "kampose.json";

        /// <inheritdoc />
        public override string Name => "build";

        /// <inheritdoc />
        public override string Description => "Generate documentation from .NET assemblies and XML comments";

        /// <inheritdoc />
        protected override Options ParseArguments(string[] args)
        {
            var options = new Options();
            var configFile = default(string);

            foreach (var arg in args)
            {
                if (arg.StartsWith('-'))
                {
                    if (arg is "-d" or "--debug")
                        options.Verbose = true;
                    else
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

        /// <inheritdoc />
        protected override int ExecuteCommand(Options options, IActivityReporter reporter)
        {
            WriteApplicationHeader();

            // Set reporter verbosity
            reporter.Verbose = options.Verbose;

            // Load configuration
            var config = Configuration.LoadFromFile(options.ConfigPath);

            // Load theme
            var theme = Theme.Load(config.Theme, config.Convention);

            // Build documentation context
            var context = new DocContextBuilder(reporter).Configure(config, theme).Build();

            // Validate assemblies and topics
            if (context.Assemblies.Count != 0)
                ValidateXmlDocumentation(context, config.Audit, reporter);
            else if (config.Assemblies.Count != 0)
                throw new ValidationException("No assemblies were found in the specified directories.");
            else if (!context.Topics.Any(static topic => !SpecialTopicIdentifiers.IsSpecialTopic(topic.Id)))
                throw new ValidationException("Without any assemblies, at least one topic must be provided to generate documentation.");

            // Create renderer and generate documentation
            var renderer = new DocRendererBuilder(reporter)
                .Build(context, theme, config.ThemeSettings);

            // create documentation service and generate documentation
            var docService = new DocumentationService(reporter);
            docService.GenerateDocumentation(renderer, theme, context, config.OutputDirectory);

            // Report summary
            var summary = reporter.ErrorCount == 0
                ? reporter.WarningCount == 0
                    ? "Documentation generated successfully."
                    : $"Documentation generated with {reporter.WarningCount} warning(s)."
                : reporter.WarningCount == 0
                    ? $"Documentation generated with {reporter.ErrorCount} error(s)."
                    : $"Documentation generated with {reporter.ErrorCount} error(s) and {reporter.WarningCount} warning(s).";

            reporter.LogInformation(summary);

            return reporter.ErrorCount == 0 ? 0 : 1;
        }

        /// <inheritdoc />
        public override void WriteHelp()
        {
            Console.WriteLine($"Usage: {nameof(Kampose)} {Name} [OPTIONS] [config-file]");
            Console.WriteLine();
            Console.WriteLine(Description);
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  -d, --debug                 Enables detailed logging to help diagnose issues during the documentation generation process.");
            Console.WriteLine("  -h, --help                  Display this help message and exit.");
            Console.WriteLine();
            Console.WriteLine($"If no configuration file is specified, the tool defaults to '{DefaultConfigFile}' in the current directory.");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine($"  {nameof(Kampose)} {Name}                      Use the default configuration file '{DefaultConfigFile}' in the current directory.");
            Console.WriteLine($"  {nameof(Kampose)} {Name} custom-config.json   Use 'custom-config.json' as the configuration file.");
            Console.WriteLine($"  {nameof(Kampose)} {Name} --debug              Generate documentation with debug output.");
        }

        /// <summary>
        /// Validates the XML documentation files and reports any issues found.
        /// </summary>
        /// <param name="context">The documentation context containing the XML documentation files.</param>
        /// <param name="auditConfig">The audit configuration specifying options for XML documentation auditing.</param>
        /// <param name="reporter">The activity reporter for logging warnings or errors.</param>
        /// <exception cref="ValidationException">Thrown when no XML documentation files are found or when issues are found and the audit configuration is set to stop on issues.</exception>
        private static void ValidateXmlDocumentation(DocContext context, AuditConfiguration auditConfig, IActivityReporter reporter)
        {
            if (!context.ContentProvider.HasDocumentation)
            {
                if (auditConfig.StopOnIssues)
                    throw new ValidationException("No XML documentation files were found in the specified directories for auditing.");

                reporter.LogWarning("No XML documentation was found in the specified directories.");
            }
            else if (auditConfig.Options.Count != 0)
            {
                reporter.BeginActivity("Auditing XML documentation");

                var issueReporter = new XmlDocIssueReporter(reporter);
                foreach (var issue in context.InspectDocumentations(auditConfig.InspectionOptions))
                    issueReporter.Report(issue);

                if (issueReporter.ReportedIssueCount == 0)
                    reporter.LogInformation("No issues found during XML documentation audit.");
                else if (auditConfig.StopOnIssues)
                    throw new ValidationException($"Documentation generation skipped due to {issueReporter.ReportedIssueCount} XML documentation issue(s).");
            }
        }

        /// <summary>
        /// Represents the options for the build command.
        /// </summary>
        public sealed class Options
        {
            /// <summary>
            /// Gets or sets the configuration file path.
            /// </summary>
            /// <value>
            /// The path to the configuration file.
            /// </value>
            public string ConfigPath { get; set; } = DefaultConfigFile;

            /// <summary>
            /// Gets or sets a value indicating whether verbose reporting is enabled.
            /// </summary>
            /// <value>
            /// <see langword="true"/> if verbose reporting is enabled; otherwise, <see langword="false"/>.
            /// </value>
            public bool Verbose { get; set; } = false;
        }
    }
}
