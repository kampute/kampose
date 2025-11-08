// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Reporters
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Reports activity progress, warnings, and errors during documentation generation.
    /// </summary>
    public interface IActivityReporter : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether verbose reporting is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if verbose reporting is enabled; otherwise, <see langword="false"/>.
        /// </value>
        bool Verbose { get; set; }

        /// <summary>
        /// Gets the number of warnings reported during the operation.
        /// </summary>
        /// <value>
        /// The number of warnings reported.
        /// </value>
        int WarningCount { get; }

        /// <summary>
        /// Gets the number of errors reported during the operation.
        /// </summary>
        /// <value>
        /// The number of errors reported.
        /// </value>
        int ErrorCount { get; }

        /// <summary>
        /// Begins a new activity with optional progress tracking.
        /// </summary>
        /// <param name="activity">The activity description.</param>
        /// <param name="progressSteps">The total number of steps for progress tracking. If negative or zero, no progress tracking is performed.</param>
        /// <returns>A disposable object that removes the progress tracking when disposed if <paramref name="progressSteps"/> is greater than zero; otherwise, <see langword="null"/>.</returns>
        IDisposable? BeginActivity(string activity, int progressSteps = 0);

        /// <summary>
        /// Begins a step within the current activity.
        /// </summary>
        /// <param name="step">The step description.</param>
        /// <returns>A disposable object that ends the step when disposed.</returns>
        IDisposable BeginStep(string step);

        /// <summary>
        /// Reports a message with the specified type.
        /// </summary>
        /// <param name="reportType">Type of the report.</param>
        /// <param name="message">The message to report.</param>
        /// <param name="details">Optional additional details.</param>
        void Report(ReportType reportType, string message, IEnumerable<string>? details = null);
    }
}
