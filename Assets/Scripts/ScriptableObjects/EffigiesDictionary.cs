using Assets.Scripts.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "EffigiesDictionary", menuName = "ScriptableObjects/EffigiesDictionary")]
    public class EffigiesDictionary : ScriptableObject
    {
        [SerializeField] private List<Effigies> _effigies;
        public Dictionary<string, Effigies> Dictionary;

        public List<string> _enemies = new();
        public List<string> _equipments = new();
        public List<string> _consumables = new();

        public void Initialize()
        {
            Dictionary = new Dictionary<string, Effigies>();
            foreach (var effigy in _effigies)
            {
                Dictionary[effigy.Name] = effigy;
            }

            // Sorting effigies into categories
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

        public Effigies TryGetEffigy(string name)
        {
            if (Dictionary.TryGetValue(name, out var effigy))
            {
                return effigy;
            }
            return null;
        }
    }
}
