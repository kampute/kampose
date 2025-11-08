// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Templates.Helpers
{
    using HandlebarsDotNet;
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides Handlebars helper methods for working with code elements and references.
    /// </summary>
    public static class MemberHelpers
    {
        /// <summary>
        /// Registers the code member helper methods with the specified Handlebars environment.
        /// </summary>
        /// <param name="handlebars">The Handlebars environment to register the helpers with.</param>
        /// <param name="documentationContext">The documentation context used for resolving URLs and encoding.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlebars"/> or <paramref name="documentationContext"/> is <see langword="null"/>.</exception>
        public static void Register(IHandlebars handlebars, IDocumentationContext documentationContext)
        {
            ArgumentNullException.ThrowIfNull(handlebars);
            ArgumentNullException.ThrowIfNull(documentationContext);

            handlebars.RegisterHelper(nameof(MemberDefinition), (output, context, arguments) => MemberDefinition(in output, arguments, documentationContext));
            handlebars.RegisterHelper(nameof(MemberSignature), (output, context, arguments) => MemberSignature(in output, arguments, documentationContext));
            handlebars.RegisterHelper(nameof(MemberName), (context, arguments) => MemberName(arguments, documentationContext));
            handlebars.RegisterHelper(nameof(MemberUrl), (context, arguments) => MemberUrl(arguments, documentationContext));
            handlebars.RegisterHelper(nameof(MemberCategory), MemberCategory);
        }

        /// <summary>
        /// Renders definition of a code element formatted according to the syntax rules of the language specified by the documentation context.
        /// </summary>
        /// <param name="output">The output writer.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <param name="docContext">The documentation context used for resolving URLs and encoding.</param>
        /// <exception cref="HandlebarsException">Thrown when the type or number of arguments is not valid.</exception>
        private static void MemberDefinition(in EncodedTextWriter output, Arguments arguments, IDocumentationContext docContext)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(MemberDefinition)} template helper function requires one argument.");

            var member = ToMember(arguments[0]);

            using var writer = output.CreateWrapper();
            docContext.Language.WriteDefinition(writer, member);
        }

        /// <summary>
        /// Renders signature of a code element formatted according to the syntax rules of the language specified by the documentation context.
        /// </summary>
        /// <param name="output">The output writer.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <param name="docContext">The documentation context used for resolving URLs and encoding.</param>
        /// <exception cref="HandlebarsException">Thrown when the type or number of arguments is not valid.</exception>
        private static void MemberSignature(in EncodedTextWriter output, Arguments arguments, IDocumentationContext docContext)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(MemberSignature)} template helper function requires one argument.");

            var member = ToMember(arguments[0]);

            using var writer = output.CreateWrapper();
            docContext.Language.WriteSignature(writer, member, docContext.DetermineNameQualifier(member));
        }

        /// <summary>
        /// Returns qualified name of a code element formatted according to the syntax rules of the language specified by the documentation context.
        /// </summary>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <param name="docContext">The documentation context used for resolving URLs and encoding.</param>
        /// <returns>The qualified name of the code element.</returns>
        /// <exception cref="HandlebarsException">Thrown when the type or number of arguments is not valid.</exception>
        private static string MemberName(Arguments arguments, IDocumentationContext docContext)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(MemberName)} template helper function requires one argument.");

            var member = ToMember(arguments[0]);
            return docContext.Language.FormatName(member, NameQualifier.DeclaringType);
        }

        /// <summary>
        /// Returns the documentation URL of a code element.
        /// </summary>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <param name="docContext">The documentation context used for resolving URLs and encoding.</param>
        /// <returns>The documentation URL of the code element.</returns>
        /// <exception cref="HandlebarsException">Thrown when the type or number of arguments is not valid.</exception>
        private static Uri? MemberUrl(Arguments arguments, IDocumentationContext docContext)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(MemberUrl)} template helper function requires one argument.");

            var member = ToMember(arguments[0]);

            return member is NamespaceAsMember
                ? (docContext.AddressProvider.TryGetNamespaceUrl(member.Name, out var nsUrl) ? nsUrl : null)
                : (docContext.AddressProvider.TryGetMemberUrl(member, out var url) ? url : null);
        }

        /// <summary>
        /// Returns the category of a code element.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="arguments">The arguments passed to the helper.</param>
        /// <returns>The category of the code element as a string.</returns>
        /// <exception cref="HandlebarsException">Thrown when the number of arguments is not valid.</exception>
        private static object MemberCategory(Context context, Arguments arguments)
        {
            if (arguments.Length != 1)
                throw new HandlebarsException($"{nameof(MemberCategory)} template helper function requires one argument.");

            return ToMember(arguments[0]) switch
            {
                NamespaceAsMember => "Namespace",
                IClassType _ => "Class",
                IStructType _ => "Struct",
                IInterfaceType _ => "Interface",
                IEnumType _ => "Enum",
                IDelegateType _ => "Delegate",
                IConstructor _ => "Constructor",
                IMethod _ => "Method",
                IProperty _ => "Property",
                IEvent _ => "Event",
                IField _ => "Field",
                IOperator _ => "Operator",
                IType _ => "Type",
                _ => "Member"
            };
        }

        /// <summary>
        /// Converts an object to an <see cref="IMember"/> if possible.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <param name="callerName">The name of the calling method (automatically provided).</param>
        /// <returns>An instance of <see cref="IMember"/>.</returns>
        /// <exception cref="HandlebarsException">Thrown when the object cannot be converted.</exception>
        private static IMember ToMember(object obj, [CallerMemberName] string? callerName = null) => obj switch
        {
            IMember member => member,
            MemberModel memberModel => memberModel.Metadata,
            NamespaceModel namespaceModel => new NamespaceAsMember(namespaceModel.Name),
            System.Reflection.MemberInfo memberInfo => memberInfo.GetMetadata(),
            string cref when CodeReference.IsMember(cref) => CodeReference.ResolveMember(cref) ?? throw new HandlebarsException($"The code reference '{cref}' could not be resolved to a member."),
            string cref when CodeReference.IsNamespace(cref) => new NamespaceAsMember(cref[2..]),
            _ => throw new HandlebarsException($"The argument of the {callerName} template helper function must be a code element or a valid code reference string.")
        };

        /// <summary>
        /// Wraps a namespace code reference as an <see cref="IMember"/> implementation for use in template helpers.
        /// </summary>
        /// <param name="Name">The name of the namespace.</param>
        private sealed record NamespaceAsMember(string Name) : IMember
        {
            public IAssembly Assembly => throw new NotSupportedException();
            public string Namespace => throw new NotSupportedException();
            public string CodeReference => $"N:{Name}";
            public IType? DeclaringType => null;
            public IType? ReflectedType => null;
            public MemberVisibility Visibility => MemberVisibility.Public;
            public bool IsVisible => true;
            public bool IsPublic => true;
            public bool IsStatic => throw new NotSupportedException();
            public bool IsUnsafe => throw new NotSupportedException();
            public bool IsSpecialName => throw new NotSupportedException();
            public bool IsDirectDeclaration => false;
            public IReadOnlyList<ICustomAttribute> CustomAttributes => [];
            public bool HasCustomAttribute(string attributeFullName) => false;
            public bool Represents(System.Reflection.MemberInfo reflection) => false;
        }
    }
}