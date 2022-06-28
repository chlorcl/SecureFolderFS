﻿using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard
{
    public sealed class VaultWizardAddExistingViewModel : VaultWizardPathSelectionBaseViewModel<IFolder>
    {
        public VaultWizardAddExistingViewModel(IMessenger messenger, VaultWizardDialogViewModel dialogViewModel)
            : base(messenger, dialogViewModel)
        {
        }

        public override Task PrimaryButtonClick(IEventDispatchFlag? flag)
        {
            flag?.NoForwarding();

            Messenger.Send(new NavigationRequestedMessage(new VaultWizardSummaryViewModel(SelectedLocation!, Messenger, DialogViewModel)));
            return Task.CompletedTask;
        }

        public override async Task<bool> SetLocation(IFolder storage)
        {
            var file = await storage.GetFileAsync(SecureFolderFS.Core.Constants.VAULT_CONFIGURATION_FILENAME);
            if (file is null)
                return false;

            await using var stream = await file.OpenStreamAsync(FileAccess.Read, FileShare.Read);
            var vaultConfig = RawVaultConfiguration.Load(stream);
            var isSupported = VaultVersion.IsVersionSupported(vaultConfig);
            if (isSupported)
            {
                LocationPath = storage.Path;
                SelectedLocation = storage;
                DialogViewModel.PrimaryButtonEnabled = true;
                return true;
            }

            return false;
        }

        protected override async Task BrowseLocationAsync()
        {
            var folder = await FileExplorerService.PickSingleFolderAsync();
            if (folder is not null)
                await SetLocation(folder);
        }
    }
}
