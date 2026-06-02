using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Manager;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.ScriptableObjects;

namespace Assets.Scripts.UI
{
    public class HallwayUIController : MonoBehaviour
    {
        [Header("HUD Elements")]
        [Tooltip("The TextMeshProUGUI component that displays the player's health.")]
        [SerializeField] private TextMeshProUGUI _hpText;

        [Tooltip("The TextMeshProUGUI component that displays the player's equipped weapon details.")]
        [SerializeField] private TextMeshProUGUI _weaponText;

        [Tooltip("The TextMeshProUGUI component that displays the player's stored potion details.")]
        [SerializeField] private TextMeshProUGUI _potionText;

        [Tooltip("The TextMeshProUGUI component displaying the current hallway/room node description.")]
        [SerializeField] private TextMeshProUGUI _roomTitleText;

        [Header("Debt Panel Elements")]
        [Tooltip("The TextMeshProUGUI component that displays the count of unresolved room seals (Debt).")]
        [SerializeField] private TextMeshProUGUI _sealsText;

        [Header("Hierarchy Containers")]
        [Tooltip("The RectTransform that acts as the container for dynamic door/navigation buttons.")]
        [SerializeField] private RectTransform _doorContainer;

        [Header("Prefab References")]
        [Tooltip("The prefab containing the Button component to instantiate for navigation doors.")]
        [SerializeField] private Button _doorButtonPrefab;

        [Header("Manager References (Optional in Editor, Auto-resolved at Start)")]
        [SerializeField] private HallwayManager _hallwayManager;
        [SerializeField] private RunManager _runManager;
        [SerializeField] private DungeonManager _dungeonManager;

        private void Start()
        {
            ResolveReferences();

            if (_doorContainer != null)
            {
                foreach (Transform child in _doorContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            PopulateDoorButtons();
            RefreshUI();
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

            if (_roomTitleText != null)
            {
                _roomTitleText.text = Manager.LocalizationManager.Instance.GetString("hallway_node_title", _dungeonManager.CurrentNode);
            }

            if (_sealsText != null)
            {
                int sealCount = _dungeonManager.UnresolvedNodes != null ? _dungeonManager.UnresolvedNodes.Count : 0;
                _sealsText.text = Manager.LocalizationManager.Instance.GetString("hallway_debt", sealCount);
            }
        }

        private void PopulateDoorButtons()
        {
            if (_doorContainer == null || _doorButtonPrefab == null || _dungeonManager == null) return;

            var state = _dungeonManager.State;
            if (state == null || !state.Nodes.TryGetValue(_dungeonManager.CurrentNode, out var currentNode)) return;

            int leftId = currentNode.LeftNode;
            int rightId = currentNode.RightNode;

            bool leftOpen = leftId >= 0 && state.Nodes.ContainsKey(leftId);
            bool rightOpen = rightId >= 0 && state.Nodes.ContainsKey(rightId);

            if (leftOpen && !rightOpen)
            {
                CreateDoorButton(leftId);
            }
            else if (!leftOpen && rightOpen)
            {
                CreateDoorButton(rightId);
            }
            else
            {
                if (leftOpen) CreateDoorButton(leftId);
                if (rightOpen) CreateDoorButton(rightId);
            }
        }

        private void CreateDoorButton(int targetNodeId)
        {
            var buttonInstance = Instantiate(_doorButtonPrefab, _doorContainer);
            
            var textComponent = buttonInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                // Node 6 is fixed as the exit node of the dungeon in MVP Phases
                textComponent.text = (targetNodeId == 6) ? Manager.LocalizationManager.Instance.GetUIString("hallway_exit") : Manager.LocalizationManager.Instance.GetString("hallway_enter_room", targetNodeId);
            }

            buttonInstance.onClick.AddListener(() => HandleDoorClicked(targetNodeId));
        }

        private void HandleDoorClicked(int targetNodeId)
        {
            ResolveReferences();
            if (_hallwayManager == null) return;

            _hallwayManager.OnDoorSelected(targetNodeId);
        }

        private void ResolveReferences()
        {
            if (_hallwayManager == null) _hallwayManager = FindAnyObjectByType<HallwayManager>();
            if (_runManager == null) _runManager = FindAnyObjectByType<RunManager>();
            if (_dungeonManager == null) _dungeonManager = FindAnyObjectByType<DungeonManager>();
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
