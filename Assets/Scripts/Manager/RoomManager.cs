using Assets.Scripts.Manager;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.SceneController;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> _encounterSlots;
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private EncounterResolver encounterResolver;
    [SerializeField] private RunManager runManager;
    [SerializeField] private EncounterSpawner encounterSpawner;

    public RoomSession Session { get; private set; }
    public string[] CurrentEncounterIds { get; private set; } = new string[4];

    private void Start()
    {
        ResolveReferences();
        BeginRoom();
    }

    public void BeginRoom()
    {
        ResolveReferences();

        Session = new RoomSession();

        CurrentEncounterIds = dungeonManager != null
            ? dungeonManager.GetCurrentRoomEncounterIds()
            : new string[4];

        PopulateSlots();

        encounterSpawner?.BindEncountersToSlots();
    }

    public void PopulateSlots()
    {
        if (_encounterSlots == null) return;

        for (int i = 0; i < _encounterSlots.Count; i++)
        {
            var slotObj = _encounterSlots[i];
            if (slotObj == null) continue;

            var slot = slotObj.GetComponent<SlotManager>();
            if (slot == null) continue;

            var encounterId = i < CurrentEncounterIds.Length ? CurrentEncounterIds[i] : string.Empty;
            slot.SetEncounterId(encounterId, i);
            slot.SetResolved(false);
        }
    }

    public void ResolveMonsterWithHealth(int slotIndex)
    {
        ResolveReferences();
        if (!IsSlotValid(slotIndex)) return;

        var encounterId = CurrentEncounterIds[slotIndex];
        var monster = encounterResolver?.GetMonster(dungeonManager, encounterId);
        if (monster == null)
        {
            Debug.LogWarning($"[RoomManager] Monster not found for id '{encounterId}'.");
            return;
        }

        var result = encounterResolver.ResolveMonsterWithHealth(monster);
        ApplyResultAndProgress(result, slotIndex);
    }

    public bool ResolveMonsterWithWeapon(int slotIndex)
    {
        ResolveReferences();
        if (!IsSlotValid(slotIndex)) return false;

        var encounterId = CurrentEncounterIds[slotIndex];
        var monster = encounterResolver?.GetMonster(dungeonManager, encounterId);
        if (monster == null) return false;

        var player = runManager?.Player;
        if (string.IsNullOrEmpty(player?.EquippedWeaponId)) return false;

        var weapon = encounterResolver.GetWeapon(dungeonManager, player.EquippedWeaponId);
        if (weapon == null) return false;

        var result = encounterResolver.TryResolveMonsterWithWeapon(monster, weapon);
        if (result == null) return false; // weapon rule violated

        ApplyResultAndProgress(result, slotIndex);
        return true;
    }

    public void ResolveWeapon(int slotIndex)
    {
        ResolveReferences();
        if (!IsSlotValid(slotIndex)) return;

        var encounterId = CurrentEncounterIds[slotIndex];
        var weapon = encounterResolver?.GetWeapon(dungeonManager, encounterId);
        if (weapon == null)
        {
            Debug.LogWarning($"[RoomManager] Weapon not found for id '{encounterId}'.");
            return;
        }

        var result = encounterResolver.ResolveWeapon(weapon);
        ApplyResultAndProgress(result, slotIndex);
    }
    public void ResolveUsePotionFromSlot(int slotIndex)
    {
        ResolveReferences();
        if (!IsSlotValid(slotIndex)) return;

        var encounterId = CurrentEncounterIds[slotIndex];
        var potion = encounterResolver?.GetPotion(dungeonManager, encounterId);
        if (potion == null)
        {
            Debug.LogWarning($"[RoomManager] Potion not found for id '{encounterId}'.");
            return;
        }

        var result = encounterResolver.ResolveUsePotion(potion, Session);
        ApplyResultAndProgress(result, slotIndex);
    }

    public void ResolveStorePotionFromSlot(int slotIndex)
    {
        ResolveReferences();
        if (!IsSlotValid(slotIndex)) return;

        var encounterId = CurrentEncounterIds[slotIndex];
        var potion = encounterResolver?.GetPotion(dungeonManager, encounterId);
        if (potion == null)
        {
            Debug.LogWarning($"[RoomManager] Potion not found for id '{encounterId}'.");
            return;
        }

        var result = encounterResolver.ResolveStorePotion(potion);
        ApplyResultAndProgress(result, slotIndex);
    }

    public bool TryEscapeRoom()
    {
        ResolveReferences();

        var player = runManager?.Player;
        if (!encounterResolver.ValidateEscape(player, Session))
        {
            Debug.LogWarning("[RoomManager] Escape is not valid: either already escaped this run or encounters already resolved.");
            return false;
        }

        if (dungeonManager == null || !dungeonManager.EscapeCurrentRoom())
            return false;

        player.CanEscape = false;
        AutoSave();
        ReturnToHallway();
        return true;
    }

    public void PopulateCurrentRoom() => BeginRoom();

    public void RegisterSlotSelection(SlotManager slotManager, bool isSelected) { }

    public bool TryAdvanceRoom()
    {
        ResolveReferences();
        return dungeonManager != null && dungeonManager.AdvanceToNextRoom();
    }

    private void ApplyResultAndProgress(EncounterResult result, int slotIndex)
    {
        if (result == null) return;

        runManager?.ApplyResult(result);

        Session.ResolvedSlots[slotIndex] = true;
        Session.ResolvedCount++;
        MarkSlotResolved(slotIndex);
        if (encounterResolver.IsPlayerDead())
        {
            HandleGameOver();
            return;
        }

        if (Session.ResolvedCount >= 3)
        {
            HandleRoomComplete();
        }
    }

    private void HandleRoomComplete()
    {
        for (int i = 0; i < Session.ResolvedSlots.Length; i++)
        {
            if (!Session.ResolvedSlots[i])
            {
                MarkSlotDiscarded(i);
                break;
            }
        }

        if (runManager?.Player != null)
            runManager.Player.CanEscape = true;

        AutoSave();
        ReturnToHallway();
    }

    private void HandleGameOver()
    {
        DataPersistenceManager.Instance.DeleteSave();
        LoadGameOverScene();
    }

    private void ReturnToHallway()
    {
        SceneController.Instance
            ?.NewTransition()
            .Load(SceneDatabase.Slots.Phases, SceneDatabase.Scenes.Hallway, setActive: true)
            .Unload(SceneDatabase.Slots.Phases)
            .WithOverlay()
            .Perform();
    }

    private void LoadGameOverScene()
    {
        SceneController.Instance
            ?.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu, setActive: true)
            .Unload(SceneDatabase.Slots.Run)
            .WithOverlay()
            .Perform();
    }

    private void AutoSave()
    {
        DataPersistenceManager.Instance?.AutoSave();
    }

    private void MarkSlotResolved(int index)
    {
        var slot = GetSlotManager(index);
        slot?.SetResolved(true);
    }

    private void MarkSlotDiscarded(int index)
    {
        var slot = GetSlotManager(index);
        slot?.SetDiscarded(true);
    }

    private SlotManager GetSlotManager(int index)
    {
        if (_encounterSlots == null || index < 0 || index >= _encounterSlots.Count) return null;
        return _encounterSlots[index]?.GetComponent<SlotManager>();
    }

    private bool IsSlotValid(int slotIndex)
    {
        if (Session == null || slotIndex < 0 || slotIndex >= 4) return false;
        if (Session.ResolvedSlots[slotIndex])
        {
            Debug.LogWarning($"[RoomManager] Slot {slotIndex} is already resolved.");
            return false;
        }
        return true;
    }

    private void ResolveReferences()
    {
        if (dungeonManager == null) dungeonManager = FindAnyObjectByType<DungeonManager>();
        if (encounterResolver == null) encounterResolver = FindAnyObjectByType<EncounterResolver>();
        if (runManager == null) runManager = FindAnyObjectByType<RunManager>();
        if (encounterSpawner == null) encounterSpawner = FindAnyObjectByType<EncounterSpawner>();
    }
}
