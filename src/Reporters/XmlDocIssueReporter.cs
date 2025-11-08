// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Reporters
{
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.XmlDoc;
    using System;

    /// <summary>
    /// Reports documentation issues to an activity reporter.
    /// </summary>
    public sealed class XmlDocIssueReporter
    {
        private readonly IActivityReporter reporter;
        private int issueCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocIssueReporter"/> class.
        /// </summary>
        /// <param name="reporter">The activity reporter to use for reporting the issues.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/> is <see langword="null"/>.</exception>
        public XmlDocIssueReporter(IActivityReporter reporter)
        {
            this.reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        /// <summary>
        /// Gets the number of reported issues.
        /// </summary>
        /// <value>
        /// The number of reported issues.
        /// </value>
        public int ReportedIssueCount => issueCount;

        public void Report(in XmlDocInspectionIssue issue)
        {
            issueCount++;
            switch (issue.IssueType)
            {
                case XmlDocInspectionIssueType.MissingRequiredTag:
                    switch (issue.XmlTag)
                    {
                        case XmlDocTag.TypeParam:
                            ReportUndocumentedTypeParameter(issue.TypeParameter!);
                            break;
                        case XmlDocTag.Param:
                            ReportUndocumentedParameter(issue.Parameter!);
                            break;
                        case XmlDocTag.Returns:
                            ReportUndocumentedReturn(issue.Parameter!);
                            break;
                        default:
                            ReportMissingRequiredTag(issue.Member, issue.XmlTag);
                            break;
                    }
                    break;
                case XmlDocInspectionIssueType.MissingOptionalTag:
                    ReportMissingOptionalTag(issue.Member, issue.XmlTag);
                    break;
                case XmlDocInspectionIssueType.UndocumentedReference:
                    ReportUndocumentedReference(issue.Member, issue.XmlTag, issue.CodeReference!);
                    break;
                case XmlDocInspectionIssueType.UntitledSeeAlso:
                    ReportUntitledSeeAlso(issue.Member, issue.Hyperlink!);
                    break;
            }
        }

        /// <summary>
        /// Reports that the specified member is missing a required XML documentation tag.
        /// </summary>
        /// <param name="member">The member that is missing a required XML documentation tag.</param>
        /// <param name="tag">The XML documentation tag that is missing.</param>
        private void ReportMissingRequiredTag(IMember member, XmlDocTag tag)
            => reporter.LogWarning($"{NameOf(member)} - Missing or empty <{ToTagName(tag)}> element.");

        /// <summary>
        /// Reports that the specified member is missing an optional XML documentation tag.
        /// </summary>
        /// <param name="member">The member that is missing an optional XML documentation tag.</param>
        /// <param name="tag">The XML documentation tag that is missing.</param>
        private void ReportMissingOptionalTag(IMember member, XmlDocTag tag)
            => reporter.LogWarning($"{NameOf(member)} - Missing or empty <{ToTagName(tag)}> element.");

        /// <summary>
        /// Reports that a generic type parameter is missing documentation.
        /// </summary>
        /// <param name="typeParameter">Information about the undocumented type parameter.</param>
        private void ReportUndocumentedTypeParameter(ITypeParameter typeParameter)
            => reporter.LogWarning($"{NameOf(typeParameter.DeclaringMember)} - Missing or empty <typeparam name=\"{typeParameter.Name}\"> element.");

        /// <summary>
        /// Reports that a method or constructor parameter is missing documentation.
        /// </summary>
        /// <param name="parameter">The undocumented parameter.</param>
        private void ReportUndocumentedParameter(IParameter parameter)
            => reporter.LogWarning($"{NameOf(parameter.Member)} - Missing or empty <param name=\"{parameter.Name}\"> element.");

        /// <summary>
        /// Reports that a method return value is missing documentation.
        /// </summary>
        /// <param name="returnParameter">The undocumented return parameter.</param>
        private void ReportUndocumentedReturn(IParameter returnParameter)
            => reporter.LogWarning($"{NameOf(returnParameter.Member)} - Missing or empty <returns> element.");

        /// <summary>
        /// Reports that a reference in an XML documentation tag is not properly described.
        /// </summary>
        /// <param name="member">The member with the undocumented reference.</param>
        /// <param name="tag">The XML documentation tag containing the reference.</param>
        /// <param name="cref">The code reference that is not described.</param>
        private void ReportUndocumentedReference(IMember member, XmlDocTag tag, string cref)
            => reporter.LogWarning($"{NameOf(member)} - Empty <{ToTagName(tag)} cref=\"{cref}\"> element.");

        /// <summary>
        /// Reports that a see-also reference to a URI has no description.
        /// </summary>
        /// <param name="member">The member that has the empty see-also element.</param>
        /// <param name="href">The URI of the see-also topic that is not described.</param>
        private void ReportUntitledSeeAlso(IMember member, string href)
            => reporter.LogWarning($"{NameOf(member)} - Untitled <seealso href=\"{href}\"> element.");

        /// <summary>
        /// Resets the number of reported issues.
        /// </summary>
        public void Reset() => issueCount = 0;

        /// <summary>
        /// Converts the specified XML documentation tag to its lowercase name.
        /// </summary>
        /// <param name="tag">The XML documentation tag to convert.</param>
        /// <returns>The lowercase name of the XML documentation tag.</returns>
        private static string ToTagName(XmlDocTag tag) => tag.ToString().ToLowerInvariant();

        /// <summary>
        /// Retrieves the full name of the member along with its category.
        /// </summary>
        /// <param name="member">The member whose name is to be retrieved.</param>
        /// <returns>A string representing the full name of the member and its category.</returns>
        private static string NameOf(IMember member)
        {
            using var writer = StringBuilderPool.Shared.GetWriter();
            Language.Default.WriteName(writer, member, NameQualifier.Full);
            switch (member)
            {
                case IClassType: writer.Write(" Class"); break;
                case IStructType: writer.Write(" Struct"); break;
                case IInterfaceType: writer.Write(" Interface"); break;
                case IEnumType: writer.Write(" Enum"); break;
                case IDelegateType: writer.Write(" Delegate"); break;
                case IConstructor: writer.Write(" Constructor"); break;
                case IMethod: writer.Write(" Method"); break;
                case IProperty: writer.Write(" Property"); break;
                case IEvent: writer.Write(" Event"); break;
                case IField: writer.Write(" Field"); break;
                case IOperator: writer.Write(" Operator"); break;
            }
            return writer.ToString();
        }
    }
}
