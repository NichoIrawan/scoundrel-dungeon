using Assets.Scripts.ScriptableObjects;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "PotionData", menuName = "ScriptableObjects/PotionScriptableObject")]
    public class PotionScriptableObject : EncounterScriptableObject
    {
        [Tooltip("HP restored when this potion is used. Zero if PotionUsedInRoom is already true.")]
        public int HealValue;
    }
}
