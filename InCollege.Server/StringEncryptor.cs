using System;
using System.Collections.Generic;

namespace CyborTools
{
    public static class StringEncryptor
    {
        /// <summary>
        /// Encrypt string with key.
        /// </summary>
        /// <param name="textToEncrypt">Text to encrypt.</param>
        /// <param name="encryptingKey">Encrypting key, provided by GetKey function.</param>
        /// <returns>Encrypted data in blocks</returns>
        public static List<int> Encrypt(string textToEncrypt, int encryptingKey)
        {
            var data = new List<int>();
            foreach (var currentChar in textToEncrypt)
                if (encryptingKey != 0)
                    data.Add(currentChar * encryptingKey);
                else
                    data.Add(currentChar);
            return data;
        }
        /// <summary>
        /// Decrypt encrypted data, using key
        /// </summary>
        /// <param name="dataToDecrypt">Text to decrypt.</param>
        /// <param name="decryptingKey">Encrypting key, provided by GetKey function.</param>
        /// <returns>Decrypted string</returns>
        public static string Decrypt(List<int> dataToDecrypt, int decryptingKey)
        {
            string decryptedString = string.Empty;
            foreach (var DataBlock in dataToDecrypt)
                if (decryptingKey != 0) decryptedString += (char)(DataBlock / decryptingKey);
                else decryptedString += (char)DataBlock;
            return decryptedString;
        }
        /// <summary>
        /// Unpack data from string
        /// </summary>
        /// <param name="packedData">String with packed data</param>
        /// <returns>Unpacked data</returns>
        public static List<int> GetData(string packedData)
        {
            var strData = new List<string>();
            var data = new List<int>();
            bool beginBlock = false;
            int i = -1;
            foreach (var currentChar in packedData)
            {
                if (currentChar == '[') { beginBlock = true; strData.Add(string.Empty); i++; };
                if (currentChar == ']') beginBlock = false;
                if ((currentChar != '[') && (currentChar != ']') && beginBlock)
                    strData[i] += currentChar;
            }
            foreach (var current in strData)
                data.Add(Convert.ToInt32(current));
            return data;
        }
        /// <summary>
        /// Pack data to string
        /// </summary>
        /// <param name="initialData">Data</param>
        /// <returns>Packed data</returns>
        public static string PackData(List<int> initialData)
        {
            var packedData = string.Empty;
            foreach (var dataBlock in initialData)
                packedData += "[" + dataBlock + "]";
            return packedData;
        }
        /// <summary>
        /// Provides key from string
        /// </summary>
        /// <param name="stringWithKey">String key</param>
        /// <returns>Key</returns>
        public static int GetKey(string stringWithKey)
        {
            int key = 0;
            foreach (var currentChar in stringWithKey)
                key += currentChar;
            return key;
        }
    }
}
