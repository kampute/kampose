// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Services
{
    using Kampose.Models;
    using Kampose.Reporters;
    using Kampose.Support;
    using Kampose.Templates;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Service responsible for generating documentation.
    /// </summary>
    public class DocumentationService
    {
        private static readonly AsyncLocal<TemplateRenderer?> currentRenderer = new();

        private readonly IActivityReporter reporter;
        private readonly AssetBundlerService assetBundler;

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
            assetBundler = new AssetBundlerService(reporter);
        }

        /// <summary>
        /// Generates documentation using the specified renderer, theme, and context.
        /// </summary>
        /// <param name="renderer">The renderer to use for generating documentation.</param>
        /// <param name="theme">The theme containing bundles to process.</param>
        /// <param name="context">The documentation context to use.</param>
        /// <param name="outputDir">The output directory where documentation will be generated.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <see langword="null"/> or empty.</exception>
        public void GenerateDocumentation(TemplateRenderer renderer, Theme theme, DocContext context, string outputDir)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentNullException.ThrowIfNull(theme);
            ArgumentNullException.ThrowIfNull(context);
            ArgumentException.ThrowIfNullOrEmpty(outputDir);

            var totalSteps = context.Sitemap.PageCount
                           + context.Assets.Count
                           + theme.ScriptFiles.Count
                           + theme.StyleFiles.Count;

            using (reporter.BeginActivity("Generating documentation", totalSteps))
            {
                GeneratePages(renderer, context, outputDir);
                BundleThemeScripts(context, theme, renderer.CommonData, outputDir);
                BundleThemeStyles(theme, outputDir);
                CopyAssets(context);
            }
        }

        /// <summary>
        /// Generates documentation pages.
        /// </summary>
        /// <param name="renderer">The renderer to use for generating documentation.</param>
        /// <param name="context">The documentation context to use.</param>
        /// <param name="outputDir">The output directory where documentation will be generated.</param>
        private void GeneratePages(TemplateRenderer renderer, DocContext context, string outputDir)
        {
            reporter.BeginActivity("Writing documentation pages");
            var generator = new FileSystemDocumentationComposer(renderer);

            currentRenderer.Value = renderer;
            try
            {
                generator.GenerateDocumentation(context, outputDir);
            }
            finally
            {
                currentRenderer.Value = null;
            }
        }

        /// <summary>
        /// Generates theme script bundles and copy them to the specified output directory.
        /// </summary>
        /// <param name="context">The documentation context.</param>
        /// <param name="theme">The theme containing bundles to process.</param>
        /// <param name="templateData">The template data available to all templates.</param>
        /// <param name="outputDir">The output directory where bundles will be saved.</param>
        private void BundleThemeScripts(DocContext context, Theme theme, IReadOnlyDictionary<string, object?> templateData, string outputDir)
        {
            if (theme.ScriptFiles.Count == 0)
                return;

            reporter.BeginActivity("Writing script bundles");

            var scriptPrelude = GenerateGlobalVariablesScript(context, templateData);
            foreach (var (targetPath, sourceFiles) in theme.ScriptFiles)
            {
                var outputPath = Path.GetFullPath(Path.Combine(outputDir, targetPath));
                assetBundler.BundleScriptFiles(sourceFiles, outputPath, scriptPrelude);
                scriptPrelude = string.Empty; // Clear prelude after first use
            }
        }

        /// <summary>
        /// Generates theme style bundles and copy them to the specified output directory.
        /// </summary>
        /// <param name="theme">The theme containing bundles to process.</param>
        /// <param name="outputDir">The output directory where bundles will be saved.</param>
        private void BundleThemeStyles(Theme theme, string outputDir)
        {
            if (theme.StyleFiles.Count == 0)
                return;

            reporter.BeginActivity("Writing style bundles");

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
        private void CopyAssets(DocContext context)
        {
            if (context.Assets.Count == 0)
                return;

            reporter.BeginActivity("Copying assets");

            foreach (var asset in context.Assets)
            {
                using var _ = reporter.BeginStep(asset.TargetPath);
                try
                {
                    CopyFile(asset.SourcePath, asset.TargetPath);
                }
                catch (Exception error)
                {
                    reporter.LogError($"Failed to copy asset '{asset.SourcePath}' to '{asset.TargetPath}'.", error);
                }
            }
        }

        /// <summary>
        /// Copies a file from the source path to the target path, creating directories as needed.
        /// </summary>
        /// <param name="sourcePath">The path of the source file.</param>
        /// <param name="targetPath">The path where the file should be copied to.</param>
        private static void CopyFile(string sourcePath, string targetPath)
        {
            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.Copy(sourcePath, targetPath, true);
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
