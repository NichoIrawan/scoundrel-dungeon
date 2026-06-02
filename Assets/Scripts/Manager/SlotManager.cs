using Assets.Scripts;
using Assets.Scripts.Manager;
using Assets.Scripts.ScriptableObjects;
using Assets.Scripts.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotManager : MonoBehaviour, IInteractable
{
    [Header("Interaction & State Management")]
    [SerializeField] private GameObject _selectedHighlight;
    [SerializeField] private GameObject _resolvedOverlay;
    [SerializeField] private GameObject _discardedOverlay;
    [SerializeField] private GameObject resolvedEffect;

    [Header("UI Component References")]
    [Tooltip("The Image component that displays the encounter artwork.")]
    [SerializeField] private Image _artworkImage;

    [Tooltip("The TextMeshProUGUI component that displays the encounter's name.")]
    [SerializeField] private TextMeshProUGUI _nameText;

    [Tooltip("The TextMeshProUGUI component that displays the encounter's value (e.g., Strength, Power, Heal Value).")]
    [SerializeField] private TextMeshProUGUI _valueText;

    [Tooltip("The Button component for selecting this slot.")]
    [SerializeField] private Button _slotButton;

    private string _encounterId;
    private int _slotIndex;
    private EncounterScriptableObject _encounterSO;
    private bool _isResolved;
    private bool _isDiscarded;

    public string EncounterId => _encounterId;
    public int SlotIndex => _slotIndex;
    public EncounterScriptableObject EncounterData => _encounterSO;

    public event Action<int> OnSlotClicked;

    private void Awake()
    {
        if (_slotButton != null)
        {
            _slotButton.onClick.AddListener(HandleButtonClick);
        }
    }

    private void OnDestroy()
    {
        if (_slotButton != null)
        {
            _slotButton.onClick.RemoveListener(HandleButtonClick);
        }
    }

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

    public void Setup(int slotIndex, EncounterScriptableObject encounter, bool isResolved)
    {
        _slotIndex = slotIndex;
        _encounterSO = encounter;
        _isResolved = isResolved;

        if (encounter == null)
        {
            if (_artworkImage != null) _artworkImage.sprite = null;
            if (_nameText != null) _nameText.text = "Empty";
            if (_valueText != null) _valueText.text = string.Empty;
            if (_slotButton != null) _slotButton.interactable = false;
            if (_resolvedOverlay != null) _resolvedOverlay.SetActive(false);
            return;
        }

        if (_artworkImage != null)
        {
            _artworkImage.sprite = encounter.Artwork;
        }

        if (_nameText != null)
        {
            _nameText.text = GetLocalizedName(encounter);
        }

        if (_valueText != null)
        {
            _valueText.text = GetValueText(encounter);
        }

        if (_resolvedOverlay != null)
        {
            _resolvedOverlay.SetActive(isResolved);
        }
        if (resolvedEffect != null)
        {
            resolvedEffect.SetActive(isResolved);
        }

        if (_slotButton != null)
        {
            _slotButton.interactable = !isResolved;
        }
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

        if (string.IsNullOrWhiteSpace(_encounterId) || _encounterSO == null)
        {
            Debug.LogWarning($"[SlotManager] Slot {_slotIndex} has no encounter assigned.");
            return;
        }

        // Highlight this slot
        SetSelected(true);

        // Invoke the OnSlotClicked event to show the action panel
        OnSlotClicked?.Invoke(_slotIndex);
    }

    private string GetLocalizedName(EncounterScriptableObject encounter)
    {
        var loc = LocalizationManager.Instance;
        if (loc != null)
        {
            if (encounter is EnemiesScriptableObject)
                return loc.GetMonsterName(encounter.EncounterId);
            if (encounter is EquipmentsScriptableObject)
                return loc.GetWeaponName(encounter.EncounterId);
            if (encounter is ConsumablesScriptableObject)
                return loc.GetPotionName(encounter.EncounterId);
        }

        return !string.IsNullOrEmpty(encounter.DisplayName)
            ? encounter.DisplayName
            : encounter.EncounterId;
    }

    private string GetValueText(EncounterScriptableObject encounter)
    {
        if (encounter is EnemiesScriptableObject monster)
        {
            return $"Strength: {monster.Strength}";
        }
        if (encounter is EquipmentsScriptableObject weapon)
        {
            return $"Power: {weapon.Power}";
        }
        if (encounter is ConsumablesScriptableObject potion)
        {
            return $"Heal: {potion.HealValue}";
        }
        return string.Empty;
    }

    private void HandleButtonClick()
    {
        if (_isResolved) return;
        OnSlotClicked?.Invoke(_slotIndex);
    }

    private void RefreshVisuals()
    {
        if (_resolvedOverlay != null) _resolvedOverlay.SetActive(_isResolved);
        if (_discardedOverlay != null) _discardedOverlay.SetActive(_isDiscarded);
        if (resolvedEffect != null) resolvedEffect.SetActive(_isResolved);
    }
}
