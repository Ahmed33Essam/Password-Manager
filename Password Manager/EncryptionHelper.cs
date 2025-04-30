using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;

namespace Password_Manager
{
    using System.Security.Cryptography;
    using System.Text;

    namespace Password_Manager
    {
        public static class EncryptionHelper
        {
            // إعدادات ثابتة لطول المفتاح والـ salt
            private const int KeySize = 32; // 256-bit
            private const int IvSize = 16;  // 128-bit
            private const int SaltSize = 16;
            private const int Iterations = 100_000;

            public static byte[] EncryptString(string plainText, string password)
            {
                byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
                byte[] key = DeriveKey(password, salt);

                using var aes = Aes.Create();
                aes.Key = key;
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor();
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] cipherText = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // الصيغة النهائية: salt + iv + cipher
                byte[] result = new byte[SaltSize + IvSize + cipherText.Length];
                Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
                Buffer.BlockCopy(aes.IV, 0, result, SaltSize, IvSize);
                Buffer.BlockCopy(cipherText, 0, result, SaltSize + IvSize, cipherText.Length);

                return result;
            }

            public static string DecryptBytes(byte[] encryptedData, string password)
            {
                byte[] salt = new byte[SaltSize];
                byte[] iv = new byte[IvSize];
                byte[] cipherText = new byte[encryptedData.Length - SaltSize - IvSize];

                Buffer.BlockCopy(encryptedData, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(encryptedData, SaltSize, iv, 0, IvSize);
                Buffer.BlockCopy(encryptedData, SaltSize + IvSize, cipherText, 0, cipherText.Length);

                byte[] key = DeriveKey(password, salt);

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                byte[] plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

                return Encoding.UTF8.GetString(plainBytes);
            }

            private static byte[] DeriveKey(string password, byte[] salt)
            {
                using var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
                return deriveBytes.GetBytes(KeySize);
            }
        }
    }
}
