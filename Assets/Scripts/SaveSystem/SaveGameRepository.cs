using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace Assets.Scripts.SaveSystem
{
    public static class SaveGameRepository
    {
        private const int PasswordSaltSize = 16;
        private const int PasswordHashSize = 32;
        private const int PasswordIterations = 100;
        public const string AccountsFolderName = "Accounts";
        public const string AuthMetaFileName = "auth.meta";
        public const string SaveFileName = "save.dat";

        public static string AccountsRootPath => Path.Combine(Application.persistentDataPath, AccountsFolderName);

        public static bool AccountExists(string username)
        {
            username = NormalizeUsername(username);
            return File.Exists(GetAuthMetaPath(username));
        }

        public static AccountAuthMeta CreateAccount(string username, string password)
        {
            username = NormalizeUsername(username);
            ValidatePassword(password);

            if (AccountExists(username))
            {
                throw new InvalidOperationException($"Account '{username}' already exists.");
            }

            Directory.CreateDirectory(GetAccountPath(username));

            var now = DateTime.UtcNow.ToString("O");
            var salt = SaveGameCrypto.GenerateRandomBytes(PasswordSaltSize);
            var hash = HashPassword(password, salt, PasswordIterations);
            var meta = new AccountAuthMeta
            {
                Username = username,
                Salt = Convert.ToBase64String(salt),
                PasswordHash = Convert.ToBase64String(hash),
                Iterations = PasswordIterations,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            WriteAuthMeta(meta);
            return meta;
        }

        public static bool Authenticate(string username, string password)
        {
            username = NormalizeUsername(username);
            var meta = ReadAuthMeta(username);
            var salt = Convert.FromBase64String(meta.Salt);
            var storedHash = Convert.FromBase64String(meta.PasswordHash);
            var attemptedHash = HashPassword(password, salt, meta.Iterations);
            return CryptographicOperations.FixedTimeEquals(storedHash, attemptedHash);
        }

        public static byte[] GetSaveKey(string username, string password)
        {
            username = NormalizeUsername(username);
            if (!Authenticate(username, password))
            {
                throw new UnauthorizedAccessException("Username or password is incorrect.");
            }

            var meta = ReadAuthMeta(username);
            return SaveGameCrypto.DeriveKey(password, Convert.FromBase64String(meta.Salt), meta.Iterations);
        }

        public static void TouchAccount(string username, string updatedAtUtc)
        {
            username = NormalizeUsername(username);
            var meta = ReadAuthMeta(username);
            meta.UpdatedAtUtc = updatedAtUtc;
            WriteAuthMeta(meta);
        }

        public static string GetAccountPath(string username)
        {
            return Path.Combine(AccountsRootPath, NormalizeUsername(username));
        }

        public static string GetAuthMetaPath(string username)
        {
            return Path.Combine(GetAccountPath(username), AuthMetaFileName);
        }

        public static string GetSavePath(string username)
        {
            return Path.Combine(GetAccountPath(username), SaveFileName);
        }

        public static AccountAuthMeta ReadAuthMeta(string username)
        {
            var path = GetAuthMetaPath(username);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Account '{username}' does not exist.", path);
            }

            return JsonUtility.FromJson<AccountAuthMeta>(File.ReadAllText(path));
        }

        private static void WriteAuthMeta(AccountAuthMeta meta)
        {
            Directory.CreateDirectory(GetAccountPath(meta.Username));
            File.WriteAllText(GetAuthMetaPath(meta.Username), JsonUtility.ToJson(meta, true));
        }

        private static byte[] HashPassword(string password, byte[] salt, int iterations)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return deriveBytes.GetBytes(PasswordHashSize);
        }

        public static string NormalizeUsername(string username)
        {
            ValidateUsername(username);
            return username.Trim();
        }

        private static void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be empty.", nameof(username));
            }

            if (username.Trim().IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("Username contains characters that cannot be used in a folder name.", nameof(username));
            }
        }

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            }
        }

    }
}
