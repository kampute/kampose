// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Reporters
{
    public enum ReportType
    {
        /// <summary>
        /// Represents a report that contains detailed information about an activity.
        /// </summary>
        Verbose,

        /// <summary>
        /// Represents a report that contains information about the progress of an activity.
        /// </summary>
        Information,

        /// <summary>
        /// Represents a report that contains information about warnings encountered during an activity.
        /// </summary>
        Warning,

        /// <summary>
        /// Represents a report that contains information about errors encountered during an activity.
        /// </summary>
        Error
    }
}
