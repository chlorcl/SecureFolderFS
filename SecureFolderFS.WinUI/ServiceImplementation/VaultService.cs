﻿using SecureFolderFS.Core.Dokany;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.WinUI.AppModels;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    internal sealed class VaultService : IVaultService
    {
        /// <inheritdoc/>
        public IAsyncValidator<IFolder> GetVaultValidator()
        {
            return new VaultValidator(StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IFileSystemInfoModel> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new DokanyFileSystemDescriptor(new DokanyAvailabilityChecker());
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICipherInfoModel> GetContentCiphersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new CipherDescriptor("XChaCha20-Poly1305", Core.Constants.CipherId.XCHACHA20_POLY1305);
            yield return new CipherDescriptor("AES-GCM", Core.Constants.CipherId.AES_GCM);
            yield return new CipherDescriptor("AES-CTR + HMAC-SHA256", Core.Constants.CipherId.AES_CTR_HMAC);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICipherInfoModel> GetFileNameCiphersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new CipherDescriptor("AES-SIV", Core.Constants.CipherId.AES_SIV);
            yield return new CipherDescriptor("None", Core.Constants.CipherId.NONE);
            await Task.CompletedTask;
        }
    }
}
