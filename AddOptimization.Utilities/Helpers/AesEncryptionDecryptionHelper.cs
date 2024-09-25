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
    public static class AesEncryptionDecryptionHelper
    {

        public static string Encrypt(string text)
        {
            var (key, iv) = GetEncryptionKeyAndIV();
            byte[] cipheredtext;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(text);
                        }
                        cipheredtext = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(cipheredtext);
        }

        public static string Decrypt(byte[] cipherText)
        {
            string plainText;
            var (key, iv) = GetEncryptionKeyAndIV();

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(cipherText))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            plainText = streamReader.ReadToEnd();
                        }
                    }
                }
            }
            return plainText;
        }

        private static (byte[] key, byte[] iv) GetEncryptionKeyAndIV()
        {
            var key = Encoding.ASCII.GetBytes("0123456789ABCDEF0123456789ABCDEF");
            var iv = Encoding.ASCII.GetBytes("1234567890123456");
            if (key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes for AES-256.");

            if (iv.Length != 16)
                throw new ArgumentException("IV must be 16 bytes for AES.");
            return (key, iv);
        }


    }
}
