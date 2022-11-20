﻿using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultCreationModel"/>
    public sealed class VaultCreationModel : IVaultCreationModel
    {
        private IFolder? _vaultFolder;

        private IVaultCreationService VaultCreationService { get; } = Ioc.Default.GetRequiredService<IVaultCreationService>();

        /// <inheritdoc/>
        public async Task<IResult> SetFolderAsync(IModifiableFolder folder, CancellationToken cancellationToken)
        {
            if (!await VaultCreationService.SetVaultFolderAsync(folder, cancellationToken))
                return new CommonResult(false);

            var configurationResult = await VaultCreationService.PrepareConfigurationAsync(cancellationToken);
            if (!configurationResult.Successful)
                return configurationResult;

            var readmeFile = await folder.TryCreateFileAsync(Constants.VaultReadme.VAULT_README_FILENAME, CreationCollisionOption.OpenIfExists, cancellationToken);
            if (readmeFile is not null)
                await readmeFile.WriteAllTextAsync(Constants.VaultReadme.VAULT_README_MESSAGE, cancellationToken);

            _vaultFolder = folder;

            return new CommonResult();
        }

        /// <inheritdoc/>
        public async Task<IResult> SetKeystoreAsync(IKeystoreModel keystoreModel, CancellationToken cancellationToken = default)
        {
            var keystoreStreamResult = await keystoreModel.GetKeystoreStreamAsync(FileAccess.ReadWrite, cancellationToken);
            if (keystoreStreamResult.Value is null || !keystoreStreamResult.Successful)
                return keystoreStreamResult;

            return await VaultCreationService.PrepareKeystoreAsync(keystoreStreamResult.Value, keystoreModel.KeystoreSerializer, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> SetPasswordAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            return await VaultCreationService.SetPasswordAsync(password, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> SetCipherSchemeAsync(ICipherInfoModel contentCipher, ICipherInfoModel nameCipher, CancellationToken cancellationToken = default)
        {
            var setCipherResult = await VaultCreationService.SetCipherSchemeAsync(contentCipher, nameCipher, cancellationToken);
            if (!setCipherResult.Successful)
                return false;

            return true;
        }

        /// <inheritdoc/>
        public async Task<IResult<IVaultModel?>> DeployAsync(CancellationToken cancellationToken = default)
        {
            if (_vaultFolder is null)
                return new CommonResult<IVaultModel?>(null);

            var deployResult = await VaultCreationService.DeployAsync(cancellationToken);
            if (!deployResult.Successful)
                return new CommonResult<IVaultModel?>(deployResult.Exception);

            // Create vault model
            IVaultModel vaultModel = new LocalVaultModel(_vaultFolder);

            // Set up widgets
            IWidgetsContextModel widgetsContextModel = new SavedWidgetsContextModel(vaultModel); // TODO: Reuse it!

            await widgetsContextModel.AddWidgetAsync(Constants.Widgets.HEALTH_WIDGET_ID, cancellationToken);
            await widgetsContextModel.AddWidgetAsync(Constants.Widgets.GRAPHS_WIDGET_ID, cancellationToken);

            return new CommonResult<IVaultModel?>(vaultModel);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            VaultCreationService.Dispose();
        }
    }
}
