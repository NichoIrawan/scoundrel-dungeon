using Assets.Scripts.SaveSystem;
using System;
using UnityEngine;

public class RunManager : MonoBehaviour, IDataPersistence
{
    private const int MAX_HP = 35;

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

    public void InitializeNewRun()
    {
        playerState = new PlayerState
        {
            Health = 35,
            EquippedWeaponId = null,
            StoredPotionId = null,
            LastKilledByWeaponStrength = null,
            CanEscape = true
        };
        runStateInitialized = true;
    }

    public void EquipWeapon(string weaponId)
    {
        playerState.EquippedWeaponId = weaponId;
        playerState.LastKilledByWeaponStrength = null;
    }

    public void StorePotion(string potionId)
    {
        playerState.StoredPotionId = potionId;
    }

    public void RecordWeaponDefeat(int enemyStrength)
    {
        playerState.LastKilledByWeaponStrength = enemyStrength;
    }

    public void ApplyResult(EncounterResult result)
    {
        if (result == null) return;

        playerState.Health += result.HealthDelta;
        playerState.Health = Mathf.Clamp(playerState.Health, 0, MAX_HP);

        if (result.EquippedWeaponId != null)
        {
            playerState.EquippedWeaponId = result.EquippedWeaponId;
        }

        if (result.StoredPotionId != null)
        {
            playerState.StoredPotionId = result.StoredPotionId;
        }

        if (result.ResetWeaponChain)
        {
            playerState.LastKilledByWeaponStrength = null;
        }
        else if (result.LastKilledByWeaponStrength.HasValue)
        {
            playerState.LastKilledByWeaponStrength = result.LastKilledByWeaponStrength;
        }
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

