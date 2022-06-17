﻿using System.Collections.Generic;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Utils;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Sdk.Services.Settings;

namespace SecureFolderFS.Sdk.Models
{
    public sealed class SavedVaultsModel : IRecipient<AddVaultRequestedMessage>, IRecipient<RemoveVaultRequestedMessage>, IRecipient<VaultSerializationRequestedMessage>
    {
        private ISecretSettingsService SecretSettingsService { get; } = Ioc.Default.GetRequiredService<ISecretSettingsService>();

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        internal IInitializableSource<IDictionary<VaultIdModel, VaultViewModel>>? InitializableSource { get; init; }

        public SavedVaultsModel()
        {
            WeakReferenceMessenger.Default.Register<AddVaultRequestedMessage>(this);
            WeakReferenceMessenger.Default.Register<RemoveVaultRequestedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultSerializationRequestedMessage>(this);
        }

        public void Initialize()
        {
            if (InitializableSource is not null && (SettingsService.IsAvailable && SecretSettingsService.IsAvailable))
            {
                var savedVaults = SettingsService.SavedVaults;
                var savedVaultModels = SecretSettingsService.SavedVaultModels;

                foreach (var item in savedVaults.Keys)
                {
                    savedVaults[item].VaultModel = savedVaultModels.TryGetValue(item, out var model) ? model : new(item);
                }

                InitializableSource.Initialize(savedVaults);
            }
        }

        public void Receive(AddVaultRequestedMessage message)
        {
            if (SettingsService.IsAvailable && SecretSettingsService.IsAvailable)
            {
                var savedVaults = SettingsService.SavedVaults!.ToDictionary();
                var savedVaultModels = SecretSettingsService.SavedVaultModels!.ToDictionary();

                savedVaults.Add(message.Value.VaultIdModel, message.Value);
                savedVaultModels.Add(message.Value.VaultIdModel, message.Value.VaultModel);

                SettingsService.SavedVaults = savedVaults!;
                SecretSettingsService.SavedVaultModels = savedVaultModels!;
            }
        }

        public void Receive(RemoveVaultRequestedMessage message)
        {
            if (SettingsService.IsAvailable && SecretSettingsService.IsAvailable)
            {
                var savedVaults = SettingsService.SavedVaults!.ToDictionary();
                var savedVaultModels = SecretSettingsService.SavedVaultModels!.ToDictionary();

                savedVaults.Remove(message.Value);
                savedVaultModels.Remove(message.Value);

                SettingsService.SavedVaults = savedVaults!;
                SecretSettingsService.SavedVaultModels = savedVaultModels!;
            }
        }

        public void Receive(VaultSerializationRequestedMessage message)
        {
            if (SettingsService.IsAvailable && SecretSettingsService.IsAvailable)
            {
                var savedVaults = SettingsService.SavedVaults!.ToDictionary();
                var savedVaultModels = SecretSettingsService.SavedVaultModels!.ToDictionary();

                if (savedVaults.ContainsKey(message.Value.VaultIdModel))
                {
                    savedVaults[message.Value.VaultIdModel] = message.Value;
                }
                if (savedVaultModels.ContainsKey(message.Value.VaultIdModel))
                {
                    savedVaultModels[message.Value.VaultIdModel] = message.Value.VaultModel;

                }

                SettingsService.SavedVaults = savedVaults!;
                SecretSettingsService.SavedVaultModels = savedVaultModels!;
            }
        }
    }
}
