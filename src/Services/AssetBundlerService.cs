// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Services
{
    using Kampose.Outputs;
    using Kampose.Reporters;
    using Kampute.DocToolkit.Support;
    using NUglify;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Provides functionality for bundling and minifying script and style files.
    /// </summary>
    public class AssetBundlerService
    {
        private readonly IActivityReporter reporter;
        private readonly DocumentationOutput output;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBundlerService"/> class.
        /// </summary>
        /// <param name="reporter">The activity reporter to use for logging messages.</param>
        /// <param name="output">The output that receives the bundles.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/> or <paramref name="output"/> is <see langword="null"/>.</exception>
        public AssetBundlerService(IActivityReporter reporter, DocumentationOutput output)
        {
            this.reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }

        /// <summary>
        /// Processes script files by concatenating and minifying them.
        /// </summary>
        /// <param name="sourceFiles">The collection of source script files to process.</param>
        /// <param name="outputPath">The full path of the bundle in the output directory.</param>
        /// <param name="additionalContent">Additional script content to include in the bundle.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceFiles"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="outputPath"/> is <see langword="null"/> or empty.</exception>
        public void BundleScriptFiles(IEnumerable<string> sourceFiles, string outputPath, string? additionalContent = null)
        {
            ArgumentNullException.ThrowIfNull(sourceFiles);
            ArgumentException.ThrowIfNullOrEmpty(outputPath);

            using var _ = reporter.BeginStep(outputPath);

            var code = GetMergedContent(sourceFiles, additionalContent);
            var minified = Uglify.Js(code);

            if (minified.HasErrors)
                reporter.LogWarning($"JavaScript minification encountered errors for '{outputPath}'", minified.Errors.Select(static e => e.Message));

            try
            {
                output.WriteFile(outputPath, minified.Code ?? code);
            }
            catch (Exception error)
            {
                reporter.LogError($"Failed to write Javascript bundle '{outputPath}'.", error);
            }
        }

        /// <summary>
        /// Processes stylesheet files by concatenating and minifying them.
        /// </summary>
        /// <param name="sourceFiles">The collection of source stylesheet files to process.</param>
        /// <param name="outputPath">The full path of the bundle in the output directory.</param>
        /// <param name="additionalContent">Additional stylesheet content to include in the bundle.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceFiles"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="outputPath"/> is <see langword="null"/> or empty.</exception>
        public void BundleStyleFiles(IEnumerable<string> sourceFiles, string outputPath, string? additionalContent = null)
        {
            ArgumentNullException.ThrowIfNull(sourceFiles);
            ArgumentException.ThrowIfNullOrEmpty(outputPath);

            using var _ = reporter.BeginStep(outputPath);

            var code = GetMergedContent(sourceFiles, additionalContent);
            var minified = Uglify.Css(code);

            if (minified.HasErrors)
                reporter.LogWarning($"CSS minification encountered errors for '{outputPath}'", minified.Errors.Select(static e => e.Message));

            try
            {
                output.WriteFile(outputPath, minified.Code ?? code);
            }
            catch (Exception error)
            {
                reporter.LogError($"Failed to write CSS bundle '{outputPath}'.", error);
            }
        }

        /// <summary>
        /// Concatenates the content of multiple files into a single string.
        /// </summary>
        /// <param name="filePaths">The collection of file paths to concatenate.</param>
        /// <param name="additionalContent">Additional content to be included in the bundle.</param>
        /// <returns>The concatenated content of all files.</returns>
        private static string GetMergedContent(IEnumerable<string> filePaths, string? additionalContent = null)
        {
            using var reusable = StringBuilderPool.Shared.GetBuilder();
            var content = reusable.Builder;

            if (!string.IsNullOrEmpty(additionalContent))
                content.AppendLine(additionalContent);

            foreach (var filePath in filePaths)
                content.AppendLine(File.ReadAllText(filePath));

            return content.ToString();
        }
    }
}
