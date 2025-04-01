using MessagingApp.Shared.Models.Payloads;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Services
{
    public class CryptographyService
    {
        public byte[] AesKey { get; set; }
        public byte[] IV { get; set; }
        public RSAParameters PublicKey  { get; private set; }
        public RSAParameters PrivateKey { get; private set; }
        public string PublicKeyString { get; private set; }
        public string PrivateKeyString { get; private set; }


        // Ref: o1-Mini: WebAsm doesn't support System.Security.Cryptography.RSA.Create, Use JS as a fall back for creating the keys if that fails for any platform.

        private readonly IJSRuntime _jsRuntime;
        private readonly bool _isWebAssembly;

        public CryptographyService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _isWebAssembly = OperatingSystem.IsBrowser(); // Detect if running in WebAssembly
        }

        private byte[] FromBase64Url(string base64Url)
        {
            // Handle potential null input from JS if something went wrong there
            if (base64Url == null)
            {
                throw new ArgumentNullException(nameof(base64Url), "Input Base64Url string cannot be null.");
            }

            string base64 = base64Url.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4) // Pad
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch (FormatException ex)
            {
                // Provide more context in the exception
                throw new FormatException($"Failed to decode Base64Url string (derived from '{base64Url}'). Error: {ex.Message}", ex);
            }
        }

        public async Task GenerateKeysAsync()
        {
            PublicKeyString = null;
            PrivateKeyString = null;

            if (_isWebAssembly)
            {
                try
                {
                    Console.WriteLine("WebAssembly detected, calling JS generateRSAKeys...");
                    // Expect object matching JS return { n, e, d, p, q, dp, dq, qi }
                    var keys = await _jsRuntime.InvokeAsync<JsRSAKeyComponents>("generateRSAKeys");
                    Console.WriteLine("JS generateRSAKeys returned.");

                    // Add more robust check in case JS returns null or empty object
                    if (keys == null || string.IsNullOrEmpty(keys.n))
                    {
                        throw new InvalidOperationException("Received null or invalid key data from JavaScript (JWK 'n' property missing or null).");
                    }

                    // Create RSAParameters from JWK components, using CORRECT property names and Base64Url decoding
                    PublicKey = new RSAParameters
                    {
                        Modulus = FromBase64Url(keys.n),  // Use keys.n
                        Exponent = FromBase64Url(keys.e) // Use keys.e
                    };

                    PrivateKey = new RSAParameters
                    {
                        Modulus = FromBase64Url(keys.n),  // Use keys.n
                        Exponent = FromBase64Url(keys.e), // Use keys.e
                        D = FromBase64Url(keys.d),
                        P = FromBase64Url(keys.p),
                        Q = FromBase64Url(keys.q),
                        DP = FromBase64Url(keys.dp),
                        DQ = FromBase64Url(keys.dq),
                        InverseQ = FromBase64Url(keys.qi) // Use keys.qi
                    };

                    // Convert RSAParameters to XML Strings
                    PublicKeyString = PublicKey.ToXmlString(false);
                    PrivateKeyString = PrivateKey.ToXmlString(true);

                    Console.WriteLine("RSA keys generated from JS and converted to XML strings.");
                }
                // Keep specific catch blocks first
                catch (JSException jsEx)
                {
                    Console.WriteLine($"JS Interop Error: {jsEx.Message}");
                    throw new InvalidOperationException("Failed during JavaScript interop key generation.", jsEx);
                }
                catch (ArgumentNullException argNullEx) // Catch nulls specifically
                {
                    Console.WriteLine($"Null argument error, likely from JS returning unexpected null value: {argNullEx.Message}");
                    throw new InvalidOperationException("Received unexpected null data from JavaScript.", argNullEx);
                }
                catch (FormatException formatEx)
                {
                    Console.WriteLine($"Base64 Decoding Error: {formatEx.Message}");
                    throw new InvalidOperationException("Failed to decode key data received from JavaScript.", formatEx);
                }
                catch (Exception ex) // General catch
                {
                    Console.WriteLine($"Unexpected error during WASM key generation: {ex.Message}");
                    throw new InvalidOperationException("An unexpected error occurred during WASM key generation.", ex);
                }
            }
            else // Server-side or MAUI Blazor Hybrid
            {
                Console.WriteLine(".NET environment detected, generating keys with RSA.Create...");
                using (RSA rsa = RSA.Create(2048))
                {
                    PublicKey = rsa.ExportParameters(false);
                    PrivateKey = rsa.ExportParameters(true);

                    PublicKeyString = BuildRsaXml(PublicKey, false);
                    PrivateKeyString = BuildRsaXml(PrivateKey, true);

                    Console.WriteLine("RSA keys generated using .NET cryptography and stored as XML strings.");
                }
            }

            // Final checks remain the same
            if (string.IsNullOrEmpty(PublicKeyString)) { /* ... throw ... */ }
            if (string.IsNullOrEmpty(PrivateKeyString)) { /* ... throw ... */ }
        }


        private string BuildRsaXml(RSAParameters parameters, bool includePrivate)
        {
            // Use StringBuilder for efficient string concatenation
            var sb = new StringBuilder();
            sb.Append("<RSAKeyValue>");

            // Public parts (always include) - Ensure parameters are not null!
            if (parameters.Modulus == null || parameters.Exponent == null)
                throw new ArgumentNullException(nameof(parameters), "Modulus or Exponent cannot be null for XML serialization.");
            sb.AppendFormat("<Modulus>{0}</Modulus>", Convert.ToBase64String(parameters.Modulus));
            sb.AppendFormat("<Exponent>{0}</Exponent>", Convert.ToBase64String(parameters.Exponent));

            if (includePrivate)
            {
                // Private parts - Ensure parameters are not null!
                if (parameters.P == null || parameters.Q == null || parameters.DP == null ||
                    parameters.DQ == null || parameters.InverseQ == null || parameters.D == null)
                    throw new ArgumentNullException(nameof(parameters), "Private key components cannot be null for XML serialization.");

                sb.AppendFormat("<P>{0}</P>", Convert.ToBase64String(parameters.P));
                sb.AppendFormat("<Q>{0}</Q>", Convert.ToBase64String(parameters.Q));
                sb.AppendFormat("<DP>{0}</DP>", Convert.ToBase64String(parameters.DP));
                sb.AppendFormat("<DQ>{0}</DQ>", Convert.ToBase64String(parameters.DQ));
                sb.AppendFormat("<InverseQ>{0}</InverseQ>", Convert.ToBase64String(parameters.InverseQ));
                sb.AppendFormat("<D>{0}</D>", Convert.ToBase64String(parameters.D));
            }

            sb.Append("</RSAKeyValue>");
            return sb.ToString();

            /* // Alternative using System.Xml.Linq (might add overhead in WASM)
            var doc = new XDocument(
                new XElement("RSAKeyValue",
                    new XElement("Modulus", Convert.ToBase64String(parameters.Modulus)),
                    new XElement("Exponent", Convert.ToBase64String(parameters.Exponent))
                    // Add private elements conditionally here if includePrivate is true
                )
            );
            if (includePrivate) {
                 doc.Root.Add(new XElement("P", Convert.ToBase64String(parameters.P)));
                 // ... add other private elements ...
                 doc.Root.Add(new XElement("D", Convert.ToBase64String(parameters.D)));
            }
            return doc.ToString(SaveOptions.DisableFormatting); // Or SaveOptions.None
            */
        }

        // --- Decrypt/Encrypt methods remain the same ---

        // --- JsRSAKeyComponents class definition remains the same ---



        public MessagePayload EncryptMessage(string plainText, string recipientPublicKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                AesKey = aes.Key;

                aes.GenerateIV();
                IV = aes.IV;

                var plainTextInByte = Encoding.UTF8.GetBytes(plainText);

                var encryptedMessage = EncryptMessageWithAes(plainTextInByte, AesKey, IV);

                // Encrypt AES key with the public key of the recipient
                byte[] encryptedAesKey = EncryptAesKeyWithRsa(AesKey, recipientPublicKey);

                // api expects the properties in string
                var result = new MessagePayload
                {
                    Content = Convert.ToBase64String(encryptedMessage),
                    EncryptionKey = Convert.ToBase64String(encryptedAesKey),
                    IV = Convert.ToBase64String(this.IV)
                };
                return result;
            }
        }

        public string DecryptMessage(string encryptedMessage, string aesKey, string iv)
        {
            byte[] encryptedMessageInByte = Convert.FromBase64String(encryptedMessage);
            byte[] aesKeyInByte = Convert.FromBase64String(aesKey);
            byte[] ivInByte = Convert.FromBase64String(iv);

            // decrypt the aes key
            var decryptedAesKey = DecryptAesKeyWithRsa(aesKeyInByte, PrivateKey);

            // using that key and the iv, decrypt the message
            var decryptedMessage = DecryptMessageWithAes(encryptedMessageInByte, decryptedAesKey, ivInByte);
            return decryptedMessage;
        }
        private static byte[] EncryptMessageWithAes(byte[] plainTextInByte, byte[] keyInByte, byte[] ivInByte) {
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyInByte;
                aes.IV = ivInByte;

                // MemoryStream stores the encrypted data in memory while we are processing
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Encrypts the data before stroing it in the memory
                    // 1: where will it be stored, 2: object that performs the AES encryption, 3: specifies that we are writing the AES data
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainTextInByte, 0, plainTextInByte.Length);
                        cryptoStream.FlushFinalBlock();
                        return memoryStream.ToArray(); // Return the cipher text from the memoryStream
                    }
                }
            }
        }

        private static byte[] EncryptAesKeyWithRsa(byte[] aesKey, string publicKey)
        {
            using (RSA rsa = RSA.Create())
            {
                // load the public key into the algorithm
                rsa.FromXmlString(publicKey);

                // RSAEncryptionPadding.OaepSHA256 = a secure padding method that mixes this data with even more hash to prevent attacks
                var result = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
                return result;
            }
        }

        private static byte[] DecryptAesKeyWithRsa(byte[] aesKeyInByte, RSAParameters privateKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(privateKey);

                var result = rsa.Decrypt(aesKeyInByte, RSAEncryptionPadding.OaepSHA256);
                return result;
            }
        }

        private static string DecryptMessageWithAes(byte[] messageInByte, byte[] aesInByte, byte[] ivInByte)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = aesInByte;
                aes.IV = ivInByte;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(messageInByte, 0, messageInByte.Length);
                        cryptoStream.FlushFinalBlock();
                        var msgInByte = memoryStream.ToArray();
                        return Encoding.UTF8.GetString(msgInByte);
                    }
                }
            }

        }

        private class JsRSAKeyComponents // Matches JWK properties returned by JS
        {
            // Ensure Nullable Reference Types are considered if enabled project-wide
            // Initialize to null! or empty string "" if you prefer, although null! indicates expected value
            public string? n { get; set; }  // Modulus (Base64Url)
            public string? e { get; set; }  // Exponent (Base64Url)
            public string? d { get; set; }  // Private Exponent (Base64Url)
            public string? p { get; set; }  // First prime factor (Base64Url)
            public string? q { get; set; }  // Second prime factor (Base64Url)
            public string? dp { get; set; } // First factor CRT exponent (Base64Url)
            public string? dq { get; set; } // Second factor CRT exponent (Base64Url)
            public string? qi { get; set; } // First CRT coefficient (InverseQ) (Base64Url)

        }
        private class JsKeys
        {
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }
        }
    }

    public static class RsaExtensions
    {
        public static string ToXmlString(this RSAParameters parameters, bool includePrivate)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(parameters);
                return rsa.ToXmlString(includePrivate);
            }
        }

        // will need this for saving private key
        public static RSAParameters FromXmlString(this string xmlString)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(xmlString);
                return rsa.ExportParameters(xmlString.Contains("<P>"));
            }
        }
        // Base64 spki - not needed, but it is plaintext without any xml or json formatting.
        public static string ToSpkiBase64String(this RSAParameters parameters)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportParameters(parameters);
                return Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            }
        }
    }
}
