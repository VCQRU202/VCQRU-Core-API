using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CoreApi_BL_App.Models
{
    public static class EncryptionHelper
    {
        private static readonly string EncryptionKey = "VCQRU2024"; // Make sure to use a secure key, ideally stored securely
        private static readonly string EncryptionIV = "VCQRU2024";  // IV should be random or fixed, but must match encryption/decryption

        // Encrypts the given password
        public static string Encrypt(string plainText)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);  // Use a key of 16, 24, or 32 bytes
                    aesAlg.IV = Encoding.UTF8.GetBytes(EncryptionIV);    // IV is 16 bytes long for AES

                    // Create a decryptor to perform the stream transform
                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    // Create memory stream to hold the encrypted data
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        // Create the CryptoStream, and pass it the memory stream and encryptor
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                // Write the plain text to the CryptoStream (this will be encrypted)
                                swEncrypt.Write(plainText);
                            }
                        }

                        // Return the encrypted byte array as a base64 string
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error encrypting the password: " + ex.Message);
            }
        }
    }

}
