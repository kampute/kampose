// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Reporters
{
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.Xml.Linq;

    /// <summary>
    /// Reports XML documentation errors to an activity reporter.
    /// </summary>
    public sealed class XmlDocErrorReporter : IXmlDocErrorHandler
    {
        private readonly IActivityReporter reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocErrorReporter"/> class.
        /// </summary>
        /// <param name="reporter">The activity reporter to use for reporting the errors.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/> is <see langword="null"/>.</exception>
        public XmlDocErrorReporter(IActivityReporter reporter)
        {
            this.reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        /// <summary>
        /// Reports that an included file could not be found.
        /// </summary>
        /// <param name="memberElement">The <c>member</c> element containing the <c>include</c> element.</param>
        /// <param name="includeFilePath">The missing include file path.</param>
        public void IncludeFileNotFound(XElement memberElement, string includeFilePath)
            => reporter.LogWarning($"{GetCodeReference(memberElement)} - Included file '{includeFilePath}' could not be found.");

        /// <summary>
        /// Reports that an included member could not be found.
        /// </summary>
        /// <param name="memberElement">The <c>member</c> element containing the <c>include</c> element.</param>
        public void IncludeMemberNotFound(XElement memberElement)
            => reporter.LogWarning($"{GetCodeReference(memberElement)} - Included member could not be found.");

        /// <summary>
        /// Reports that an inherited member could not be resolved.
        /// </summary>
        /// <param name="memberElement">The <c>member</c> element containing the <c>inheritdoc</c> element.</param>
        public void InheritDocNotFound(XElement memberElement)
            => reporter.LogWarning($"{GetCodeReference(memberElement)} - Inherited member could not be resolved.");

        /// <summary>
        /// Retrieves the code reference of the member from the specified <c>member</c> element.
        /// </summary>
        /// <param name="memberElement">The <c>member</c> element.</param>
        /// <returns>The code reference of the member.</returns>
        private static string GetCodeReference(XElement memberElement) => memberElement.Attribute("name")!.Value;
    }
}
