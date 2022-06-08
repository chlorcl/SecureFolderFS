﻿using System.IO;
using SecureFolderFS.Core.Sdk.Streams;

namespace SecureFolderFS.Core.Storage
{
    public interface IVaultFile : IVaultItem
    {
        ICleartextFileStream OpenStream(FileMode mode, FileAccess access, FileShare share, FileOptions options);
    }
}
