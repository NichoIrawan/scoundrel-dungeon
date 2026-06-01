using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.SaveSystem
{
    public class DataPersistenceManager : MonoBehaviour
    {
        private static DataPersistenceManager instance;

        public static DataPersistenceManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                instance = FindAnyObjectByType<DataPersistenceManager>();
                if (instance != null)
                {
                    return instance;
                }

                var gameObject = new GameObject(nameof(DataPersistenceManager));
                return gameObject.AddComponent<DataPersistenceManager>();
            }
            private set => instance = value;
        }

        [Header("File Storage Config")]
        [SerializeField] private bool createNewGameWhenNoSaveExists = true;
        [SerializeField] private bool saveOnApplicationQuit = true;

        private GameData gameData;
        private List<IDataPersistence> dataPersistenceObjects = new();
        private FileDataHandler dataHandler;
        private string activeUsername;
        private string activePassword;

        public bool HasGameData => gameData != null;
        public bool HasActiveAccount => !string.IsNullOrEmpty(activeUsername);

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            dataHandler = new FileDataHandler();
        }

        private void Start()
        {
            dataPersistenceObjects = FindAllDataPersistenceObjects();

            if (SaveGameBridge.HasActiveGameData)
            {
                gameData = SaveGameBridge.ActiveGameData;
                activeUsername = SaveGameBridge.ActiveUsername;
                LoadDataIntoObjects();
            }
        }

        public AccountAuthMeta CreateAccount(string username, string password)
        {
            return SaveGameRepository.CreateAccount(username, password);
        }

        public void Login(string username, string password)
        {
            activeUsername = SaveGameRepository.NormalizeUsername(username);
            activePassword = password;
            dataPersistenceObjects = FindAllDataPersistenceObjects();

            gameData = dataHandler.Load(activeUsername, activePassword);
            if (gameData == null && createNewGameWhenNoSaveExists)
            {
                NewGame();
                SaveGame();
            }

            LoadDataIntoObjects();
        }

        public void NewGame()
        {
            gameData = new GameData
            {
                Username = activeUsername
            };

            SaveGameBridge.SetActiveGameData(activeUsername, gameData);
        }

        public void LoadGame()
        {
            EnsureActiveAccount();
            dataPersistenceObjects = FindAllDataPersistenceObjects();

            gameData = dataHandler.Load(activeUsername, activePassword);
            if (gameData == null)
            {
                if (!createNewGameWhenNoSaveExists)
                {
                    Debug.LogWarning($"No save data found for account '{activeUsername}'.");
                    return;
                }

                NewGame();
            }

            LoadDataIntoObjects();
        }

        public void SaveGame()
        {
            EnsureActiveAccount();

            if (gameData == null)
            {
                NewGame();
            }

            dataPersistenceObjects = FindAllDataPersistenceObjects();
            foreach (var dataPersistenceObject in dataPersistenceObjects)
            {
                dataPersistenceObject.SaveData(ref gameData);
            }

            gameData.Username = activeUsername;
            gameData.UpdatedAtUtc = DateTime.UtcNow.ToString("O");
            dataHandler.Save(activeUsername, activePassword, gameData);
            SaveGameBridge.SetActiveGameData(activeUsername, gameData);
        }

        private void LoadDataIntoObjects()
        {
            if (gameData == null)
            {
                return;
            }

            SaveGameBridge.SetActiveGameData(activeUsername, gameData);
            foreach (var dataPersistenceObject in dataPersistenceObjects)
            {
                dataPersistenceObject.LoadData(gameData);
            }
        }

        private List<IDataPersistence> FindAllDataPersistenceObjects()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
                .OfType<IDataPersistence>()
                .ToList();
        }

        private void EnsureActiveAccount()
        {
            if (string.IsNullOrEmpty(activeUsername) || string.IsNullOrEmpty(activePassword))
            {
                throw new InvalidOperationException("Login must be called before saving or loading game data.");
            }
        }

        private void OnApplicationQuit()
        {
            if (saveOnApplicationQuit && HasActiveAccount && HasGameData)
            {
                SaveGame();
            }
        }
    }
}
