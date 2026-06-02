using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections;

namespace Assets.Scripts.Manager
{
    public class LocalizationManager : MonoBehaviour
    {
        private static LocalizationManager _instance;

        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<LocalizationManager>();
                return _instance;
            }
        }

        private const string UITable = "UI Table";
        private const string AuthTable = "Authentication Table";
        private const string MonsterTable = "Monster Table";
        private const string WeaponTable = "Weapon Table";
        private const string PotionTable = "Potion Table";
        private const string SystemTable = "System Table";

        private const string LocalePrefsKey = "selected_locale";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            var savedLocale = PlayerPrefs.GetString(LocalePrefsKey, "en");
            StartCoroutine(ApplyLocaleCoroutine(savedLocale));
        }

        public void SwitchLanguage(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode)) return;
            PlayerPrefs.SetString(LocalePrefsKey, localeCode);
            PlayerPrefs.Save();
            StartCoroutine(ApplyLocaleCoroutine(localeCode));
        }

        private IEnumerator ApplyLocaleCoroutine(string localeCode)
        {
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

        public string GetString(string key) => GetString(UITable, key);

        public string GetString(string tableName, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;

            try
            {
                var op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, key);
                if (op.IsDone)
                    return op.Result ?? key;

                return key;
            }
            catch
            {
                return key;
            }
        }

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


        public string GetUIString(string key) => GetString(UITable, key);
        public string GetAuthString(string key) => GetString(AuthTable, key);
        public string GetMonsterName(string key) => GetString(MonsterTable, key);
        public string GetWeaponName(string key) => GetString(WeaponTable, key);
        public string GetPotionName(string key) => GetString(PotionTable, key);
        public string GetSystemString(string key) => GetString(SystemTable, key);
    }
}
