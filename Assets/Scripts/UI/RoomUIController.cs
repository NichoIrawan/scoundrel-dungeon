using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Manager;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.ScriptableObjects;

namespace Assets.Scripts.UI
{
    public class RoomUIController : MonoBehaviour
    {
        [Header("HUD Elements")]
        [Tooltip("The TextMeshProUGUI component that displays the player's health.")]
        [SerializeField] private TextMeshProUGUI _hpText;

        [Tooltip("The TextMeshProUGUI component that displays the player's equipped weapon details.")]
        [SerializeField] private TextMeshProUGUI _weaponText;

        [Tooltip("The TextMeshProUGUI component that displays the player's stored potion details.")]
        [SerializeField] private TextMeshProUGUI _potionText;

        [Tooltip("The TextMeshProUGUI component that displays the current room and resolved encounters.")]
        [SerializeField] private TextMeshProUGUI _roomCountText;

        [Tooltip("The button used to consume the currently stored potion.")]
        [SerializeField] private Button _useStoredPotionButton;

        [Tooltip("The button used to attempt escaping the current room.")]
        [SerializeField] private Button _escapeButton;

        [Header("Hierarchy Containers")]
        [Tooltip("The RectTransform that acts as the grid layout container for encounter slots.")]
        [SerializeField] private GameObject _encounterGrid;

        [Header("Menu references")]
        [Tooltip("The ActionPanelView menu reference in the scene.")]
        [SerializeField] private ActionPanelView _actionPanel;

        [Header("Manager References (Optional in Editor, Auto-resolved at Start)")]
        [SerializeField] private RoomManager _roomManager;
        [SerializeField] private RunManager _runManager;
        [SerializeField] private DungeonManager _dungeonManager;
        [SerializeField] private EncounterResolver _encounterResolver;

        private readonly List<SlotManager> _spawnedSlotViews = new();

        private void Start()
        {
            ResolveReferences();

            // Set up static button click listeners
            if (_escapeButton != null)
            {
                _escapeButton.onClick.AddListener(HandleEscapeClicked);
            }

            if (_useStoredPotionButton != null)
            {
                _useStoredPotionButton.onClick.AddListener(HandleUseStoredPotionClicked);
            }

            // Cache existing slot views from the hierarchy
            SpawnSlotViews();

            // Initial UI refresh
            RefreshUI();

            // Ensure action panel starts hidden
            if (_actionPanel != null)
            {
                _actionPanel.Hide();
            }
        }

        private void OnDestroy()
        {
            if (_escapeButton != null)
            {
                _escapeButton.onClick.RemoveListener(HandleEscapeClicked);
            }

            if (_useStoredPotionButton != null)
            {
                _useStoredPotionButton.onClick.RemoveListener(HandleUseStoredPotionClicked);
            }

            // Unsubscribe from slot click listeners to prevent memory leaks
            foreach (var slot in _spawnedSlotViews)
            {
                if (slot != null)
                {
                    slot.OnSlotClicked -= HandleSlotClicked;
                }
            }
        }

        public void RefreshUI()
        {
            ResolveReferences();

            if (_runManager == null || _dungeonManager == null) return;

            var player = _runManager.Player;

            if (_hpText != null)
            {
                _hpText.text = Manager.LocalizationManager.Instance.GetString("hud_hp", player.Health);
            }

            if (_weaponText != null)
            {
                if (string.IsNullOrEmpty(player.EquippedWeaponId))
                {
                    _weaponText.text = Manager.LocalizationManager.Instance.GetUIString("hud_weapon_none");
                }
                else
                {
                    var weapon = _dungeonManager.GetEncounter(player.EquippedWeaponId) as EquipmentsScriptableObject;
                    string weaponName = weapon != null ? GetLocalizedWeaponName(weapon) : player.EquippedWeaponId;
                    int weaponPower = weapon != null ? weapon.Power : 0;
                    
                    string restriction = player.LastKilledByWeaponStrength.HasValue
                        ? Manager.LocalizationManager.Instance.GetString("hud_weapon_restriction", player.LastKilledByWeaponStrength.Value)
                        : string.Empty;

                    _weaponText.text = Manager.LocalizationManager.Instance.GetString("hud_weapon_equipped", weaponName, weaponPower, restriction);
                }
            }

            if (_potionText != null)
            {
                if (string.IsNullOrEmpty(player.StoredPotionId))
                {
                    _potionText.text = Manager.LocalizationManager.Instance.GetUIString("hud_potion_none");
                }
                else
                {
                    var potion = _dungeonManager.GetEncounter(player.StoredPotionId) as ConsumablesScriptableObject;
                    string potionName = potion != null ? GetLocalizedPotionName(potion) : player.StoredPotionId;
                    int healValue = potion != null ? potion.HealValue : 0;
                    _potionText.text = Manager.LocalizationManager.Instance.GetString("hud_potion_equipped", potionName, healValue);
                }
            }

            bool hasSession = _roomManager != null && _roomManager.Session != null;
            
            if (_roomCountText != null)
            {
                int roomIndex = _dungeonManager != null ? _dungeonManager.CurrentNode + 1 : 0;
                int resolvedCount = hasSession ? _roomManager.Session.ResolvedCount : 0;
                _roomCountText.text = Manager.LocalizationManager.Instance.GetString("hud_room_count", roomIndex, resolvedCount);
            }

            if (_useStoredPotionButton != null)
            {
                bool canUseStored = !string.IsNullOrEmpty(player.StoredPotionId) 
                                    && hasSession 
                                    && !_roomManager.Session.PotionUsedInRoom;
                _useStoredPotionButton.interactable = canUseStored;
            }

            if (_escapeButton != null)
            {
                bool canEscape = hasSession && _encounterResolver != null 
                                 && _encounterResolver.ValidateEscape(player, _roomManager.Session);
                _escapeButton.interactable = canEscape;
            }

            RefreshSlotViews();
            ClearSlotSelection();
        }

        private void ClearSlotSelection()
        {
            bool hasSession = _roomManager != null && _roomManager.Session != null;

            for (int i = 0; i < _spawnedSlotViews.Count; i++)
            {
                var slot = _spawnedSlotViews[i];
                if (slot != null)
                {
                    bool isResolved = hasSession && _roomManager.Session.ResolvedSlots[i];
                    slot.SetSelected(isResolved);
                }
            }
        }

        private void SpawnSlotViews()
        {
            if (_encounterGrid == null || _roomManager == null) return;

            // Find existing SlotManager children in the grid
            var slots = _encounterGrid.GetComponentsInChildren<SlotManager>();
            _spawnedSlotViews.Clear();
            _spawnedSlotViews.AddRange(slots);

            // Subscribe to slot click events
            foreach (var slot in _spawnedSlotViews)
            {
                slot.OnSlotClicked += HandleSlotClicked;
            }
        }

        private void RefreshSlotViews()
        {
            if (_roomManager == null || _dungeonManager == null) return;

            string[] encounterIds = _roomManager.CurrentEncounterIds;
            bool hasSession = _roomManager.Session != null;

            for (int i = 0; i < _spawnedSlotViews.Count; i++)
            {
                if (i >= encounterIds.Length) break;

                string encounterId = encounterIds[i];

                // Skip null or empty encounter IDs
                if (string.IsNullOrWhiteSpace(encounterId))
                {
                    _spawnedSlotViews[i].Setup(i, null, false);
                    continue;
                }

                var encounter = _dungeonManager.GetEncounter(encounterId);
                bool isResolved = hasSession && _roomManager.Session.ResolvedSlots[i];

                _spawnedSlotViews[i].Setup(i, encounter, isResolved);
            }
        }

        private void HandleSlotClicked(int slotIndex)
        {
            ResolveReferences();

            if (_roomManager == null || _dungeonManager == null || _actionPanel == null) return;

            string[] encounterIds = _roomManager.CurrentEncounterIds;
            if (slotIndex < 0 || slotIndex >= encounterIds.Length) return;

            string encounterId = encounterIds[slotIndex];
            var encounter = _dungeonManager.GetEncounter(encounterId);
            if (encounter == null) return;

            // Highlight the selected slot, and retain highlight for already resolved slots
            bool hasSession = _roomManager.Session != null;
            for (int i = 0; i < _spawnedSlotViews.Count; i++)
            {
                bool isResolved = hasSession && _roomManager.Session.ResolvedSlots[i];
                _spawnedSlotViews[i].SetSelected(i == slotIndex || isResolved);
            }

            // Route setup based on type
            if (encounter is EnemiesScriptableObject monster)
            {
                bool canAttack = _encounterResolver != null && _encounterResolver.CanUseEquippedWeaponAgainst(monster.Strength);
                string monsterName = GetLocalizedMonsterName(monster);

                _actionPanel.SetupMonster(
                    monsterName,
                    monster.Strength,
                    canAttack,
                    onAttack: () =>
                    {
                        _roomManager.ResolveMonsterWithWeapon(slotIndex);
                        _actionPanel.Hide();
                        RefreshUI();
                    },
                    onTakeDamage: () =>
                    {
                        _roomManager.ResolveMonsterWithHealth(slotIndex);
                        _actionPanel.Hide();
                        RefreshUI();
                    },
                    onCancel: () =>
                    {
                        _actionPanel.Hide();
                        ClearSlotSelection();
                    }
                );
            }
            else if (encounter is ConsumablesScriptableObject potion)
            {
                string potionName = GetLocalizedPotionName(potion);
                bool potionUsed = _roomManager.Session != null && _roomManager.Session.PotionUsedInRoom;

                _actionPanel.SetupPotion(
                    potionName,
                    potion.HealValue,
                    potionUsed,
                    onUse: () =>
                    {
                        _roomManager.ResolveUsePotionFromSlot(slotIndex);
                        _actionPanel.Hide();
                        RefreshUI();
                    },
                    onStore: () =>
                    {
                        _roomManager.ResolveStorePotionFromSlot(slotIndex);
                        _actionPanel.Hide();
                        RefreshUI();
                    },
                    onCancel: () =>
                    {
                        _actionPanel.Hide();
                        ClearSlotSelection();
                    }
                );
            }
            else if (encounter is EquipmentsScriptableObject weapon)
            {
                string weaponName = GetLocalizedWeaponName(weapon);

                _actionPanel.SetupWeapon(
                    weaponName,
                    weapon.Power,
                    onEquip: () =>
                    {
                        _roomManager.ResolveWeapon(slotIndex);
                        _actionPanel.Hide();
                        RefreshUI();
                    },
                    onCancel: () =>
                    {
                        _actionPanel.Hide();
                        ClearSlotSelection();
                    }
                );
            }

            _actionPanel.Show();
        }

        private void HandleEscapeClicked()
        {
            ResolveReferences();
            if (_roomManager == null) return;

            _roomManager.TryEscapeRoom();
            RefreshUI();
        }

        private void HandleUseStoredPotionClicked()
        {
            ResolveReferences();
            if (_roomManager == null || _encounterResolver == null || _runManager == null || _dungeonManager == null) return;

            var result = _encounterResolver.TryUseStoredPotion(_dungeonManager, _roomManager.Session);
            if (result != null)
            {
                _runManager.ApplyResult(result);
                RefreshUI();
            }
        }

        private void ResolveReferences()
        {
            if (_roomManager == null) _roomManager = FindAnyObjectByType<RoomManager>();
            if (_runManager == null) _runManager = FindAnyObjectByType<RunManager>();
            if (_dungeonManager == null) _dungeonManager = FindAnyObjectByType<DungeonManager>();
            if (_encounterResolver == null) _encounterResolver = FindAnyObjectByType<EncounterResolver>();
        }

        private string GetLocalizedMonsterName(EnemiesScriptableObject monster)
        {
            var loc = LocalizationManager.Instance;
            return loc != null ? loc.GetMonsterName(monster.EncounterId) : monster.DisplayName;
        }

        private string GetLocalizedWeaponName(EquipmentsScriptableObject weapon)
        {
            var loc = LocalizationManager.Instance;
            return loc != null ? loc.GetWeaponName(weapon.EncounterId) : weapon.DisplayName;
        }

        private string GetLocalizedPotionName(ConsumablesScriptableObject potion)
        {
            var loc = LocalizationManager.Instance;
            return loc != null ? loc.GetPotionName(potion.EncounterId) : potion.DisplayName;
        }
    }
}
