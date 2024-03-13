using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Utilities.Helpers
{
    public static class LicenseKeyHelper
    {
        public static LicenseKey GenerateLicenseKey(string licenseName, TimeSpan licenseDuration)
        {
            // Generate a new AesManaged object to perform string symmetric encryption
            using (AesManaged aes = new AesManaged())
            {
                // Generate a key and initialization vector (IV) for the encryption
                aes.GenerateKey();
                aes.GenerateIV();

                // Convert the licensee name and license duration to bytes
                byte[] licenseeNameBytes = Encoding.UTF8.GetBytes(licenseName);
                byte[] licenseDurationBytes = BitConverter.GetBytes(licenseDuration.TotalSeconds);

                // Encrypt the licensee name and license duration using the AES algorithm
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(licenseeNameBytes, 0, licenseeNameBytes.Length);
                        csEncrypt.Write(licenseDurationBytes, 0, licenseDurationBytes.Length);
                        csEncrypt.FlushFinalBlock();

                        // The encrypted licensee name and license duration are stored in the memory stream
                        byte[] encryptedData = msEncrypt.ToArray();

                        // Combine the key, IV, and encrypted data into a single array
                        byte[] licenseKeyData = new byte[aes.Key.Length + aes.IV.Length + encryptedData.Length];
                        Buffer.BlockCopy(aes.Key, 0, licenseKeyData, 0, aes.Key.Length);
                        Buffer.BlockCopy(aes.IV, 0, licenseKeyData, aes.Key.Length, aes.IV.Length);
                        Buffer.BlockCopy(encryptedData, 0, licenseKeyData, aes.Key.Length + aes.IV.Length, encryptedData.Length);

                        // Convert the license key data to a base64-encoded string
                        string licenseKey = Convert.ToBase64String(licenseKeyData);

                        // Set the expiration date of the license key to be 6 months from now
                        DateTime expirationDate = DateTime.Now.Add(licenseDuration);

                        // Return the license key
                        return new LicenseKey { Key = licenseKey, ExpirationDate = expirationDate };
                    }
                }
            }
        }


        public static bool ValidateLicenseKey(LicenseKey licenseKey)
        {
            // Convert the license key from a base64-encoded string to a byte array
            byte[] licenseKeyData = Convert.FromBase64String(licenseKey.Key);

            // Extract the key, IV, and encrypted data from the license key data
            byte[] aesKey = new byte[32];
            byte[] aesIV = new byte[16];
            byte[] encryptedData = new byte[licenseKeyData.Length - aesKey.Length - aesIV.Length];
            Buffer.BlockCopy(licenseKeyData, 0, aesKey, 0, aesKey.Length);
            Buffer.BlockCopy(licenseKeyData, aesKey.Length, aesIV, 0, aesIV.Length);
            Buffer.BlockCopy(licenseKeyData, aesKey.Length + aesIV.Length, encryptedData, 0, encryptedData.Length);

            // Generate a new AesManaged object to perform string symmetric decryption
            using (AesManaged aes = new AesManaged())
            {
                // Set the keyand IV of the AesManaged object to the values extracted from the license key data
                aes.Key = aesKey;
                aes.IV = aesIV;

                // Decrypt the encrypted data using the AES algorithm
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        // Read the decrypted data into a byte array
                        byte[] decryptedData = new byte[msDecrypt.Length];
                        int decryptedByteCount = csDecrypt.Read(decryptedData, 0, decryptedData.Length);

                        // Convert the decrypted data to a licensee name and license duration
                        byte[] licenseeNameBytes = new byte[decryptedByteCount / 2];
                        byte[] licenseDurationBytes = new byte[decryptedByteCount / 2];
                        Buffer.BlockCopy(decryptedData, 0, licenseeNameBytes, 0, licenseeNameBytes.Length);
                        Buffer.BlockCopy(decryptedData, licenseeNameBytes.Length, licenseDurationBytes, 0, licenseDurationBytes.Length);
                        string licenseeName = Encoding.UTF8.GetString(licenseeNameBytes);
                        double licenseDurationSeconds = BitConverter.ToDouble(licenseDurationBytes, 0);
                        TimeSpan licenseDuration = TimeSpan.FromSeconds(licenseDurationSeconds);

                        // Check if the license key has expired
                        if (licenseKey.ExpirationDate < DateTime.Now)
                        {
                            return false;
                        }

                        // Check if the license key is valid by comparing the decrypted licensee name and license duration
                        // with the input licensee name and license duration
                        return licenseeName == licenseKey.Key && licenseDuration == licenseKey.ExpirationDate.Subtract(DateTime.Now);
                    }
                }
            }
        }
    }
    public class LicenseKey
    {
        public string Key { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
