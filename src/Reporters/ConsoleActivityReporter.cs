// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Reporters
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Reports activity progress, warnings, and errors during documentation generation and output the results to the console.
    /// </summary>
    public sealed class ConsoleActivityReporter : IActivityReporter
    {
        private readonly StringBuilder buffer = new();
        private readonly List<ActivityLogEntry> logs = [];
        private readonly char primaryBullet;
        private readonly char secondaryBullet;
        private readonly char filledProgressCell;
        private readonly char emptyProgressCell;

        private string currentActivity = string.Empty;
        private string currentStep = string.Empty;
        private int currentPercentage = 0;
        private int totalSteps = 0;
        private int completedSteps = 0;
        private int activityRow = -1;
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleActivityReporter"/> class.
        /// </summary>
        public ConsoleActivityReporter()
        {
            Console.CursorVisible = false;

            try
            {
                primaryBullet = '•';
                secondaryBullet = '‣';
                filledProgressCell = '█';
                emptyProgressCell = '░';
                Console.OutputEncoding.GetBytes([
                    primaryBullet,
                    secondaryBullet,
                    filledProgressCell,
                    emptyProgressCell
                ]);
            }
            catch
            {
                primaryBullet = '*';
                secondaryBullet = '-';
                filledProgressCell = '#';
                emptyProgressCell = '.';
            }
        }

        /// <summary>
        /// Gets the number of warnings reported during the operation.
        /// </summary>
        /// <value>
        /// The number of warnings reported.
        /// </value>
        public int WarningCount => logs.Count(static log => log.Type == ReportType.Warning);

        /// <summary>
        /// Gets the number of errors reported during the operation.
        /// </summary>
        /// <value>
        /// The number of errors reported.
        /// </value>
        public int ErrorCount => logs.Count(static log => log.Type == ReportType.Error);

        /// <summary>
        /// Begins a new activity with optional progress tracking.
        /// </summary>
        /// <param name="activity">The activity description.</param>
        /// <param name="progressSteps">The total number of steps for progress tracking. If negative or zero, no progress tracking is performed.</param>
        /// <returns>A disposable object that removes the progress tracking when disposed if <paramref name="progressSteps"/> is greater than zero; otherwise, <see langword="null"/>.</returns>
        public IDisposable? BeginActivity(string activity, int progressSteps = 0)
        {
            ThrowIfDisposed();

            if (activity != currentActivity)
            {
                currentActivity = activity;
                DrawActivity();
            }

            if (progressSteps > 0)
            {
                totalSteps = progressSteps;
                completedSteps = 0;
                DrawProgress(currentPercentage = 0);
                return new ProgressScope(this);
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

            currentStep = step;
            return new StepScope(this);
        }

        /// <summary>
        /// Reports a message with the specified type.
        /// </summary>
        /// <param name="reportType">The type of report.</param>
        /// <param name="message">The message to report.</param>
        /// <param name="details">An optional collection of additional details associated with the message.</param>
        public void Report(ReportType reportType, string message, IEnumerable<string>? details = null)
        {
            ThrowIfDisposed();

            logs.Add(new ActivityLogEntry(reportType, message, details));
        }

        /// <summary>
        /// Releases all resources used by the <see cref="IActivityReporter"/>.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;

            ClearLines();
            DrawLogMessages();

            Console.CursorVisible = true;
            disposed = true;
        }

        /// <summary>
        /// Ends the current step, incrementing the progress if applicable.
        /// </summary>
        private void EndStep()
        {
            if (disposed || completedSteps >= totalSteps)
                return;

            completedSteps++;
            var percentage = 100 * completedSteps / totalSteps;
            if (currentPercentage != percentage)
                DrawProgress(currentPercentage = percentage);
        }

        private void EndProgress()
        {
            if (disposed || totalSteps <= 0)
                return;

            totalSteps = completedSteps = currentPercentage = 0;

            Console.SetCursorPosition(0, activityRow + 1);
            DrawLine(string.Empty);
            Console.SetCursorPosition(0, activityRow);
        }

        /// <summary>
        /// Retrieves the current console width.
        /// </summary>
        /// <returns>The width of the console window, or a default value if it cannot be determined.</returns>
        private static int GetConsoleWidth()
        {
            const int defaultWidth = 80;
            try
            {
                var width = Console.WindowWidth;
                return width > 0 ? width : defaultWidth;
            }
            catch (IOException)
            {
                return defaultWidth;
            }
        }

        /// <summary>
        /// Draws a line to the console, ensuring it fits within the console width.
        /// </summary>
        /// <param name="line">The line to draw.</param>
        private void DrawLine(string line)
        {
            var consoleWidth = GetConsoleWidth();

            buffer.Clear();

            if (line.Length <= consoleWidth)
            {
                buffer.Append(line);
                var padding = consoleWidth - line.Length;
                if (padding > 0)
                    buffer.Append(' ', padding);
            }
            else
            {
                var startIndex = line.Length - consoleWidth + 3;
                if (startIndex > 0)
                    buffer.Append("...").Append(line[startIndex..]);
            }

            Console.Write(buffer);
        }

        /// <summary>
        /// Draws the current activity on the console.
        /// </summary>
        private void DrawActivity()
        {
            if (activityRow == -1)
            {
                activityRow = Console.CursorTop;
                EnsureConsoleSpace(1); // Ensure space for progress line
            }

            Console.SetCursorPosition(0, activityRow);
            Console.ForegroundColor = ConsoleColor.White;
            DrawLine(currentActivity + "...");
            Console.ResetColor();
        }

        /// <summary>
        /// Draws the progress line on the console.
        /// </summary>
        /// <param name="percentage">The percentage of progress completed, ranging from 0 to 100.</param>
        private void DrawProgress(int percentage)
        {
            if (activityRow < 0)
                return;

            var consoleWidth = GetConsoleWidth();
            var progressWidth = Math.Min(40, consoleWidth - 30);
            if (progressWidth <= 0)
            {
                DrawLine(string.Empty);
                return;
            }

            var filledProgressWidth = Math.Max(0, Math.Min(progressWidth, percentage * progressWidth / 100));

            Console.SetCursorPosition(0, activityRow + 1);

            Console.ForegroundColor = ConsoleColor.Green;
            buffer.Clear().Append(filledProgressCell, filledProgressWidth);
            Console.Write(buffer);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            buffer.Clear().Append(emptyProgressCell, progressWidth - filledProgressWidth);
            Console.Write(buffer);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($" {percentage}%");

            Console.ResetColor();

            var remainingWidth = consoleWidth - progressWidth - $" {percentage}%".Length;
            if (remainingWidth > 0)
            {
                buffer.Clear().Append(' ', remainingWidth);
                Console.Write(buffer);
            }
        }

        /// <summary>
        /// Draws all logged messages on the console, grouped by their type.
        /// </summary>
        private void DrawLogMessages()
        {
            const string NoIndent = "";
            const string Indent = "  ";

            if (logs.Count == 0)
                return;

            var consoleWidth = GetConsoleWidth();
            var primaryIndent = primaryBullet + Indent[1..];
            var secondaryIndent = secondaryBullet + Indent[1..];

            foreach (var group in logs.GroupBy(static log => log.Type).OrderBy(static group => group.Key))
            {
                var useIndentation = true;
                var messageColor = ConsoleColor.White;

                switch (group.Key)
                {
                    case ReportType.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("WARNINGS:");
                        Console.WriteLine();
                        Console.ResetColor();
                        break;
                    case ReportType.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERRORS:");
                        Console.WriteLine();
                        Console.ResetColor();
                        break;
                    case ReportType.Information:
                        messageColor = Console.ForegroundColor;
                        useIndentation = false;
                        break;
                    default:
                        continue; // Skip other report types
                }

                foreach (var message in group)
                {
                    Console.ForegroundColor = messageColor;

                    var outerIndent = useIndentation ? primaryIndent : NoIndent;
                    var lineWidth = consoleWidth - outerIndent.Length;
                    foreach (var line in TextUtility.SplitLines(message.Text, lineWidth, true))
                    {
                        Console.Write(outerIndent);
                        Console.WriteLine(line);
                        outerIndent = useIndentation ? Indent : NoIndent;
                    }

                    Console.ResetColor();

                    if (message.Details.Count == 0)
                        continue;

                    Console.WriteLine();
                    foreach (var detail in message.Details)
                    {
                        var innerIndent = $"{secondaryBullet} ";
                        lineWidth -= innerIndent.Length;
                        foreach (var line in TextUtility.SplitLines(detail, lineWidth, true))
                        {
                            Console.Write(outerIndent);
                            Console.Write(innerIndent);
                            Console.WriteLine(line);
                            innerIndent = Indent;
                        }
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }

        /// <summary>
        /// Clears the currently displayed activity and progress lines from the console.
        /// </summary>
        private void ClearLines()
        {
            if (activityRow < 0)
                return;

            var consoleWidth = GetConsoleWidth();

            buffer.Clear().Append(' ', consoleWidth);

            Console.SetCursorPosition(0, activityRow);
            Console.Write(buffer);

            if (totalSteps > 0)
            {
                totalSteps = completedSteps = currentPercentage = 0;
                Console.Write(buffer);
            }

            Console.SetCursorPosition(0, activityRow);
            activityRow = -1;
        }

        /// <summary>
        /// Ensures there is enough space in the console buffer for the specified number of lines below the current activity row.
        /// </summary>
        /// <param name="requiredLines">The number of lines needed below the activity row.</param>
        private void EnsureConsoleSpace(int requiredLines)
        {
            var bufferHeight = Console.BufferHeight;
            var neededRows = activityRow + requiredLines + 1;

            if (neededRows > bufferHeight)
            {
                var linesToScroll = neededRows - bufferHeight;
                Console.SetCursorPosition(0, bufferHeight - 1);
                for (var i = 0; i < linesToScroll; i++)
                    Console.WriteLine();

                activityRow -= linesToScroll;
            }
        }

        /// <summary>
        /// Throws an exception if the reporter has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the reporter has been disposed.</exception>
        private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, nameof(IActivityReporter));

        /// <inheritdoc/>
        /// <remarks>
        /// This implementation does not support verbose reporting; setting this property has no effect.
        /// </remarks>
        bool IActivityReporter.Verbose
        {
            get => false;
            set { /* No-op */ }
        }

        /// <summary>
        /// Represents a scope for a step in the activity reporter.
        /// When disposed, it increments the progress if there is an active progress tracking.
        /// </summary>
        private sealed class StepScope : IDisposable
        {
            private readonly ConsoleActivityReporter reporter;
            private bool disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="StepScope"/> class.
            /// </summary>
            /// <param name="reporter">The activity reporter to associate with this step scope.</param>
            public StepScope(ConsoleActivityReporter reporter)
            {
                this.reporter = reporter;
            }

            /// <summary>
            /// Disposes the step scope, ending the current step.
            /// </summary>
            public void Dispose()
            {
                if (!disposed)
                {
                    reporter.EndStep();
                    disposed = true;
                }
            }
        }

        /// <summary>
        /// Represents a scope for a progress tracking in the activity reporter.
        /// When disposed, it clears the progress tracking and updates the display.
        /// </summary>
        private sealed class ProgressScope : IDisposable
        {
            private readonly ConsoleActivityReporter reporter;
            private bool disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProgressScope"/> class.
            /// </summary>
            /// <param name="reporter">The activity reporter to associate with this progress scope.</param>
            public ProgressScope(ConsoleActivityReporter reporter)
            {
                this.reporter = reporter;
            }

            /// <summary>
            /// Disposes the progress scope, ending the current progress tracking.
            /// </summary>
            public void Dispose()
            {
                if (!disposed)
                {
                    reporter.EndProgress();
                    disposed = true;
                }
            }
        }

        /// <summary>
        /// Represents a reported message in the console activity reporter.
        /// </summary>
        private sealed class ActivityLogEntry
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ActivityLogEntry"/> class.
            /// </summary>
            /// <param name="type">The type of report associated with the message.</param>
            /// <param name="text">The main text content of the message.</param>
            /// <param name="details">An optional collection of additional details related to the message.</param>
            public ActivityLogEntry(ReportType type, string text, IEnumerable<string>? details = null)
            {
                Type = type;
                Text = text;
                Details = details != null ? [.. details] : [];
            }

            /// <summary>
            /// Gets the type of report associated with the message.
            /// </summary>
            /// <value>
            /// The report type associated with the message.
            /// </value>
            public ReportType Type { get; }

            /// <summary>
            /// Gets the text of the message.
            /// </summary>
            /// <value>
            /// The text of the message.
            /// </value>
            public string Text { get; }

            /// <summary>
            /// Gets the details of the message.
            /// </summary>
            /// <value>
            /// The details of the message as a list of strings.
            /// </value>
            public List<string> Details { get; }
        }
    }
}
