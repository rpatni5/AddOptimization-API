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
    }
}
