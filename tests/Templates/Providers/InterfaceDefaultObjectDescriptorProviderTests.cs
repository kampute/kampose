// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Templates.Providers
{
    using HandlebarsDotNet;
    using Kampose.Templates.Providers;
    using NUnit.Framework;

    [TestFixture]
    public class InterfaceDefaultObjectDescriptorProviderTests
    {
        private IHandlebars handlebars = null!;

        [SetUp]
        public void Setup()
        {
            var provider = new InterfaceDefaultObjectDescriptorProvider();
            var cfg = new HandlebarsConfiguration();
            cfg.ObjectDescriptorProviders.Add(provider);
            handlebars = Handlebars.Create(cfg);
        }

        [Test]
        public void RenderTemplate_WithDerivedClassInstance_AccessesAllPropertiesIncludingInterfaceDefaults()
        {
            var template = handlebars.Compile("{{defaultProperty}}, {{baseProperty}}, {{derivedProperty}}");

            var result = template(new DerivedClass());

            Assert.That(result, Is.EqualTo("Default, Base, Derived"));
        }

        [Test]
        public void RenderTemplate_WhenIteratingHeterogeneousCollection_AccessesCorrectPropertiesForEachElement()
        {
            var template = handlebars.Compile("{{#each items}}[{{defaultProperty}}, {{baseProperty}}, {{derivedProperty}}]{{/each}}");

            var result = template(new
            {
                Items = new ITestInterface[]
                {
                    new DerivedClass(1),
                    new BaseClass(),
                    new DerivedClass(2)
                }
            });

            Assert.That(result, Is.EqualTo("[Default, Base1, Derived1][Default, Base, ][Default, Base2, Derived2]"));
        }

        private interface ITestInterface
        {
            string DefaultProperty => "Default";
        }

        private class BaseClass : ITestInterface
        {
            public BaseClass() => BaseProperty = "Base";
            public BaseClass(int id) => BaseProperty = $"Base{id}";

            public string BaseProperty { get; set; }
        }

        private class DerivedClass : BaseClass
        {
            public DerivedClass() : base() => DerivedProperty = "Derived";
            public DerivedClass(int id) : base(id) => DerivedProperty = $"Derived{id}";

            public string DerivedProperty { get; set; }
        }
    }
}