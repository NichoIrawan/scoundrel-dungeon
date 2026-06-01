using System.IO;
using UnityEngine;

namespace Assets.Scripts.SaveSystem
{
    public class FileDataHandler
    {
        public GameData Load(string username, string password)
        {
            var savePath = SaveGameRepository.GetSavePath(username);
            if (!File.Exists(savePath))
            {
                return null;
            }

            var key = SaveGameRepository.GetSaveKey(username, password);
            var encryptedBytes = File.ReadAllBytes(savePath);
            var json = SaveGameCrypto.DecryptString(encryptedBytes, key);
            return JsonUtility.FromJson<GameData>(json);
        }

        public void Save(string username, string password, GameData data)
        {
            var accountPath = SaveGameRepository.GetAccountPath(username);
            Directory.CreateDirectory(accountPath);

            var key = SaveGameRepository.GetSaveKey(username, password);
            var json = JsonUtility.ToJson(data, true);
            var encryptedBytes = SaveGameCrypto.EncryptString(json, key);
            File.WriteAllBytes(SaveGameRepository.GetSavePath(username), encryptedBytes);
            SaveGameRepository.TouchAccount(username, data.UpdatedAtUtc);
        }
    }
}
