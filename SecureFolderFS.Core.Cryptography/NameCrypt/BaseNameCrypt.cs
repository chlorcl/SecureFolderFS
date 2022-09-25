﻿using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.Helpers;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace SecureFolderFS.Core.Cryptography.NameCrypt
{
    /// <inheritdoc cref="INameCrypt"/>
    internal abstract class BaseNameCrypt : INameCrypt
    {
        protected readonly SecretKey macKey;
        protected readonly SecretKey encryptionKey;
        protected readonly ICipherProvider cipherProvider;

        protected BaseNameCrypt(SecretKey macKey, SecretKey encryptionKey, ICipherProvider cipherProvider)
        {
            this.macKey = macKey;
            this.encryptionKey = encryptionKey;
            this.cipherProvider = cipherProvider;
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public virtual string EncryptName(ReadOnlySpan<char> cleartextName, ReadOnlySpan<byte> directoryId)
        {
            // Allocate byte* for encoding
            Span<byte> bytes = stackalloc byte[Encoding.UTF8.GetByteCount(cleartextName)];

            // Get bytes from cleartext name
            var count = Encoding.UTF8.GetBytes(cleartextName, bytes);

            // Encrypt
            var encryptedName = EncryptFileName(bytes.Slice(0, count), directoryId);

            // Encode with url64
            return EncodingHelpers.EncodeBaseUrl64(Convert.ToBase64String(encryptedName));
        }

        /// <inheritdoc/>
        public virtual string? DecryptName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            var ciphertextNameBuffer = Convert.FromBase64String(EncodingHelpers.DecodeBaseUrl64(ciphertextName.ToString()));
            var cleartextNameBuffer = DecryptFileName(ciphertextNameBuffer, directoryId);
            if (cleartextNameBuffer is null)
                return null;

            return Encoding.UTF8.GetString(cleartextNameBuffer);
        }

        protected abstract byte[] EncryptFileName(ReadOnlySpan<byte> cleartextFileNameBuffer, ReadOnlySpan<byte> directoryId);

        protected abstract byte[]? DecryptFileName(ReadOnlySpan<byte> ciphertextFileNameBuffer, ReadOnlySpan<byte> directoryId);
    }
}
