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
    using Kampute.DocToolkit.Support;
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
                        options.Debug = true;
                    else if (arg is "-c" or "--clean")
                        options.Clean = true;
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
            reporter.Verbose = options.Debug;

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

            // Clean output directory if requested
            if (options.Clean)
                CleanOutputDirectory(config.OutputDirectory, reporter);

            // Create renderer and generate documentation
            var renderer = new DocRendererBuilder(reporter)
                .Build(context, theme, config.ThemeSettings);

            // create documentation service and generate documentation
            var docService = new DocumentationService(reporter);
            docService.GenerateDocumentation(renderer, theme, context, config.OutputDirectory);

            // Verify referenced URLs
            VerifyReferencedUrls(context, config, reporter);

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
            Console.WriteLine("  -c, --clean                 Clear the output directory before generating documentation.");
            Console.WriteLine("  -d, --debug                 Enable detailed logging to help diagnose issues during the documentation generation process.");
            Console.WriteLine("  -h, --help                  Display this help message and exit.");
            Console.WriteLine();
            Console.WriteLine($"If no configuration file is specified, the tool defaults to '{DefaultConfigFile}' in the current directory.");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine($"  {nameof(Kampose)} {Name}                      Use the default configuration file '{DefaultConfigFile}' in the current directory.");
            Console.WriteLine($"  {nameof(Kampose)} {Name} custom-config.json   Use 'custom-config.json' as the configuration file.");
            Console.WriteLine($"  {nameof(Kampose)} {Name} --debug              Generate documentation with debug output.");
            Console.WriteLine($"  {nameof(Kampose)} {Name} --clean              Clear the output directory before generating documentation.");
            Console.WriteLine();
        }

        /// <summary>
        /// Cleans the output directory by deleting all files and subdirectories within it.
        /// </summary>
        /// <param name="outputDirectory">The output directory to clean.</param>
        /// <param name="reporter">The activity reporter for logging.</param>
        private static void CleanOutputDirectory(string outputDirectory, IActivityReporter reporter)
        {
            if (!Directory.Exists(outputDirectory))
                return;

            using var _ = reporter.BeginActivity("Cleaning output directory");
            foreach (var dir in Directory.EnumerateDirectories(outputDirectory))
                Directory.Delete(dir, true);
            foreach (var file in Directory.EnumerateFiles(outputDirectory))
                File.Delete(file);
        }

        /// <summary>
        /// Validates the XML documentation files in the context based on the audit configuration.
        /// </summary>
        /// <param name="context">The documentation context.</param>
        /// <param name="auditConfig">The audit configuration.</param>
        /// <param name="reporter">The activity reporter for logging.</param>
        /// <exception cref="ValidationException">Thrown if validation fails and stopping on issues is enabled.</exception>
        private static void ValidateXmlDocumentation(DocContext context, AuditConfiguration auditConfig, IActivityReporter reporter)
        {
            if (!context.ContentProvider.HasDocumentation)
            {
                if (auditConfig.StopOnIssues)
                    throw new ValidationException("No XML documentation files were found in the specified directories for auditing.");

                reporter.LogWarning("No XML documentation was found in the specified directories.");
                return;
            }

            if (auditConfig.Options.Count == 0)
                return;

            using var _ = reporter.BeginActivity("Auditing XML documentation");

            var issueReporter = new XmlDocIssueReporter(reporter);
            foreach (var issue in context.InspectDocumentations(auditConfig.InspectionOptions))
                issueReporter.Report(issue);

            if (issueReporter.ReportedIssueCount == 0)
                reporter.LogVerbose("No issues found during XML documentation audit.");
            else if (auditConfig.StopOnIssues)
                throw new ValidationException($"Documentation generation skipped due to {issueReporter.ReportedIssueCount} XML documentation issue(s).");
        }

        /// <summary>
        /// Verifies the referenced URLs in the documentation context.
        /// </summary>
        /// <param name="context">The documentation context.</param>
        /// <param name="config">The documentation configuration.</param>
        /// <param name="reporter">The activity reporter for logging.</param>
        /// <exception cref="ValidationException">Thrown if URL verification fails and stopping on issues is enabled.</exception>
        private static void VerifyReferencedUrls(DocContext context, Configuration config, IActivityReporter reporter)
        {
            if (context.UrlReferences.Count == 0)
                return;

            var unverifiedUrls = context.UrlReferences
                .Where(urlRef => !UriHelper.IsQueryOrFragmentOnly(urlRef.SourceUrl))
                .DistinctBy(urlRef => urlRef.SourceUrl)
                .ToList();

            if (unverifiedUrls.Count == 0)
                return;

            var issueCount = 0;
            var verifyExternalLinks = config.Audit.VerifyExternalLinks;
            using var urlVerifier = new UrlVerifier(context, config.OutputDirectory, config.BaseUrl);

            using var _ = reporter.BeginActivity("Verifying referenced URLs", unverifiedUrls.Count);
            foreach (var urlReference in unverifiedUrls)
            {
                using var __ = reporter.BeginStep(urlReference.SourceUrl);
                var verificationResult = urlVerifier.VerifyUrl(urlReference, verifyExternalLinks);
                if (verificationResult != UrlVerifier.VerificationResult.OK)
                {
                    issueCount++;
                    reporter.LogWarning($"{verificationResult} URL '{urlReference.SourceUrl}' referenced in '{urlReference.ReferencingModel}' {urlReference.ReferencingModel.ModelType}.");
                }
            }

            if (issueCount == 0)
                reporter.LogVerbose("No issues found during URL verification.");
            else if (config.Audit.StopOnIssues)
                throw new ValidationException($"Documentation generation failed due to {issueCount} invalid URL reference(s).");
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
            /// Gets or sets a value indicating whether the output directory should be cleaned before generating documentation.
            /// </summary>
            /// <value>
            /// <see langword="true"/> if the output directory should be cleaned; otherwise, <see langword="false"/>.
            /// </value>
            public bool Clean { get; set; } = false;

            /// <summary>
            /// Gets or sets a value indicating whether detailed logging is enabled.
            /// </summary>
            /// <value>
            /// <see langword="true"/> if detailed logging is enabled; otherwise, <see langword="false"/>.
            /// </value>
            public bool Debug { get; set; } = false;
        }
    }
}
