using Assets.Scripts.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "EncounterRegistry", menuName = "ScriptableObjects/EncounterRegistry")]
    public class EncounterRegistry : ScriptableObject
    {
        [SerializeField] private List<Encounter> _effigies;
        public Dictionary<string, Encounter> Dictionary;

        public List<string> _enemies = new();
        public List<string> _equipments = new();
        public List<string> _consumables = new();

        public void Initialize()
        {
            Dictionary = new Dictionary<string, Encounter>();
            _enemies.Clear();
            _equipments.Clear();
            _consumables.Clear();

            foreach (var encounter in _effigies)
            {
                Dictionary[encounter.Name] = encounter;
            }

            foreach (var kvp in Dictionary)
            {
                if (kvp.Value is EnemiesScriptableObject)
                {
                    _enemies.Add(kvp.Key);
                }
                else if (kvp.Value is EquipmentsScriptableObject)
                {
                    _equipments.Add(kvp.Key);
                }
                else if (kvp.Value is ConsumablesScriptableObject)
                {
                    _consumables.Add(kvp.Key);
                }
            }
        }

        public Encounter TryGetEncounter(string id)
        {
            if (Dictionary.TryGetValue(id, out var encounter))
            {
                return encounter;
            }

            return null;
        }
    }
}
