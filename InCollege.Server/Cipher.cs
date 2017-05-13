using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

//Made by Selami Güngör
//Refactored and optimized by [CYBOR]

namespace SG.Algoritma
{
    public static class Cipher
    {
        static (byte[] key, RijndaelManaged AES) CachedAES;
        // Set your salt here, change it to meet your flavor:
        // The salt bytes must be at least 8 bytes.
        static readonly byte[] SaltBytes = Encoding.ASCII.GetBytes("SomeShit1234");

        /// <summary>
        /// Encrypt a string.
        /// </summary>
        /// <param name="plainText">String to be encrypted</param>
        /// <param name="password">Password</param>
        public static string Encrypt(string plainText, string password)
        {
            if (plainText == null)
                return null;

            if (password == null)
                password = String.Empty;

            // Get the bytes of the string
            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(plainText);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            var bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

            return Convert.ToBase64String(bytesEncrypted);
        }

        /// <summary>
        /// Decrypt a string.
        /// </summary>
        /// <param name="encryptedText">String to be decrypted</param>
        /// <param name="password">Password used during encryption</param>
        /// <exception cref="FormatException"></exception>
        public static string Decrypt(string encryptedText, string password)
        {
            if (encryptedText == null)
                return null;

            if (password == null)
                password = String.Empty;

            // Get the bytes of the string
            var bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            var bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);

            return Encoding.UTF8.GetString(bytesDecrypted);
        }

        private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, GetAES(passwordBytes, SaltBytes).CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                    cs.Close();
                }
                encryptedBytes = ms.ToArray();
            }

            return encryptedBytes;
        }

        private static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, GetAES(passwordBytes, SaltBytes).CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                    cs.Close();
                }
                decryptedBytes = ms.ToArray();
            }

            return decryptedBytes;
        }

        static RijndaelManaged GetAES(byte[] passwordBytes, byte[] saltBytes)
        {
            RijndaelManaged AES;
            if (CachedAES.key?.SequenceEqual(passwordBytes) ?? false)
                AES = CachedAES.AES;
            else
            {
                if (CachedAES.AES != null)
                    CachedAES.AES.Dispose();
                CachedAES = (passwordBytes, new RijndaelManaged
                {
                    KeySize = 256,
                    BlockSize = 128,
                    Mode = CipherMode.CBC
                });

                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                CachedAES.AES.Key = key.GetBytes(CachedAES.AES.KeySize / 8);
                CachedAES.AES.IV = key.GetBytes(CachedAES.AES.BlockSize / 8);

                AES = CachedAES.AES;
            }

            return AES;
        }
    }
}