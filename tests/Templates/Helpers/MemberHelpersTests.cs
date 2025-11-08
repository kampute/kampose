// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Templates.Helpers
{
    using HandlebarsDotNet;
    using Kampose.Templates.Helpers;
    using Kampose.Test;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;

    [TestFixture]
    public class MemberHelpersTests
    {
        private IHandlebars handlebars;

        [SetUp]
        public void SetUp()
        {
            handlebars = Handlebars.Create();
            var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>();
            MemberHelpers.Register(handlebars, docContext);

            MetadataProvider.RegisterRuntimeAssemblies();
        }

        [TestCase("T:System.Collections.Generic.List`1.Enumerator", ExpectedResult = "public struct List&lt;T&gt;.Enumerator : IEnumerator&lt;T&gt;")]
        [TestCase("M:System.Uri.#ctor(System.String)", ExpectedResult = "public Uri([StringSyntax(&quot;Uri&quot;)] string uriString)")]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = "public static void WriteLine()")]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = "public static void WriteLine(string value)")]
        [TestCase("M:System.Linq.Enumerable.Repeat``1(``0,System.Int32)", ExpectedResult = "public static IEnumerable&lt;TResult&gt; Repeat&lt;TResult&gt;(TResult element, int count)")]
        [TestCase("M:System.DateTime.op_Addition(System.DateTime,System.TimeSpan)", ExpectedResult = "public static DateTime operator +(DateTime d, TimeSpan t)")]
        [TestCase("P:System.Collections.Generic.List`1.Count", ExpectedResult = "public int Count { get; }")]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = "public T this[int index] { get; set; }")]
        [TestCase("P:System.Collections.Generic.List`1.Enumerator.Current", ExpectedResult = "public T Current { get; }")]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = "public static readonly DateTime MaxValue")]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = "public event AssemblyLoadEventHandler AssemblyLoad")]
        public string MemberDefinition_RendersMemberDefinition(string cref)
        {
            var template = handlebars.Compile("{{#memberDefinition value}}");
            return template(new { value = cref }).Replace("\r", string.Empty);
        }

        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "List&lt;T&gt;")]
        [TestCase("T:System.Collections.Generic.List`1.Enumerator", ExpectedResult = "List&lt;T&gt;.Enumerator")]
        [TestCase("M:System.Uri.#ctor(System.String)", ExpectedResult = "Uri(string)")]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = "Console.WriteLine()")]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = "Console.WriteLine(string)")]
        [TestCase("M:System.Linq.Enumerable.Repeat``1(``0,System.Int32)", ExpectedResult = "Enumerable.Repeat&lt;TResult&gt;(TResult, int)")]
        [TestCase("M:System.DateTime.op_Addition(System.DateTime,System.TimeSpan)", ExpectedResult = "Addition(DateTime, TimeSpan)")]
        [TestCase("P:System.Collections.Generic.List`1.Count", ExpectedResult = "List&lt;T&gt;.Count")]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = "List&lt;T&gt;.Item[int]")]
        [TestCase("P:System.Collections.Generic.List`1.Enumerator.Current", ExpectedResult = "List&lt;T&gt;.Enumerator.Current")]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = "DateTime.MaxValue")]
        [TestCase("F:System.DayOfWeek.Monday", ExpectedResult = "DayOfWeek.Monday")]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = "AppDomain.AssemblyLoad")]
        public string MemberSignature_RendersMemberSignature(string cref)
        {
            var template = handlebars.Compile("{{#memberSignature value}}");
            return template(new { value = cref });
        }

        [TestCase("N:System.Collections.Generic", ExpectedResult = "System.Collections.Generic")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "List&lt;T&gt;")]
        [TestCase("T:System.Range", ExpectedResult = "Range")]
        [TestCase("T:System.ICloneable", ExpectedResult = "ICloneable")]
        [TestCase("T:System.Action", ExpectedResult = "Action")]
        [TestCase("T:System.DayOfWeek", ExpectedResult = "DayOfWeek")]
        [TestCase("M:System.Uri.#ctor(System.String)", ExpectedResult = "Uri")]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = "Console.WriteLine")]
        [TestCase("M:System.DateTime.op_Addition(System.DateTime,System.TimeSpan)", ExpectedResult = "DateTime.Addition")]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = "List&lt;T&gt;.Item[]")]
        [TestCase("P:System.Collections.Generic.List`1.Count", ExpectedResult = "List&lt;T&gt;.Count")]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = "DateTime.MaxValue")]
        [TestCase("F:System.DayOfWeek.Monday", ExpectedResult = "DayOfWeek.Monday")]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = "AppDomain.AssemblyLoad")]
        public string MemberName_RendersMemberName(string cref)
        {
            var template = handlebars.Compile("{{#memberName value}}");
            return template(new { value = cref });
        }

        [TestCase("N:System.Collections.Generic", ExpectedResult = "Namespace")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "Class")]
        [TestCase("T:System.Range", ExpectedResult = "Struct")]
        [TestCase("T:System.ICloneable", ExpectedResult = "Interface")]
        [TestCase("T:System.Action", ExpectedResult = "Delegate")]
        [TestCase("T:System.DayOfWeek", ExpectedResult = "Enum")]
        [TestCase("M:System.Uri.#ctor(System.String)", ExpectedResult = "Constructor")]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = "Method")]
        [TestCase("M:System.DateTime.op_Addition(System.DateTime,System.TimeSpan)", ExpectedResult = "Operator")]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = "Property")]
        [TestCase("P:System.Collections.Generic.List`1.Count", ExpectedResult = "Property")]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = "Field")]
        [TestCase("F:System.DayOfWeek.Monday", ExpectedResult = "Field")]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = "Event")]
        public string MemberCategory_ReturnsCategory(string cref)
        {
            var template = handlebars.Compile("{{#memberCategory value}}");
            return template(new { value = cref });
        }

        [TestCase("N:System.Collections.Generic", ExpectedResult = "https://example.com/system.collections.generic")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "https://example.com/system.collections.generic.list-1")]
        [TestCase("T:System.Collections.Generic.List`1.Enumerator", ExpectedResult = "https://example.com/system.collections.generic.list-1.enumerator")]
        [TestCase("M:System.Uri.#ctor(System.String)", ExpectedResult = "https://example.com/system.uri.-ctor(system.string)")]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = "https://example.com/system.console.writeline")]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = "https://example.com/system.console.writeline(system.string)")]
        [TestCase("M:System.Linq.Enumerable.Repeat``1(``0,System.Int32)", ExpectedResult = "https://example.com/system.linq.enumerable.repeat--1(--0,system.int32)")]
        [TestCase("M:System.DateTime.op_Addition(System.DateTime,System.TimeSpan)", ExpectedResult = "https://example.com/system.datetime.op_addition(system.datetime,system.timespan)")]
        [TestCase("P:System.Collections.Generic.List`1.Count", ExpectedResult = "https://example.com/system.collections.generic.list-1.count")]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = "https://example.com/system.collections.generic.list-1.item(system.int32)")]
        [TestCase("P:System.Collections.Generic.List`1.Enumerator.Current", ExpectedResult = "https://example.com/system.collections.generic.list-1.enumerator.current")]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = "https://example.com/system.datetime.maxvalue")]
        [TestCase("F:System.DayOfWeek.Monday", ExpectedResult = "https://example.com/system.dayofweek.monday")]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = "https://example.com/system.appdomain.assemblyload")]
        public string MemberUrl_ReturnsUrl(string cref)
        {
            var template = handlebars.Compile("{{#memberUrl value}}");
            return template(new { value = cref });
        }
    }
}
