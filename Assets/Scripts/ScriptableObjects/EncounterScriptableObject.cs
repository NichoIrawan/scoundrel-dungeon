using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    public abstract class EncounterScriptableObject : ScriptableObject
    {
        [Tooltip("Unique identifier used in save data and registry lookups. Never localized.")]
        public string EncounterId;

        [Tooltip("Raw display name. Used as registry key. Localized text is fetched via LocalizationManager.")]
        public string DisplayName;

        [Tooltip("Artwork displayed on the encounter slot.")]
        public Sprite Artwork;
    }
}
