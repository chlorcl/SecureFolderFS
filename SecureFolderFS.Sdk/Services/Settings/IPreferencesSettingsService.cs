﻿using SecureFolderFS.Core.Enums;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.Settings
{
    /// <summary>
    /// A service to manage settings of user preferences.
    /// </summary>
    public interface IPreferencesSettingsService : ISettingsModel
    {
        /// <summary>
        /// Determines the type of the file system provider to use.
        /// </summary>
        FileSystemAdapterType ActiveFileSystemAdapter { get; set; }

        /// <summary>
        /// Determines whether to launch SecureFolderFS on system startup.
        /// </summary>
        bool StartOnSystemStartup { get; set; }

        /// <summary>
        /// Determines whether to continue on the previously selected vault.
        /// </summary>
        bool ContinueOnLastVault { get; set; }
    }
}
