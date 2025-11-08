// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Reporters
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides extension methods for the <see cref="IActivityReporter"/> interface.
    /// </summary>
    public static class ActivityReporterExtensions
    {
        /// <summary>
        /// Logs a verbose message to the activity reporter.
        /// </summary>
        /// <param name="reporter">The activity reporter instance.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="details">An optional collection of additional details to include with the message.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/> or <paramref name="message"/> is <see langword="null"/>.</exception>
        public static void LogVerbose(this IActivityReporter reporter, string message, IEnumerable<string>? details = null)
        {
            ArgumentNullException.ThrowIfNull(reporter);
            ArgumentNullException.ThrowIfNull(message);

            reporter.Report(ReportType.Verbose, message, details);
        }

        /// <summary>
        /// Logs an informational message to the activity reporter.
        /// </summary>
        /// <param name="reporter">The activity reporter instance.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="details">An optional collection of additional details to include with the message.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/> or <paramref name="message"/> is <see langword="null"/>.</exception>
        public static void LogInformation(this IActivityReporter reporter, string message, IEnumerable<string>? details = null)
        {
            ArgumentNullException.ThrowIfNull(reporter);
            ArgumentNullException.ThrowIfNull(message);

            reporter.Report(ReportType.Information, message, details);
        }

        /// <summary>
        /// Logs a warning message to the activity reporter.
        /// </summary>
        /// <param name="reporter">The activity reporter instance.</param>
        /// <param name="message">The warning message to log.</param>
        /// <param name="details">An optional collection of additional details to include with the message.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/> or <paramref name="message"/> is <see langword="null"/>.</exception>
        public static void LogWarning(this IActivityReporter reporter, string message, IEnumerable<string>? details = null)
        {
            ArgumentNullException.ThrowIfNull(reporter);
            ArgumentNullException.ThrowIfNull(message);

            reporter.Report(ReportType.Warning, message, details);
        }

        /// <summary>
        /// Logs an error message to the activity reporter.
        /// </summary>
        /// <param name="reporter">The activity reporter instance.</param>
        /// <param name="message">The error message to log.</param>
        /// <param name="details">An optional collection of additional details to include with the message.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/> or <paramref name="message"/> is <see langword="null"/>.</exception>
        public static void LogError(this IActivityReporter reporter, string message, IEnumerable<string>? details = null)
        {
            ArgumentNullException.ThrowIfNull(reporter);
            ArgumentNullException.ThrowIfNull(message);

            reporter.Report(ReportType.Error, message, details);
        }

        /// <summary>
        /// Logs an error message to the activity reporter.
        /// </summary>
        /// <param name="reporter">The activity reporter instance.</param>
        /// <param name="message">The error message to log.</param>
        /// <param name="exception">The exception associated with the error.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/>, <paramref name="message"/>, or <paramref name="exception"/> is <see langword="null"/>.</exception>
        public static void LogError(this IActivityReporter reporter, string message, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(reporter);
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(exception);

            reporter.Report(ReportType.Error, message, GetExceptionMessages(exception));
        }

        /// <summary>
        /// Logs an exception to the activity reporter.
        /// </summary>
        /// <param name="reporter">The activity reporter instance.</param>
        /// <param name="exception">The exception to log.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reporter"/> or <paramref name="exception"/> is <see langword="null"/>.</exception>
        public static void LogError(this IActivityReporter reporter, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(reporter);
            ArgumentNullException.ThrowIfNull(exception);

            reporter.Report(ReportType.Error, string.Join('\n', GetExceptionMessages(exception)));
        }

        /// <summary>
        /// Retrieves a sequence of error messages from the specified exception and its inner exceptions.
        /// </summary>
        /// <param name="exception">The root <see cref="Exception"/> from which to extract messages.</param>
        /// <returns>An enumerable sequence of error messages from the exception and its inner exceptions, in order of nesting.</returns>
        private static IEnumerable<string> GetExceptionMessages(Exception exception)
        {
            for (var current = exception; current != null; current = current.InnerException)
                yield return current.Message;
        }
    }
}
