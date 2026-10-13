// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Transformers
{
    /// <summary>
    /// Transforms Markdown content for Markdown output published on an Azure DevOps Wiki.
    /// </summary>
    /// <remarks>
    /// Azure DevOps Wiki does not render GitHub alerts, so each alert is converted to a blockquote whose first line is the
    /// bold title of the alert, as XML documentation notes are rendered.
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/azure/devops/project/wiki/markdown-guidance">Azure DevOps Wiki Markdown guidance</seealso>
    public sealed class DevOpsMarkdownTransformer : MarkdownToMarkdownTransformer
    {
        /// <inheritdoc/>
        /// <remarks>
        /// The directive is replaced with the bold title of the alert, such as <c>&gt; **Note** \</c>, whose trailing backslash
        /// breaks the line before the content of the alert.
        /// </remarks>
        protected override string RewriteAlertDirective(string directiveLine, string kind)
        {
            var title = char.ToUpperInvariant(kind[0]) + kind[1..].ToLowerInvariant();
            return $"> **{title}** \\";
        }
    }
}
