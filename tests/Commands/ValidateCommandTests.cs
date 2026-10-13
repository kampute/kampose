// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Commands
{
    using Kampose.Commands;
    using Kampose.Reporters;
    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class ValidateCommandTests
    {
        [Test]
        public void Parse_ValidateCommandName_ReturnsValidateCommand()
        {
            var (command, args) = CommandParser.Parse(["validate", "custom-config"]);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(command, Is.TypeOf<ValidateCommand>());
                Assert.That(args, Is.EqualTo(new[] { "custom-config" }));
            }
        }

        [TestCase("--clean")]
        [TestCase("-c")]
        public void Execute_CleanOption_ThrowsCommandException(string option)
        {
            using var reporter = new TextWriterActivityReporter(TextWriter.Null);

            Assert.That(() => new ValidateCommand().Execute([option], reporter), Throws.TypeOf<CommandException>());
        }
    }
}
