using Assets.Scripts.SaveSystem;
using System;
using UnityEngine;

public class RunManager : MonoBehaviour, IDataPersistence
{
    [SerializeField] private bool loadActiveSaveOnStart = true;
    [SerializeField] private PlayerState playerState = new();

    public PlayerState Player => playerState;

    private bool runStateInitialized;

    private void Awake()
    {
        InitializeRunState();
    }

    private void InitializeRunState()
    {
        if (runStateInitialized)
        {
            return;
        }

        if (loadActiveSaveOnStart && SaveGameBridge.HasActiveGameData)
        {
            LoadData(SaveGameBridge.ActiveGameData);
        }

        runStateInitialized = true;
    }

    public void EquipWeapon(string weaponId)
    {
        playerState.EquippedWeaponId = weaponId;
        playerState.LastDefeatedEnemyStrength = 0;
    }

    public void StorePotion(string potionId)
    {
        playerState.StoredPotionId = potionId;
    }

    public bool TryUseStoredPotion(int healAmount)
    {
        if (string.IsNullOrEmpty(playerState.StoredPotionId) || playerState.PotionUsedThisRoom)
        {
            return false;
        }

        playerState.Health += Math.Max(0, healAmount);
        playerState.StoredPotionId = null;
        playerState.PotionUsedThisRoom = true;
        return true;
    }

    public void ResetRoomLimitedActions()
    {
        playerState.PotionUsedThisRoom = false;
    }

    public void RecordWeaponDefeat(int enemyStrength)
    {
        playerState.LastDefeatedEnemyStrength = enemyStrength;
    }

    public void SaveCurrentRun(string username, string password)
    {
        DataPersistenceManager.Instance.Login(username, password);
        DataPersistenceManager.Instance.SaveGame();
    }

    public void LoadRun(string username, string password)
    {
        DataPersistenceManager.Instance.Login(username, password);
    }

    public void LoadData(GameData data)
    {
        ApplyGameData(data);
    }

    public void SaveData(ref GameData data)
    {
        data.RunState ??= new RunState();
        data.RunState.Player = playerState;
    }

    public void ApplyGameData(GameData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        playerState = saveData.RunState?.Player ?? new PlayerState();
        runStateInitialized = true;
        SaveGameBridge.SetActiveGameData(saveData.Username, saveData);
    }
}
