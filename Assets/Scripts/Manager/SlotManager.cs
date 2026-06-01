using Assets.Scripts;
using Assets.Scripts.Manager;
using Assets.Scripts.ScriptableObjects;
using UnityEngine;

public class SlotManager : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _selectedHighlight;
    [SerializeField] private GameObject _resolvedOverlay;
    [SerializeField] private GameObject _discardedOverlay;
    [SerializeField] private RoomManager roomManager;

    private string _encounterId;
    private int _slotIndex;
    private EncounterScriptableObject _encounterSO;
    private bool _isResolved;
    private bool _isDiscarded;

    public string EncounterId => _encounterId;
    public int SlotIndex => _slotIndex;
    public EncounterScriptableObject EncounterData => _encounterSO;

    public void SetEncounterId(string encounterId, int slotIndex)
    {
        _encounterId = encounterId;
        _slotIndex = slotIndex;
        _encounterSO = null;
        _isResolved = false;
        _isDiscarded = false;

        gameObject.name = string.IsNullOrWhiteSpace(encounterId)
            ? $"Empty Slot {slotIndex}"
            : $"Slot {slotIndex} [{encounterId}]";

        RefreshVisuals();
    }

    public void SetEncounterData(EncounterScriptableObject so)
    {
        _encounterSO = so;
    }

    public void SetResolved(bool resolved)
    {
        _isResolved = resolved;
        RefreshVisuals();
    }

    public void SetDiscarded(bool discarded)
    {
        _isDiscarded = discarded;
        RefreshVisuals();
    }

    public string EffigyName => _encounterId;

    public void SetEffigyName(string value) => SetEncounterId(value, _slotIndex);

    public void SetSelected(bool isSelected)
    {
        if (_selectedHighlight != null)
            _selectedHighlight.SetActive(isSelected);
    }

    public void Interact()
    {
        if (_isResolved || _isDiscarded)
        {
            Debug.Log($"[SlotManager] Slot {_slotIndex} already resolved/discarded — ignoring.");
            return;
        }

        ResolveRoomManager();

        if (roomManager == null)
        {
            Debug.LogWarning("[SlotManager] No RoomManager found.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_encounterId))
        {
            Debug.LogWarning($"[SlotManager] Slot {_slotIndex} has no encounter assigned.");
            return;
        }

        if (_encounterSO is EnemiesScriptableObject)
        {
            roomManager.ResolveMonsterWithHealth(_slotIndex);
        }
        else if (_encounterSO is EquipmentsScriptableObject)
        {
            roomManager.ResolveWeapon(_slotIndex);
        }
        else if (_encounterSO is ConsumablesScriptableObject)
        {
            roomManager.ResolveUsePotionFromSlot(_slotIndex);
        }
        else
        {
            Debug.LogWarning($"[SlotManager] Unknown encounter type for '{_encounterId}'. Cannot route interaction.");
        }
    }

    private void RefreshVisuals()
    {
        if (_resolvedOverlay != null) _resolvedOverlay.SetActive(_isResolved);
        if (_discardedOverlay != null) _discardedOverlay.SetActive(_isDiscarded);
    }

    private void ResolveRoomManager()
    {
        if (roomManager != null) return;
        roomManager = FindAnyObjectByType<RoomManager>();
    }
}
