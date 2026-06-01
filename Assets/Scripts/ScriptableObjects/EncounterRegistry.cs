using Assets.Scripts.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "EncounterRegistry", menuName = "ScriptableObjects/EncounterRegistry")]
    public class EncounterRegistry : ScriptableObject
    {
        [SerializeField] private List<EncounterScriptableObject> _encounters;

        public Dictionary<string, EncounterScriptableObject> Dictionary { get; private set; }
        public List<string> MonsterIds { get; private set; } = new();
        public List<string> WeaponIds { get; private set; } = new();
        public List<string> PotionIds { get; private set; } = new();

        public void Initialize()
        {
            Dictionary = new Dictionary<string, EncounterScriptableObject>();
            MonsterIds = new List<string>();
            WeaponIds = new List<string>();
            PotionIds = new List<string>();

            if (_encounters == null) return;

            foreach (var encounter in _encounters)
            {
                if (encounter == null) continue;

                var key = !string.IsNullOrWhiteSpace(encounter.EncounterId)
                    ? encounter.EncounterId
                    : encounter.DisplayName;

                if (string.IsNullOrWhiteSpace(key))
                {
                    Debug.LogWarning($"[EncounterRegistry] Encounter asset '{encounter.name}' has no EncounterId or DisplayName. Skipping.");
                    continue;
                }

                Dictionary[key] = encounter;

                if (encounter is EnemiesScriptableObject)
                {
                    MonsterIds.Add(key);
                }
                else if (encounter is EquipmentsScriptableObject)
                {
                    WeaponIds.Add(key);
                }
                else if (encounter is ConsumablesScriptableObject)
                {
                    PotionIds.Add(key);
                }
                else
                {
                    Debug.LogWarning($"[EncounterRegistry] Unknown encounter type '{encounter.GetType().Name}' for key '{key}'.");
                }
            }
        }

        public EncounterScriptableObject TryGetEncounter(string id)
        {
            if (Dictionary == null)
            {
                Debug.LogError("[EncounterRegistry] Dictionary is null. Was Initialize() called?");
                return null;
            }

            return Dictionary.TryGetValue(id, out var encounter) ? encounter : null;
        }
    }
}
