using System.Security.Cryptography;

namespace Temporal.Workflows.Encryption;

/// <summary>
/// AES-256-CBC encryption helper used by the Temporal payload codec.
/// Equivalent to GoTraining's <c>pkg/encryption</c> package.
/// </summary>
public static class EncryptionHelper
{
    private const int KeySize = 32; // 256 bits
    private const int IvSize = 16;  // 128 bits (AES block size)

    /// <summary>
    /// Encrypts <paramref name="data"/> using AES-256-CBC.
    /// The IV is prepended to the ciphertext so it can be extracted during decryption.
    /// </summary>
    public static byte[] Encrypt(byte[] data, byte[] key)
    {
        if (key.Length != KeySize)
            throw new ArgumentException($"Key must be {KeySize} bytes (256 bits).", nameof(key));

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var cipher = encryptor.TransformFinalBlock(data, 0, data.Length);

        // Layout: [IV (16 bytes)] [ciphertext]
        var result = new byte[IvSize + cipher.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, IvSize);
        Buffer.BlockCopy(cipher, 0, result, IvSize, cipher.Length);
        return result;
    }

    /// <summary>
    /// Decrypts data produced by <see cref="Encrypt"/>.
    /// </summary>
    public static byte[] Decrypt(byte[] encryptedData, byte[] key)
    {
        if (key.Length != KeySize)
            throw new ArgumentException($"Key must be {KeySize} bytes (256 bits).", nameof(key));
        if (encryptedData.Length < IvSize)
            throw new ArgumentException("Data is too short to contain an IV.", nameof(encryptedData));

        using var aes = Aes.Create();
        aes.Key = key;

        var iv = new byte[IvSize];
        Buffer.BlockCopy(encryptedData, 0, iv, 0, IvSize);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(encryptedData, IvSize, encryptedData.Length - IvSize);
    }

    /// <summary>Generates a cryptographically random 256-bit key.</summary>
    public static byte[] GenerateKey() => RandomNumberGenerator.GetBytes(KeySize);
}
