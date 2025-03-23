using MessagingApp.Shared.Models.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Services
{
    public class CryptographyService
    {
        public byte[] AesKey { get; set; }
        public byte[] IV { get; set; }
        public RSAParameters PublicKey  { get; set; }
        private RSAParameters PrivateKey { get; set; }

        public void GenerateKeys()
        {
            using (RSA rsa = RSA.Create(2048))
            {
                PublicKey = rsa.ExportParameters(false);
                PrivateKey = rsa.ExportParameters(true);
            }
        }

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
    }
}
