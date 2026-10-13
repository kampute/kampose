// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Transformers
{
    using System;

    /// <summary>
    /// Transforms Markdown content for Markdown output published on a GitLab Wiki.
    /// </summary>
    /// <remarks>
    /// GitLab renders the GitHub alert syntax, so alerts are kept as alerts instead of being converted to titled blockquotes.
    /// </remarks>
    /// <seealso href="https://docs.gitlab.com/user/markdown/#alerts">GitLab Flavored Markdown alerts</seealso>
    public sealed class GitLabMarkdownTransformer : MarkdownToMarkdownTransformer
    {
        /// <inheritdoc/>
        /// <remarks>
        /// The directive is kept, with the kind of the alert in lowercase as GitLab documents it, such as <c>&gt; [!note]</c>.
        /// </remarks>
        protected override string RewriteAlertDirective(string directiveLine, string kind)
        {
            // The directive is the first content of the blockquote, so the first "[!" on its line opens it.
            var kindStart = directiveLine.IndexOf("[!", StringComparison.Ordinal) + 2;
            return string.Concat(directiveLine.AsSpan(0, kindStart), kind.ToLowerInvariant(), directiveLine.AsSpan(kindStart + kind.Length));
        }
    }
}
