using System.Text;
using Google.Protobuf;
using Temporalio.Api.Common.V1;
using Temporalio.Converters;

namespace Temporal.Workflows.Encryption;

/// <summary>
/// Custom Temporal payload codec that encrypts all workflow data at rest.
/// Equivalent to GoTraining's <c>PayloadCodec</c>.
///
/// Temporal serialises workflow inputs/outputs as <c>Payload</c> protobufs.
/// This codec intercepts encode/decode to wrap every payload in AES-256-CBC
/// encryption so that sensitive data (PII, card numbers) is never stored in
/// plain text in the Temporal server history.
///
/// Wire format:
///   metadata["encoding"] = "binary/encrypted"
///   data                 = [IV (16 bytes)] [AES-256-CBC ciphertext]
/// </summary>
public class PayloadCodec(byte[] encryptionKey) : IPayloadCodec
{
    private const string EncodingMetadataKey = "encoding";
    private const string EncryptedEncoding = "binary/encrypted";

    public Task<IReadOnlyCollection<Payload>> EncodeAsync(IReadOnlyCollection<Payload> payloads)
    {
        var encoded = payloads
            .Select(payload =>
            {
                var plaintext = payload.ToByteArray();
                var ciphertext = EncryptionHelper.Encrypt(plaintext, encryptionKey);

                return new Payload
                {
                    Metadata =
                    {
                        [EncodingMetadataKey] = ByteString.CopyFromUtf8(EncryptedEncoding)
                    },
                    Data = ByteString.CopyFrom(ciphertext)
                };
            })
            .ToList();

        return Task.FromResult<IReadOnlyCollection<Payload>>(encoded);
    }

    public Task<IReadOnlyCollection<Payload>> DecodeAsync(IReadOnlyCollection<Payload> payloads)
    {
        var decoded = payloads
            .Select(payload =>
            {
                // Only decrypt payloads that were encrypted by this codec
                if (!payload.Metadata.TryGetValue(EncodingMetadataKey, out var enc) ||
                    enc.ToStringUtf8() != EncryptedEncoding)
                {
                    return payload;
                }

                var plaintext = EncryptionHelper.Decrypt(payload.Data.ToByteArray(), encryptionKey);
                return Payload.Parser.ParseFrom(plaintext);
            })
            .ToList();

        return Task.FromResult<IReadOnlyCollection<Payload>>(decoded);
    }
}
