// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampose.Models
{
    using System;

    /// <summary>
    /// Represents a reference to an asset file used in documentation generation.
    /// </summary>
    /// <remarks>
    /// This class is used to define a mapping between the source location of an asset and its target location.
    /// </remarks>
    public sealed class AssetReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetReference"/> class with the specified source and target paths.
        /// </summary>
        /// <param name="sourcePath">The source path of the asset.</param>
        /// <param name="targetPath">The target path of the asset.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sourcePath"/> or <paramref name="targetPath"/> is <see langword="null"/> or empty.</exception>
        public AssetReference(string sourcePath, string targetPath)
        {
            ArgumentException.ThrowIfNullOrEmpty(sourcePath);
            ArgumentException.ThrowIfNullOrEmpty(targetPath);

            SourcePath = sourcePath;
            TargetPath = targetPath;
        }

        /// <summary>
        /// Gets the source path of the asset.
        /// </summary>
        /// <value>
        /// The source path of the asset.
        /// </value>
        public string SourcePath { get; }

        /// <summary>
        /// Gets the target path of the asset.
        /// </summary>
        /// <value>
        /// The target path of the asset.
        /// </value>
        public string TargetPath { get; }
    }
}
