using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections;

namespace Assets.Scripts.Manager
{
    /// <summary>
    /// Manages locale initialization, language switching, and string retrieval.
    /// Uses Unity Localization Package (com.unity.localization).
    /// Only exposes language switching from the Main Menu (TDD §9.10).
    /// TDD §3.3, §9
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        private static LocalizationManager _instance;

        /// <summary>Singleton access. TDD §3.3 (Core scene persistent manager).</summary>
        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<LocalizationManager>();
                return _instance;
            }
        }

        // Table names — must match string table names in Unity Localization settings
        private const string UITable = "UI Table";
        private const string AuthTable = "Authentication Table";
        private const string MonsterTable = "Monster Table";
        private const string WeaponTable = "Weapon Table";
        private const string PotionTable = "Potion Table";
        private const string SystemTable = "System Table";

        // PlayerPrefs key for persisting the last-used locale
        private const string LocalePrefsKey = "selected_locale";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Initialize();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Initialization (TDD §9)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sets the locale to the last-saved preference, or English by default.
        /// TDD §9.3 — English is the default language.
        /// </summary>
        public void Initialize()
        {
            var savedLocale = PlayerPrefs.GetString(LocalePrefsKey, "en");
            StartCoroutine(ApplyLocaleCoroutine(savedLocale));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Language Switching (TDD §9.10)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Switches the active locale. Only valid from Main Menu.
        /// Persists the choice to PlayerPrefs.
        /// TDD §9.10 — locale codes: "en" or "ja".
        /// </summary>
        public void SwitchLanguage(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode)) return;
            PlayerPrefs.SetString(LocalePrefsKey, localeCode);
            PlayerPrefs.Save();
            StartCoroutine(ApplyLocaleCoroutine(localeCode));
        }

        private IEnumerator ApplyLocaleCoroutine(string localeCode)
        {
            // Wait until LocalizationSettings is initialized
            yield return LocalizationSettings.InitializationOperation;

            var locales = LocalizationSettings.AvailableLocales.Locales;
            Locale target = null;

            foreach (var locale in locales)
            {
                if (locale.Identifier.Code == localeCode)
                {
                    target = locale;
                    break;
                }
            }

            if (target != null)
            {
                LocalizationSettings.SelectedLocale = target;
                Debug.Log($"[LocalizationManager] Locale set to '{localeCode}'.");
            }
            else
            {
                Debug.LogWarning($"[LocalizationManager] Locale '{localeCode}' not found. Keeping current locale.");
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // String Retrieval (TDD §9.8)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the localized string for the given key from the UI Table.
        /// Returns the raw key if the entry is not found (fail-safe).
        /// TDD §9.7
        /// </summary>
        public string GetString(string key) => GetString(UITable, key);

        /// <summary>
        /// Returns the localized string for the given key from a specific table.
        /// TDD §9.7
        /// </summary>
        public string GetString(string tableName, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;

            try
            {
                var op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, key);
                if (op.IsDone)
                    return op.Result ?? key;

                // Synchronous fallback — returns key while async completes
                return key;
            }
            catch
            {
                return key;
            }
        }

        /// <summary>
        /// Returns a formatted localized string with runtime arguments.
        /// TDD §9.8 — placeholder format: {0}, {1}, etc.
        /// </summary>
        public string GetString(string key, params object[] args)
        {
            var template = GetString(key);
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Table-specific helpers (TDD §9.7)
        // ─────────────────────────────────────────────────────────────────────────

        public string GetUIString(string key) => GetString(UITable, key);
        public string GetAuthString(string key) => GetString(AuthTable, key);
        public string GetMonsterName(string key) => GetString(MonsterTable, key);
        public string GetWeaponName(string key) => GetString(WeaponTable, key);
        public string GetPotionName(string key) => GetString(PotionTable, key);
        public string GetSystemString(string key) => GetString(SystemTable, key);
    }
}
