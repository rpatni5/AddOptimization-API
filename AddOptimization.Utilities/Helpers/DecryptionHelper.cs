using NPOI.POIFS.Crypt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Utilities.Helpers
{
    public static class DecryptionHelper
    {
        private static readonly byte[] StaticKey = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
        private static readonly byte[] StaticIv = Encoding.UTF8.GetBytes("123456789012");

        public static string Decrypt(string encryptedBase64)
        {
            byte[] encryptedData = Convert.FromBase64String(encryptedBase64);

            byte[] iv = StaticIv;
            Array.Copy(encryptedData, 0, iv, 0, iv.Length);

            int ciphertextTagLength = encryptedData.Length - iv.Length;
            byte[] ciphertextTag = new byte[ciphertextTagLength];
            Array.Copy(encryptedData, iv.Length, ciphertextTag, 0, ciphertextTagLength);

            int tagLength = 16;
            int ciphertextLength = ciphertextTag.Length - tagLength;

            byte[] ciphertext = new byte[ciphertextLength];
            byte[] tag = new byte[tagLength];

            Array.Copy(ciphertextTag, 0, ciphertext, 0, ciphertextLength);
            Array.Copy(ciphertextTag, ciphertextLength, tag, 0, tagLength);

            using (AesGcm aesGcm = new AesGcm(StaticKey))
            {
                byte[] decryptedData = new byte[ciphertextLength];
                aesGcm.Decrypt(iv, ciphertext, tag, decryptedData);

                return Encoding.UTF8.GetString(decryptedData).TrimEnd('\0');
            }
        }

        public static string Encrypt(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using (AesGcm aesGcm = new AesGcm(StaticKey))
            {
                byte[] encryptedData = new byte[plainBytes.Length];
                byte[] tag = new byte[16]; // 16 bytes for the authentication tag

                // Encrypt the data using the provided IV and key
                aesGcm.Encrypt(StaticIv, plainBytes, encryptedData, tag);

                // Combine IV + encrypted data + authentication tag
                byte[] combinedData = new byte[StaticIv.Length + encryptedData.Length + tag.Length];
                Buffer.BlockCopy(StaticIv, 0, combinedData, 0, StaticIv.Length);
                Buffer.BlockCopy(encryptedData, 0, combinedData, StaticIv.Length, encryptedData.Length);
                Buffer.BlockCopy(tag, 0, combinedData, StaticIv.Length + encryptedData.Length, tag.Length);

                // Return the result as a Base64 string
                return Convert.ToBase64String(combinedData);
            }
        }
    }
}
