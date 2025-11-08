// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Reporters
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Reports activity progress, warnings, and errors during documentation generation and outputs the results to
    /// a <see cref="TextWriter"/>.
    /// </summary>
    public sealed class TextWriterActivityReporter : IActivityReporter
    {
        private readonly TextWriter writer;
        private readonly TextWriter errorWriter;
        private readonly bool disposeWriter;
        private readonly bool disposeErrorWriter;

        private string currentActivity = string.Empty;
        private string currentStep = string.Empty;
        private int warningCount = 0;
        private int errorCount = 0;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextWriterActivityReporter"/> class.
        /// </summary>
        /// <param name="writer">The TextWriter to output messages to.</param>
        /// <param name="disposeWriter">Whether to dispose the TextWriter when this reporter is disposed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is null.</exception>
        public TextWriterActivityReporter(TextWriter writer, bool disposeWriter = false)
            : this(writer, writer, disposeWriter, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextWriterActivityReporter"/> class with separate writers for standard output and error output.
        /// </summary>
        /// <param name="writer">The TextWriter to output informational messages to.</param>
        /// <param name="errorWriter">The TextWriter to output errors and warnings to.</param>
        /// <param name="disposeWriter">Whether to dispose the <paramref name="writer"/> when this reporter is disposed.</param>
        /// <param name="disposeErrorWriter">Whether to dispose the <paramref name="errorWriter"/> when this reporter is disposed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="errorWriter"/> is null.</exception>
        public TextWriterActivityReporter(TextWriter writer, TextWriter errorWriter, bool disposeWriter = false, bool disposeErrorWriter = false)
        {
            this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
            this.errorWriter = errorWriter ?? throw new ArgumentNullException(nameof(errorWriter));
            this.disposeWriter = disposeWriter;
            this.disposeErrorWriter = disposeErrorWriter;
        }

        /// <summary>
        /// Gets or sets a value indicating whether verbose reporting is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if verbose reporting is enabled; otherwise, <see langword="false"/>.
        /// </value>
        public bool Verbose { get; set; } = false;

        /// <summary>
        /// Gets the number of warnings reported during the operation.
        /// </summary>
        /// <value>
        /// The number of warnings reported.
        /// </value>
        public int WarningCount => warningCount;

        /// <summary>
        /// Gets the number of errors reported during the operation.
        /// </summary>
        /// <value>
        /// The number of errors reported.
        /// </value>
        public int ErrorCount => errorCount;

        /// <summary>
        /// Begins a new activity.
        /// </summary>
        /// <param name="activity">The activity description.</param>
        /// <param name="progressSteps">This parameter is ignored in this implementation, as it does not support progress tracking.</param>
        /// <returns>Always returns <see langword="null"/> since this reporter does not support progress tracking.</returns>
        public IDisposable? BeginActivity(string activity, int progressSteps = 0)
        {
            ThrowIfDisposed();

            if (currentActivity != activity)
            {
                currentActivity = activity;
                currentStep = string.Empty;
                writer.Write(activity);
                writer.WriteLine("...");
                writer.Flush();
            }
            return null;
        }

        /// <summary>
        /// Begins a step within the current activity.
        /// </summary>
        /// <param name="step">The step description.</param>
        /// <returns>A disposable object that ends the step when disposed.</returns>
        public IDisposable BeginStep(string step)
        {
            ThrowIfDisposed();

            if (currentStep != step)
            {
                currentStep = step;
                if (Verbose)
                {
                    writer.Write("  ");
                    writer.WriteLine(step);
                    writer.Flush();
                }
            }
            return NullDisposable.Instance;
        }

        /// <summary>
        /// Reports a message with the specified type.
        /// </summary>
        /// <param name="reportType">Type of the report.</param>
        /// <param name="message">The message to report.</param>
        /// <param name="details">An optional collection of additional details associated with the message.</param>
        public void Report(ReportType reportType, string message, IEnumerable<string>? details = null)
        {
            ThrowIfDisposed();

            var writeTypeIndicator = false;
            var targetWriter = writer;

            switch (reportType)
            {
                case ReportType.Warning:
                    warningCount++;
                    writeTypeIndicator = true;
                    targetWriter = errorWriter;
                    break;
                case ReportType.Error:
                    errorCount++;
                    writeTypeIndicator = true;
                    targetWriter = errorWriter;
                    break;
            }

            var indent = Verbose && writeTypeIndicator && !string.IsNullOrEmpty(currentStep) ? "  " : string.Empty;

            if (writeTypeIndicator)
            {
                targetWriter.Write(indent);
                targetWriter.Write(reportType);
                targetWriter.Write(": ");
            }
            targetWriter.WriteLine(message);

            if (details != null)
            {
                indent += "- ";
                foreach (var detail in details)
                {
                    if (!string.IsNullOrEmpty(detail))
                    {
                        targetWriter.Write(indent);
                        targetWriter.WriteLine(detail);
                    }
                }
            }

            targetWriter.Flush();
        }

        /// <summary>
        /// Releases all resources used by the <see cref="TextWriterActivityReporter"/>.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;

            if (disposeWriter)
                writer?.Dispose();

            if (disposeErrorWriter && errorWriter != writer)
                errorWriter?.Dispose();

            disposed = true;
        }

        /// <summary>
        /// Throws an exception if the reporter has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the reporter has been disposed.</exception>
        private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, nameof(TextWriterActivityReporter));

        /// <summary>
        /// Represents a disposable object that does nothing when disposed.
        /// </summary>
        private sealed class NullDisposable : IDisposable
        {
            /// <summary>
            /// Represents a singleton instance of the <see cref="NullDisposable"/> class.
            /// </summary>
            public static readonly NullDisposable Instance = new();

            /// <summary>
            /// Initializes a new instance of the <see cref="NullDisposable"/> class.
            /// </summary>
            private NullDisposable() { }

            /// <summary>
            /// Releases all resources used by the current instance of the class.
            /// </summary>
            public void Dispose() { }
        }
    }
}