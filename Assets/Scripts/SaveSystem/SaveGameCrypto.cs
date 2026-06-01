using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Assets.Scripts.SaveSystem
{
    public static class SaveGameCrypto
    {
        private const string Magic = "SDAV1";
        private const int KeySizeBytes = 32;
        private const int IvSizeBytes = 16;
        private const int MacSizeBytes = 32;

        public static byte[] GenerateRandomBytes(int length)
        {
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return bytes;
        }

        public static byte[] DeriveKey(string password, byte[] salt, int iterations)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return deriveBytes.GetBytes(KeySizeBytes);
        }

        public static byte[] EncryptString(string plainText, byte[] key)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            byte[] cipherText;
            using (var encryptor = aes.CreateEncryptor())
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                cipherText = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }

            var magicBytes = Encoding.ASCII.GetBytes(Magic);
            var signedBytes = Combine(magicBytes, aes.IV, cipherText);
            var mac = ComputeMac(signedBytes, key);
            return Combine(magicBytes, aes.IV, mac, cipherText);
        }

        public static string DecryptString(byte[] encryptedBytes, byte[] key)
        {
            var magicBytes = Encoding.ASCII.GetBytes(Magic);
            var minimumLength = magicBytes.Length + IvSizeBytes + MacSizeBytes + 1;
            if (encryptedBytes == null || encryptedBytes.Length < minimumLength)
            {
                throw new InvalidDataException("Save file is too small or empty.");
            }

            for (int i = 0; i < magicBytes.Length; i++)
            {
                if (encryptedBytes[i] != magicBytes[i])
                {
                    throw new InvalidDataException("Save file format is not recognized.");
                }
            }

            var iv = new byte[IvSizeBytes];
            Buffer.BlockCopy(encryptedBytes, magicBytes.Length, iv, 0, IvSizeBytes);

            var storedMac = new byte[MacSizeBytes];
            Buffer.BlockCopy(encryptedBytes, magicBytes.Length + IvSizeBytes, storedMac, 0, MacSizeBytes);

            var cipherLength = encryptedBytes.Length - magicBytes.Length - IvSizeBytes - MacSizeBytes;
            var cipherText = new byte[cipherLength];
            Buffer.BlockCopy(encryptedBytes, magicBytes.Length + IvSizeBytes + MacSizeBytes, cipherText, 0, cipherLength);

            var signedBytes = Combine(magicBytes, iv, cipherText);
            var expectedMac = ComputeMac(signedBytes, key);
            if (!CryptographicOperations.FixedTimeEquals(storedMac, expectedMac))
            {
                throw new CryptographicException("Save file authentication failed.");
            }

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        private static byte[] ComputeMac(byte[] bytes, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(bytes);
        }

        private static byte[] Combine(params byte[][] arrays)
        {
            var length = 0;
            foreach (var array in arrays)
            {
                length += array.Length;
            }

            var result = new byte[length];
            var offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }
    }
}
