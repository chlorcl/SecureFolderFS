﻿using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="BaseDictionaryDatabaseModel{TDictionaryValue}"/>
    public sealed class SingleFileDatabaseModel : BaseDictionaryDatabaseModel<object?>
    {
        private readonly IFile _databaseFile;

        public SingleFileDatabaseModel(IFile databaseFile, IAsyncSerializer<Stream> serializer)
            : base(serializer)
        {
            _databaseFile = databaseFile;
        }

        /// <inheritdoc/>
        public override TValue? GetValue<TValue>(string key, Func<TValue?>? defaultValue) where TValue : default
        {
            if (settingsCache.TryGetValue(key, out var value))
                return (TValue?)value;

            var fallback = defaultValue is not null ? defaultValue() : default;
            settingsCache[key] = fallback;

            return fallback;
        }

        /// <inheritdoc/>
        public override bool SetValue<TValue>(string key, TValue? value) where TValue : default
        {
            settingsCache[key] = value;
            return true;
        }

        /// <inheritdoc/>
        public override async Task<bool> LoadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await storageSemaphore.WaitAsync(cancellationToken);
                await using var stream = await _databaseFile.TryOpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
                if (stream is null)
                    return false;

                var settings = await serializer.DeserializeAsync<Stream, Dictionary<string, object?>?>(stream, cancellationToken);
                settingsCache.Clear();

                if (settings is null) // No settings saved, set cache to empty and return true.
                    return true;

                foreach (var item in settings)
                {
                    settingsCache[item.Key] = item.Value;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _ = storageSemaphore.Release();
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await storageSemaphore.WaitAsync(cancellationToken);
                await using var dataStream = await _databaseFile.TryOpenStreamAsync(FileAccess.ReadWrite, FileShare.Read, cancellationToken);
                if (dataStream is null)
                    return false;

                await using var settingsStream = await serializer.SerializeAsync<Stream, IDictionary<string, object?>>(settingsCache, cancellationToken);

                // Overwrite existing content
                dataStream.Position = 0L;
                dataStream.SetLength(0L);

                // Copy contents
                settingsStream.Position = 0L;
                await settingsStream.CopyToAsync(dataStream, cancellationToken);

                return true;
            }
            finally
            {
                _ = storageSemaphore.Release();
            }
        }
    }
}
