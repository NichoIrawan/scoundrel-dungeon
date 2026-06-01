using Assets.Scripts.ScriptableObjects;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "ConsumablesData", menuName = "ScriptableObjects/ConsumablesScriptableObject")]
    public class ConsumablesScriptableObject : EncounterScriptableObject
    {
        [Tooltip("HP restored when used. Replaces old 'HealAmount' field. TDD §4.2")]
        public int HealValue;
    }
}
