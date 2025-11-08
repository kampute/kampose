// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Test.Reporters
{
    using Kampose.Reporters;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class TextWriterActivityReporterTests
    {
        [Test]
        public void Constructor_WithNullWriter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TextWriterActivityReporter(null!));
        }

        [Test]
        public void Constructor_WithNullErrorWriter_ThrowsArgumentNullException()
        {
            using var writer = new StringWriter();
            Assert.Throws<ArgumentNullException>(() => new TextWriterActivityReporter(writer, null!));
        }

        [Test]
        public void BeginActivity_WritesActivityAndFlushes()
        {
            using var writer = new StringWriter();
            using var reporter = new TextWriterActivityReporter(writer);

            reporter.BeginActivity("Test Activity");

            var output = writer.ToString();
            Assert.That(output, Does.Contain("Test Activity..."));
        }

        [Test]
        public void BeginStep_WithVerboseTrue_WritesStepAndFlushes()
        {
            using var writer = new StringWriter();
            using var reporter = new TextWriterActivityReporter(writer, verbose: true);

            reporter.BeginActivity("Test Activity");
            reporter.BeginStep("Test Step");

            var output = writer.ToString();
            Assert.That(output, Does.Contain("  Test Step"));
        }

        [Test]
        public void BeginStep_WithVerboseFalse_DoesNotWriteStep()
        {
            using var writer = new StringWriter();
            using var reporter = new TextWriterActivityReporter(writer, verbose: false);

            reporter.BeginActivity("Test Activity");
            reporter.BeginStep("Test Step");

            var output = writer.ToString();
            Assert.That(output, Does.Not.Contain("Test Step"));
        }

        [Test]
        public void Report_WithWarning_IncrementsWarningCount()
        {
            using var writer = new StringWriter();
            using var reporter = new TextWriterActivityReporter(writer);

            reporter.Report(ReportType.Warning, "Test warning");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(reporter.WarningCount, Is.EqualTo(1));
                Assert.That(reporter.ErrorCount, Is.Zero);
            }
        }

        [Test]
        public void Report_WithError_IncrementsErrorCount()
        {
            using var writer = new StringWriter();
            using var reporter = new TextWriterActivityReporter(writer);

            reporter.Report(ReportType.Error, "Test error");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(reporter.ErrorCount, Is.EqualTo(1));
                Assert.That(reporter.WarningCount, Is.Zero);
            }
        }

        [Test]
        public void Report_WithWarning_WritesToErrorWriter()
        {
            using var stdoutWriter = new StringWriter();
            using var stderrWriter = new StringWriter();
            using var reporter = new TextWriterActivityReporter(stdoutWriter, stderrWriter);

            reporter.Report(ReportType.Warning, "Test warning");

            var stdoutOutput = stdoutWriter.ToString();
            var stderrOutput = stderrWriter.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(stdoutOutput, Is.Empty);
                Assert.That(stderrOutput, Does.Contain("Warning: Test warning"));
            }
        }

        [Test]
        public void Report_WithError_WritesToErrorWriter()
        {
            using var stdoutWriter = new StringWriter();
            using var stderrWriter = new StringWriter();
            using var reporter = new TextWriterActivityReporter(stdoutWriter, stderrWriter);

            reporter.Report(ReportType.Error, "Test error");

            var stdoutOutput = stdoutWriter.ToString();
            var stderrOutput = stderrWriter.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(stdoutOutput, Is.Empty);
                Assert.That(stderrOutput, Does.Contain("Error: Test error"));
            }
        }

        [Test]
        public void Report_WithInformation_WritesToStandardWriter()
        {
            using var stdoutWriter = new StringWriter();
            using var stderrWriter = new StringWriter();
            using var reporter = new TextWriterActivityReporter(stdoutWriter, stderrWriter);

            reporter.Report(ReportType.Information, "Test info");

            var stdoutOutput = stdoutWriter.ToString();
            var stderrOutput = stderrWriter.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(stdoutOutput, Does.Contain("Test info"));
                Assert.That(stderrOutput, Is.Empty);
            }
        }

        [Test]
        public void Report_WithDetails_WritesDetailsIndented()
        {
            using var writer = new StringWriter();
            using var reporter = new TextWriterActivityReporter(writer);

            var details = new[] { "Detail 1", "Detail 2" };
            reporter.Report(ReportType.Error, "Test error", details);

            var output = writer.ToString();
            Assert.That(output, Does.Contain("- Detail 1"));
            Assert.That(output, Does.Contain("- Detail 2"));
        }

        [Test]
        public void Report_WithEmptyDetails_SkipsEmptyLines()
        {
            using var writer = new StringWriter();
            using var reporter = new TextWriterActivityReporter(writer);

            var details = new[] { "Detail 1", "", "Detail 2" };
            reporter.Report(ReportType.Error, "Test error", details);

            var output = writer.ToString();

            Assert.That(output, Does.Contain("- Detail 1"));
            Assert.That(output, Does.Contain("- Detail 2"));
        }

        [Test]
        public void OutputIsImmediatelyFlushed_ForCICDScenarios()
        {
            using var writer = new FlushTrackingWriter();
            using var reporter = new TextWriterActivityReporter(writer, verbose: true);

            reporter.BeginActivity("Activity");
            Assert.That(writer.FlushCount, Is.GreaterThan(0), "Activity should flush output");

            var flushCountAfterActivity = writer.FlushCount;
            reporter.BeginStep("Step");
            Assert.That(writer.FlushCount, Is.GreaterThan(flushCountAfterActivity), "Step should flush output");

            var flushCountAfterStep = writer.FlushCount;
            reporter.Report(ReportType.Error, "Error");
            Assert.That(writer.FlushCount, Is.GreaterThan(flushCountAfterStep), "Report should flush output");
        }

        [Test]
        public void Dispose_WithDisposeWriterTrue_DisposesWriter()
        {
            var writer = new DisposableTrackingWriter();
            var reporter = new TextWriterActivityReporter(writer, verbose: false, disposeWriter: true);

            reporter.Dispose();

            Assert.That(writer.IsDisposed, Is.True);
        }

        [Test]
        public void Dispose_WithDisposeWriterFalse_DoesNotDisposeWriter()
        {
            var writer = new DisposableTrackingWriter();
            var reporter = new TextWriterActivityReporter(writer, verbose: false, disposeWriter: false);

            reporter.Dispose();

            Assert.That(writer.IsDisposed, Is.False);
        }

        [Test]
        public void Dispose_WithSeparateWriters_DisposesOnlyWhenRequested()
        {
            var stdoutWriter = new DisposableTrackingWriter();
            var stderrWriter = new DisposableTrackingWriter();
            var reporter = new TextWriterActivityReporter(stdoutWriter, stderrWriter, verbose: false, disposeWriter: true, disposeErrorWriter: true);

            reporter.Dispose();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(stdoutWriter.IsDisposed, Is.True);
                Assert.That(stderrWriter.IsDisposed, Is.True);
            }
        }

        [Test]
        public void Dispose_WithSameWriterForBoth_DisposesOnlyOnce()
        {
            var writer = new DisposableTrackingWriter();
            var reporter = new TextWriterActivityReporter(writer, writer, verbose: false, disposeWriter: true, disposeErrorWriter: true);

            reporter.Dispose();

            Assert.That(writer.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void MultipleCalls_ToSameActivity_WritesOnlyOnce()
        {
            using var writer = new StringWriter();
            using var reporter = new TextWriterActivityReporter(writer);

            reporter.BeginActivity("Activity");
            var outputAfterFirst = writer.ToString();

            reporter.BeginActivity("Activity");
            var outputAfterSecond = writer.ToString();

            Assert.That(outputAfterFirst, Is.EqualTo(outputAfterSecond));
        }

        [Test]
        public void MultipleCalls_ToSameStep_WritesOnlyOnce()
        {
            using var writer = new StringWriter();
            using var reporter = new TextWriterActivityReporter(writer, verbose: true);

            reporter.BeginStep("Step");
            var outputAfterFirst = writer.ToString();

            reporter.BeginStep("Step");
            var outputAfterSecond = writer.ToString();

            Assert.That(outputAfterFirst, Is.EqualTo(outputAfterSecond));
        }

        /// <summary>
        /// Test helper to track flush calls.
        /// </summary>
        private class FlushTrackingWriter : StringWriter
        {
            public int FlushCount { get; private set; }

            public override void Flush()
            {
                FlushCount++;
                base.Flush();
            }
        }

        /// <summary>
        /// Test helper to track dispose calls.
        /// </summary>
        private class DisposableTrackingWriter : StringWriter
        {
            public bool IsDisposed { get; private set; }
            public int DisposeCount { get; private set; }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    IsDisposed = true;
                    DisposeCount++;
                }
                base.Dispose(disposing);
            }
        }
    }
}
