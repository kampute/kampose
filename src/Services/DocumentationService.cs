// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Services
{
    using Kampose.Builders;
    using Kampose.Models;
    using Kampose.Outputs;
    using Kampose.Reporters;
    using Kampose.Support;
    using Kampose.Templates;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Service responsible for generating documentation.
    /// </summary>
    public class DocumentationService
    {
        private static readonly AsyncLocal<TemplateRenderer?> currentRenderer = new();

        private readonly IActivityReporter reporter;

        /// <summary>
        /// Gets the current template renderer being used for documentation generation.
        /// </summary>
        /// <value>
        /// The <see cref="TemplateRenderer"/> currently in use, or <see langword="null"/> if no generation is in progress.
        /// </value>
        public static TemplateRenderer? CurrentRenderer => currentRenderer.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentationService"/> class.
        /// </summary>
        /// <param name="reporter">The activity reporter to use for tracking progress.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/> is <see langword="null"/>.</exception>
        public DocumentationService(IActivityReporter reporter)
        {
            this.reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        /// <summary>
        /// Validates the configured documentation sources and generates the documentation into the specified output.
        /// </summary>
        /// <param name="config">The documentation configuration.</param>
        /// <param name="output">The output that receives the generated files.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> or <paramref name="output"/> is <see langword="null"/>.</exception>
        /// <exception cref="ValidationException">Thrown when the sources are invalid, or when audit or URL verification issues are found and <see cref="AuditConfiguration.StopOnIssues"/> is enabled.</exception>
        /// <remarks>
        /// The output is prepared only after the validations that precede generation have passed. Referenced URLs are verified
        /// against the files recorded in <see cref="DocumentationOutput.GeneratedFiles"/>, not against the contents of the output directory.
        /// </remarks>
        public void GenerateDocumentation(Configuration config, DocumentationOutput output)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(output);

            // Load theme
            var theme = Theme.Load(config.Theme, config.Convention);

            // Build documentation context
            var context = new DocContextBuilder(reporter).Configure(config, theme).Build();

            // Validate assemblies and topics
            if (context.Assemblies.Count != 0)
                ValidateXmlDocumentation(context, config.Audit);
            else if (config.Assemblies.Count != 0)
                throw new ValidationException("No assemblies were found in the specified directories.");
            else if (!context.Topics.Any(static topic => !SpecialTopicIdentifiers.IsSpecialTopic(topic.Id)))
                throw new ValidationException("Without any assemblies, at least one topic must be provided to generate documentation.");

            output.Prepare(reporter);

            // Create renderer and generate documentation
            var renderer = new DocRendererBuilder(reporter).Build(context, config, theme);
            GenerateFiles(renderer, theme, context, output);

            // Verify referenced URLs
            VerifyReferencedUrls(context, config, output.GeneratedFiles);
        }

        /// <summary>
        /// Generates the documentation files using the specified renderer, theme, and context.
        /// </summary>
        /// <param name="renderer">The renderer to use for generating documentation.</param>
        /// <param name="theme">The theme containing bundles to process.</param>
        /// <param name="context">The documentation context to use.</param>
        /// <param name="output">The output that receives the generated files.</param>
        private void GenerateFiles(TemplateRenderer renderer, Theme theme, DocContext context, DocumentationOutput output)
        {
            var totalSteps = context.Sitemap.PageCount
                           + context.Assets.Count
                           + theme.ScriptFiles.Count
                           + theme.StyleFiles.Count;

            using (reporter.BeginActivity("Generating documentation", totalSteps))
            {
                var assetBundler = new AssetBundlerService(reporter, output);

                GeneratePages(renderer, context, output);
                BundleThemeScripts(assetBundler, context, theme, renderer.CommonData, output.OutputDirectory);
                BundleThemeStyles(assetBundler, theme, output.OutputDirectory);
                CopyAssets(context, output);
            }
        }

        /// <summary>
        /// Generates documentation pages.
        /// </summary>
        /// <param name="renderer">The renderer to use for generating documentation.</param>
        /// <param name="context">The documentation context to use.</param>
        /// <param name="output">The output that receives the pages.</param>
        private void GeneratePages(TemplateRenderer renderer, DocContext context, DocumentationOutput output)
        {
            using var _ = reporter.BeginActivity("Rendering documentation pages");
            var generator = new FileSystemDocumentationComposer(renderer);

            currentRenderer.Value = renderer;
            try
            {
                generator.GenerateDocumentation(output.CreatePageWriterFactory(), context);
            }
            finally
            {
                currentRenderer.Value = null;
            }
        }

        /// <summary>
        /// Generates theme script bundles.
        /// </summary>
        /// <param name="assetBundler">The bundler that produces the bundles.</param>
        /// <param name="context">The documentation context.</param>
        /// <param name="theme">The theme containing bundles to process.</param>
        /// <param name="templateData">The template data available to all templates.</param>
        /// <param name="outputDir">The output directory of the bundles.</param>
        private void BundleThemeScripts(AssetBundlerService assetBundler, DocContext context, Theme theme, IReadOnlyDictionary<string, object?> templateData, string outputDir)
        {
            if (theme.ScriptFiles.Count == 0)
                return;

            using var _ = reporter.BeginActivity("Bundling scripts");

            var scriptPrelude = GenerateGlobalVariablesScript(context, templateData);
            foreach (var (targetPath, sourceFiles) in theme.ScriptFiles)
            {
                var outputPath = Path.GetFullPath(Path.Combine(outputDir, targetPath));
                assetBundler.BundleScriptFiles(sourceFiles, outputPath, scriptPrelude);
                scriptPrelude = string.Empty; // Clear prelude after first use
            }
        }

        /// <summary>
        /// Generates theme style bundles.
        /// </summary>
        /// <param name="assetBundler">The bundler that produces the bundles.</param>
        /// <param name="theme">The theme containing bundles to process.</param>
        /// <param name="outputDir">The output directory of the bundles.</param>
        private void BundleThemeStyles(AssetBundlerService assetBundler, Theme theme, string outputDir)
        {
            if (theme.StyleFiles.Count == 0)
                return;

            using var _ = reporter.BeginActivity("Bundling styles");

            foreach (var (targetPath, sourceFiles) in theme.StyleFiles)
            {
                var outputPath = Path.GetFullPath(Path.Combine(outputDir, targetPath));
                assetBundler.BundleStyleFiles(sourceFiles, outputPath);
            }
        }

        /// <summary>
        /// Copies assets to their destination paths.
        /// </summary>
        /// <param name="context">The documentation context containing assets.</param>
        /// <param name="output">The output that receives the assets.</param>
        private void CopyAssets(DocContext context, DocumentationOutput output)
        {
            if (context.Assets.Count == 0)
                return;

            using var _ = reporter.BeginActivity("Processing assets");

            foreach (var asset in context.Assets)
            {
                using var __ = reporter.BeginStep(asset.TargetPath);
                try
                {
                    output.CopyFile(asset.SourcePath, asset.TargetPath);
                }
                catch (Exception error)
                {
                    reporter.LogError($"Failed to copy asset '{asset.SourcePath}' to '{asset.TargetPath}'.", error);
                }
            }
        }

        /// <summary>
        /// Validates the XML documentation files in the context based on the audit configuration.
        /// </summary>
        /// <param name="context">The documentation context.</param>
        /// <param name="auditConfig">The audit configuration.</param>
        /// <exception cref="ValidationException">Thrown if validation fails and stopping on issues is enabled.</exception>
        private void ValidateXmlDocumentation(DocContext context, AuditConfiguration auditConfig)
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
                throw new ValidationException($"Stopped due to {issueReporter.ReportedIssueCount} XML documentation issue(s).");
        }

        /// <summary>
        /// Verifies the referenced URLs in the documentation context.
        /// </summary>
        /// <param name="context">The documentation context.</param>
        /// <param name="config">The documentation configuration.</param>
        /// <param name="generatedFiles">The files generated for the documentation.</param>
        /// <exception cref="ValidationException">Thrown if URL verification fails and stopping on issues is enabled.</exception>
        private void VerifyReferencedUrls(DocContext context, Configuration config, GeneratedFileSet generatedFiles)
        {
            if (context.UrlReferences.Count == 0)
                return;

            var unverifiedUrls = context.UrlReferences
                .Where(urlRef => !UriHelper.IsQueryOrFragmentOnly(urlRef.SourceUrl))
                .DistinctBy(urlRef => string.Join('\0', [urlRef.ReferencingModel.Url, urlRef.SourceUrl]))
                .ToList();

            if (unverifiedUrls.Count == 0)
                return;

            var issueCount = 0;
            var verifyExternalLinks = config.Audit.VerifyExternalLinks;
            using var urlVerifier = new UrlVerifier(context, generatedFiles, config.BaseUrl);

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
                throw new ValidationException($"Stopped due to {issueCount} invalid URL reference(s).");
        }

        /// <summary>
        /// Generates a script containing global variables for the sitemap and theme parameters.
        /// </summary>
        /// <param name="context">The documentation context.</param>
        /// <param name="templateData">The template data available to all templates.</param>
        /// <returns>A string containing the JavaScript code that initializes <c>kampose</c> global variable.</returns>
        private static string GenerateGlobalVariablesScript(DocContext context, IReadOnlyDictionary<string, object?> templateData)
        {
            var kampose = new
            {
                context.Sitemap,
                config = templateData
            };

            using var reusable = StringBuilderPool.Shared.GetBuilder();
            return reusable.Builder
                .Append("window.kampose = ")
                .Append(Json.Stringify(kampose))
                .Append(';')
                .ToString();
        }
    }
}
