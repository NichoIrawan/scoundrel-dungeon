using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class ActionPanelView : MonoBehaviour
    {
        [Header("UI Component References")]
        [Tooltip("The description text showing information about the selected card.")]
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [Tooltip("The first action button (A).")]
        [SerializeField] private Button _actionButtonA;

        [Tooltip("The text label for action button (A).")]
        [SerializeField] private TextMeshProUGUI _actionButtonAText;

        [Tooltip("The second action button (B).")]
        [SerializeField] private Button _actionButtonB;

        [Tooltip("The text label for action button (B).")]
        [SerializeField] private TextMeshProUGUI _actionButtonBText;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetupMonster(
            string monsterName,
            int monsterStrength,
            bool canAttack,
            Action onAttack,
            Action onTakeDamage,
            Action onCancel)
        {
            ResetButtons();

            if (_descriptionText != null)
            {
                _descriptionText.text = Manager.LocalizationManager.Instance.GetString("action_monster_desc", monsterName, monsterStrength);
            }

            // Action Button A: Attack with Weapon
            if (_actionButtonA != null)
            {
                _actionButtonA.gameObject.SetActive(true);
                _actionButtonA.interactable = canAttack;
                if (_actionButtonAText != null)
                {
                    _actionButtonAText.text = canAttack ? Manager.LocalizationManager.Instance.GetUIString("action_attack") : Manager.LocalizationManager.Instance.GetUIString("action_attack_restricted");
                }
                _actionButtonA.onClick.AddListener(() => onAttack?.Invoke());
            }

            // Action Button B: Take Damage
            if (_actionButtonB != null)
            {
                _actionButtonB.gameObject.SetActive(true);
                _actionButtonB.interactable = true;
                if (_actionButtonBText != null)
                {
                    _actionButtonBText.text = Manager.LocalizationManager.Instance.GetString("action_take_damage", monsterStrength);
                }
                _actionButtonB.onClick.AddListener(() => onTakeDamage?.Invoke());
            }
        }

        public void SetupPotion(
            string potionName,
            int healValue,
            bool alreadyUsedPotionInRoom,
            Action onUse,
            Action onStore,
            Action onCancel)
        {
            ResetButtons();

            if (_descriptionText != null)
            {
                string healDetails = alreadyUsedPotionInRoom 
                    ? Manager.LocalizationManager.Instance.GetUIString("action_potion_heal_used")
                    : Manager.LocalizationManager.Instance.GetString("action_potion_heal_active", healValue);
                _descriptionText.text = Manager.LocalizationManager.Instance.GetString("action_potion_desc", potionName, healDetails);
            }

            // Action Button A: Use Potion
            if (_actionButtonA != null)
            {
                _actionButtonA.gameObject.SetActive(true);
                _actionButtonA.interactable = true;
                if (_actionButtonAText != null)
                {
                    _actionButtonAText.text = alreadyUsedPotionInRoom ? Manager.LocalizationManager.Instance.GetUIString("action_potion_use_zero") : Manager.LocalizationManager.Instance.GetUIString("action_potion_use");
                }
                _actionButtonA.onClick.AddListener(() => onUse?.Invoke());
            }

            // Action Button B: Store Potion
            if (_actionButtonB != null)
            {
                _actionButtonB.gameObject.SetActive(true);
                _actionButtonB.interactable = true;
                if (_actionButtonBText != null)
                {
                    _actionButtonBText.text = Manager.LocalizationManager.Instance.GetUIString("action_potion_store");
                }
                _actionButtonB.onClick.AddListener(() => onStore?.Invoke());
            }
        }

        public void SetupWeapon(
            string weaponName,
            int weaponPower,
            Action onEquip,
            Action onCancel)
        {
            ResetButtons();

            if (_descriptionText != null)
            {
                _descriptionText.text = string.Empty;
            }

            // Action Button A: Equip
            if (_actionButtonA != null)
            {
                _actionButtonA.gameObject.SetActive(true);
                _actionButtonA.interactable = true;
                if (_actionButtonAText != null)
                {
                    _actionButtonAText.text = Manager.LocalizationManager.Instance.GetUIString("action_weapon_equip");
                }
                _actionButtonA.onClick.AddListener(() => onEquip?.Invoke());
            }

            // Action Button B: Not needed for Weapons, so hide it
            if (_actionButtonB != null)
            {
                _actionButtonB.gameObject.SetActive(false);
            }
        }

        private void ResetButtons()
        {
            if (_actionButtonA != null)
            {
                _actionButtonA.onClick.RemoveAllListeners();
                _actionButtonA.gameObject.SetActive(false);
            }
            if (_actionButtonB != null)
            {
                _actionButtonB.onClick.RemoveAllListeners();
                _actionButtonB.gameObject.SetActive(false);
            }
        }
    }
}
