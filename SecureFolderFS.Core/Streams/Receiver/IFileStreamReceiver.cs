﻿using System;
using System.IO;

namespace SecureFolderFS.Core.Streams.Receiver
{
    internal interface IFileStreamReceiver : IDisposable
    {
        /// <summary>
        /// Opens a new cleartext stream wrapping <paramref name="ciphertextStream"/>.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path of the file.</param>
        /// <param name="ciphertextStream">The ciphertext stream to wrap by the cleartext stream.</param>
        /// <returns>A new instance of cleartext stream.</returns>
        Stream OpenCleartextStream(string ciphertextPath, Stream ciphertextStream);
    }
}
